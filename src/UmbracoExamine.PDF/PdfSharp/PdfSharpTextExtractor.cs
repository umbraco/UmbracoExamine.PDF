using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;
using Umbraco.Core.Logging;

namespace UmbracoExamine.PDF.PdfSharp
{
    /// <summary>
    /// Extracts text from a PDF
    /// </summary>
    /// <remarks>
    /// This extractor is loosely based on this https://github.com/DavidS/PdfTextract/blob/master/PdfTextract/PdfTextExtractor.cs
    /// but uses the differences array in the font encodings and ToUnicode CMaps to do a better job. I'm sure iText still does a much
    /// better job of text extraction, this is still missing a number of features such as support for standard CMaps and 
    /// </remarks>
    public class PdfSharpTextExtractor : IPdfTextExtractor 
    {
        private readonly IAdobeGlyphList _adobeGlyphList;
        private readonly ILogger _logger;
        private Dictionary<string, FontResource> FontLookup;
        private string CurrentFont;

        public PdfSharpTextExtractor(IAdobeGlyphList adobeGlyphList, ILogger logger)
        {
            _adobeGlyphList = adobeGlyphList;
            _logger = logger;
            FontLookup = new Dictionary<string, FontResource>();
        }

        /// <summary>
        /// Extract all text from a PDF document
        /// </summary>
        /// <param name="pdfFileStream"></param>
        /// <returns></returns>
        public string GetTextFromPdf(Stream pdfFileStream)
        {
            using (var document = PdfReader.Open(pdfFileStream, PdfDocumentOpenMode.ReadOnly))
            {
                // we collect the results of text extraction in a string builder. 
                var result = new StringBuilder();
                foreach (var page in document.Pages)
                {
                    // generate the lookup tables we will need for each page
                    ParseFonts(page);

                    // extract the text
                    ExtractText(ContentReader.ReadContent(page), result);

                    // delineate each page with a newline
                    result.AppendLine();

                }
                return result.ToString();
            }
        }

        ///
        /// Build a dictionary of font names and their associated information used to encode the data
        /// as unicode strings.
        ///
        private void ParseFonts(PdfPage page)
        {
            // try to extract the font resource
            var fontResource = page?.Resources?.Elements?.GetDictionary("/Font")?.Elements;
            if (fontResource == null || fontResource.Keys == null) return;

            // process each font in the font resource
            foreach (var fontName in fontResource.Keys)
            {
                // create a font object and add it to our dictionary
                var resource = fontResource[fontName] as PdfReference;

                // make sure we aren't processing nothing
                if (resource == null) continue;

                // make a font resource object
                var font = new FontResource(fontName, resource, _adobeGlyphList, _logger);

                // add it to our dictionary
                FontLookup[fontName] = font;
            }
        }

        /// <summary>
        /// Recursive method to crawl a PDF object (page or some object of a page) and extract text from that object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="target"></param>
        private void ExtractText(CObject obj, StringBuilder target)
        {
            switch (obj)
            {
                case COperator cOperator:
                    ExtractTextFromOperator(cOperator, target);
                    return;
                case CSequence sequence: //CArray, CSequence
                    ExtractTextFromEnumable(sequence, target);
                    return;
                case CString cString:
                    ExtractTextFromString(cString, target);
                    return;
                case CInteger _:
                case CComment _:
                case CName _:
                case CNumber _:
                    //Do nothing
                    return;
                default:
                    throw new NotImplementedException(obj.GetType().AssemblyQualifiedName);
            }
        }

        /// <summary>
        /// Recursively extract text from each object in a sequence object
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="target"></param>
        private void ExtractTextFromEnumable(CSequence sequence, StringBuilder target)
        {
            foreach (var obj in sequence)
            {
                ExtractText(obj, target);
            }
        }

        /// <summary>
        /// Process an operator. If this is font selection (tf) set the current font used for text extraction, if it is a text showing operator (tj)
        /// then recursively process the object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="target"></param>
        private void ExtractTextFromOperator(COperator obj, StringBuilder target)
        {
            if (obj.OpCode.OpCodeName == OpCodeName.Tf)
            {
                var fontName = obj.Operands.OfType<CName>().FirstOrDefault();
                if (fontName != null)
                {
                    //This is likely the wrong way to do this
                    CurrentFont = fontName.Name;
                }
            }
            if (obj.OpCode.OpCodeName == OpCodeName.Tj || obj.OpCode.OpCodeName == OpCodeName.TJ)
            {
                foreach (var element in obj.Operands)
                {
                    ExtractText(element, target);
                }
            }
        }

        /// <summary>
        /// Extract text from a cString, performing any necessary font encoding
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="target"></param>
        private void ExtractTextFromString(CString obj, StringBuilder target)
        {
            string text = obj.Value;

            if (!string.IsNullOrEmpty(CurrentFont) && FontLookup.TryGetValue(CurrentFont, out var font))
            {
                //Do character sub with the current fontMap
                text = font.Encode(text);
            }

            target.Append(text);
        }
    }
}
