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
    /// getAds 的摘要描述
    /// </summary>
    public class getHelp : IHttpHandler
    {
        private HttpContext context;
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            if (context.Request.Params["Type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Type必填", ""));
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Params["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", ""));

            if (context.Request.Params["Type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Type必填", ""));
            if (context.Request.Params["SiteID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", ""));
            if (context.Request.Params["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));

            String ChkM = context.Request.Params["CheckM"].ToString();
            String SiteID = context.Request.Params["SiteID"].ToString();
            String Type = context.Request.Params["Type"].ToString();

            GetStr GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);
            String OrgName = GS.GetOrgName(Setting);
            PostForm.HelpPage items = JsonConvert.DeserializeObject<PostForm.HelpPage>(context.Request.Params["Items"]);
            if (GS.MD5Check(Type + OrgName, ChkM))
            {
                switch (Type)
                {
                    case "HelpPage":
                        ResponseWriteEnd(context, getNewsList(items));
                        break;
                }
            }
            else ResponseWriteEnd(context, ErrorMsg("error", "CheckM驗證失敗", ""));
        }
        private string getNewsList(PostForm.HelpPage items)
        {
            HelpPage root = new HelpPage();
            root.Data = new List<HelpPageItem>();
            HelpPageItem item = new HelpPageItem();
            item.PageID = items.PageID;
            item.StepID = items.StepID;
            item.PicURL = context.Server.UrlEncode("http://yltravel.hawk.net.tw/upload/HelpPage/" + items.StepID + ".png");
            item.Description = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
            root.Data.Add(item);
            return JsonConvert.SerializeObject(root);
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
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

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "getVoices error", "", RspnMsg);
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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "取得影片"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " getEvent.ashx"));

                cmd.ExecuteNonQuery();
            }
        }
        #endregion
    }
}