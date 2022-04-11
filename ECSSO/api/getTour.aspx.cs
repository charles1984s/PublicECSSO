using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ECSSO.Library;
using Newtonsoft.Json;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

namespace ECSSO.api
{
    public partial class getTour1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                int statement = 0;
                string returnMsg = "";

                if (Request["type"] == null || Request["type"].ToString() == "") statement = 1;
                if (Request["SiteID"] == null || Request["SiteID"].ToString() == "") statement = 2;
                switch (statement)
                {
                    case 0:
                        {
                            String SiteID = Request["SiteID"].ToString();
                            String type = Request["type"].ToString();
                            String id = (Request["id"] == null || Request["id"].ToString() == "") ? "" : Request["id"].ToString();

                            GetStr gs = new GetStr();
                            if (gs.GetSettingForChecked(SiteID) == "")
                            {
                                returnMsg = ErrorMsg("error", "請檢查SiteID是否正確", "");
                            }
                            else
                            {
                                //交通部觀光局 platform : 0 (default)
                                if (type == "all")
                                {
                                    returnMsg = "node: " + createWebRequest(SiteID, "node") + "<br />";
                                    returnMsg += "event: " + createWebRequest(SiteID, "event") + "<br />";
                                    returnMsg += "shop: " + createWebRequest(SiteID, "shop") + "<br />";
                                    returnMsg += "hotel: " + createWebRequest(SiteID, "hotel");
                                }
                                //政府資訊開放平台 platform : 1
                                else
                                {
                                    var url = Request.Url.Authority + "/api/getTour.ashx";
                                    string uri = string.Format("{0}://{1}?SiteID={2}&type={3}&id={4}&platform={5}", Request.Url.Scheme, url, SiteID, type, id, "1");
                                    WebRequest request = WebRequest.Create(uri);

                                    request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                                    request.Method = "GET";
                                    using (WebResponse wr = request.GetResponse())
                                    {
                                        //在這裡對接收到的頁面內容進行處理
                                        using (Stream dataStream = wr.GetResponseStream())
                                        {
                                            using (StreamReader reader = new StreamReader(dataStream))
                                            {
                                                string responseFromServer = reader.ReadToEnd();
                                                Response.ContentType = "text/xml";
                                                returnMsg = responseFromServer;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case 1:
                        {
                            returnMsg = ErrorMsg("error", "Type必填", "");
                            break;
                        }
                    case 2:
                        {
                            returnMsg = ErrorMsg("error", "SiteID必填", "");
                            break;
                        }
                }
                Response.Write(returnMsg);
            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }
        }

        protected string createWebRequest(string SiteID, string type)
        {
            string fileName = type + ".xml";//客戶端儲存的檔名
            string path = Server.MapPath("../xml/" + SiteID + "/");//路徑
            var url = Request.Url.Authority + "/api/getTour.ashx";
            string uri = string.Format("{0}://{1}?SiteID={2}&type={3}", Request.Url.Scheme, url, SiteID, type);
            string returnMsg = "";

            try
            {
                WebRequest request = WebRequest.Create(uri);
                request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                request.Method = "GET";
                using (WebResponse wr = request.GetResponse())
                {
                    //在這裡對接收到的頁面內容進行處理
                    using (Stream dataStream = wr.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(dataStream))
                        {
                            string responseFromServer = reader.ReadToEnd();
                            if (!System.IO.Directory.Exists(path))
                            {
                                System.IO.Directory.CreateDirectory(path);
                            }
                            if (!System.IO.File.Exists(path + fileName))
                            {
                                FileStream fs = System.IO.File.Create(path + fileName);
                                fs.Close();
                            }
                            using (StreamWriter w = new StreamWriter(path + fileName, false))
                            {
                                w.WriteLine(responseFromServer, Encoding.UTF8);
                            }
                        }
                    }
                }
                returnMsg = "Successfully saved.";
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }
            return returnMsg;
        }
        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "getTour error", "", RspnMsg);
            }

            ContextErrorMessager root = new ContextErrorMessager();
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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "Tour"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " getTour.ashx"));

                cmd.ExecuteNonQuery();
            }
        }
        #endregion
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
    }
}