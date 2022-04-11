using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    public class IKPDCust
    {
        public bool result { get; set; } //驗證是否成功
        public string apid { get; set; } //
        public string ip { get; set; }
        public string user_account { get; set; }
        public string IDNO { get; set; }
        public string user_name { get; set; }
        public string user_email { get; set; }
        public string unit_code { get; set; }
        public string unit_name { get; set; }
        public string org_code { get; set; }
        public string org_code_proxy { get; set; }
        public string description { get; set; }
    }
}