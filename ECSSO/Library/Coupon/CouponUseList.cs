using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Coupon
{
    public class CouponUseList : RsponseListJson
    {
        public List<CouponUseItem> list { get; set; }
        public string VCode { get; set; }
    }
    public class CouponUseItem {
        public int id { get; set; }
        public string memID { get; set; }
        public string memName { get; set; }
        public string GCode { get; set; }
        public string stat { get; set; }
        public string getDate { get; set; }
        public string canUseDate { get; set; }
        public string ExchangeDay { get; set; }
        public string ExpireDate { get; set; }

    }
}