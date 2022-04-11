using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Coupon
{
    public class CouponList : responseJson
    {
        public List<CouponItem> list { get; set; }
        public int TotalPage { get; set; }
        public int CurrentPage { get; set; }
    }
    public class CouponItem
    {
        public CouponOptions options { get; set; }
        public CouponTableValue value { get; set; }
    }
    public class CouponOptions
    {
        public String classes { get; set; }
        public bool expanded { get; set; }
    }
    public class CouponTableValue
    {
        public int id { get; set; }
        public string title { get; set; }
        public string sendCount { get; set; }
        public string startTimt { get; set; }
        public string endTimt { get; set; }
        public string getType { get; set; }
        public string disp { get; set; }
    }
}