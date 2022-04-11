using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Order.Check
{
    public class OrderCheck : RowValue
    {
        public string title { get; set; }
        public int view { get => id; }
        public int delete { get => id; }
    }
}