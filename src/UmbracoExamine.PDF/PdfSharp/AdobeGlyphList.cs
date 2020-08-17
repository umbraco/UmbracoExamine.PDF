using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UmbracoExamine.PDF.PdfSharp
{
    /// <summary>
    /// The standard Adobe Glyf List for converting standard Glyf characters to Unicode.
    /// To prevent continual parsing of the file, this uses a singleton pattern.
    /// </summary>
    public class AdobeGlyphList : IAdobeGlyphList
    {
        private Dictionary<string, string> Dictionary { get; set; }
        private readonly IAdobeGlphListDataProvider _glyphListDataProvider;

        /// <summary>
        /// Constructur is private so we don't have numerous copies of this around
        /// </summary>
        public AdobeGlyphList(IAdobeGlphListDataProvider glyphListDataProvider)
        {
            _glyphListDataProvider = glyphListDataProvider;
            Init();
        }

        /// <summary>
        /// Lookup a unicode character by it's glyph
        /// </summary>
        /// <param name="glyph"></param>
        /// <returns></returns>
        public string Lookup(string glyph)
        {
            if (glyph.StartsWith(@"/")) glyph = glyph.Substring(1);
            Dictionary.TryGetValue(glyph, out string unicode);
            return unicode;
        }

        /// <summary>
        /// Read in the Adobe Glyphlist file into the a dictionary which we can use to do lookups
        /// </summary>
        private void Init()
        {
            Dictionary = new Dictionary<string, string>();
            string line;

            // Read the file and display it line by line.
            using (var file = _glyphListDataProvider.GetStream())
            {
                while ((line = file.ReadLine()) != null)
                {
                    // comments start with #
                    if (line.StartsWith("#")) continue;

                    // try to match a glyph-unicode mapping on the line
                    var match = Regex.Match(line, @"(?<glyph>^.*);((?<unicode>[0-9A-F]{4})\s*)+");
                    if (match.Success)
                    {
                        string glyphName = match.Groups["glyph"].Value;

                        // convert the unicode hex string to a unicode character
                        var chars = match.Groups["unicode"].Captures.Cast<Capture>().Select(c => (char)Convert.ToInt32(c.Value, 16));
                        string unicode = string.Concat(chars);

                        // add it to our map
                        Dictionary[glyphName] = unicode;
                    }
                }
            }
        }
    }
}
