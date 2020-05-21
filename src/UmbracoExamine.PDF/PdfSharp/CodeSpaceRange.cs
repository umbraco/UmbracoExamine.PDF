using System.Collections.Generic;

namespace UmbracoExamine.PDF.PdfSharp
{
    /// <summary>
    /// Model a code space range used in the CMAPs
    /// </summary>
    public class CodeSpaceRange
    {
        public CodeSpaceRange()
        {
            Mapping = new Dictionary<int, Map>();
        }

        /// <summary>
        /// The lowest value the map holds
        /// </summary>
        public int Low { get; set; }

        /// <summary>
        /// The highest value the map holds
        /// </summary>
        public int High { get; set; }

        /// <summary>
        /// The number of bytes each character is
        /// </summary>
        public int NumberOfBytes { get; set; }

        /// <summary>
        /// The map for a given CID, used to lookup values from the PDF doc and return unicode chars
        /// </summary>
        public Dictionary<int, Map> Mapping { get; }

        /// <summary>
        /// Add a value to our code space range
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="unicode"></param>
        /// <param name="sourceByteLength"></param>
        public void AddMap(int cid, string unicode, int sourceByteLength)
        {
            Mapping[cid] = new Map { UnicodeValue = unicode, SourceByteLength = sourceByteLength };
        }

        /// <summary>
        /// Holds the unicode value, and how many bytes long the CID uses
        /// </summary>
        public class Map
        {
            public string UnicodeValue { get; set; }
            public int SourceByteLength { get; set; }
        }
    }
}
