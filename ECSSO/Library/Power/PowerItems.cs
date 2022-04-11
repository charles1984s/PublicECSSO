using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Power
{
    public class PowerItems : responseJson
    {
        public List<PowerGroup> group { get; set; }
    }
    public class PowerGroup
    {
        public string name { get; set; }
        public string key { get; set; }
        public List<PowerItem> list { get; set; }
    }
    public class savePowerGroup
    {
        public string job { get; set; }
        public string key { get; set; }
        public bool run { get; set; }
    }
    public class PowerItem
    {
        public string job { get; set; }
        public string name { get; set; }
        public bool isSelf { get; set; }
        public PowerCheckItem exe { get; set; }
        public PowerCheckItem add { get; set; }
        public PowerCheckItem edit { get; set; }
        public PowerCheckItem del { get; set; }
    }
    public class PowerCheckItem
    {
        public bool run { get; set; }
        public bool enable { get; set; }
    }
}