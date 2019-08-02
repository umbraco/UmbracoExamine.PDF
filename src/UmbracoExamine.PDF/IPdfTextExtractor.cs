namespace UmbracoExamine.PDF
{
    public interface IPdfTextExtractor
    {
        string GetTextFromPdf(string pdfFileName);
    }
}