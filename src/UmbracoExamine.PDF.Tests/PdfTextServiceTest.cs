using System;
using System.IO;
using Moq;
using NUnit.Framework;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using UmbracoExamine.PDF.PdfSharp;

namespace UmbracoExamine.PDF.Tests
{
    [TestFixture]
    public class PdfTextServiceTest
    {
        private PdfTextService _pdfTextService;
        private DirectoryInfo _testFilesDir;

        [SetUp]
        public void Setup()
        {
            var fs = new Mock<IMediaFileSystem>();
            fs.Setup<System.IO.Stream>(m => m.OpenFile(It.IsAny<string>())).Returns<string>(path => System.IO.File.OpenRead(path));
            var logger = new Mock<ILogger>();
            var glyphList = new AdobeGlyphList(new AdobeGlphListDataProvider());
            _pdfTextService = new PdfTextService(new PdfSharpTextExtractor(glyphList, logger.Object), fs.Object, logger.Object);
            _testFilesDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while(!_testFilesDir.Name.Equals("UmbracoExamine.PDF.Tests", StringComparison.InvariantCultureIgnoreCase))
            {
                _testFilesDir = _testFilesDir.Parent;
            }
        }


        [Test]
        [TestCase("Converting_file_to_PDF.pdf", "Converting", "http://r.office.microsoft.com/r/rlidMSAddinPDFXPS", "Microsoft Word 2008")]
        [TestCase("managecookies.pdf", "Cookie", "www.cookiereports.com", "Manage Your Cookies")]
        [TestCase("PDFStandards.PDF", "standardization", "implementation")]
        [TestCase("SurviorFlipCup.pdf", "Drink the beer", "game", "following")]
        [TestCase("Office16Word.pdf", "this", "is", "simple", "word")]
        [TestCase("windows-vista.pdf", "WINDOWS", "VISTA", "ULTIMATE", "supplements", "Anytime Upgrade", "it is invalid")]
        [TestCase("file-example_pdf_1mb.pdf", "Lorem ipsum", "Vestibulum", "vehicula")]
        [TestCase("11.pdf", "How to use the tools")]
        public void ExtractText__must_return_the_text_from_the_file_on_the_path(string pdfFileName,
            params string[] expectedSentences)
        {
            var absolutePath = Path.GetFullPath(Path.Combine(_testFilesDir.FullName, "TestFiles/", pdfFileName));
            var text = _pdfTextService.ExtractText(absolutePath).ToLower();

            foreach (var expectedSentence in expectedSentences)
            {
                StringAssert.Contains(expectedSentence.ToLower(), text);
            }
        }

        /// <summary>
        /// Adobe Reader can't read text out of this, so I don't think it's a crime that we can't either.
        /// </summary>
        /// <param name="pdfFileName"></param>
        /// <param name="expectedSentences"></param>
        [Test]
        [TestCase("12.pdf", "This here be some text")]
        public void ExtractText__it_is_a_known_issue_we_cant_handle_some_files(string pdfFileName,
            params string[] expectedSentences)
        {
            var absolutePath = Path.GetFullPath(Path.Combine(_testFilesDir.FullName, "TestFiles/", pdfFileName));
            var text = _pdfTextService.ExtractText(absolutePath).ToLower();

            foreach (var expectedSentence in expectedSentences)
            {
                StringAssert.DoesNotContain(expectedSentence.ToLower(), text, "If this test fails, it is actually a success. Then we can suddenly handle a pdf file, that was known not to be handled.");
            }
        }
    }
}
