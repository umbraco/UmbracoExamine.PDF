using System.Linq;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace UmbracoExamine.PDF
{
    /// <summary>
    /// Registers the ExaminePDF index, and dependencies.
    /// </summary>
    public class ExaminePdfComposer : IUserComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            if (builder.Services.Any(x => x.ServiceType == typeof(IPdfTextExtractor)))
            {
                // Assume that Examine.Pdf is already composed if any implementation of IPdfTextExtractor is registered.
                return;
            }
            builder.AddExaminePdf();
        }
    }
}
