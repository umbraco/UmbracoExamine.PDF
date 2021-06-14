using Examine;
using Examine.Lucene;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Util;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace UmbracoExamine.PDF
{
    // See ConfigureIndexOptions in the CMS to see how this is used.
    /// <summary>
    /// Configures the index options to construct the Examine PDF Index
    /// </summary>
    public class ConfigurePdfIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
    {
        private readonly IOptions<IndexCreatorSettings> _settings;

        public ConfigurePdfIndexOptions(IOptions<IndexCreatorSettings> settings)
        {
            _settings = settings;
        }

        public void Configure(string name, LuceneDirectoryIndexOptions options)
        {
            if (name.Equals(PdfIndexConstants.PdfIndexName))
            {
                options.Analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);
                options.Validator = new PdfValueSetValidator(null);
                options.FieldDefinitions = new FieldDefinitionCollection(
                    new FieldDefinition(PdfIndexConstants.PdfContentFieldName,FieldDefinitionTypes.FullText));

                // Ensures indexes are unlocked on startup
                options.UnlockIndex = true;

                if (_settings.Value.LuceneDirectoryFactory == LuceneDirectoryFactory.SyncedTempFileSystemDirectoryFactory)
                {
                    // if this directory factory is enabled then a snapshot deletion policy is required
                    options.IndexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
                }
            }
        }

        public void Configure(LuceneDirectoryIndexOptions options)
        {
            throw new System.NotImplementedException("This is never called and is just part of the interface");
        }
    }
}
