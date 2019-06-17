using Umbraco.Core;
using Umbraco.Core.Composing;

namespace UmbracoExamine.PDF
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class PDFIndexerComposer : ComponentComposer<ExaminePDFComponent>, IUserComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            //Register our address service and other services used to make this all work
            composition.Register<PDFTextService>(Lifetime.Singleton);
            composition.RegisterUnique<IPDFIndexValueSetBuilder, PDFIndexValueSetBuilder>();
            composition.Register<PDFIndexPopulator>(Lifetime.Singleton);
            composition.RegisterUnique<PDFIndexCreator>();
        }
    }
}