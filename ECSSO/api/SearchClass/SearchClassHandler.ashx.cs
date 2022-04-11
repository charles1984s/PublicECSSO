using ECSSO.Library;
using ECSSO.Library.SearchClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.api.SearchClass
{
    /// <summary>
    /// SearchClassHandler 的摘要描述
    /// </summary>
    public class SearchClassHandler : IHttpHandler
    {
        private CustSearchDto response;
        private CheckToken checkToken;
        public void ProcessRequest(HttpContext context)
        {
            checkToken = new CheckToken();
            try {
                checkToken.check(context);
                if (checkToken.response.RspnCode == "200") {
                    response = new CustSearchDto();
                    checkToken.response = response;
                    switch (context.Request.Form["type"]) {
                        case "Get":
                            response.GetAll(checkToken);
                            break;
                        case "Post":
                            Library.SearchClass.SearchClass c = JsonConvert.DeserializeObject<Library.SearchClass.SearchClass>(context.Request.Form["Class"]);
                            response.theClass = c;
                            c.CreateOrEdit(checkToken);
                            c.setSearchBind(checkToken);
                            break;
                        case "Del":
                            Library.SearchClass.SearchClass c1 = JsonConvert.DeserializeObject<Library.SearchClass.SearchClass>(context.Request.Form["Class"]);
                            c1.Delete(checkToken);
                            break;
                        default:
                            throw new Exception("操作不存在");
                    }
                }
                else throw new Exception("Token不存在");
            }
            catch (Exception ex)
            {
                checkToken.response.RspnCode = "500";
                checkToken.response.RspnMsg = ex.Message;
            }
            finally
            {
                context.Response.Write(checkToken.printMsg());
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