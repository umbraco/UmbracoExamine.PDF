using Examine;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Examine;

namespace UmbracoExamine.PDF
{
    public interface IPdfIndexValueSetBuilder : IValueSetBuilder<IMedia> { }

    /// <summary>
    /// Builds a ValueSet for PDF Documents
    /// </summary>
    public class PdfIndexValueSetBuilder : IPdfIndexValueSetBuilder
    {
        private PdfTextService _pdfTextService;

        public PdfIndexValueSetBuilder(PdfTextService pdfTextService)
        {
            _pdfTextService = pdfTextService;
        }
        public IEnumerable<ValueSet> GetValueSets(params IMedia[] content)
        {
            foreach (var item in content)
            {
                var umbracoFile = item.GetValue<string>(Constants.Conventions.Media.File);
                if (string.IsNullOrWhiteSpace(umbracoFile)) continue;
                var filepath = IOHelper.MapPath(umbracoFile);
                var fileTextContent = ExtractTextFromFile(filepath);
                var indexValues = new Dictionary<string, object>
                {
                    ["fileTextContent"] = fileTextContent,
                    ["__IndexType"] = IndexTypes.Media,
                    ["__NodeId"] = item.Id
                };

                var valueSet = new ValueSet(item.Id.ToString(), IndexTypes.Media, indexValues);

                yield return valueSet;
            }
        }

        private string ExtractTextFromFile(string filePath)
        {
            return _pdfTextService.ExtractText(filePath);
        }
    }
}