using Examine;
using Umbraco.Core.Composing;

namespace UmbracoExamine.PDF
{
    public class PDFIndexerComponent : IComponent
    {
        private readonly IExamineManager _examineManager;
        private readonly PDFIndexCreator _PDFIndexCreator;

        public PDFIndexerComponent(IExamineManager examineManager, PDFIndexCreator pdfIndexCreator)
        {
            _examineManager = examineManager;
            _PDFIndexCreator = pdfIndexCreator;
        }

        public void Initialize()
        {
            foreach (var index in _PDFIndexCreator.Create()) { 
                _examineManager.AddIndex(index);
            }
        }

        public void Terminate() {
            
        }

    }
}