using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Analytics
{
    public class FlowCheckItem : FlowItemValue
    {
        public string upload { get; set; }
        public string download { get; set; }
    }
    public class FlowCheckYearItem : FlowCheckItem {
        public string year { get; set; }
    }
    public class FlowCheckMonthItem : FlowCheckItem
    {
        public int month { get; set; }
    }
    public class FlowCheckDetailItem : FlowCheckItem
    {
        public string date { get; set; }
    }
}