using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Meal.Library
{
    public class POSData
    {
        public class Printer
        {
            public string id { get; set; }
            public string title { get; set; }
            public string PrinterName { get; set; }
        }

        public class LANIP
        {
            public string IP { get; set; }
        }


        public class EasycardPort
        {
            public string Port { get; set; }
        }

    }
}