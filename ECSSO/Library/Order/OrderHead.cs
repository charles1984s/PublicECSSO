using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Order
{
    public class OrderHead : RowValue
    {
        public string cdate { get; set; }
        public string orderID { get; set; }
        public string recipient { get; set; }
        public long oPrice { get; set; }
        public long bonus { get; set; }
        public long discont { get; set; }
        public long coupon { get; set; }
        public long freight { get; set; }
        public long price { get; set; }
        public long price2 { get; set; }
        public long servicePrice { get; set; }
        public string status { get; set; }
        public string payType { get; set; }
        public string logistics { get; set; }
        public string deliveryDate { get; set; }
        public string view { get; set; }
        public string export { get; set; }
        public string finish { get; set; }
        public string othersLink { get; set; }
    }
    public class OrderReportData
    {
        public string orderID { get; set; }
        public string serID { get; set; }
        public string cdate { get; set; }
        public string recipient { get; set; }
        public string recipientPhone { get; set; }
        public string recipientTel { get; set; }
        public string recipientArr { get; set; }
        public string recipientEmail { get; set; }
        public string sender { get; set; }
        public string senderPhone { get; set; }
        public string senderTel { get; set; }
        public string senderMemID { get; set; }
        public string senderArr { get; set; }
        public string invoiceTitle { get; set; }
        public string invoiceNo { get; set; }
        public long oPrice { get; set; }
        public long bonus { get; set; }
        public long discont { get; set; }
        public long freight { get; set; }
        public long price { get; set; }
        public long coupon { get; set; }
        public double servicePrice { get; set; }
        public string couponTitle { get; set; }
        public string status { get; set; }
        public string payType { get; set; }
        public string store2File1 { get; set; }
        public string store2File2 { get; set; }
        public string store2img1 { get; set; }
        public string store2Code { get; set; }
        public List<string> logistics { get; set; }
        public List<string> Temperature { get; set; }
        public string memo { get; set; }
        public List<OrderProd> prods{ get; set; }
    }
    public class OrderProd {
        public string serNo { get; set; }
        public string itemNo { get; set; }
        public string name { get; set; }
        public string memo { get; set; }
        public string size { get; set; }
        public string color { get; set; }
        public double price { get; set; }
        public int qty { get; set; }
        public long discount { get; set; }
        public long subtotal { get; set; }
        
    }
}