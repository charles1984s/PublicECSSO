using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Order.Check
{
    public class OrderHdViewItem: RowValue
    {
        public string id { get; set; }
        public string CustName { get; set; }
        public string ProudName { get; set; }
        public string Spec { get; set; }
        public int Qty { get; set; }
        public Double Price { get; set; }
        public string Memo { get; set; }
        public string Date { get; set; }
    }
}