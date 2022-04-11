using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.ThirdParty
{
    public class ThirdPartyList : responseJson
    {
        public List<ThirdPartyItem> List { get; set; }
    }
}