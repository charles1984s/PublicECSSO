using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beacon.Library
{
    public class beacon
    {
        public string minorID { get; set; }
        public string uuidID { get; set; }
        public string majorID { get; set; }
        public string locationName { get; set; }
        public string map_x { get; set; }
        public string map_y { get; set; }
        public string mapPicSource { get; set; }
        public string locationNumber { get; set; }
        public string location { get; set; }
        public string webpage { get; set; }

    }

    public class webpageurl
    {
        public string webpage { get; set; }

    }

    public class RootObject
    {
        public String RspnCode { get; set; }
        public String RspnMsg { get; set; }
    }
}