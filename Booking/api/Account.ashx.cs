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

namespace Booking.api
{
    /// <summary>
    /// Account 的摘要描述
    /// </summary>
    public class Account : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Params["Type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Type必填", ""));
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Params["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", ""));

            if (context.Request.Params["Type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Type必填", ""));
            if (context.Request.Params["SiteID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", ""));
            if (context.Request.Params["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));

            String ChkM = context.Request.Params["CheckM"].ToString();
            String SiteID = context.Request.Params["SiteID"].ToString();
            String Type = context.Request.Params["Type"].ToString();


            ECSSO.GetStr GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);
            String OrgName = GS.GetOrgName(Setting);
            
            switch (Type) 
            {                 
                case "1":   //驗證帳密

                    PostForm.Account postf = JsonConvert.DeserializeObject<PostForm.Account>(context.Request.Params["ItemData"]);
                    String ID = postf.ID;
                    String Pwd = postf.Pwd;

                    break;
                case "2":   //帳號所屬商店

                    break;
                case "3":   //帳號對應角色

                    break;
                case "4":   //角色對應的webjob群組及權限

                    break;
                default:

                    break;
            }

            /* 帳號所屬商店
            select isnull(a.storeid,'') as id,isnull(b.title,'') as title
            from authors as a left join bookingStore as b on a.storeid=b.id
            where a.empl_id='ewaladmin'
            group by a.storeid,b.title 
             */

            /* 帳號對應角色
            select isnull(a.cid,'') as id,isnull(b.title,'') as title
            from authors as a left join Character as b on a.cid=b.id
            where a.empl_id='ewaladmin' and a.storeid='1'
            group by a.cid,b.title  
             */

            /*            
             * 角色對應的webjob群組
            select b.id,b.title 
            from dbo.WebjobsGroup_Character as a 
            left join dbo.WebjobsGroup_Head as b on a.Gid=b.id 
            where a.Cid='1'
                         
             * webjob群組內的作業權限            
            select a.job_id,c.job_name,isnull(b.canadd,'N') as canadd,isnull(b.canedit,'N') as canedit,isnull(b.candel,'N') as candel,isnull(b.canexe,'N') as canexe
            from WebjobsGroup as a
            left join authors as b on a.job_id=b.job_id and storeid='1' and cid='3' and empl_id='ewaladmin'
            left join webjobs as c on a.job_id=c.job_id
            where Gid='3'
             */


        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "booking error", "", RspnMsg);
            }

            ECSSO.Library.Products.RootObject root = new ECSSO.Library.Products.RootObject();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
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