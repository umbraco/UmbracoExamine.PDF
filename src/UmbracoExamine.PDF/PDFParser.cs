using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace UmbracoExamine.PDF
{
    using Umbraco.Core.IO;

    /// <summary>
    /// Parses a PDF file and extracts the text from it.
    /// </summary>
    internal class PDFParser
    {
        /// <summary>
        /// Stores the unsupported range of character
        /// </summary>
        /// <remarks>
        /// used as a reference:
        /// http://www.tamasoft.co.jp/en/general-info/unicode.html
        /// http://en.wikipedia.org/wiki/Summary_of_Unicode_character_assignments
        /// http://en.wikipedia.org/wiki/Unicode
        /// http://en.wikipedia.org/wiki/Basic_Multilingual_Plane
        /// </remarks>
        private static readonly Lazy<HashSet<char>> UnsupportedRange = new Lazy<HashSet<char>>(() =>
        {
            var unsupportedRange = new HashSet<char>();
            // Create collection with chars to skip
            foreach (var c in Enumerable.Range(0x0000, 0x001F))
            {
                unsupportedRange.Add((char)c);
            }
            unsupportedRange.Add((char)0x1F);
            // Remove replace chars from collection
            foreach( var c in ReplaceWithSpace )
            {
                unsupportedRange.Remove(c);
            }
            return unsupportedRange;
        });

        private static readonly HashSet<char> ReplaceWithSpace = new HashSet<char> {'\r', '\n'};


        public string GetTextFromAllPages(string pdfPath, MediaFileSystem mediaFileSystem, Action<Exception> onError)
        {
            var output = new StringWriter();

            try
            {
                using (var stream = mediaFileSystem.OpenFile(pdfPath))
                using (var reader = new PdfReader(stream))
                {
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        var result =
                            ExceptChars(
                                PdfTextExtractor.GetTextFromPage(reader, i, new SimpleTextExtractionStrategy()),
                                UnsupportedRange.Value,
                                ReplaceWithSpace);
                        output.Write(result + " ");
                    }
                }

            }
            catch (Exception ex)
            {
                onError(ex);
            }

            return output.ToString();
        }


        /// <summary>
        /// Remove all toExclude chars from string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="toExclude"></param>
        /// <param name="replaceWithSpace"></param>
        /// <returns></returns>
        private static string ExceptChars(string str, HashSet<char> toExclude, HashSet<char> replaceWithSpace)
        {
            var sb = new StringBuilder(str.Length);
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (toExclude.Contains(c) == false)
                {
                    if (replaceWithSpace.Contains(c))
                    {
                        sb.Append(" ");
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }

            }
            return sb.ToString();
        }

    }


}
