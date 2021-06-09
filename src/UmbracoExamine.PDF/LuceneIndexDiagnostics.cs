using System.Collections.Generic;
using Examine.Lucene.Providers;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace UmbracoExamine.PDF
{
    //TODO: Delete this for 8.1.2 since it exists there
    internal class LuceneIndexDiagnostics : IIndexDiagnostics
    {
        private readonly ILogger<LuceneIndexDiagnostics> _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        public LuceneIndexDiagnostics(LuceneIndex index, ILogger<LuceneIndexDiagnostics> logger, IHostingEnvironment hostingEnvironment)
        {
            Index = index;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        public LuceneIndex Index { get; }

        // public int DocumentCount
        // {
        //     get
        //     {
        //         try
        //         {
        //             return Index.GetIndexDocumentCount();
        //         }
        //         catch (AlreadyClosedException)
        //         {
        //             _logger.Warn(typeof(UmbracoContentIndex), "Cannot get GetIndexDocumentCount, the writer is already closed");
        //             return 0;
        //         }
        //     }
        // }
        //
        // public int FieldCount
        // {
        //     get
        //     {
        //         try
        //         {
        //             return Index.GetIndexFieldCount();
        //         }
        //         catch (AlreadyClosedException)
        //         {
        //             _logger.Warn(typeof(UmbracoContentIndex), "Cannot get GetIndexFieldCount, the writer is already closed");
        //             return 0;
        //         }
        //     }
        // }

        public Attempt<string> IsHealthy()
        {
            var isHealthy = Index.IsHealthy(out var indexError);
            return isHealthy ? Attempt<string>.Succeed() : Attempt.Fail(indexError.Message);
        }

        public virtual IReadOnlyDictionary<string, object> Metadata
        {
            get
            {
                var luceneDir = Index.GetLuceneDirectory();
                var d = new Dictionary<string, object>
                {
                    [nameof(UmbracoExamineIndex.CommitCount)] = Index.CommitCount,
                    [nameof(UmbracoExamineIndex.DefaultAnalyzer)] = Index.DefaultAnalyzer.GetType().Name,
                    ["LuceneDirectory"] = luceneDir.GetType().Name
                };

                if (luceneDir is FSDirectory fsDir)
                {
                    var rootDir = _hostingEnvironment.ApplicationPhysicalPath;
                    d[nameof(UmbracoExamineIndex.LuceneIndexFolder)] = fsDir.Directory.ToString().ToLowerInvariant().TrimStart(rootDir.ToLowerInvariant()).Replace("\\", "/").EnsureStartsWith('/');
                }

                return d;
            }
        }


        // TODO (V9): Implement these
        // Check if it is even necessary, it looks very much like this already exists in the CMS...
        public long GetDocumentCount()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<string> GetFieldNames()
        {
            throw new System.NotImplementedException();
        }
    }
}
