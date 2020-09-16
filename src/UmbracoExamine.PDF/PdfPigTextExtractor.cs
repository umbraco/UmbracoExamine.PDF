using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace UmbracoExamine.PDF
{
    public class PdfPigTextExtractor : IPdfTextExtractor
    {
        public string GetTextFromPdf(Stream pdfFileStream)
        {
            using (PdfDocument document = PdfDocument.Open(pdfFileStream))
            {
                var result = new StringBuilder();

                foreach (Page page in document.GetPages())
                {
                    // page.Text in some test cases runs words together where page.GetWords keeps them seperated
                    // so we use page.GetWords() instead of the simpler page.Text
                    IEnumerable<Word> words = page.GetWords();
                    result.Append(string.Join(" ", words));
                    result.AppendLine();
                }
                return result.ToString();
            }
        }
    }
}
