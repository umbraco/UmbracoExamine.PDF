using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Examine;

namespace UmbracoExamine.PDF
{
    /// <summary>
    ///     Performs the data lookups required to rebuild a PDF index
    /// </summary>
    public class PdfIndexPopulator : IndexPopulator
    {
        private const string PdfFileExtension = "pdf";
        private readonly IExamineManager _examineManager;
        private readonly IMediaService _mediaService;
        private readonly IPdfIndexValueSetBuilder _mediaValueSetBuilder;
        private readonly int? _parentId;

        /// <summary>
        ///     Default constructor to lookup all content data
        /// </summary>
        /// <param name="mediaService"></param>
        /// <param name="mediaValueSetBuilder"></param>
        public PdfIndexPopulator(IMediaService mediaService, IPdfIndexValueSetBuilder mediaValueSetBuilder,
            IExamineManager examineManager)
            : this(null, mediaService, mediaValueSetBuilder, examineManager)
        {
        }

        /// <summary>
        ///     Optional constructor allowing specifying custom query parameters
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="mediaService"></param>
        /// <param name="mediaValueSetBuilder"></param>
        public PdfIndexPopulator(int? parentId, IMediaService mediaService,
            IPdfIndexValueSetBuilder mediaValueSetBuilder, IExamineManager examineManager)
        {
            _parentId = parentId;
            _mediaService = mediaService;
            _mediaValueSetBuilder = mediaValueSetBuilder;
            _examineManager = examineManager;
            RegisterIndex(PdfIndexCreator.PdfIndexName);
        }


        /// <summary>
        ///     if any of the media is a pdf remove it from the PDFIndex
        /// </summary>
        /// <param name="media"></param>
        public void RemoveFromIndex(IEnumerable<IMedia> media)
        {
            HandleMediaItemsInIndex(media, (items, index) =>
            {
                var ids = items.Select(m => m.Id.ToString());
                if (ids.Any())
                {
                    index.DeleteFromIndex(ids);
                }
            });
        }

        private void HandleMediaItemsInIndex(IEnumerable<IMedia> media, Action<IMedia[], IIndex> action)
        {
            if (_examineManager.TryGetIndex(PdfIndexCreator.PdfIndexName, out var index))
            {
                var mediaToIndex = media.Where(m => m.GetValue<string>("umbracoExtension") == PdfFileExtension)
                    .ToArray();
                if (mediaToIndex.Any())
                {
                    action(mediaToIndex, index);
                }
            }
        }


        /// <summary>
        ///     Add any media that is a pdf to the PDFIndex
        /// </summary>
        /// <param name="media"></param>
        public void AddToIndex(IEnumerable<IMedia> media)
        {
            HandleMediaItemsInIndex(media,
                (items, index) => { index.IndexItems(_mediaValueSetBuilder.GetValueSets(items)); });
        }

        /// <summary>
        ///     Crawl all media content and index any documents with the .pdf extension
        /// </summary>
        /// <param name="indexes"></param>
        protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
        {
            if (indexes.Count == 0)
            {
                return;
            }

            const int pageSize = 10000;
            var pageIndex = 0;

            var mediaParentId = -1;

            if (_parentId.HasValue && _parentId.Value > 0)
            {
                mediaParentId = _parentId.Value;
            }

            IMedia[] media;

            do
            {
                media = _mediaService.GetPagedDescendants(mediaParentId, pageIndex, pageSize, out _)
                    .Where(m => m.GetValue<string>("umbracoExtension") == PdfFileExtension)
                    .ToArray();

                if (media.Length > 0)
                {
                    foreach (var index in indexes)
                    {
                        index.IndexItems(_mediaValueSetBuilder.GetValueSets(media));
                    }
                }

                pageIndex++;
            } while (media.Length == pageSize);
        }
    }
}