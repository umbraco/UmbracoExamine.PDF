using System.Collections.Generic;
using System.Linq;
using Examine.Lucene;
using Examine.Lucene.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Infrastructure.Examine;

namespace UmbracoExamine.PDF
{
    public class PdfLuceneIndex : LuceneIndex, IIndexDiagnostics
    {
        public PdfLuceneIndex(string name,
            ILoggerFactory loggerFactory,
            IOptionsSnapshot<LuceneDirectoryIndexOptions> indexOptions,
            IHostingEnvironment hostingEnvironment)
            : base(loggerFactory, name, indexOptions)
        {
            _diagnostics = new PdfIndexDiagnostics(this, loggerFactory, hostingEnvironment, indexOptions);
        }

        #region IIndexDiagnostics

        private readonly IIndexDiagnostics _diagnostics;

        public long DocumentCount => _diagnostics.GetDocumentCount();

        public int FieldCount => _diagnostics.GetFieldNames().Count();

        public Attempt<string> IsHealthy() => _diagnostics.IsHealthy();
        public virtual IReadOnlyDictionary<string, object> Metadata => _diagnostics.Metadata;

        #endregion
    }
}
