using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using System.Linq;
using Umbraco.Core.Logging;

namespace UmbracoExamine.PDF.PdfSharp
{
    public class FontResource
    {
        private readonly IAdobeGlyphList _adobeGlyphList;
        private readonly ILogger _logger;
        ///
        /// Parse the fonts from the page's resources structure including the encoding differences and CMAPs
        ///
        public FontResource(string fontName, PdfReference resource, IAdobeGlyphList adobeGlyphList, ILogger logger)
        {
            // store some injected services
            _adobeGlyphList = adobeGlyphList;
            _logger = logger;

            var resourceElements = (resource?.Value as PdfDictionary)?.Elements;
            //Extract the encoding differences array
            var differences = resourceElements?.GetDictionary("/Encoding")?.Elements?.GetArray("/Differences");
            if (differences != null)
            {
                _differences = differences;
            }

            //Extract the CMAPs 
            var unicodeDictionary = resourceElements?.GetDictionary("/ToUnicode");
            var stream = unicodeDictionary?.Stream;
            if (stream != null)
            {
                _cmap = new CMap(stream, fontName, _logger);
            }
        }


        private readonly CMap _cmap;
        private PdfArray _differences;

        /// <summary>
        /// Encodes the given pdf text string into unicode
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Encode(string text)
        {
            // if a pdf has a differences array we use it primarily to convert to unicode chars
            // the differences array can contain a map of indexes to characters, or to glyph names
            // if it is a glyph name we lookup the unicode value of the glyph, otherwise we convert
            // it to a character.
            if (_differences != null && _differences.Elements.Count > 0)
            {
                // generate an enumerable of converted characters
                var chars = text.ToCharArray().Select(ch =>
                {
                    // if the character falls within the bounds of the differences array we convert it
                    // otherwise we return it
                    if (_differences.Elements.Count > ch)
                    {
                        var item = _differences.Elements[ch];
                        // if we get a PdfName rather than a number look it up in the glyphMap
                        if (item is PdfName name)
                        {
                            return _adobeGlyphList.Lookup(name.Value);
                        }
                        // if we got a number, convert it to a character
                        if (item is PdfInteger number)
                        {
                            return ((char)number.Value).ToString();
                        }
                    }
                    return ch.ToString();
                });
                return string.Concat(chars);
            }

            // if this font has a /ToUnciode CMAP then we will try to resolve the text using the cmap
            if (_cmap != null)
            {
                return _cmap.Encode(text);
            }

            // fallback on just displaying the text
            return text;
        }
    }
}
