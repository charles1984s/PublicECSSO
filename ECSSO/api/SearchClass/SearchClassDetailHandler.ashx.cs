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
    /// SearchClassDetailHandler 的摘要描述
    /// </summary>
    public class SearchClassDetailHandler : IHttpHandler
    {
        private SearchClassDetailDto response;
        private CheckToken checkToken;
        public void ProcessRequest(HttpContext context)
        {
            checkToken = new CheckToken();
            try
            {
                checkToken.check(context);
                if (checkToken.response.RspnCode == "200")
                {
                    response = new SearchClassDetailDto(
                        checkToken, 
                        int.Parse(context.Request.Form["id"]),
                        int.Parse(context.Request.Form["type"])
                    );
                    checkToken.response = response;
                    switch (response.type)
                    {
                        case 1:
                            break;
                        case 2:
                            response.getAllTag();
                            break;
                        case 3:
                            response.insertTag(JsonConvert.DeserializeObject<SearchClassDetailInputOfInsertDto>(context.Request.Form["data"]));
                            break;
                        case 4:
                            response.insertMenu(JsonConvert.DeserializeObject<CustSearchBindDto>(context.Request.Form["data"]));
                            break;
                        case 5:
                            response.Delete(JsonConvert.DeserializeObject<CustSearchBind>(context.Request.Form["data"]));
                            break;
                        default:
                            throw new Exception("操作不存在");
                    }
                }
                else throw new Exception("Token不存在");
            }
            catch (Exception ex)
            {
                //checkToken.response.RspnCode = "500";
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