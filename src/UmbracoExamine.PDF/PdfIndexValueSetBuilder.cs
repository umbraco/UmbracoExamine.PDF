using Examine;
using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Examine;
using System;
using Microsoft.Extensions.Logging;

namespace UmbracoExamine.PDF
{
    public interface IPdfIndexValueSetBuilder : IValueSetBuilder<IMedia> { }

    /// <summary>
    /// Builds a ValueSet for PDF Documents
    /// </summary>
    public class PdfIndexValueSetBuilder : IPdfIndexValueSetBuilder
    {
        private PdfTextService _pdfTextService;
        private readonly ILogger<PdfIndexValueSetBuilder> _logger;

        public PdfIndexValueSetBuilder(PdfTextService pdfTextService, ILogger<PdfIndexValueSetBuilder> logger)
        {
            _pdfTextService = pdfTextService;
            _logger = logger;
        }
        public IEnumerable<ValueSet> GetValueSets(params IMedia[] content)
        {
            foreach (var item in content)
            {
                var umbracoFile = item.GetValue<string>(Constants.Conventions.Media.File);
                if (string.IsNullOrWhiteSpace(umbracoFile)) continue;

                string fileTextContent;
                string fileLinks;
                try
                {
                    fileTextContent = ExtractTextFromFile(umbracoFile);
                    fileLinks = ExtractLinkFromFile(umbracoFile);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not read the file {MediaFile}", umbracoFile);
                    continue;
                }
                var indexValues = new Dictionary<string, object>
                {
                    ["nodeName"] = item.Name,
                    ["id"] = item.Id,
                    ["path"] =  item.Path,
                    [PdfIndexConstants.PdfContentFieldName] = fileTextContent,
                    [PdfIndexConstants.PdfLinksFieldName] = fileLinks
                };

                var valueSet = new ValueSet(item.Id.ToString(), PdfIndexConstants.PdfCategory, item.ContentType.Alias, indexValues);

                yield return valueSet;
            }
        }

        private string ExtractTextFromFile(string filePath)
        {
            try
            {
                return _pdfTextService.ExtractText(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not extract text from PDF {PdfFilePath}", filePath);
                return string.Empty;
            }
        }

        private string ExtractLinkFromFile(string filePath)
        {
            try
            {
                return _pdfTextService.ExtractLink(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not extract text from PDF {PdfFilePath}", filePath);
                return string.Empty;
            }
        }
    }
}
