using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Power
{
    public class GroupResponse : responseJson
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}