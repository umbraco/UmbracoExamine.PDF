using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Examine;

namespace UmbracoExamine.PDF
{
    /// <summary>
    /// Performs the data lookups required to rebuild a PDF index
    /// </summary>
    public class PdfIndexPopulator : IndexPopulator
    {
        private readonly int? _parentId;
        private readonly IMediaService _mediaService;
        private readonly IPdfIndexValueSetBuilder _mediaValueSetBuilder;
        private readonly IExamineManager _examineManager;

        /// <summary>
        /// Default constructor to lookup all content data
        /// </summary>
        /// <param name="mediaService"></param>
        /// <param name="mediaValueSetBuilder"></param>
        public PdfIndexPopulator(IMediaService mediaService, IPdfIndexValueSetBuilder mediaValueSetBuilder, IExamineManager examineManager)
            : this(null, mediaService, mediaValueSetBuilder, examineManager)
        {
        }

        /// <summary>
        /// Optional constructor allowing specifying custom query parameters
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="mediaService"></param>
        /// <param name="mediaValueSetBuilder"></param>
        public PdfIndexPopulator(int? parentId, IMediaService mediaService, IPdfIndexValueSetBuilder mediaValueSetBuilder, IExamineManager examineManager)
        {
            _parentId = parentId;
            _mediaService = mediaService;
            _mediaValueSetBuilder = mediaValueSetBuilder;
            _examineManager = examineManager;
            RegisterIndex("PDFIndex");

            //Tie into various events one media saved/trashed/deleted/restored
            Umbraco.Core.Services.Implement.MediaService.Saved += MediaService_Saved;
            Umbraco.Core.Services.Implement.MediaService.Deleted += MediaService_Deleted;
            Umbraco.Core.Services.Implement.MediaService.Trashed += MediaService_Trashed;
            Umbraco.Core.Services.Implement.MediaService.Moved += MediaService_Moved;
        }


        /// <summary>
        /// If pdfs were restored form recycle bin, re-index them
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaService_Moved(IMediaService sender, Umbraco.Core.Events.MoveEventArgs<IMedia> e)
        {
            var restored = e.MoveInfoCollection
                .Where(m => m.OriginalPath.StartsWith("-1")) //recyclebin
                .Select(m => m.Entity);
            if (restored.Any())
            {
                AddToIndex(restored);
            }
        }

        /// <summary>
        /// Remove any pdfs that have been trashed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaService_Trashed(IMediaService sender, Umbraco.Core.Events.MoveEventArgs<IMedia> e)
        {
            RemoveFromIndex(e.MoveInfoCollection.Select(m => m.Entity));
        }

        /// <summary>
        /// Remvoe any pdfs that have been deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaService_Deleted(IMediaService sender, Umbraco.Core.Events.DeleteEventArgs<IMedia> e)
        {
            RemoveFromIndex(e.DeletedEntities);
        }

        /// <summary>
        /// if any of the media is a pdf remove it from the PDFIndex
        /// </summary>
        /// <param name="media"></param>
        private void RemoveFromIndex(IEnumerable<IMedia> media)
        {
            if (_examineManager.TryGetIndex("PDFIndex", out var index))
            {
                var ids = media.Where(m => m.GetValue<string>("umbracoExtension") == "pdf").Select(m => m.Id.ToString());
                if (ids.Any())
                {
                    index.DeleteFromIndex(ids);
                }
            }

        }

        /// <summary>
        /// Index any saved pdfs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaService_Saved(IMediaService sender, Umbraco.Core.Events.SaveEventArgs<Umbraco.Core.Models.IMedia> e)
        {
            AddToIndex(e.SavedEntities);
        }

        /// <summary>
        /// Add any media that is a pdf to the PDFIndex
        /// </summary>
        /// <param name="media"></param>
        private void AddToIndex(IEnumerable<IMedia> media)
        {
            if (_examineManager.TryGetIndex("PDFIndex", out var index))
            {
                var mediaToIndex = media.Where(m => m.GetValue<string>("umbracoExtension") == "pdf").ToArray();
                if (mediaToIndex.Any())
                {
                    index.IndexItems(_mediaValueSetBuilder.GetValueSets(mediaToIndex));
                }
            }
        }

        /// <summary>
        /// Crawl all media content and index any documents with the .pdf extension
        /// </summary>
        /// <param name="indexes"></param>
        protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
        {
            if (indexes.Count == 0) return;

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
                    .Where(m => m.GetValue<string>("umbracoExtension") == "pdf")
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