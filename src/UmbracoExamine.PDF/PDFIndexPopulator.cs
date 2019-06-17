using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Examine;

namespace UmbracoExamine.PDF
{
    public class PDFIndexPopulator : IndexPopulator
    {
        private readonly int? _parentId;
        private readonly IMediaService _mediaService;
        private readonly IPDFIndexValueSetBuilder _mediaValueSetBuilder;

        /// <summary>
        /// Default constructor to lookup all content data
        /// </summary>
        /// <param name="mediaService"></param>
        /// <param name="mediaValueSetBuilder"></param>
        public PDFIndexPopulator(IMediaService mediaService, IPDFIndexValueSetBuilder mediaValueSetBuilder)
            : this(null, mediaService, mediaValueSetBuilder)
        {
        }

        /// <summary>
        /// Optional constructor allowing specifying custom query parameters
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="mediaService"></param>
        /// <param name="mediaValueSetBuilder"></param>
        public PDFIndexPopulator(int? parentId, IMediaService mediaService, IPDFIndexValueSetBuilder mediaValueSetBuilder)
        {
            _parentId = parentId;
            _mediaService = mediaService;
            _mediaValueSetBuilder = mediaValueSetBuilder;
            RegisterIndex("PDFIndex");
        }

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
                media = _mediaService.GetPagedDescendants(mediaParentId, pageIndex, pageSize, out var total)
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