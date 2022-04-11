using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Component
{
    public class CokerComponentList : responseJson
    {
        public List<Component> list { get; set; }
    }
}