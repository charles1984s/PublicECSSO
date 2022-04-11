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
    /// SetTemplate 的摘要描述
    /// </summary>
    public class SetTemplate : IHttpHandler
    {
        private class RootObject 
        {
            public String ID { get; set; }
        }
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Form["Type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Type必填", "", ""));
            if (context.Request.Form["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", "", ""));
            if (context.Request.Form["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", "", ""));

            if (context.Request.Form["Type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Type必填", "", ""));
            if (context.Request.Form["SiteID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", "", ""));
            if (context.Request.Form["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", "", ""));

            String ChkM = context.Request.Params["CheckM"].ToString();
            String SiteID = context.Request.Params["SiteID"].ToString();
            String Type = context.Request.Params["Type"].ToString();

            GetStr GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);
            String OrgName = GS.GetOrgName(Setting);
            String AUID = "";

            if (GS.MD5Check(SiteID + OrgName, ChkM))
            {
                if (context.Request.Form["ItemData"] != null && context.Request.Form["ItemData"] != "")
                {
                    RootObject postf = JsonConvert.DeserializeObject<RootObject>(context.Request.Params["ItemData"]);
                    if (postf.ID != null && postf.ID != "") AUID = postf.ID;
                }                

                switch (Type)
                {
                    case "css":
                        ResponseWriteEnd(context, SetCSS(OrgName, AUID));
                        break;
                    case "head":
                        ResponseWriteEnd(context, SetHead(OrgName, AUID));
                        break;
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

        public String SetCSS(String Orgname, String ID)
        {
            String CssPath = "";
            String Setting = "";
            String WebURL = "";
            String FileName = "";
            String ResourceURL = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("select dbname,dbusername,CONVERT(nvarchar(50), dbpassword) dbpassword,folder,web_url from cocker_cust where crm_org=@orgname and stat='Y' and End_Date >= replace((CONVERT([nvarchar](10),getdate(),(120))),'-','/')", conn);
                cmd.Parameters.Add(new SqlParameter("@orgname", Orgname));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_" + reader["dbusername"].ToString() + "; password=i_" + reader["dbpassword"].ToString() + "; database=" + reader["dbname"].ToString();
                            CssPath = reader["folder"].ToString() + @"\upload\_export\";
                            WebURL = "http://" + reader["web_url"].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            if (CssPath != "")
            {
                if (ID == "")
                {
                    #region CSS模板-全部匯出
                    using (SqlConnection conn = new SqlConnection(Setting))
                    {
                        conn.Open();
                        SqlCommand cmd;
                        cmd = new SqlCommand("select hid from head", conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    FileName = "index" + reader["hid"].ToString() + ".css";
                                    ResourceURL = WebURL + @"/css/index.asp?hid=" + reader["hid"].ToString();
                                    WriteFile(ResourceURL, CssPath, FileName);
                                }
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                    #endregion
                }
                else 
                {
                    #region CSS模板
                    FileName = "index" + ID + ".css";
                    ResourceURL = WebURL + @"/css/index.asp?hid=" + ID;
                    WriteFile(ResourceURL, CssPath, FileName);
                    #endregion
                }                
            }
            PublicLog(Setting);
            return "success";
        }

        public String SetHead(String Orgname,String ID) 
        {
            String CssPath = "";
            String Setting = "";
            String WebURL = "";
            String FileName = "";
            String ResourceURL = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("select dbname,dbusername,CONVERT(nvarchar(50), dbpassword) dbpassword,folder,web_url from cocker_cust where crm_org=@orgname and stat='Y' and End_Date >= replace((CONVERT([nvarchar](10),getdate(),(120))),'-','/')", conn);
                cmd.Parameters.Add(new SqlParameter("@orgname", Orgname));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_" + reader["dbusername"].ToString() + "; password=i_" + reader["dbpassword"].ToString() + "; database=" + reader["dbname"].ToString();
                            CssPath = reader["folder"].ToString() + @"\upload\_export\";
                            WebURL = "http://" + reader["web_url"].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            if (CssPath != "")
            {
                if (ID == "")
                {
                    #region Head模板-全部匯出
                    using (SqlConnection conn = new SqlConnection(Setting))
                    {
                        conn.Open();
                        SqlCommand cmd;
                        cmd = new SqlCommand("select id from menu_authors", conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    FileName = "head" + reader["id"].ToString() + ".html";
                                    ResourceURL = WebURL + @"/tw/window/view/headType/head.asp?au_id=" + reader["id"].ToString();
                                    WriteFile(ResourceURL, CssPath + @"html\", FileName);
                                }
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                    #endregion
                }
                else 
                { 
                    #region 單筆匯出
                    FileName = "head" + ID + ".html";
                    ResourceURL = WebURL + @"/tw/window/view/headType/head.asp?au_id=" + ID;
                    WriteFile(ResourceURL, CssPath + @"html\", FileName);
                #endregion
                }
            }
            PublicLog(Setting);
            return "success";
        }

        #region 寫入檔案
        public void WriteFile(String Resource, String Folder, String Filename)
        {
            //檔案是否存在
            bool Fileresult = System.IO.File.Exists(Folder + Filename);

            //資料夾是否存在            
            if (!Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }

            string line = SendForm(Resource, "");

            //檔案是否存在            
            if (!File.Exists(Folder + Filename))
            {
                FileStream fs = File.Create(Folder + Filename);
                fs.Close();
            }

            //檔案寫入
            using (StreamWriter sw = new StreamWriter(Folder + Filename))
            {
                sw.WriteLine(line);
            }
        }
        #endregion

        #region HttpWebRequest送出資料
        public String SendForm(String TradePostUrl, String param)
        {
            byte[] bs = Encoding.ASCII.GetBytes(param);

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(TradePostUrl);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = bs.Length;
            string result = null;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }
            using (WebResponse wr = req.GetResponse())
            {
                StreamReader sr = new StreamReader(wr.GetResponseStream(), System.Text.Encoding.GetEncoding("utf-8"));
                result = sr.ReadToEnd();
                sr.Close();
            }

            return result;
        }
        #endregion

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting, String Pno)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "SetTemplate error", "", RspnMsg);
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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "版型發布"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " SetTemplate.ashx"));

                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region 紀錄發布時間
        private void PublicLog(String Setting)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("update head set releaseTime =@releaseTime ", conn);
                cmd.Parameters.Add(new SqlParameter("@releaseTime", DateTime.Now.ToString("yyyyMMddHHmmss")));
                cmd.ExecuteNonQuery();
            }
        }
        #endregion


    }
}