using ECSSO.Library;
using ECSSO.Library.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace ECSSO.api.FrontDesk
{
    /// <summary>
    /// OrdersFHandler 的摘要描述
    /// </summary>
    public class OrdersFHandler : IHttpHandler
    {
        private GetStr GS { get; set; }
        private ResponseOrderHead orderHead { get; set; }
        private string setting { get; set; }
        public void ProcessRequest(HttpContext context)
        {
            GS = new GetStr();
            try
            {
                orderHead = new ResponseOrderHead
                {
                    result = new responseJson { 
                        RspnCode="500",
                        RspnMsg="驗證未通過"
                    }
                };
                if (string.IsNullOrEmpty(context.Request.Form["siteid"])) throw new Exception("siteid必填");
                else if (string.IsNullOrEmpty(context.Request.Form["id"])) throw new Exception("orderID必填");
                else if (string.IsNullOrEmpty(context.Request.Form["MemberID"])) throw new Exception("MemberID必填");
                else if (string.IsNullOrEmpty(context.Request.Form["token"])) throw new Exception("token必填");
                else {
                    setting = GS.GetSetting2(context.Request.Form["siteid"]);
                    if (checkToken(context.Request.Form["token"], context.Request.Form["MemberID"]))
                    {
                        orderHead.getStore2Order(setting, context.Request.Form["id"]);
                        orderHead.result.RspnCode = "200";
                        orderHead.result.RspnMsg = "success";
                    }
                    else {
                        orderHead.result.RspnCode = "401";
                        throw new Exception("token錯誤");
                    }
                }
            }
            catch (Exception e)
            {
                orderHead.result.RspnMsg = e.Message;
            }
            finally {
                GS.ResponseWriteEnd(context, JsonConvert.SerializeObject(orderHead));
            }
        }
        private bool checkToken(string token,string MemID)
        {
            bool result=false; 
            using (SqlConnection conn = new SqlConnection(setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select token.* from token 
                    left join Cust on cust.id=token.ManagerID
                    where Cust.mem_id=@MemID and token.id=@token
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@MemID", MemID));
                cmd.Parameters.Add(new SqlParameter("@token", token));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read()) result = true;
                }
                catch(Exception e) {
                    throw e;
                }
            }
            return result;
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