using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using ECSSO.Library.CustFormLibary;
using Newtonsoft.Json;

namespace ECSSO.api.CustFormAPI
{
    /// <summary>
    /// from 的摘要描述
    /// </summary>
    public class form : IHttpHandler
    {
        private RsponseFormData formData;
        private string setting;
        public void ProcessRequest(HttpContext context)
        {
            GetStr GS = new GetStr();
            string code, message;
            code = "404";
            message = "not fount";
            formData = new RsponseFormData();
            try
            {
                if (context.Request.Form["token"] != null)
                {
                    this.setting = GS.checkToken(context.Request.Form["token"]);
                    if (this.setting.IndexOf("error") < 0)
                    {
                        if (context.Request.Form["id"] != null && context.Request.Form["id"].ToString() != "")
                        {
                            getFormData(context.Request.Form["id"].ToString());
                            code = "200";
                            message = "success";
                        }
                    }
                    else {
                        code = "401";
                        message = "Token已過期" + this.setting;
                    }
                }
            }
            catch (Exception ex)
            {
                code = "500";
                message = ex.StackTrace;
            }
            finally
            {
                context.Response.Write(printMsg(code, message));
            }
        }
        #region 回傳error字串
        public void getFormData(string id) {
            
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from form where id=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if(reader.Read())
                    {
                        formData.data = new FormData
                        {
                            id = int.Parse(reader["id"].ToString()),
                            title = reader["title"].ToString(),
                            signature = reader["signature"].ToString().Replace("<br />", "\r\n"),
                            introduction = reader["introduction"].ToString().Replace("<br />","\r\n"),
                            dispCont = reader["disp_cont"].ToString()=="Y"
                        };
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        private String printMsg(String RspnCode, String RspnMsg)
        {
            formData.RspnCode = RspnCode;
            formData.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(formData);
        }
        #endregion
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}