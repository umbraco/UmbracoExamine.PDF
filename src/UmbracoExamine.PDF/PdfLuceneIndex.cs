using System.Collections.Generic;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Store;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Examine;
using Umbraco.Web.Search;

namespace UmbracoExamine.PDF
{

    public class PdfLuceneIndex : LuceneIndex, IIndexDiagnostics, IUmbracoPdfIndex
    {
        public PdfLuceneIndex(string name, Directory luceneDirectory, FieldDefinitionCollection fieldDefinitions,
            Analyzer analyzer, PdfValueSetValidator validator,
            IProfilingLogger logger,
            IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypesFactory = null)
            : base(name, luceneDirectory, fieldDefinitions, analyzer, validator, indexValueTypesFactory)
        {
            _diagnostics = new PdfIndexDiagnostics(this, logger);
        }

        #region IIndexDiagnostics

        private readonly IIndexDiagnostics _diagnostics;

        public int DocumentCount => _diagnostics.DocumentCount;
        public int FieldCount => _diagnostics.FieldCount;
        public Attempt<string> IsHealthy() => _diagnostics.IsHealthy();
        public virtual IReadOnlyDictionary<string, object> Metadata => _diagnostics.Metadata;

        #endregion
    }
}
