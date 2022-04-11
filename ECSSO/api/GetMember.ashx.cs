using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using ECSSO.Library;

namespace ECSSO.api
{
    /// <summary>
    /// GetMember 的摘要描述
    /// </summary>
    public class GetMember : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Params["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填"));
            if (context.Request.Params["MemberID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "MemberID必填"));

            if (context.Request.Params["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Params["SiteID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填"));
            if (context.Request.Params["MemberID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "MemberID必填"));

            String ChkM = context.Request.Params["CheckM"].ToString();
            String SiteID = context.Request.Params["SiteID"].ToString();
            
            String MemberID = context.Request.Params["MemberID"].ToString();

            GetStr GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);
            String OrgName = GS.GetOrgName(Setting);
            
            if (GS.MD5Check(SiteID + OrgName + MemberID, ChkM))
            {
                Library.Member MB = new Library.Member();
                ResponseWriteEnd(context, MB.GetMemberData(MemberID, Setting));
            }
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

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg)
        {

            Library.Products.RootObject root = new Library.Products.RootObject();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);

        }
        #endregion        

        
    }
}