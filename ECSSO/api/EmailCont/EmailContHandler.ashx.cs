using ECSSO.Library;
using ECSSO.Library.EmailCont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace ECSSO.api.EmailCont
{
    /// <summary>
    /// EmailContHandler 的摘要描述
    /// </summary>
    public class EmailContHandler : IHttpHandler
    {
        private CheckToken checkToken;
        private EmailContResponse EmailCont;
        private HttpContext context;
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            EmailCont = new EmailContResponse();
            checkToken = new CheckToken(EmailCont);
            try
            {
                EmailCont.RspnCode = "500";
                checkToken.check(context);
                EmailCont.setSetting(checkToken.setting);
                if (EmailCont.RspnCode == "200")
                {
                    EmailCont.RspnCode = "500.1";
                    Method();
                    EmailCont.RspnCode = "200";
                }
                else throw new Exception("Token不存在");
            }
            catch (Exception ex)
            {
                EmailCont.RspnMsg = ex.Message;
            }
            finally
            {
                context.Response.Write(checkToken.printMsg());
            }
        }
        public void Method()
        {
            switch (context.Request.Form["type"])
            {
                case "list":
                    EmailCont.setList();
                    EmailCont.RspnCode = "200";
                    break;
                case "saveAll":
                    EmailCont.list = JsonConvert.DeserializeObject<List<Library.EmailCont.EmailCont>>(context.Request["data"]);
                    EmailCont.list.ForEach(e => {
                        EmailCont.save(checkToken.setting, e);
                    });
                    break;
                case "save":
                    int id;
                    if (int.TryParse(context.Request.Form["id"], out id))
                    {
                        EmailCont.save(
                            checkToken.setting,
                            new Library.EmailCont.EmailCont
                            {
                                id = id,
                                signature = context.Request.Form["signature"] ?? "",
                                introduction = context.Request.Form["introduction"] ?? ""
                            }
                        );
                        EmailCont.RspnCode = "200";
                    }
                    else
                    {
                        EmailCont.RspnCode = "404.2";
                        throw new Exception("id不存在");
                    }
                    break;
                default:
                    EmailCont.RspnCode = "404.1";
                    throw new Exception("操作不存在");
            }
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}