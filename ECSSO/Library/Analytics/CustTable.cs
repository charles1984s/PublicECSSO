using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Analytics
{
    public class CustTable : responseJson
    {
        public string title { get; set; }
        public string type { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string value { get; set; }
        public int result { get; set; }
        public List<CustTableAime> aime { get; set; }
        public List<CustTableCondition> condition { get; set; }
    }
    public class CustTableAime
    {
        public int id { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }
    public class CustTableCondition
    {
        public int id { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public List<CustTableConditionItem> Condition { get; set; }
        public string nextCondition { get; set; }
    }
    public class CustTableConditionItem
    {
        public int id { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public string next { get; set; }
    }
}