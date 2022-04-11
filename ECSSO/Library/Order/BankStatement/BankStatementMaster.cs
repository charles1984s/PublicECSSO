using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Order.BankStatement
{
    public class BankStatementMaster: RowValue
    {
        public string importNo { get; set; }
        public string date { get; set; }
        public string taxType { get; set; }
    }
}