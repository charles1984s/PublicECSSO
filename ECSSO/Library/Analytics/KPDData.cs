using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Analytics
{
    public class KPDData : responseJson
    {
        public List<KPDDataItem> list;
    }
    public class KPDDataItem {
        public string code { get; set; }
        public string title { get; set; }
    }
}