using ECSSO.Library.CustFormLibary;
using System;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
namespace ECSSO.api.CustFormAPI
{
    /// <summary>
    /// fromList 的摘要描述
    /// </summary>
    public class fromList : IHttpHandler
    {
        HttpContext context;
        CustForm custForm;
        string setting, orderby, column, seq;
        public void ProcessRequest(HttpContext context)
        {
            GetStr GS = new GetStr();
            string code, message;
            code = "404";
            message = "not fount";
            custForm = new CustForm();
            try
            {
                if (context.Request.Form["token"] != null)
                {

                    this.setting = GS.checkToken(context.Request.Form["token"]);
                    if (this.setting.IndexOf("error")<0)
                    {
                        this.context = context;
                        if (context.Request.Form["c"] != null) column = context.Request.Form["c"].ToString();
                        else column = "id";
                        if (context.Request.Form["s"] != null) seq = context.Request.Form["s"].ToString();
                        else seq = "ASC";
                        orderby = column + " " + seq;

                        custForm.from = new List<FormItem>();
                        getFromList();
                        code = "200";
                        message = "success";
                    }
                    else {
                        code = "401";
                        message = "Token已過期"+ this.setting;
                    }
                }
            }
            catch (Exception ex)
            {
                code = "500";
                message = ex.StackTrace;
            }
            finally {
                context.Response.Write(printMsg(code, message));
            }
        }
        private void getFromList()
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from form
                    ORDER BY    
                    case WHEN @orderby = 'id ASC' Then id ELSE null END ASC,
                    case WHEN @orderby = 'id DESC' Then id ELSE null END DESC,
                    CASE WHEN @orderby = 'title ASC' then title ELSE null END ASC,
                    CASE WHEN @orderby = 'title DESC' then title ELSE null END DESC
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@orderby", orderby));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        FormItem form = new FormItem();
                        form.options = new FormOptions();
                        form.options.classes = "formItem" + reader["id"].ToString() + " formLogItem" + reader["id"].ToString();
                        form.options.expanded = reader["disp_cont"].ToString() == "Y";
                        form.value = new FormTableValue();
                        form.value.id = Int32.Parse(reader["id"].ToString());
                        form.value.title = reader["title"].ToString();
                        form.value.log = custForm.setCustForm2(
                            "cell-btn cell-log",
                            "<div class='delbtn btn btn-xs btn-default btn-page'><i class='glyphicon glyphicon-file'></i></div>"
                        );
                        form.value.edit = custForm.setCustForm2(
                            "cell-btn cell-edit",
                            "<div class='delbtn btn btn-xs btn-default btn-edit'><i class='glyphicon glyphicon-pencil'></i></div>"
                        );
                        custForm.from.Add(form);
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        #region 回傳error字串
        private String printMsg(String RspnCode, String RspnMsg)
        {
            custForm.RspnCode = RspnCode;
            custForm.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(custForm);
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