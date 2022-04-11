using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ECSSO.api
{
    /// <summary>
    /// QA 的摘要描述
    /// </summary>
    public class QA : IHttpHandler
    {

        public class QAStr
        {
            public String ID { get; set; }
            public String OrderNO { get; set; }
            public String Question { get; set; }
            public String Qdate { get; set; }
            public String Answer { get; set; }
            public String Adate { get; set; }
        }

        public void ProcessRequest(HttpContext context)
        {
            #region 檢查post值
            if (context.Request.Form["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Form["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填"));
            if (context.Request.Form["type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "type必填"));
            if (context.Request.Form["Items"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Items必填"));

            if (context.Request.Form["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Form["SiteID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填"));
            if (context.Request.Form["type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "type必填"));
            if (context.Request.Form["Items"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Items必填"));
            #endregion

            String CheckM = context.Request.Form["CheckM"].ToString();
            String SiteID = context.Request.Form["SiteID"].ToString();
            String type = context.Request.Form["type"].ToString();

            GetStr GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);
            String OrgName = GS.GetOrgName(Setting);

            if (GS.MD5Check(type + OrgName, CheckM))
            {
                
                QAStr QA = JsonConvert.DeserializeObject<QAStr>(context.Request.Form["Items"]);

                switch (type)
                {
                    case "OrderEdit":
                        
                        using (SqlConnection conn = new SqlConnection(Setting))
                        {
                            conn.Open();

                            SqlCommand cmd = new SqlCommand();
                            cmd.CommandText = "sp_OrderQAEdit";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = conn;
                            cmd.Parameters.Add(new SqlParameter("@id", QA.ID));
                            cmd.Parameters.Add(new SqlParameter("@type", "1"));
                            cmd.Parameters.Add(new SqlParameter("@answer", QA.Answer));
                            cmd.ExecuteNonQuery();                            
                        }

                        String LogStr = "sp_OrderQAEdit '" + QA.ID + "','1','" + QA.Answer + "'";
                        GS.SaveLog(Setting, "admin", "訂單QA管理", "修改", "", QA.ID, LogStr, "/api/QA.ashx");

                        ResponseWriteEnd(context, "success");

                        break;
                    default:

                        break;
                }
            }
            else {

                ResponseWriteEnd(context, ErrorMsg("error", "CheckM error"));
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