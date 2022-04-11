using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Order.BankStatement
{
    public class BankStatementDetial: RowValue
    {
        public string orderNumber { get; set; }
        public string productId { get; set; }
        public string productName { get; set; }
        public int price { get; set; }
        public int qty { get; set; }
        public int amt { get; set; }
    }
}