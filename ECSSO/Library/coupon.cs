using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    public class coupon
    {
        public int cid { get; set; }
        public string memid { get; set; }
        public string VCode { get; set; }
        public int Price { get; set; }
        public string ExpireDate { get; set; }
        public string Stat { get; set; }
        public string ExchangeDay { get; set; }
        public string title { get; set; }
    }
}