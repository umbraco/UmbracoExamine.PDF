using System.Collections.Generic;
using System.IO;
using System.Text;
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

        public string GetLinkFromPdf(Stream pdfFileStream)
        {
            using (PdfDocument document = PdfDocument.Open(pdfFileStream))
            {
                var result = new StringBuilder();

                foreach (Page page in document.GetPages())
                {
                    // page.Text in some test cases runs words together where page.GetWords keeps them seperated
                    // so we use page.GetWords() instead of the simpler page.Text
                    IEnumerable<Hyperlink> links = page.GetHyperlinks();
                    result.Append(string.Join(" ", links));
                    result.AppendLine();
                }
                return result.ToString();
            }
        }
    }
}
