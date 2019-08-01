using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Examine;
using Umbraco.Web.Scheduling;

namespace UmbracoExamine.PDF
{
    /// <summary>
    ///     Component to index PDF documents in the media library
    /// </summary>
    public class ExaminePdfComponent : IComponent
    {
        private readonly IExamineManager _examineManager;
        private readonly ILogger _logger;
        private readonly PdfIndexCreator _pdfIndexCreator;
        private readonly PdfIndexPopulator _pdfIndexPopulator;

        public ExaminePdfComponent(
            IExamineManager examineManager,
            PdfIndexCreator pdfIndexCreator,
            ILogger logger,
            PdfIndexPopulator pdfIndexPopulator)
        {
            _examineManager = examineManager;
            _pdfIndexCreator = pdfIndexCreator;
            _logger = logger;
            _pdfIndexPopulator = pdfIndexPopulator;
        }

        public void Initialize()
        {
            foreach (var index in _pdfIndexCreator.Create())
            {
                //TODO: This should not be needed but Umbraco needs to update itself to fix the issue
                // ensure the index is unlocked 
                if (index is LuceneIndex luceneIndex)
                {
                    var dir = luceneIndex.GetLuceneDirectory();
                    if (IndexWriter.IsLocked(dir))
                    {
                        _logger.Info(typeof(ExaminePdfComponent),
                            "Forcing index {IndexerName} to be unlocked since it was left in a locked state",
                            luceneIndex.Name);
                        IndexWriter.Unlock(dir);
                    }
                }

                _examineManager.AddIndex(index);
            }

            //Tie into various events one media saved/trashed/deleted/restored
            MediaService.Saved += MediaService_Saved;
            MediaService.Deleted += MediaService_Deleted;
            MediaService.Trashed += MediaService_Trashed;
            MediaService.Moved += MediaService_Moved;
        }

        public void Terminate()
        {
        }

        /// <summary>
        ///     If pdfs were restored form recycle bin, re-index them
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaService_Moved(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            var restored = e.MoveInfoCollection
                .Where(m => m.OriginalPath.StartsWith(Constants.System.RecycleBinMediaPathPrefix))
                .Select(m => m.Entity);
            if (restored.Any())
            {
                _pdfIndexPopulator.AddToIndex(restored);
            }
        }

        /// <summary>
        ///     Remove any pdfs that have been trashed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaService_Trashed(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            _pdfIndexPopulator.RemoveFromIndex(e.MoveInfoCollection.Select(m => m.Entity));
        }

        /// <summary>
        ///     Remvoe any pdfs that have been deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaService_Deleted(IMediaService sender, DeleteEventArgs<IMedia> e)
        {
            _pdfIndexPopulator.RemoveFromIndex(e.DeletedEntities);
        }


        /// <summary>
        ///     Index any saved pdfs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaService_Saved(IMediaService sender, SaveEventArgs<IMedia> e)
        {
            _pdfIndexPopulator.AddToIndex(e.SavedEntities);
        }
    }
}