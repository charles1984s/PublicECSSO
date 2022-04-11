using System;
using System.Collections.Generic;
using System.Web;
using ECSSO.Library;
using ECSSO;
using System.Net;
using System.IO;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ECSSO.api
{
    /// <summary>
    /// getMovie 的摘要描述
    /// </summary>
    public class getMovie : IHttpHandler
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

                            if (GS.MD5Check(Type + Items, ChkM))
                            {
                                CommandSetting cmdSetting = new CommandSetting();
                                DataListSetting dataList = new DataListSetting();
                                dataList.Data = new List<object>();

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

                                switch (cmdSetting.isVerityState(GS.GetIPAddress(), Token, strSqlConnection))
                                {
                                    case 0:
                                        if (Type == "getByMenuSubID")
                                        {
                                            string selectString = "SELECT TOP " + maxValue + " * FROM ( "
                                                                + "select TOP " + (minValue + maxValue) + " id, title, img1, Cont, media_link from menu where menu.disp_opt = 'Y' and sub_id = @ClasID ORDER BY Id DESC"
                                                                + ")a ORDER BY id;";
                                            using (SqlConnection conn = new SqlConnection(cmdSetting._strSqlConnection))
                                            {
                                                List<BaseLinkIdWithContent> baseLinks = new List<BaseLinkIdWithContent>();
                                                using (SqlCommand cmd = new SqlCommand(selectString, conn))
                                                {
                                                    if (conn.State == ConnectionState.Closed) conn.Open();
                                                    cmd.Parameters.AddWithValue("@ClasID", "132");
                                                    try
                                                    {
                                                        using (SqlDataReader reader = cmd.ExecuteReader())
                                                        {
                                                            if (reader.HasRows)
                                                            {
                                                                while (reader.Read())
                                                                {
                                                                    string picurl = (reader["img1"] is DBNull) ? "" : Uri.EscapeDataString(GS.GetAllLinkString(cmdSetting._orgName, reader["img1"].ToString(), Lng, "Image"));
                                                                    string detailurl = (reader["media_link"] is DBNull) ? "" : Uri.EscapeDataString(reader["media_link"].ToString());

                                                                    BaseLinkIdWithContent item = new BaseLinkIdWithContent()
                                                                    {
                                                                        Id = (reader["Id"] is DBNull) ? "" : reader["Id"].ToString(),
                                                                        Title = (reader["Title"] is DBNull) ? "" : reader["Title"].ToString(),
                                                                        Image = picurl,
                                                                        Link = detailurl,
                                                                        Brief = (reader["Cont"] is DBNull) ? "" : Uri.EscapeDataString(WebUtility.HtmlDecode(reader["Cont"].ToString()))
                                                                    };
                                                                    baseLinks.Add(item);
                                                                }
                                                            }
                                                        }
                                                        baseLinks = baseLinks.OrderByDescending(o => o.Id).ToList();
                                                        foreach (BaseLinkWithContent item in baseLinks.Select(o => new BaseLinkWithContent() { Title = o.Title, Image = o.Image, Link = o.Link,Brief = o.Brief }).ToList())
                                                        {
                                                            dataList.Data.Add(item);
                                                        }
                                                        returnMsg = JsonConvert.SerializeObject(dataList);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        returnMsg = ErrorMsg("error", ex.ToString(), "");
                                                        //returnMsg = ErrorMsg("error", "請檢查資料欄位、查詢語法是否有誤。", "");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            returnMsg = ErrorMsg("error", "Type不存在", "");
                                        }
                                        break;
                                    case 1:
                                        returnMsg = ErrorMsg("error", "Token不存在", "");
                                        break;
                                    case 2:
                                        returnMsg = ErrorMsg("error", "Token權限出問題", "");
                                        break;
                                }
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

            //context.Response.ContentType = "text/html";
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
                InsertLog(Setting, "getMovie error", "", RspnMsg);
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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "影片"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " getMovie.ashx"));

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