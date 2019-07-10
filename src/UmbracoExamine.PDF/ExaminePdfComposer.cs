using Umbraco.Core;
using Umbraco.Core.Composing;

namespace UmbracoExamine.PDF
{
    /// <summary>
    /// Registers the ExaminePDFComponent and all of it's injected dependencies
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ExaminePdfComposer : ComponentComposer<ExaminePdfComponent>, IUserComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            //Register our address service and other services used to make this all work
            composition.Register<PdfTextService>(Lifetime.Singleton);
            composition.RegisterUnique<IPdfIndexValueSetBuilder, PdfIndexValueSetBuilder>();
            composition.RegisterUnique<IPdfTextExtractor, PdfSharpTextExtractor>();
            composition.Register<PdfIndexPopulator>(Lifetime.Singleton);
            composition.RegisterUnique<PdfIndexCreator>();
        }
    }
}