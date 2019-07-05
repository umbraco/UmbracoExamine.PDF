using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;
using PdfTextract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace UmbracoExamine.PDF
{
    /// <summary>
    /// Extracts the text from a PDF document
    /// </summary>
    public class PDFTextService
    {
        public string ExtractText(string filePath)
        {
            return ExceptChars(PdfTextExtractor.GetText(filePath), UnsupportedRange.Value, ReplaceWithSpace);
        }

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
            foreach (var c in ReplaceWithSpace)
            {
                unsupportedRange.Remove(c);
            }
            return unsupportedRange;
        });

        private static readonly HashSet<char> ReplaceWithSpace = new HashSet<char> { '\r', '\n' };

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