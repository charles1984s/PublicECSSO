using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Order
{
    public class OrderDetail
    {
        public int id { get; set; }
        public int qty { get; set; }
        public string name { get; set; }
        public string sizeTitle { get; set; }
        public string colorTitle { get; set; }
    }
}