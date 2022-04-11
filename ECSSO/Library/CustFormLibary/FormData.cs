using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.CustFormLibary
{
    public class RsponseFormData : responseJson
    {
        public FormData data { get; set; }
    }
    public class FormData
    {
        public int id { get; set; }
        public string title { get; set; }
        public string introduction { get; set; }
        public string signature { get; set; }
        public bool dispCont { get; set; }
    }
}