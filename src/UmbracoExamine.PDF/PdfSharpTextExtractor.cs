using System;
using System.Text;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;

namespace UmbracoExamine.PDF
{
    /// <summary>
    /// Extracts text from a PDF
    /// </summary>
    /// <remarks>
    /// In many cases there will be a lot of 'junk' returned as text from a PDF, this is because it seems that this extractor
    /// (which i think is based on this https://github.com/DavidS/PdfTextract/blob/master/PdfTextract/PdfTextExtractor.cs)
    /// or PDFSharp itself doesn't handle unicode. There's an SO article a bit about that here https://stackoverflow.com/questions/10141143/c-sharp-extract-text-from-pdf-using-pdfsharp
    /// There's comments in there like:
    /// "You need to extract the ToUnicode CMaps from the document to convert the binary indexes of the text-strings, unless you're lucky and the binary indexes are ASCII values themselves"
    /// "No, PdfSharp does not provide all the tools for text extraction. Functionality has yet to be added for ToUnicode CMaps, which are necessary to extract the text of Unicode PDFs."
    /// </remarks>
    public class PdfSharpTextExtractor : IPdfTextExtractor
    {
        public string GetTextFromPdf(string pdfFileName)
        {
            using (var document = PdfReader.Open(pdfFileName, PdfDocumentOpenMode.ReadOnly))
            {
                var result = new StringBuilder();
                foreach (var page in document.Pages)
                {
                    ExtractText(ContentReader.ReadContent(page), result);
                    result.AppendLine();
                }
                return result.ToString();
            }
            
        }

        private static void ExtractText(CObject obj, StringBuilder target)
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

        private static void ExtractTextFromEnumable(CSequence sequence, StringBuilder target)
        {
            foreach (var obj in sequence)
            {
                ExtractText(obj, target);
            }
        }

        private static void ExtractTextFromOperator(COperator obj, StringBuilder target)
        {
            if (obj.OpCode.OpCodeName == OpCodeName.Tj || obj.OpCode.OpCodeName == OpCodeName.TJ)
            {
                foreach (var element in obj.Operands)
                {
                    ExtractText(element, target);
                }

                target.Append(" ");
            }
        }


        private static void ExtractTextFromString(CString obj, StringBuilder target)
        {
            target.Append(obj.Value);
        }

    }
}
