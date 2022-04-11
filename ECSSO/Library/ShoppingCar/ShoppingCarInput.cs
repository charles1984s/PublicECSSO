using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.ShoppingCar
{
    public class ShoppingCarInput
    {
        public int au_id { get; set; }
        public int sub_id { get; set; }
        public int prod_sub_id { get; set; }
        public int prod_id { get; set; }
        public int qty { get; set; }
        public int prod_color { get; set; }
        public int prod_size { get; set; }
        public string posno { get; set; }
        public string prodSalesType { get; set; }
        public string cust { get; set; }
        public int priceType { get; set; }
        public bool isAdditional { get; set; }
        public int bid { get; set; }
    }
}