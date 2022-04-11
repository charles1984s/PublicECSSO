using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Analytics
{
    public class Aims: responseJson
    {
        public int id { get; set; }
        public List<CustTableAime> items { get; set; }

    }
    public class Condition : responseJson
    {
        public int id { get; set; }
        public List<CustTableCondition> items { get; set; }

    }
}