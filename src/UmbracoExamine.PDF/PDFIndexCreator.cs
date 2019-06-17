using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using System.Collections.Generic;
using Umbraco.Examine;

namespace UmbracoExamine.PDF
{
    public class PDFIndexCreator : LuceneIndexCreator
    {
        public override IEnumerable<IIndex> Create()
        {
            
            var index = new LuceneIndex("PDFIndex",
                CreateFileSystemLuceneDirectory("PDFIndex"),
                new FieldDefinitionCollection(
                    new FieldDefinition("fileTextContent", FieldDefinitionTypes.FullTextSortable)
                ),
                new StandardAnalyzer(Version.LUCENE_30));

            return new[] { index };
        }
    }
}