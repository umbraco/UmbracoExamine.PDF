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
using Umbraco.Core.IO;
using UmbracoExamine.PDF.PdfSharp;

namespace UmbracoExamine.PDF
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
        private Dictionary<string, FontResource> FontLookup;
        private string CurrentFont;

        public PdfSharpTextExtractor()
        {
            FontLookup = new Dictionary<string, FontResource>();
        }

        public string GetTextFromPdf(Stream pdfFileStream)
        {
            using (var document = PdfReader.Open(pdfFileStream, PdfDocumentOpenMode.ReadOnly))
            {
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
            var fontResource = page.Resources.Elements.GetDictionary("/Font")?.Elements;
            if (fontResource == null) return;
            //All that above isn't going to do, but it's close...
            foreach (var fontName in fontResource.Keys)
            {
                var resource = fontResource[fontName] as PdfReference;
                var font = new FontResource(fontName, resource);

                FontLookup[fontName] = font;
            }
        }

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

        private void ExtractTextFromEnumable(CSequence sequence, StringBuilder target)
        {
            foreach (var obj in sequence)
            {
                ExtractText(obj, target);
            }
        }

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
