using System.IO;

namespace UmbracoExamine.PDF.PdfSharp
{
    /// <summary>
    /// Provide a TextReader for the Adobe Glyph List file.
    /// </summary>
    public interface IAdobeGlphListDataProvider
    {
        TextReader GetStream();
    }
}
