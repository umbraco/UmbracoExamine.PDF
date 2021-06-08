using Examine;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace UmbracoExamine.PDF
{
    /// <summary>
    /// Registers the ExaminePDF index, and dependencies.
    /// </summary>
    public class ExaminePdfComposer : IUserComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            //Register the services used to make this all work
            builder.Services.AddUnique<IPdfTextExtractor, PdfPigTextExtractor>();
            builder.Services.AddSingleton<PdfTextService>();
            builder.Services.AddUnique<IPdfIndexValueSetBuilder, PdfIndexValueSetBuilder>();
            builder.Services.AddSingleton<IIndexPopulator, PdfIndexPopulator>();
            builder.Services.AddSingleton<PdfIndexPopulator>();

            builder.Services
                .AddExamineLuceneIndex<PdfLuceneIndex, ConfigurationEnabledDirectoryFactory>(PdfIndexConstants.PdfIndexName)
                .ConfigureOptions<ConfigurePdfIndexOptions>();

            builder.AddNotificationHandler<MediaCacheRefresherNotification, PdfCacheNotificationHandler>();
        }
    }
}
