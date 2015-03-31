//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Examine;
//using Examine.LuceneEngine.Config;
//using Examine.LuceneEngine.Providers;
//using Lucene.Net.Analysis;
//using Lucene.Net.Analysis.Standard;
//using Moq;
//using Umbraco.Core.Models;
//using Umbraco.Core.Models.Membership;
//using Umbraco.Core.Persistence.DatabaseModelDefinitions;
//using Umbraco.Core.Services;
//using UmbracoExamine.Config;
//using UmbracoExamine.DataServices;

//namespace UmbracoExamine.PDF.Tests
//{
//    /// <summary>
//    /// Used internally by test classes to initialize a new index from the template
//    /// </summary>
//    internal static class IndexInitializer
//    {
        
//        public static LuceneSearcher GetLuceneSearcher(Lucene.Net.Store.Directory luceneDir)
//        {
//            return new LuceneSearcher(luceneDir, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
//        }

//        public static PDFIndexer GetPdfIndexer(Lucene.Net.Store.Directory luceneDir)
//        {
//            var i = new PDFIndexer(luceneDir,
//                                      new TestDataService(),
//                                      new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29),
//                                      false);

//            i.IndexingError += IndexingError;

//            return i;
//        }
        
//        internal static void IndexingError(object sender, IndexingErrorEventArgs e)
//        {
//            throw new ApplicationException(e.Message, e.InnerException);
//        }


//    }
//}