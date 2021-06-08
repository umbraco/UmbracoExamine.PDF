using System;
using System.IO;
using System.Text;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;

namespace UmbracoExamine.PDF
{
    // TODO (V9): Remove this for V9 Since it's obsolete, and problem not needed, then we might be able to remove the PdfSharp dependency as well?
    [Obsolete("This does not work very well, use the default IPdfTextExtractor implemented by PdfPigTextExtractor instead")]
    public class PdfSharpTextExtractor : IPdfTextExtractor
    {
        public string GetTextFromPdf(Stream pdfFileStream)
        {
            using (var document = PdfReader.Open(pdfFileStream, PdfDocumentOpenMode.ReadOnly))
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
