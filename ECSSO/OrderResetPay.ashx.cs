using Microsoft.Security.Application;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO
{
    /// <summary>
    /// OrderResetPay 的摘要描述
    /// </summary>
    public class OrderResetPay : IHttpHandler
    {
        string OrderNo, payment_type, LogisticstypeID;
        string siteid;
        public void ProcessRequest(HttpContext context)
        {
            #region 檢查必要參數
            if (context.Request.Params["OrderNo"] != null)
            {
                OrderNo = context.Request.Params["OrderNo"].ToString();
            }

            if (context.Request.Params["SiteID"] != null)
            {
                siteid = context.Request.Params["SiteID"].ToString();
            }
            payment_type = context.Request.Params["payment_type"] ?? "";
            LogisticstypeID = context.Request.Params["LogisticstypeID"] ?? "";
            String CheckM = "";
            if (context.Request.Params["CheckM"] != null)
            {
                CheckM = Encoder.HtmlEncode(context.Request.Params["CheckM"].ToString());
            }
            #endregion

            GetStr GS = new GetStr();
            string DefaultURL = GS.GetDefaultURL(siteid);
            DefaultURL = DefaultURL.IndexOf("http") == 0 ? DefaultURL : "http://" + DefaultURL;
            if (GS.MD5Check(siteid + GS.GetOrgName(GS.GetSetting(siteid)) + OrderNo, CheckM))
            {
                String Setting = GS.GetSetting(siteid);
                using (SqlConnection conn2 = new SqlConnection(Setting))
                {
                    conn2.Open();
                    SqlCommand cmd2 = new SqlCommand();
                    cmd2.CommandText = "sp_updateOrderRepaydate";
                    cmd2.CommandType = CommandType.StoredProcedure;
                    cmd2.Connection = conn2;
                    cmd2.Parameters.Add(new SqlParameter("@id", OrderNo));
                    cmd2.Parameters.Add(new SqlParameter("@payment_type", payment_type));
                    cmd2.Parameters.Add(new SqlParameter("@LogisticstypeID", LogisticstypeID));
                    cmd2.ExecuteNonQuery();
                }
                context.Response.Redirect(DefaultURL + "/tw/shop.asp?id=" + OrderNo);
            }
            else
            {
                context.Response.Redirect(DefaultURL);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}