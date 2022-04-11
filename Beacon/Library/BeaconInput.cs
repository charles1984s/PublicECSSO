using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beacon.Library
{
    public class BeaconInput
    {
        public class Datum
        {
            public string ID { get; set; }
            public string type { get; set; }
        }

        public class Items
        {
            public string key { get; set; }
            public string verCode { get; set; }
            public string MAC { get; set; }
            public string TxPower { get; set; }
            public List<Datum> Data { get; set; }
        }


        public class Detail
        {
            public string img { get; set; }
            public string video { get; set; }
            public string Description { get; set; }
        }

        public class ReturnObject
        {
            public string stat { get; set; }
            public List<Detail> Detail { get; set; }
        }

        public class List
        {
            public string id { get; set; }
            public string title { get; set; }
        }

        public class ReturnObject2
        {
            public string stat { get; set; }
            public List<List> List { get; set; }
        }

        public class BeaconPower {
            public string key { get; set; }
            public string MAC { get; set; }
            public string TxPower { get; set; }
        }

        public class Error
        {
            public string RspnCode { get; set; }
            public string RspnMsg { get; set; }
        }
    }
}