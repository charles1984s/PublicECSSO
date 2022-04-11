using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Order.Check
{
    public class OrderHdItem: RowValue
    {
        public string id { get; set; }
        public string CustID { get; set; }
        public string CustName { get; set; }
        public string Date { get; set; }
        public string check { get => id; }
    }
}