using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace UmbracoExamine.PDF.PdfSharp
{
    /// <summary>
    /// The standard Adobe Glyf List for converting standard Glyf characters to Unicode.
    /// To prevent continual parsing of the file, this uses a singleton pattern.
    /// </summary>
    public class AdobeGlyphList
    {
        private static AdobeGlyphList _instance;
        private Dictionary<string, string> Dictionary { get; set; }

        /// <summary>
        /// Constructur is private so we don't have numerous copies of this around
        /// </summary>
        private AdobeGlyphList()
        {
            Init();
        }

        /// <summary>
        /// Access the Glyph List by Singleton pattern
        /// </summary>
        public static AdobeGlyphList Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AdobeGlyphList();
                }
                return _instance;
            }
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

            // get the location of the glyphlist.txt file which we will read in
            string glyphListLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Plugins/UmbracoExamine.PDF/glyphlist.txt");

            if (!File.Exists(glyphListLocation))
            {
                throw new Exception("App_Plugins/UmbracoExamine.PDF/glyphlist.txt does not exist");
            }

            // Read the file and display it line by line.
            using (var file = new System.IO.StreamReader(glyphListLocation))
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

                        var chars = match.Groups["unicode"].Captures.Cast<Capture>().Select(c => (char)Convert.ToInt32(c.Value, 16));
                        string unicode = string.Concat(chars);

                        Dictionary[glyphName] = unicode;
                    }
                }
            }
        }
    }
}
