using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using ECSSO.Library;
using System.Net;
using System.IO;
using System.Text;

namespace ECSSO.api
{
    /// <summary>
    /// SetDeviceProd 的摘要描述
    /// </summary>
    public class SetDeviceProd : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Params["Type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Type必填", "", ""));
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", "", ""));
            if (context.Request.Params["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", "", ""));

            if (context.Request.Params["Type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Type必填", "", ""));
            if (context.Request.Params["SiteID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", "", ""));
            if (context.Request.Params["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", "", ""));

            String ChkM = context.Request.Params["CheckM"].ToString();
            String SiteID = context.Request.Params["SiteID"].ToString();
            String Type = context.Request.Params["Type"].ToString();

            GetStr GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);
            String OrgName = GS.GetOrgName(Setting);

            if (GS.MD5Check(SiteID + OrgName, ChkM))
            {
                if (context.Request.Form["ItemData"] != null && context.Request.Form["ItemData"] != "")
                {
                    switch (Type)
                    {
                        case "ADD":
                            ResponseWriteEnd(context, Insert(context.Request.Form["ItemData"],OrgName));
                            break;
                        case "DEL":
                            ResponseWriteEnd(context, Delete(context.Request.Form["ItemData"], OrgName));
                            break;
                    }
                }
                else 
                {
                    ResponseWriteEnd(context, ErrorMsg("error", "ItemData必填", Setting, ""));
                }
            }
            else
            {
                ResponseWriteEnd(context, ErrorMsg("error", "error:0", Setting, ""));        //驗證碼錯誤
            } 
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private class Datas
        {
            public string VerCode { get; set; }
            public List<string> ProdID { get; set; }
        }

        public String Insert(String JsonStr, String Orgname)
        {
            var json = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Datas> root = json.Deserialize<List<Datas>>(JsonStr);
            String Setting = GetSetting(Orgname);

            SqlConnection conn = new SqlConnection(Setting);
            conn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "sp_DeviceProd";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = conn;

            foreach (Datas dt in root)
            {
                String VerCode = dt.VerCode;

                for (int i = 0; i < dt.ProdID.Count; i++)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("@type", "add"));
                    cmd.Parameters.Add(new SqlParameter("@Did", VerCode));
                    cmd.Parameters.Add(new SqlParameter("@Pid", dt.ProdID[i]));
                    cmd.ExecuteNonQuery();                    
                }
            }
            conn.Dispose();
            cmd.Dispose();

            return "success";
        }

        public String Delete(String JsonStr, String Orgname)
        {
            var json = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Datas> root = json.Deserialize<List<Datas>>(JsonStr);
            String Setting = GetSetting(Orgname);

            SqlConnection conn = new SqlConnection(Setting);
            conn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "sp_DeviceProd";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = conn;

            foreach (Datas dt in root)
            {
                String VerCode = dt.VerCode;

                for (int i = 0; i < dt.ProdID.Count; i++)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("@type", "del"));
                    cmd.Parameters.Add(new SqlParameter("@Did", VerCode));
                    cmd.Parameters.Add(new SqlParameter("@Pid", dt.ProdID[i]));
                    cmd.ExecuteNonQuery();                    
                }
            }

            conn.Dispose();
            cmd.Dispose();

            return "success";
        }

        private String GetSetting(String Orgname)
        {
            String Setting = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("select dbname,dbusername,CONVERT(nvarchar(50), dbpassword) dbpassword,folder,web_url from cocker_cust where crm_org=@orgname and stat='Y' and End_Date >= (CONVERT([nvarchar](10),getdate(),(120)))", conn);
                cmd.Parameters.Add(new SqlParameter("@orgname", Orgname));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=" + reader["dbusername"].ToString() + "; password=" + reader["dbpassword"].ToString() + "; database=" + reader["dbname"].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return Setting;
        }

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting, String Pno)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "booking error", "", RspnMsg);
            }

            Library.booking.RootObject root = new Library.booking.RootObject();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            root.Pno = Pno;
            return JsonConvert.SerializeObject(root);

        }
        #endregion

        private void ResponseWriteEnd(HttpContext context, string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }

        #region Get IP
        private string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string sIPAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(sIPAddress))
            {
                return context.Request.ServerVariables["REMOTE_ADDR"];
            }
            else
            {
                string[] ipArray = sIPAddress.Split(new Char[] { ',' });
                return ipArray[0];
            }
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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "候位前台"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " booking.ashx"));

                cmd.ExecuteNonQuery();
            }
        }
        #endregion
    }
}