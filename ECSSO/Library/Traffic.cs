using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    #region input
    public class Traffic
    {
        public string AreaID { get; set; }
    }
    #endregion
    #region output
    public class TrafficItem
    {
        public string URL { get; set; }
    }
    #endregion
}