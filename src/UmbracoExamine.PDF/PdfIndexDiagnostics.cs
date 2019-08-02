using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Examine;

namespace UmbracoExamine.PDF
{
    internal class PdfIndexDiagnostics : LuceneIndexDiagnostics
    {
        private readonly PdfLuceneIndex _index;

        public PdfIndexDiagnostics(PdfLuceneIndex index, ILogger logger) : base(index, logger)
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
