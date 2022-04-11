using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Component
{
    public class ObjectItems : responseJson
    {
        public List<ObjectItem> List { get; set; }
    }
    public class ObjectItem {
        public int ID { get; set; }
        public string Title { get; set; }
        public string ico { get; set; }
    }
}