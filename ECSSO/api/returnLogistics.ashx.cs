using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace ECSSO.api
{
    /// <summary>
    /// returnLogistics 的摘要描述
    /// </summary>
    public class returnLogistics : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            String MerchantID = context.Request.Params["MerchantID"];                   //廠商編號
            String MerchantTradeNo = context.Request.Params["MerchantTradeNo"];         //廠商交易編號
            String RtnCode = context.Request.Params["RtnCode"];                         //目前物流狀態
            String RtnMsg = context.Request.Params["RtnMsg"];                           //物流狀態說明
            String AllPayLogisticsID = context.Request.Params["AllPayLogisticsID"];     //AllPay的物流交易編號
            String LogisticsType = context.Request.Params["LogisticsType"];             //物流類型
            String LogisticsSubType = context.Request.Params["LogisticsSubType"];       //物流子類型
            String GoodsAmount = context.Request.Params["GoodsAmount"];                 //商品金額
            String UpdateStatusDate = context.Request.Params["UpdateStatusDate"];       //物流狀態更新時間 yyyy/MM/dd HH:mm:ss
            String ReceiverName = context.Request.Params["ReceiverName"];               //收件人姓名
            String ReceiverPhone = context.Request.Params["ReceiverPhone"];             //收件人電話
            String ReceiverCellPhone = context.Request.Params["ReceiverCellPhone"];     //收件人手機
            String ReceiverEmail = context.Request.Params["ReceiverEmail"];             //收件人 email
            String ReceiverAddress = context.Request.Params["ReceiverAddress"];         //收件人地址
            String CVSPaymentNo = "";                                                   //寄貨編號:若超商取貨統一超商、全家超商為店到店，則會回傳
            String CVSValidationNo = "";                                                //驗證碼:若超商取貨為統一超商店到店，則會回傳
            String BookingNote = "";                                                    //托運單號
            String CheckMacValue = context.Request.Params["CheckMacValue"];

            if (context.Request.Params["CVSPaymentNo"] != null) CVSPaymentNo = context.Request.Params["CVSPaymentNo"];
            if (context.Request.Params["CVSValidationNo"] != null) CVSValidationNo = context.Request.Params["CVSValidationNo"];
            if (context.Request.Params["BookingNote"] != null) BookingNote = context.Request.Params["BookingNote"];
            



            String HashKey = "";
            String HashIV = "";

            String OrderID = MerchantTradeNo.Substring(0,9);
            String SiteID = MerchantTradeNo.Substring(9, MerchantTradeNo.Length - 12);
            String SerNo = MerchantTradeNo.Substring(MerchantTradeNo.Length - 3, 3);
            GetStr GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("select b.mer_id,b.HashKey,b.HashIV from CurrentUseFrame as a left join head as b on a.id=b.hid", conn);
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
            testStr.Add("MerchantTradeNo", MerchantTradeNo);
            testStr.Add("RtnCode", RtnCode);
            testStr.Add("RtnMsg", RtnMsg);
            testStr.Add("AllPayLogisticsID", AllPayLogisticsID);
            testStr.Add("LogisticsType", LogisticsType);
            testStr.Add("LogisticsSubType", LogisticsSubType);
            testStr.Add("GoodsAmount", GoodsAmount);
            testStr.Add("UpdateStatusDate", UpdateStatusDate);
            testStr.Add("ReceiverName", ReceiverName);
            testStr.Add("ReceiverPhone", ReceiverPhone);
            testStr.Add("ReceiverCellPhone", ReceiverCellPhone);
            testStr.Add("ReceiverEmail", ReceiverEmail);
            testStr.Add("ReceiverAddress", ReceiverAddress);
            testStr.Add("CVSPaymentNo", CVSPaymentNo);
            testStr.Add("CVSValidationNo", CVSValidationNo);
            testStr.Add("BookingNote", BookingNote);
            
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
            
            if (sCheckMacValue == CheckMacValue) 
            { 
                //驗證碼正確
                
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "Insert_orders_Logistics_Log";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@AllPayLogisticsID", AllPayLogisticsID));
                    cmd.Parameters.Add(new SqlParameter("@RtnCode", RtnCode));
                    cmd.Parameters.Add(new SqlParameter("@RtnMsg", RtnMsg));
                    cmd.Parameters.Add(new SqlParameter("@UpdateStatusDate", UpdateStatusDate));
                    cmd.Parameters.Add(new SqlParameter("@OrderID", OrderID));
                    cmd.Parameters.Add(new SqlParameter("@OrderSerNo", SerNo));
                    cmd.Parameters.Add(new SqlParameter("@CVSPaymentNo", CVSPaymentNo));
                    cmd.Parameters.Add(new SqlParameter("@CVSValidationNo", CVSValidationNo));
                    cmd.Parameters.Add(new SqlParameter("@BookingNote", BookingNote));
                    cmd.ExecuteNonQuery();
                }
                ResponseWriteEnd(context, RtnCode);
            }
            else
            {
                //驗證碼錯誤
                ResponseWriteEnd(context, "驗證碼錯誤");
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

    }
}