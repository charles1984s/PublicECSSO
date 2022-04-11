using System;
using System.Collections.Generic;
using System.Web;

namespace ECSSO.Library
{
    public class OrderMenu
    {

        public class OrderSpec
        {
            public List<string> OtherID { get; set; }
            public List<string> Memo { get; set; }
            public string Qty { get; set; }
        }

        public class OrderItem
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public List<OrderSpec> OrderSpec { get; set; }
        }

        public class OrderList
        {
            public string ID { get; set; }
            public string Qty { get; set; }
            public List<OrderItem> OrderItems { get; set; }
        }

        public class OrderData
        {
            public string Vercode { get; set; }
            public string Tableid { get; set; }
            public string ReturnUrl { get; set; }
            public string ErrorUrl { get; set; }
            public string PayType { get; set; }
            public string BonusDiscount { get; set; }
            public string BonusAmt { get; set; }
            public string MemID { get; set; }
            public string Checkm { get; set; }
            public List<OrderList> OrderLists { get; set; }
        }

        public class MenuOrders
        {
            public OrderData OrderData { get; set; }
        }

    }
}