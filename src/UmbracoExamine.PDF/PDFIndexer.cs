using Examine;
using Lucene.Net.Analysis;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core;
using UmbracoExamine.DataServices;

namespace UmbracoExamine.PDF
{
    /// <summary>
    /// An Umbraco Lucene.Net indexer which will index the text content of a file
    /// </summary>
    public class PDFIndexer : BaseUmbracoIndexer
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PDFIndexer()
        {
            SupportedExtensions = new[] { ".pdf" };
            UmbracoFileProperty = "umbracoFile";
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>

        public PDFIndexer(DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer, bool async)
            : base(
                new IndexCriteria(Enumerable.Empty<IIndexField>(), Enumerable.Empty<IIndexField>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), null),
                indexPath, dataService, analyzer, async)
        {
            SupportedExtensions = new[] { ".pdf" };
            UmbracoFileProperty = "umbracoFile";
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="luceneDirectory"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>

        public PDFIndexer(Lucene.Net.Store.Directory luceneDirectory, IDataService dataService, Analyzer analyzer, bool async)
            : base(
                new IndexCriteria(Enumerable.Empty<IIndexField>(), Enumerable.Empty<IIndexField>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), null),
                luceneDirectory, dataService, analyzer, async)
        {
            SupportedExtensions = new[] { ".pdf" };
            UmbracoFileProperty = "umbracoFile";
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the supported extensions for files, currently the system will only
        /// process PDF files.
        /// </summary>
        /// <value>The supported extensions.</value>
        public IEnumerable<string> SupportedExtensions { get; set; }

        /// <summary>
        /// Gets or sets the umbraco property alias (defaults to umbracoFile)
        /// </summary>
        /// <value>The umbraco file property.</value>
        public string UmbracoFileProperty { get; set; }

        /// <summary>
        /// Gets the name of the Lucene.Net field which the content is inserted into
        /// </summary>
        /// <value>The name of the text content field.</value>
        public const string TextContentFieldName = "FileTextContent";

        protected override IEnumerable<string> SupportedTypes
        {
            get
            {
                return new string[] { IndexTypes.Media };
            }
        }

        #endregion Properties

        /// <summary>
        /// Set up all properties for the indexer based on configuration information specified. This will ensure that
        /// all of the folders required by the indexer are created and exist.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            if (!string.IsNullOrEmpty(config["extensions"]))
                SupportedExtensions = config["extensions"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            //checks if a custom field alias is specified
            if (!string.IsNullOrEmpty(config["umbracoFileProperty"]))
                UmbracoFileProperty = config["umbracoFileProperty"];
        }

        /// <summary>
        /// Provides the means to extract the text to be indexed from the file specified
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected virtual string ExtractTextFromFile(FileInfo file)
        {
            var pdf = new PDFParser();

            Action<Exception> onError = (e) => OnIndexingError(new IndexingErrorEventArgs("Could not read PDF", -1, e));

            var txt = pdf.GetTextFromAllPages(file.FullName, onError);
            return txt;
        }

        /// <summary>
        /// Collects all of the data that needs to be indexed as defined in the index set.
        /// </summary>
        /// <param name="node">Media item XML being indexed</param>
        /// <param name="type">Type of index (should only ever be media)</param>
        /// <returns>Fields containing the data for the index</returns>
        protected override Dictionary<string, string> GetDataToIndex(XElement node, string type)
        {
            var fields = base.GetDataToIndex(node, type);

            //find the field which contains the file
            XElement fileElement = node.Elements().FirstOrDefault(x =>
            {
                if (x.Attribute("alias") != null)
                    return (string)x.Attribute("alias") == this.UmbracoFileProperty;
                else
                    return x.Name == this.UmbracoFileProperty;
            });

            //make sure the file exists
            if (fileElement != default(XElement) && !string.IsNullOrEmpty(fileElement.Value))
            {
                // Parse the current value
                string filePath = fileElement.Value;
                if ((filePath).StartsWith("{"))
                {
                    filePath = JObject.Parse(filePath).Value<string>("src");
                }

                if (!filePath.IsNullOrWhiteSpace())
                {
                    // Get the file path from the data service
                    string fullPath = this.DataService.MapPath(filePath);
                    var fi = new FileInfo(fullPath);
                    if (fi.Exists)
                    {
                        try
                        {
                            if (!SupportedExtensions.Select(x => x.ToUpper()).Contains(fi.Extension.ToUpper()))
                            {
                                DataService.LogService.AddInfoLog((int)node.Attribute("id"), "UmbracoExamine.FileIndexer: Extension '" + fi.Extension + "' is not supported at this time");
                            }
                            else
                            {
                                fields.Add(TextContentFieldName, ExtractTextFromFile(fi));
                            }
                        }
                        catch (Exception ex)
                        {
                            DataService.LogService.AddErrorLog((int)node.Attribute("id"), "An error occurred: " + ex);
                        }
                    }
                    else
                    {
                        DataService.LogService.AddInfoLog((int)node.Attribute("id"), "UmbracoExamine.FileIndexer: No file found at path " + filePath);
                    }
                }
            }

            return fields;
        }
    }
}