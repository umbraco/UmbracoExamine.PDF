using System.Collections.Generic;
using Examine;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using Umbraco.Core.Logging;
using Umbraco.Examine;

namespace UmbracoExamine.PDF
{

    /// <summary>
    ///     Create the Lucene index for PDF documents
    /// </summary>
    public class PdfIndexCreator : LuceneIndexCreator
    {
        public PdfIndexCreator(IProfilingLogger logger)
        {
            _logger = logger;
        }

        public const string PdfIndexName = "PDFIndex";
        private readonly IProfilingLogger _logger;

        public override IEnumerable<IIndex> Create()
        {
            var index = new PdfLuceneIndex(PdfIndexName,
                CreateFileSystemLuceneDirectory(PdfIndexName),
                new FieldDefinitionCollection(
                    new FieldDefinition("fileTextContent", FieldDefinitionTypes.FullTextSortable)
                ),
                new StandardAnalyzer(Version.LUCENE_30),
                new PdfValueSetValidator(null),
                _logger);

            return new[] { index };
        }
    }
}
