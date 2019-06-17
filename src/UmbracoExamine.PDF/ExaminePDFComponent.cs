using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace UmbracoExamine.PDF
{
    public class ExaminePDFComponent : IComponent
    {
        private readonly IExamineManager _examineManager;
        private readonly PDFIndexCreator _PDFIndexCreator;
        private readonly IProfilingLogger _logger;

        public ExaminePDFComponent(IExamineManager examineManager, PDFIndexCreator pdfIndexCreator, IProfilingLogger profilingLogger)
        {
            _examineManager = examineManager;
            _PDFIndexCreator = pdfIndexCreator;
            _logger = profilingLogger;
        }

        public void Initialize()
        {
            foreach (var index in _PDFIndexCreator.Create()) {
                // ensure the index is unlocked 
                if (index is LuceneIndex)
                {
                    var luceneIndex = index as LuceneIndex;
                    var dir = luceneIndex.GetLuceneDirectory();
                    if (IndexWriter.IsLocked(dir))
                    {
                        _logger.Info(typeof(ExamineExtensions), "Forcing index {IndexerName} to be unlocked since it was left in a locked state", luceneIndex.Name);
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