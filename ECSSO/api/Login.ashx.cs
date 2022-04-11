using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.api
{
    /// <summary>
    /// Login 的摘要描述
    /// </summary>
    public class Login : IHttpHandler
    {
        HttpContext context;
        String orgName, account, type, password, talken, setting;

        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            
            if (context.Request.Form["orgName"] == null || context.Request.Form["orgName"] == "") ResponseWriteEnd("error", "404", "orgName", "null or empty");
            else if (context.Request.Form["account"] == null || context.Request.Form["account"] == "") ResponseWriteEnd("error", "404", "account", "null or empty");
            else if (context.Request.Form["type"] == null || context.Request.Form["type"] == "") ResponseWriteEnd("error", "404", "type", "null or empty");
            else {
                setting = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();
                orgName = context.Request.Form["orgName"].ToString();
                account = context.Request.Form["account"].ToString();
                type = context.Request.Form["type"].ToString();
                switch (context.Request.Form["type"].ToString()) { 
                    case "1"://登入
                        if (context.Request.Form["password"] == null || context.Request.Form["password"] == "") ResponseWriteEnd("error", "404", "password", "null or empty");
                        else
                        {
                            password = context.Request.Form["password"].ToString();
                            login();
                        }
                        break;
                    case "2"://更新talken
                        if (context.Request.Form["talken"] == null || context.Request.Form["talken"] == "") ResponseWriteEnd("error", "404", "talken", "null or empty");
                        else {
                            talken = context.Request.Form["talken"].ToString();
                            updateTalken();
                        }
                        break;
                }
            }
        }
        #region Talken更新
        private void updateTalken() {
            if (checkOrgName())
            {
                GetStr GS = new GetStr();
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "sp_updateTalken";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@orgname", orgName));
                    cmd.Parameters.Add(new SqlParameter("@userid", account));
                    cmd.Parameters.Add(new SqlParameter("@talken", talken));
                    cmd.Parameters.Add(new SqlParameter("@ip", GS.GetIPAddress()));
                    SqlParameter SPOutput = cmd.Parameters.Add("@ReturnCode", SqlDbType.NVarChar, 7);
                    SPOutput.Direction = ParameterDirection.Output;

                    string ReturnCode = null;
                    try
                    {
                        cmd.ExecuteNonQuery();
                        ReturnCode = SPOutput.Value.ToString();
                        if (ReturnCode == "error")
                        {
                            ResponseWriteEnd("error", "500", "talken", "error");
                        }
                        else
                        {
                            context.Session["Orgname"] = orgName;
                            context.Session["LoginID"] = account;
                            ResponseWriteEnd("success", "200", "talken", talken);
                        }
                    }
                    catch
                    {
                        ResponseWriteEnd("error", "500", "orgName", "error");
                    }
                }
            }
            else ResponseWriteEnd("error", "404", "orgName", "not exist");
        }
        #endregion
        #region 檢查網站是否存在
        private bool checkOrgName() {
            bool result = false;
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from cocker_cust where comp_en_name = @orgname Collate Chinese_Taiwan_Stroke_CS_AI", conn);
                cmd.Parameters.Add(new SqlParameter("@orgname", orgName));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            result = true;
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
                return result;
            }
        }
        #endregion
        #region 登入
        public void login() {
            if (checkOrgName())
            {
                GetStr GS = new GetStr();
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "sp_login";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@orgname", orgName));
                    cmd.Parameters.Add(new SqlParameter("@userid", account));
                    cmd.Parameters.Add(new SqlParameter("@userpwd", password));
                    cmd.Parameters.Add(new SqlParameter("@ip", GS.GetIPAddress()));
                    SqlParameter SPOutput = cmd.Parameters.Add("@ReturnCode", SqlDbType.NVarChar, 50);
                    SPOutput.Direction = ParameterDirection.Output;

                    string ReturnCode = null;
                    try
                    {
                        cmd.ExecuteNonQuery();
                        ReturnCode = SPOutput.Value.ToString();
                        if (ReturnCode == "error:1")
                        {
                            ResponseWriteEnd("error", "500", "account", "error");
                        }
                        else
                        {
                            //context.Session["Orgname"] = orgName;
                            //context.Session["LoginID"] = account;
                            ResponseWriteEnd("success", "200", "talken", ReturnCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        //ResponseWriteEnd("error", "500", "orgName", ex.Message);
                    }
                }
            }
            else ResponseWriteEnd("error", "404", "orgName", "not exist");
        }
        #endregion
        #region 輸出
        private void ResponseWriteEnd(string status, string code, string id, string msg)
        {
            context.Response.Write("{\"status\":\"" + status + "\",\"code\":\"" + code + "\",\"id\":\"" + id + "\",\"msg\":\"" + msg + "\"}");
            context.Response.End();
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