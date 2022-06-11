using System.Linq;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace UmbracoExamine.PDF
{
    /// <summary>
    /// Registers the ExaminePDF index, and dependencies.
    /// </summary>
    public class ExaminePdfComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder) => builder.AddExaminePdf();
    }
}
