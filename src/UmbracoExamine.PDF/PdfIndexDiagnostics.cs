using System.Collections.Generic;
using System.Linq;
using Examine.Lucene;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Infrastructure.Examine;

namespace UmbracoExamine.PDF
{
    internal class PdfIndexDiagnostics : LuceneIndexDiagnostics
    {
        private readonly PdfLuceneIndex _index;

        public PdfIndexDiagnostics(
            PdfLuceneIndex index,
            ILoggerFactory loggerFactory,
            IHostingEnvironment hostingEnvironment,
            IOptionsSnapshot<LuceneDirectoryIndexOptions> indexOptions)
            : base(index, loggerFactory.CreateLogger<LuceneIndexDiagnostics>(), hostingEnvironment, indexOptions)
        {
            _index = index;
        }

        public override IReadOnlyDictionary<string, object> Metadata
        {
            get
            {
                var d = base.Metadata.ToDictionary(x => x.Key, x => x.Value);

                if (_index.ValueSetValidator is ValueSetValidator vsv)
                {
                    d[nameof(ValueSetValidator.IncludeItemTypes)] = vsv.IncludeItemTypes;
                    d[nameof(ContentValueSetValidator.ExcludeItemTypes)] = vsv.ExcludeItemTypes;
                }

                if (_index.ValueSetValidator is PdfValueSetValidator cvsv)
                {
                    d[nameof(ContentValueSetValidator.ParentId)] = cvsv.ParentId;
                }

                return d.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
            }
        }
    }
}
