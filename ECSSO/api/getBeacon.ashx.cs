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
    /// getBeacon 的摘要描述
    /// </summary>
    public class getBeacon : IHttpHandler
    {

        private HttpContext context;
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            GetStr GS = new GetStr();

            int statement = 0;
            string returnMsg = "Something has wrong!!", tableName = "";
            try
            {
                if (context.Request.Params["Type"] == null || context.Request.Params["Type"].ToString() == "") statement = 1;
                if (context.Request.Params["Items"] == null || context.Request.Params["Items"].ToString() == "") statement = 2;
                if (context.Request.Params["CheckSum"] == null || context.Request.Params["CheckSum"].ToString() == "") statement = 3;
                if (context.Request.Params["Token"] == null || context.Request.Params["Token"].ToString() == "") statement = 4;
                if (context.Request.Params["Lng"] == null || context.Request.Params["Lng"].ToString() == "") statement = 5;

                switch (statement)
                {
                    case 0:
                        {
                            String ChkM = context.Request.Params["CheckSum"].ToString();
                            String Type = context.Request.Params["Type"].ToString();
                            String Token = context.Request.Params["Token"].ToString();
                            String Lng = GS.CheckStringIsNotNull(context.Request.Params["Lng"].ToString());
                            Beacon items = JsonConvert.DeserializeObject<Beacon>(context.Request.Params["Items"]);
                            string Items = context.Request.Params["Items"].ToString();
                            string strSqlConnection = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();

                            int minValue = 0, maxValue = 0;


                            if (GS.MD5Check(Type + Items, ChkM))
                            {
                                returnMsg = new CommandSetting().getDataResult(GS.GetIPAddress(), Token, strSqlConnection, Type, Lng, tableName, Items, "", minValue, maxValue);
                            }
                            else
                            {
                                returnMsg = ErrorMsg("error", "CheckSum驗證失敗", "");
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
                            returnMsg = ErrorMsg("error", "Items必填", "");
                            break;
                        }
                    case 3:
                        {
                            returnMsg = ErrorMsg("error", "CheckSum必填", "");
                            break;
                        }
                    case 4:
                        {
                            returnMsg = ErrorMsg("error", "Token必填", "");
                            break;
                        }
                    case 5:
                        {
                            returnMsg = ErrorMsg("error", "Lng必填", "");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                returnMsg = ErrorMsg("error", ex.ToString(), "");
            }

            context.Response.Write(returnMsg);
            context.Response.End();
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
                InsertLog(Setting, "getBeacon error", "", RspnMsg);
            }

            Library.Products.RootObject root = new Library.Products.RootObject();
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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "取得Beacon"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " getBeacon.ashx"));

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