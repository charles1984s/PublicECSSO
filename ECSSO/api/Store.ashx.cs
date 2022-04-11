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
    /// Store 的摘要描述
    /// </summary>
    public class Store : IHttpHandler
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

            GetStr GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);
            String OrgName = GS.GetOrgName(Setting);


            String DetailStr = "Type = " + Type + ",SiteID = " + SiteID + ",ChkM = " + ChkM;
            InsertLog(Setting, "呼叫Store", "", DetailStr);

            PostForm.BookingPost postf;
            String StoreID = "";
            String VerCode = "";

            switch (Type)
            {
                case "1":
                    #region 查詢所屬門市(VerCode)
                    if (GS.MD5Check(SiteID + OrgName, ChkM))
                    {
                        if (context.Request.Params["ItemData"] != null && context.Request.Params["ItemData"] != "")
                        {
                            postf = JsonConvert.DeserializeObject<PostForm.BookingPost>(context.Request.Params["ItemData"]);
                            if (postf.VerCode != null && postf.VerCode != "")
                            {
                                VerCode = postf.VerCode;
                                ResponseWriteEnd(context, SearchStore2(Setting, VerCode));
                            }
                            else
                            {
                                ResponseWriteEnd(context, ErrorMsg("error", "VerCode必填", Setting));
                            }
                        }
                        else 
                        {
                            ResponseWriteEnd(context, ErrorMsg("error", "ItemData必填", Setting));
                        }
                    }
                    else
                    {
                        ResponseWriteEnd(context, ErrorMsg("error", "error:0", Setting));        //驗證碼錯誤
                    }
                    #endregion
                    break;
                case "5":
                    #region 查詢所有門市(StoreID)
                    if (GS.MD5Check(SiteID + OrgName, ChkM))
                    {
                        if (context.Request.Params["ItemData"] != null && context.Request.Params["ItemData"] != "")
                        {
                            postf = JsonConvert.DeserializeObject<PostForm.BookingPost>(context.Request.Params["ItemData"]);
                            if (postf.StoreID != null && postf.StoreID != "") StoreID = postf.StoreID;
                        }
                        ResponseWriteEnd(context, SearchStore(Setting, StoreID));
                    }
                    else
                    {
                        ResponseWriteEnd(context, ErrorMsg("error", "error:0", Setting));        //驗證碼錯誤
                    }
                    #endregion
                    break;
            }
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

            Library.Products.RootObject root = new Library.Products.RootObject();
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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "查詢門市資料"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " store.ashx"));

                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region 查詢所屬門市(VerCode)
        private String SearchStore2(String Setting, String VerCode)
        {
            booking.root2 root = new booking.root2();

            if (VerCode != "")
            {                
                List<booking.BookingStore> BStores = new List<booking.BookingStore>();
                GetStr GS = new GetStr();
                
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand("select storeid from device where stat='Y' and VerCode=@VerCode and Orgname=@Orgname", conn);
                    cmd.Parameters.Add(new SqlParameter("@VerCode", VerCode)); 
                    cmd.Parameters.Add(new SqlParameter("@Orgname", GS.GetOrgName(Setting)));                    
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                using (SqlConnection conn2 = new SqlConnection(Setting))
                                {
                                    conn2.Open();
                                    SqlCommand cmd2;
                                    
                                    cmd2 = new SqlCommand("select id,title from bookingStore where disp_opt='Y' and id=@id", conn2);
                                    cmd2.Parameters.Add(new SqlParameter("@id", reader[0].ToString()));
                                    
                                    SqlDataReader reader2 = cmd2.ExecuteReader();
                                    try
                                    {
                                        if (reader2.HasRows)
                                        {
                                            while (reader2.Read())
                                            {
                                                booking.BookingStore BStoreList = new booking.BookingStore()
                                                {
                                                    StoreID = reader2[0].ToString(),
                                                    StoreTitle = reader2[1].ToString(),
                                                    BookingSeat = null
                                                };
                                                BStores.Add(BStoreList);
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        reader2.Close();
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
                root.BookingStore = BStores;
                return JsonConvert.SerializeObject(root);
            }
            else 
            {
                return JsonConvert.SerializeObject(root);
            }
        }
        #endregion

        #region 查詢所有門市(StoreID)
        private String SearchStore(String Setting, String StoreID)
        {
            booking.root2 root = new booking.root2();
            List<booking.BookingStore> BStores = new List<booking.BookingStore>();

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                if (StoreID != "")
                {
                    cmd = new SqlCommand("select id,title from bookingStore where disp_opt='Y' and id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", StoreID));
                }
                else
                {
                    cmd = new SqlCommand("select id,title from bookingStore where disp_opt='Y'", conn);
                }
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            booking.BookingStore BStoreList = new booking.BookingStore()
                            {
                                StoreID = reader[0].ToString(),
                                StoreTitle = reader[1].ToString(),
                                BookingSeat = null
                            };
                            BStores.Add(BStoreList);
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            root.BookingStore = BStores;
            return JsonConvert.SerializeObject(root);
        }
        #endregion
    }
}