using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    public class responseJson
    {
        protected CheckToken checkToken;
        public string code;
        public string error;
        public bool success;
        public String RspnCode { get { return code; } set { code = value; success = (value == "200"); } }
        public String RspnMsg { get { return error; } set { error = value; } }
        public String Token { get; set; }
        public String printMsg()
        {
            return JsonConvert.SerializeObject(this);
        }
        public void setToken(CheckToken token)
        {
            checkToken = token;
        }
    }
    public class FileResponseJson
    {
        public String code { get; set; }
        public String error { get; set; }
        public bool success { get; set; }
        public String path { get; set; }
        public String printMsg()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    public class RsponseListJson : responseJson {
        public int TotalPage { get; set; }
        public int CurrentPage { get; set; }
    }
}