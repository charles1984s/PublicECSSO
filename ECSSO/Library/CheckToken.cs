using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    public class CheckToken
    {
        public string setting { get; set; }
        public GetStr GS { get; set; }
        public responseJson response { get; set; }
        public TokenItem token { get; set; }
        public HttpContext context { get; set; }
        public CheckToken()
        {
            GS = new GetStr();
            response = new responseJson
            {
                RspnCode = "404",
                RspnMsg = "not fount"
            };
        }
        public CheckToken(responseJson response)
        {
            GS = new GetStr();
            this.response = response;
            this.response.RspnCode = "404";
            this.response.RspnMsg = "not fount";
            response.setToken(this);
        }
        public void check(HttpContext context) {
            if (string.IsNullOrEmpty(context.Request.Headers["token"]) || string.IsNullOrEmpty(context.Request.Form["token"]))
            {
                token = new TokenItem
                {
                    token = string.IsNullOrEmpty(context.Request.Headers["token"])? context.Request.Form["token"]: context.Request.Headers["token"]
                };
                this.setting = GS.checkToken(token);
                if (this.setting.IndexOf("error") < 0)
                {
                    this.context = context;
                    response.RspnCode = "200";
                    response.RspnMsg = "Token驗證成功";
                }
                else
                {
                    response.RspnCode = "401";
                    response.RspnMsg = "Token已過期";
                }
            }
            else throw new Exception("Token不存在");
        }
        public String printMsg()
        {
            return JsonConvert.SerializeObject(response);
        }
    }
}