using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Web.Configuration;
using System.Net;
using System.IO;

namespace ECSSO.api
{
    /// <summary>
    /// Logistics 的摘要描述
    /// </summary>
    public class Logistics : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Params["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", ""));
            if (context.Request.Params["OrderID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填", ""));
            if (context.Request.Params["SerNo"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SerNo必填", ""));

            if (context.Request.Params["CheckM"] == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Params["SiteID"] == "") ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", ""));
            if (context.Request.Params["OrderID"] == "") ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填", ""));
            if (context.Request.Params["SerNo"] == "") ResponseWriteEnd(context, ErrorMsg("error", "SerNo必填", ""));

            String CheckM = context.Request.Params["CheckM"];
            String SiteID = context.Request.Params["SiteID"];
            String OrderID = context.Request.Params["OrderID"];
            String SerNo = context.Request.Params["SerNo"];

            GetStr GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);
            String OrgName = GS.GetOrgName(Setting);

            if (GS.MD5Check(SiteID + OrgName + OrderID, CheckM))
            {
                String AllpayUrl = "https://logistics-stage.ecpay.com.tw/Express/Create";
                String MerchantID = "";
                String HashKey = "";
                String HashIV = "";
                String SenderName = "";
                String SenderCellPhone = "";
                String SenderZipCode = "";
                String SenderAddress = "";

                /*orders_Logistics資料*/
                String LogisticsType = "";
                String LogisticsSubType = "";
                String GoodsAmount = "";
                String Temperature = "";
                String Specification = "";
                String Distance = "";
                String ReceiverStoreID = "";        //收件人門市代號
                String GoodsName = "";
                String CollectionAmount = "0";
                String IsCollection = "N";

                /*orders_hd資料*/
                String ReceiverName = "";
                String ReceiverCellPhone = "";
                String ReceiverZipCode = "";
                String ReceiverAddress = "";

                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("select b.ecpay_mer_id,b.ecpay_HashKey,b.ecpay_HashIV,Logistics_SenderName,Logistics_SenderCellPhone,Logistics_SenderZipCode,Logistics_SenderAddress,ecpay_logistics_url from CurrentUseFrame as a left join head as b on a.id=b.hid", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                MerchantID = reader[0].ToString();
                                HashKey = reader[1].ToString();
                                HashIV = reader[2].ToString();
                                SenderName = reader[3].ToString();
                                SenderCellPhone = reader[4].ToString();
                                SenderZipCode = reader[5].ToString();
                                SenderAddress = reader[6].ToString();
                                AllpayUrl = "https://" + reader[7].ToString() + "/Express/Create";
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }

                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("select ship_zip,addr,cell,name from orders_hd where id=@orderNo", conn);
                    cmd.Parameters.Add(new SqlParameter("@orderNo", OrderID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ReceiverName = reader["name"].ToString();
                                ReceiverCellPhone = reader["cell"].ToString();
                                ReceiverZipCode = reader["ship_zip"].ToString();
                                ReceiverAddress = reader["addr"].ToString();
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }

                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("select LogisticsType,LogisticsSubType,GoodsAmount,Temperature,Specification,Distance,ReceiverStoreID,GoodsName,CollectionAmount,IsCollection from orders_Logistics where order_no=@orderNo and ser_no=@ser_no", conn);
                    cmd.Parameters.Add(new SqlParameter("@orderNo", OrderID));
                    cmd.Parameters.Add(new SqlParameter("@ser_no", SerNo));   
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                LogisticsType = reader["LogisticsType"].ToString();
                                LogisticsSubType = reader["LogisticsSubType"].ToString();
                                GoodsAmount = reader["GoodsAmount"].ToString();
                                Temperature = reader["Temperature"].ToString();
                                Specification = reader["Specification"].ToString();
                                Distance = reader["Distance"].ToString();
                                ReceiverStoreID = reader["ReceiverStoreID"].ToString();
                                GoodsName = reader["GoodsName"].ToString();
                                CollectionAmount = reader["CollectionAmount"].ToString();
                                IsCollection = reader["IsCollection"].ToString();
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }

                
                
                SortedDictionary<string, string> testStr = new SortedDictionary<string, string>();
                testStr.Add("MerchantID", MerchantID);
                testStr.Add("MerchantTradeNo", OrderID + SiteID + SerNo);
                testStr.Add("MerchantTradeDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                testStr.Add("LogisticsType", LogisticsType);//宅配
                testStr.Add("LogisticsSubType", LogisticsSubType);//TCAT:黑貓 ECAN:宅配通         
                testStr.Add("GoodsAmount", GoodsAmount);
                testStr.Add("ServerReplyURL", WebConfigurationManager.AppSettings["Protocol"] + "://" + WebConfigurationManager.AppSettings["Server_Host"] + "/api/returnLogistics.ashx");
                testStr.Add("ClientReplyURL", WebConfigurationManager.AppSettings["Protocol"] + "://" + WebConfigurationManager.AppSettings["Server_Host"] + "/api/returnLogistics.ashx");
                testStr.Add("SenderName", SenderName);
                testStr.Add("SenderCellPhone", SenderCellPhone);
                testStr.Add("ReceiverName", ReceiverName);
                testStr.Add("ReceiverCellPhone", ReceiverCellPhone);

                //超商取貨
                testStr.Add("ReceiverStoreID", ReceiverStoreID);

                //貨到付款
                testStr.Add("CollectionAmount", CollectionAmount);
                testStr.Add("IsCollection", IsCollection);

                ////宅配
                testStr.Add("SenderZipCode", SenderZipCode);
                testStr.Add("SenderAddress", SenderAddress);
                testStr.Add("ReceiverZipCode", ReceiverZipCode);
                testStr.Add("ReceiverAddress", ReceiverAddress);
                testStr.Add("Temperature", Temperature);
                testStr.Add("Distance", Distance);
                testStr.Add("Specification", Specification);
                //testStr.Add("ScheduledPickupTime", "1");
                //testStr.Add("ScheduledDeliveryTime", "3");

                //店到店
                testStr.Add("GoodsName", GoodsName);
                testStr.Add("LogisticsC2CReplyURL", WebConfigurationManager.AppSettings["Protocol"] + "://" + WebConfigurationManager.AppSettings["Server_Host"] + "/api/returnLogistics.ashx");

                ////宅配通
                //testStr.Add("ScheduledDeliveryDate", "");
                testStr.Add("PackageCount", "3");


                string str = string.Empty;
                string str_pre = string.Empty;
                foreach (var test in testStr)
                {
                    str += string.Format("&{0}={1}", test.Key, test.Value);
                }
                str_pre += "HashKey=" + HashKey + str + "&HashIV=" + HashIV;//2000132
                string urlEncodeStrPost = HttpUtility.UrlEncode(str_pre);
                string ToLower = urlEncodeStrPost.ToLower();
                MD5 md5Hasher = MD5.Create();

                byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(ToLower));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("X2"));//MD5碼 大小寫
                }
                string sCheckMacValue = sBuilder.ToString();

                //GetStr GS = new GetStr();
                //String Param = "";


                testStr.Add("CheckMacValue", sCheckMacValue);

                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                //sb.Append("<html><body>").AppendLine();
                sb.Append("<form name='allpayTest'  id='allpayTest' action='" + AllpayUrl + "' method='POST'>").AppendLine();
                foreach (var aa in testStr)
                {
                    //Param = Param + aa.Key + "=" + aa.Value + "&";
                    sb.Append("<input type='hidden' name='" + aa.Key + "' value='" + aa.Value + "'>").AppendLine();
                }

                sb.Append("</form>").AppendLine();
                //sb.Append("<script> var theForm = document.forms['allpayTest'];  if (!theForm) { theForm = document.allpayTest; } theForm.submit(); </script>").AppendLine();
                //sb.Append("<html><body>").AppendLine();

                context.Response.Write(sb.ToString());
                //context.Response.Write(postTo(AllpayUrl, testStr));
            }
            else 
            {
                ResponseWriteEnd(context, ErrorMsg("error", "CheckM error", Setting));
            }
        }
        private string postTo(string uri,SortedDictionary<string, string> pairs)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                string postData = "";
                foreach (var pair in pairs)
                {
                    postData += "&" + pair.Key + "=" + pair.Value;
                }
                if (postData != "") postData = postData.Substring(1, postData.Length-1);
                //Console.WriteLine("postData:" + postData);
                //context.Response.End();
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();

                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();

                dataStream = response.GetResponseStream();
                StreamReader reader2 = new StreamReader(dataStream);
                string responseFromServer = reader2.ReadToEnd();

                reader2.Close();
                dataStream.Close();
                response.Close();
                return responseFromServer;
            }
            catch (WebException even)
            {
                using (StreamReader sr =
                    new StreamReader(even.Response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public class RootObject
        {
            public string RspnCode { get; set; }
            public string RspnMsg { get; set; }
        }

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "Logistics error", "", RspnMsg);
            }

            RootObject root = new RootObject();
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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "物流"));
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