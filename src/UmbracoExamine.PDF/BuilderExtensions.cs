using System.Linq;
using Examine;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace UmbracoExamine.PDF
{
    public static class BuilderExtensions
    {
        public static IUmbracoBuilder AddExaminePdf(this IUmbracoBuilder builder)
        {
            if (builder.Services.Any(x => x.ServiceType == typeof(IPdfTextExtractor)))
            {
                // Assume that Examine.Pdf is already composed if any implementation of IPdfTextExtractor is registered.
                return builder;
            }

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

            return builder;
        }
    }
}
