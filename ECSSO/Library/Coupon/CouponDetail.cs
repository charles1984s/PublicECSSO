using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Coupon
{
    public class CouponDetail : responseJson
    {
        public int id { get; set; }
        public string title { get; set; }
        public int fullGetPrice { get; set; }
        public string vcode { get; set; }
        public string GCode { get; set; }
        public string FCode { get; set; }
        public int getQty { get; set; }
        public int stocks { get; set; }
        public string prodStockNo { get; set; }
        public string ActivationDate { get; set; }
        public string ExpireDate { get; set; }
        public bool stat { get; set; }
        public bool locked { get; set; }
        public int fullPrice { get; set; }
        public int discount { get; set; }
        public int percent { get; set; }
        public int giftID { get; set; }
        public string giftTitle { get; set; }
        public int activeType { get; set; }
        public int getType { get; set; }
        public int useType { get; set; }
        public int noType { get; set; }
        public int useQty { get; set; }
        public int LogisticsID { get; set; }
        public string LogisticsTitle { get; set; }
    }
}