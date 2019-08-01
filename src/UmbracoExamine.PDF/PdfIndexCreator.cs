using System.Collections.Generic;
using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using Umbraco.Examine;

namespace UmbracoExamine.PDF
{
    /// <summary>
    ///     Create the Lucene index for PDF documents
    /// </summary>
    public class PdfIndexCreator : LuceneIndexCreator
    {
        public const string PdfIndexName = "PDFIndex";

        public override IEnumerable<IIndex> Create()
        {
            var index = new LuceneIndex(PdfIndexName,
                CreateFileSystemLuceneDirectory(PdfIndexName),
                new FieldDefinitionCollection(
                    new FieldDefinition("fileTextContent", FieldDefinitionTypes.FullTextSortable)
                ),
                new StandardAnalyzer(Version.LUCENE_30));

            return new[] { index };
        }
    }
}