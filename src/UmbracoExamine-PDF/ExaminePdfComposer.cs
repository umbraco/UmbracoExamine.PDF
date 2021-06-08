using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace UmbracoExamine.PDF
{
    /// <summary>
    ///     Registers the ExaminePDFComponent and all of it's injected dependencies
    /// </summary>
    // TODO (V9): Replace with check elsewhere
    //[RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ExaminePdfComposer : ComponentComposer<ExaminePdfComponent>, IUserComposer
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);

            //Register the services used to make this all work
            builder.Services.AddUnique<IPdfTextExtractor, PdfPigTextExtractor>();
            builder.Services.AddSingleton<PdfTextService>();
            builder.Services.AddUnique<IPdfIndexValueSetBuilder, PdfIndexValueSetBuilder>();
            builder.Services.AddSingleton<IIndexPopulator, PdfIndexPopulator>();
            builder.Services.AddSingleton<PdfIndexPopulator>();
            builder.Services.AddUnique<PdfIndexCreator>();
        }
    }
}
