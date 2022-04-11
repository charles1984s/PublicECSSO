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
    /// FormColumn 的摘要描述
    /// </summary>
    public class FormColumn : IHttpHandler
    {
        HttpContext context;
        FormColumns formColumn;
        string setting;
        int id;
        public void ProcessRequest(HttpContext context)
        {
            GetStr GS = new GetStr();
            string code, message;
            code = "404";
            message = "not fount";
            formColumn = new FormColumns();
            try
            {
                if (context.Request.Form["token"] != null)
                {

                    this.setting = GS.checkToken(context.Request.Form["token"]);
                    if (this.setting.IndexOf("error") < 0)
                    {
                        this.context = context;
                        if (context.Request.Form["id"] != null)
                        {
                            id = int.Parse(context.Request.Form["id"].ToString());
                            formColumn.columnItems = new List<FormColumnItem>();
                            getColumnItems();
                            code = "200";
                            message = "success";
                        }
                    }
                    else
                    {
                        code = "401";
                        message = "Token已過期";
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

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        private void getColumnItems()
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from contactUsColumn where f_id=@f_id order by ser_num
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@f_id", id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        int displayType = int.Parse(reader["dispType"].ToString());
                        int col_id = int.Parse(reader["id"].ToString());
                        FormColumnItem form = new FormColumnItem
                        {
                            id = col_id,
                            f_id = int.Parse(reader["f_id"].ToString()),
                            title = reader["title"].ToString(),
                            display = reader["disp_opt"].ToString()=="Y",
                            displayType = displayType,
                            must = reader["must"].ToString()=="Y",
                            span = int.Parse(reader["colspan"].ToString()),
                            ser = int.Parse(reader["ser_num"].ToString()),
                            initText = reader["initText"].ToString(),
                            memo = reader["memo"].ToString(),
                            dispOther = reader["disp_other"].ToString()=="Y"
                        };
                        form.detail = new List<ColumnDetail>();
                        if (displayType != 1)
                        {
                            using (SqlConnection conn2 = new SqlConnection(setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2 = new SqlCommand(@"
                                    select * from columnDetail where col_id=@col_id
                                ", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@col_id", col_id));
                                SqlDataReader reader2 = null;
                                try
                                {
                                    reader2 = cmd2.ExecuteReader();
                                    while (reader2.Read())
                                    {
                                        ColumnDetail columnDetail = new ColumnDetail
                                        {
                                            id = int.Parse(reader2["id"].ToString()),
                                            title = reader2["title"].ToString(),
                                            ser = int.Parse(reader2["ser_no"].ToString())
                                        };
                                        form.detail.Add(columnDetail);
                                    }
                                }
                                finally
                                {
                                    reader2.Close();
                                }
                            }
                        }
                        formColumn.columnItems.Add(form);
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
            formColumn.RspnCode = RspnCode;
            formColumn.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(formColumn);
        }
        #endregion
    }
}