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
    public class getAds : IHttpHandler
    {
        private HttpContext context;
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            GetStr GS = new GetStr();

            int statement = 0;
            string returnMsg = "Something has wrong!!", tableName = "Menu";
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
                            Ads items = JsonConvert.DeserializeObject<Ads>(context.Request.Params["Items"]);
                            string Items = context.Request.Params["Items"].ToString();
                            string strSqlConnection = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();

                            int minValue = 0, maxValue = 0;
                            if (items.Range != null)
                            {
                                if (items.Range.From != null || items.Range.From != "")
                                {
                                    minValue = (Int32.TryParse(items.Range.From, out minValue) ? Int32.Parse(items.Range.From) : 1) - 1;
                                }
                                if (items.Range.GetCount != null || items.Range.GetCount != "")
                                {
                                    maxValue = (items.Range.GetCount.ToLower() == "max") ? 10 : (Int32.TryParse(items.Range.GetCount, out maxValue) ? Int32.Parse(items.Range.GetCount) : 0);
                                }
                                minValue = (minValue == -1) ? 0 : minValue;
                                if (maxValue < 0 || minValue < 0)
                                {
                                    returnMsg = ErrorMsg("error", "From或是GetCount不得為負數", "");
                                    break;
                                }
                            }

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
                InsertLog(Setting, "getAds error", "", RspnMsg);
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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "取得廣告"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " getAds.ashx"));

                cmd.ExecuteNonQuery();
            }
        }
        #endregion
    }
}