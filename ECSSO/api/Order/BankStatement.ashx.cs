using ECSSO.Library;
using ECSSO.Library.Order.BankStatement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.api.Order
{
    /// <summary>
    /// BankStatement 的摘要描述
    /// </summary>
    public class BankStatement : IHttpHandler
    {
        private CheckToken checkToken;
        public void ProcessRequest(HttpContext context)
        {
            checkToken = new CheckToken(new BankStatementOutput());
            checkToken.context = context;
            checkToken.setting = checkToken.GS.GetSetting(context.Request.Form["sitid"]);
            try
            {
                Method();
                checkToken.response.success = true;
                checkToken.response.RspnCode = "200";
                checkToken.response.RspnMsg = "success";
            }
            catch (Exception e)
            {
                checkToken.response.success = false;
                checkToken.response.RspnMsg = e.Message;
            }
            finally {
                context.Response.Write(checkToken.printMsg());
            }
        }
        public void Method() {
            ((BankStatementOutput)checkToken.response).memid = checkToken.context.Request.Form["memid"];
            switch (checkToken.context.Request.Form["type"]) {
                case "List":
                    ((BankStatementOutput)checkToken.response).getMaster();
                    break;
                case "Detail":
                    ((BankStatementOutput)checkToken.response).getDatail(int.Parse(checkToken.context.Request.Form["id"]));
                    break;
                default:
                    throw new Exception("方法錯誤");
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