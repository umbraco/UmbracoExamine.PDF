using System.Collections.Generic;
using Examine;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Infrastructure.Examine;

namespace UmbracoExamine.PDF
{

    /// <summary>
    ///     Create the Lucene index for PDF documents
    /// </summary>
    public class PdfIndexCreator : LuceneIndexCreator
    {
        // TODO (V9): Find the alternative to LuceneIndexCreator
        public PdfIndexCreator(IProfilingLogger logger)
        {
            _logger = logger;
        }

        private readonly IProfilingLogger _logger;

        public override IEnumerable<IIndex> Create()
        {
            var index = new PdfLuceneIndex(PdfIndexConstants.PdfIndexName,
                CreateFileSystemLuceneDirectory(PdfIndexConstants.PdfIndexName),
                new FieldDefinitionCollection(
                    new FieldDefinition(PdfIndexConstants.PdfContentFieldName, FieldDefinitionTypes.FullText)
                ),
                new StandardAnalyzer(Version.LUCENE_30),
                new PdfValueSetValidator(null),
                _logger);

            return new[] { index };
        }
    }
}
