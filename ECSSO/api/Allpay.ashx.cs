using System;
using System.Collections.Generic;
using System.Web;
using AllPay.Payment.Integration;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Newtonsoft.Json;

namespace ECSSO.api
{
    /// <summary>
    /// Allpay 的摘要描述
    /// </summary>
    public class Allpay : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Form["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Form["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", ""));
            if (context.Request.Form["OrderID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填", ""));

            if (context.Request.Form["SiteID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填", ""));
            if (context.Request.Form["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Form["OrderID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填", ""));

            GetStr GS = new GetStr();
            String ChkM = context.Request.Form["CheckM"].ToString();
            String SiteID = context.Request.Form["SiteID"].ToString();
            String OrderID = context.Request.Form["OrderID"].ToString();

            String Setting = GS.GetSetting2(SiteID);
            String OrgName = GS.GetOrgName(Setting);

            if (GS.MD5Check(SiteID + OrgName + OrderID, ChkM))
            {
                String MerID = string.Empty;
                String HashKey = string.Empty;
                String HashIV = string.Empty;
                //String AllpayURL = "https://payment.allpay.com.tw";
                String AllpayURL = "https://payment.ecpay.com.tw";
                String MerchantTradeNo = string.Empty;
                String TradeNo = string.Empty;
                String TotalAmount = string.Empty;
                String BonusDiscount = string.Empty;
                String MemID = string.Empty;
                String OrderState = string.Empty;
                String PayType = string.Empty;

                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select id,mer_id,hashkey,hashiv from head", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                MerchantTradeNo = OrderID + reader[0].ToString();
                                MerID = reader[1].ToString();
                                HashKey = reader[2].ToString();
                                HashIV = reader[3].ToString();
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
                    SqlCommand cmd = new SqlCommand("select tsr,convert(int,amt)+convert(int,freightamount)-convert(int,bonus_discount)-convert(int,discount_amt),isnull(bonus_discount,'') as bonus_discount,mem_id,state,payment_type from orders_hd where id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", OrderID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                TradeNo = reader[0].ToString();
                                TotalAmount = reader[1].ToString();
                                BonusDiscount = reader[2].ToString();
                                if (BonusDiscount == "")
                                {
                                    BonusDiscount = "0";
                                }
                                MemID = reader[3].ToString();
                                OrderState = reader[4].ToString();
                                PayType = reader[5].ToString();
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }

                String Result = "";
                if (OrderState == "2" && PayType == "Credit" && TradeNo != "")
                {
                    Result = DoAction_N(MerID, HashKey, HashIV, AllpayURL, MerchantTradeNo, TradeNo, TotalAmount);

                    if (Result == "success")
                    {
                        DeleteOrder(Setting, OrderID, MemID, BonusDiscount);
                    }
                    ResponseWriteEnd(context, ErrorMsg("success", Result, Setting));
                }
                else
                {
                    Result = "必須為信用卡已付款才可刷退";
                }
                ResponseWriteEnd(context, ErrorMsg("error", Result, Setting));
            }
            else
            {
                ResponseWriteEnd(context, ErrorMsg("error", "驗證碼錯誤", Setting));
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

        #region 設定放棄刷卡(尚未關帳時可操作)
        private String DoAction_N(String MerID, String HashKey, String HashIV, String AllpayURL, String MerchantTradeNo, String TradeNo, String TotalAmount)
        {
            /*
            * 信用卡關帳/退刷/取消/放棄的範例程式碼。
            */
            List<string> enErrors = new List<string>();
            Hashtable htFeedback = null;
            String ReturnStr = "";

            try
            {
                using (AllInOne oPayment = new AllInOne())
                {
                    /* 服務參數 */
                    oPayment.ServiceMethod = HttpMethod.ServerPOST;
                    oPayment.ServiceURL = AllpayURL + "/CreditDetail/DoAction";
                    oPayment.HashKey = HashKey;
                    oPayment.HashIV = HashIV;
                    oPayment.MerchantID = MerID;
                    /* 基本參數 */
                    oPayment.Action.MerchantTradeNo = MerchantTradeNo;
                    oPayment.Action.TradeNo = TradeNo;
                    oPayment.Action.Action = ActionType.N;
                    oPayment.Action.TotalAmount = Decimal.Parse(TotalAmount);
                    enErrors.AddRange(oPayment.DoAction(ref htFeedback));
                }
                if (enErrors.Count() == 0)
                {
                    /* 執行後的回傳的基本參數 */
                    string szMerchantID = String.Empty;
                    string szMerchantTradeNo = String.Empty;
                    string szTradeNo = String.Empty;
                    string szRtnCode = String.Empty;
                    string szRtnMsg = String.Empty;
                    // 取得資料於畫面
                    foreach (string szKey in htFeedback.Keys)
                    {
                        switch (szKey)
                        {
                            /* 執行後的回傳的基本參數 */
                            case "MerchantID": szMerchantID = htFeedback[szKey].ToString(); break;
                            case "MerchantTradeNo": szMerchantTradeNo = htFeedback[szKey].ToString();
                                break;
                            case "TradeNo": szTradeNo = htFeedback[szKey].ToString(); break;
                            case "RtnCode": szRtnCode = htFeedback[szKey].ToString(); break;
                            case "RtnMsg": szRtnMsg = htFeedback[szKey].ToString(); break;
                            default: break;
                        }
                    }
                    // 其他資料處理。
                    // ………
                    if (szRtnCode == "1")   //交易成功
                    {
                        ReturnStr = "success";
                    }
                    else  //失敗,丟設定刷退
                    {
                        ReturnStr = DoAction_N(MerID, HashKey, HashIV, AllpayURL, MerchantTradeNo, TradeNo, TotalAmount);
                    }
                }
            }
            finally
            {
                // 顯示錯誤訊息。
                if (enErrors.Count() > 0)
                {
                    ReturnStr = String.Join("\\r\\n", enErrors);
                }
            }

            return ReturnStr;
        }
        #endregion

        #region 設定刷退(關帳時使用)
        private String DoAction_R(String MerID, String HashKey, String HashIV, String AllpayURL, String MerchantTradeNo, String TradeNo, String TotalAmount)
        {
            /*
            * 信用卡關帳/退刷/取消/放棄的範例程式碼。
            */
            List<string> enErrors = new List<string>();
            Hashtable htFeedback = null;
            String ReturnStr = "";

            try
            {
                using (AllInOne oPayment = new AllInOne())
                {
                    /* 服務參數 */
                    oPayment.ServiceMethod = HttpMethod.ServerPOST;
                    oPayment.ServiceURL = AllpayURL + "/CreditDetail/DoAction";
                    oPayment.HashKey = HashKey;
                    oPayment.HashIV = HashIV;
                    oPayment.MerchantID = MerID;
                    /* 基本參數 */
                    oPayment.Action.MerchantTradeNo = MerchantTradeNo;
                    oPayment.Action.TradeNo = TradeNo;
                    oPayment.Action.Action = ActionType.R;
                    oPayment.Action.TotalAmount = Decimal.Parse(TotalAmount);
                    enErrors.AddRange(oPayment.DoAction(ref htFeedback));
                }
                if (enErrors.Count() == 0)
                {
                    /* 執行後的回傳的基本參數 */
                    string szMerchantID = String.Empty;
                    string szMerchantTradeNo = String.Empty;
                    string szTradeNo = String.Empty;
                    string szRtnCode = String.Empty;
                    string szRtnMsg = String.Empty;
                    // 取得資料於畫面
                    foreach (string szKey in htFeedback.Keys)
                    {
                        switch (szKey)
                        {
                            /* 執行後的回傳的基本參數 */
                            case "MerchantID": szMerchantID = htFeedback[szKey].ToString(); break;
                            case "MerchantTradeNo": szMerchantTradeNo = htFeedback[szKey].ToString();
                                break;
                            case "TradeNo": szTradeNo = htFeedback[szKey].ToString(); break;
                            case "RtnCode": szRtnCode = htFeedback[szKey].ToString(); break;
                            case "RtnMsg": szRtnMsg = htFeedback[szKey].ToString(); break;
                            default: break;
                        }
                    }
                    // 其他資料處理。
                    // ………
                    if (szRtnCode == "1")   //交易成功
                    {
                        ReturnStr = "success";
                    }
                }
            }
            finally
            {
                // 顯示錯誤訊息。
                if (enErrors.Count() > 0)
                {
                    ReturnStr = String.Join("\\r\\n", enErrors);
                }
            }
            return ReturnStr;
        }
        #endregion

        #region 作廢訂單
        private void DeleteOrder(String Setting, String MerchantTradeNo, String MemberID, String BonusDiscount) 
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "UpdateOrderState";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@id", MerchantTradeNo));
                cmd.Parameters.Add(new SqlParameter("@State", "4"));
                cmd.ExecuteNonQuery();
            }


            if (MemberID != "")
            {
                Int32 ProdBonus = 0;
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select isnull(sum(bonus),0) from orders where order_no=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", MerchantTradeNo));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read()) ProdBonus = Convert.ToInt32(reader[0].ToString());
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
                ProdBonus = ProdBonus + Convert.ToInt32(BonusDiscount);

                #region 更新會員紅利
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "sp_UpdateOrder";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@orderID", MerchantTradeNo));
                    cmd.Parameters.Add(new SqlParameter("@bonus_memo", MerchantTradeNo + "付款失敗"));
                    cmd.Parameters.Add(new SqlParameter("@mem_id", MemberID));
                    cmd.Parameters.Add(new SqlParameter("@bonus_add", ProdBonus));
                    cmd.Parameters.Add(new SqlParameter("@user_id", "guest"));
                    cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                    cmd.Parameters.Add(new SqlParameter("@filename", "allpay.ashx"));
                    cmd.Parameters.Add(new SqlParameter("@type", "2"));
                    cmd.Parameters.Add(new SqlParameter("@bonus_spend", "0"));
                    cmd.Parameters.Add(new SqlParameter("@bonus_total_add", ProdBonus));
                    cmd.ExecuteNonQuery();
                }
                #endregion

                #region 更新庫存
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select qty,productid,colorid,sizeid from orders where order_no=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", MerchantTradeNo));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Stock(reader[0].ToString(), reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), Setting);
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
                #endregion
            }
        }
        #endregion

        #region 取得IP
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

        #region 更新庫存
        private void Stock(String stock, String prod_id, String colorid, String sizeid, String Setting)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_stocks2";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@prod_id", prod_id));
                cmd.Parameters.Add(new SqlParameter("@qty", stock));
                cmd.Parameters.Add(new SqlParameter("@prod_color", colorid));
                cmd.Parameters.Add(new SqlParameter("@prod_size", sizeid));
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        public class ErrorObject
        {
            public string RspnCode { get; set; }
            public string RspnMsg { get; set; }
            public string Pno { get; set; }
        }

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "bill error", "", RspnMsg);
            }

            ErrorObject root = new ErrorObject();
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
                cmd.Parameters.Add(new SqlParameter("@prog_name", "刷退"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " Allpay.ashx"));

                cmd.ExecuteNonQuery();
            }
        }
        #endregion
    }
}