using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace UmbracoExamine.PDF
{
    /// <summary>
    /// Component to index PDF documents in the media library
    /// </summary>
    public class ExaminePdfComponent : IComponent
    {
        private readonly IExamineManager _examineManager;
        private readonly PdfIndexCreator _pdfIndexCreator;
        private readonly ILogger _logger;

        public ExaminePdfComponent(IExamineManager examineManager, PdfIndexCreator pdfIndexCreator, ILogger logger)
        {
            _examineManager = examineManager;
            _pdfIndexCreator = pdfIndexCreator;
            _logger = logger;
        }

        public void Initialize()
        {
            foreach (var index in _pdfIndexCreator.Create()) {
                // ensure the index is unlocked 
                if (index is LuceneIndex luceneIndex)
                {
                    var dir = luceneIndex.GetLuceneDirectory();
                    if (IndexWriter.IsLocked(dir))
                    {
                        _logger.Info(typeof(ExaminePdfComponent), "Forcing index {IndexerName} to be unlocked since it was left in a locked state", luceneIndex.Name);
                        IndexWriter.Unlock(dir);
                    }
                }
                _examineManager.AddIndex(index);
            }
        }

        public void Terminate() {
            
        }

    }
}