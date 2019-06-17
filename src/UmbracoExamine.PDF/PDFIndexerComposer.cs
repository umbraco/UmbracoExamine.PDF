using Umbraco.Core;
using Umbraco.Core.Composing;

namespace UmbracoExamine.PDF
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class PDFIndexerComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            //Compose the Address Indexer & register services it will need
            composition.Components().Append<PDFIndexerComponent>();

            //Register our address service and other services used to make this all work
            composition.Register<PDFTextService>(Lifetime.Singleton);
            composition.RegisterUnique<IPDFIndexValueSetBuilder, PDFIndexValueSetBuilder>();
            composition.Register<PDFIndexPopulator>(Lifetime.Singleton);
            composition.RegisterUnique<PDFIndexCreator>();
        }
    }
}