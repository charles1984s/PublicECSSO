using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    public class FB
    {
        #region 登入基本資料

        public class me
        {
            public string id { get; set; }
            public string email { get; set; }
            public string first_name { get; set; }
            public string gender { get; set; }
            public string last_name { get; set; }
            public string link { get; set; }
            public string locale { get; set; }
            public string name { get; set; }
            public int timezone { get; set; }
            public string updated_time { get; set; }
            public bool verified { get; set; }
        }
        #endregion
    }
}