using System;
using System.Collections.Generic;
using System.Web;
using ECSSO.Library;
using ECSSO;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ECSSO.api
{
    /// <summary>
    /// Authorization 的摘要描述
    /// </summary>
    public class Authorization : IHttpHandler
    {
        private HttpContext context;
        private GetStr GS;
        private String setting, settingSite, orgname, APPID, UserPwd;
        public void ProcessRequest(HttpContext context)
        {
            setting = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();
            this.context = context;
            GS = new GetStr();
            if (context.Request.Params["APPID"] == null) GS.ResponseWriteEnd(context, ErrorMsg("error", "APPID必填", ""));
            if (context.Request.Params["CheckM"] == null) GS.ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Params["SiteID"] == null) GS.ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", ""));

            if (context.Request.Params["APPID"].ToString() == "") GS.ResponseWriteEnd(context, ErrorMsg("error", "APPID必填", ""));
            if (context.Request.Params["SiteID"].ToString() == "") GS.ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", ""));
            if (context.Request.Params["CheckM"].ToString() == "") GS.ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));

            settingSite = GS.GetSetting2(context.Request.Params["SiteID"]);
            orgname = GS.GetOrgName(settingSite);
            APPID = context.Request.Params["APPID"].ToString();
            UserPwd = context.Request.Params["CheckM"].ToString();
            using (SqlConnection conn = new SqlConnection(settingSite))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select OrgName,ApiCode,sys.fn_VarBinToHexStr(hashbytes('MD5', OrgName + convert(nvarchar(50),ApiCode))) as author from GetAPI where Email=@Email", conn);
                cmd.Parameters.Add(new SqlParameter("@Email", APPID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        //this.WebTitle.Text = reader[0].ToString();
                        if (GS.MD5Check(orgname + reader["ApiCode"].ToString().ToUpper(), UserPwd))
                        {
                            UserPwd = reader["author"].ToString();
                            ProcessRequest();
                        }
                        else context.Response.Write(ErrorMsg("error", "驗證錯誤" + orgname + reader["ApiCode"].ToString().ToUpper(), ""));
                    }
                }
                catch { 
                    GS.ResponseWriteEnd(context, ErrorMsg("error", "權限錯誤", ""));
                }
            }

        }

        #region 驗證
        public void ProcessRequest() {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_login";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@orgname", orgname));
                cmd.Parameters.Add(new SqlParameter("@userid", APPID));
                cmd.Parameters.Add(new SqlParameter("@userpwd", UserPwd));
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
                        GS.ResponseWriteEnd(context, ErrorMsg("error", "驗證失敗", ""));
                    }
                    else if (ReturnCode == "error:2")
                    {
                        GS.ResponseWriteEnd(context, ErrorMsg("error", "網站不存在", ""));
                    }else
                    {
                        ECSSO.Library.Products.RootObject root = new ECSSO.Library.Products.RootObject();
                        root.RspnCode = "success";
                        root.Token = ReturnCode;
                        root.RspnMsg = GS.HtmlEncode("成功");
                        context.Response.Write(JsonConvert.SerializeObject(root));
                    }
                }
                catch
                {
                    GS.ResponseWriteEnd(context, ErrorMsg("error", "權限不足", ""));
                }
            }
        }
        #endregion

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "Authorization error", "", RspnMsg);
            }

            ECSSO.Library.Products.RootObject root = new ECSSO.Library.Products.RootObject();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);

        }
        #endregion

        #region insert log
        private void InsertLog(String Setting, String JobName, String JobTitle, String Detail)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_userlogAdd";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@id", "guest"));
                cmd.Parameters.Add(new SqlParameter("@prog_name", "取得驗證"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GS.GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " getAuthorization.ashx"));
                cmd.ExecuteNonQuery();
            }
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