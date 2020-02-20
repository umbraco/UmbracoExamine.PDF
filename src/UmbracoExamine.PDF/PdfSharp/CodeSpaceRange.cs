using System.Collections.Generic;

namespace UmbracoExamine.PDF.PdfSharp
{
    public class CodeSpaceRange
    {
        public CodeSpaceRange()
        {
            Mapping = new Dictionary<int, Map>();
        }

        public int Low { get; set; }
        public int High { get; set; }
        public int NumberOfBytes { get; set; }
        public Dictionary<int, Map> Mapping { get; set; }

        public void AddMap(int cid, string unicode, int sourceByteLength)
        {
            Mapping[cid] = new Map { UnicodeValue = unicode, SourceByteLength = sourceByteLength };
        }

        public class Map
        {
            public string UnicodeValue { get; set; }
            public int SourceByteLength { get; set; }
        }
    }
}
