using System;
using System.Globalization;
using System.Linq;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;

namespace UmbracoExamine.PDF
{
    /// <summary>
    /// Component to index PDF documents in the media library
    /// </summary>
    public class ExaminePdfComponent : IComponent //TODO: ExamineUserComponent 
    {
        private readonly IExamineManager _examineManager;
        private readonly PdfIndexCreator _pdfIndexCreator;
        private readonly PdfIndexPopulator _pdfIndexPopulator;
        private readonly IMediaService _mediaService;

        public ExaminePdfComponent(
            IExamineManager examineManager,
            PdfIndexCreator pdfIndexCreator,
            PdfIndexPopulator pdfIndexPopulator,
            IMediaService mediaService)
        {
            _examineManager = examineManager;
            _pdfIndexCreator = pdfIndexCreator;
            _pdfIndexPopulator = pdfIndexPopulator;
            _mediaService = mediaService;
        }

        public void Initialize()
        {
            foreach (var index in _pdfIndexCreator.Create())
                _examineManager.AddIndex(index);

            MediaCacheRefresher.CacheUpdated += MediaCacheRefresherUpdated;
        }

        public void Terminate()
        {
        }

        private void MediaCacheRefresherUpdated(MediaCacheRefresher sender, CacheRefresherEventArgs args)
        {
            //TODO: if (!ExamineComponent.ExamineEnabled) return;

            if (args.MessageType != MessageType.RefreshByPayload)
                throw new NotSupportedException();

            foreach (var payload in (MediaCacheRefresher.JsonPayload[])args.MessageObject)
            {
                if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
                {
                    _pdfIndexPopulator.RemoveFromIndex(payload.Id);
                }
                else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                {
                    // ExamineEvents does not support RefreshAll
                    // just ignore that payload
                    // so what?!
                }
                else // RefreshNode or RefreshBranch (maybe trashed)
                {
                    var media = _mediaService.GetById(payload.Id);
                    if (media == null)
                    {
                        // gone fishing, remove entirely
                        _pdfIndexPopulator.RemoveFromIndex(payload.Id);
                        continue;
                    }

                    if (media.Trashed)
                        _pdfIndexPopulator.RemoveFromIndex(payload.Id);

                    // just that media
                    _pdfIndexPopulator.AddToIndex(media);

                    // branch
                    if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        const int pageSize = 500;
                        var page = 0;
                        var total = long.MaxValue;
                        while (page * pageSize < total)
                        {
                            var descendants = _mediaService.GetPagedDescendants(media.Id, page++, pageSize, out total);
                            foreach (var descendant in descendants)
                            {
                                _pdfIndexPopulator.AddToIndex(descendant);
                            }
                        }
                    }
                }
            }
        }

        
    }
}
