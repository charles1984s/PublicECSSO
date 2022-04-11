using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Order.Check
{
    public class checkProdType
    {
        public int check_id { get; set; }
        public int prod_id { get; set; }
        public int size_id { get; set; }
        public int color_id { get; set; }
        public double price { get; set; }
    }
}