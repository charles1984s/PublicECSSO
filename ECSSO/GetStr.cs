using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using ECSSO.Library;

namespace ECSSO
{
    public class GetStr
    {
        private string str = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private string[] byteUnit = { "Byte", "K", "M", "G", "T" };
        #region 取得DB連結字串
        public String GetSetting(String SiteID)
        {
            String Str_Return = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select dbname,dbusername,CONVERT(nvarchar(50), dbpassword) dbpassword from cocker_cust where id=@id", conn);
                int u = 0;
                if (int.TryParse(SiteID, out u))
                {
                    cmd.Parameters.Add(new SqlParameter("@id", SiteID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Str_Return = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=" + reader["dbusername"].ToString() + "; password=" + reader["dbpassword"].ToString() + "; database=" + reader["dbname"].ToString();
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            return Str_Return;
        }
        #endregion

        #region 取得DB連結字串(for XML)
        public String GetSettingForChecked(String SiteID)
        {
            String Str_Return = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select dbname,dbusername,CONVERT(nvarchar(50), dbpassword) dbpassword from cocker_cust where id=@id and stat='Y' ", conn);
                int u = 0;
                if (int.TryParse(SiteID, out u))
                {
                    cmd.Parameters.Add(new SqlParameter("@id", SiteID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Str_Return = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=" + reader["dbusername"].ToString() + "; password=" + reader["dbpassword"].ToString() + "; database=" + reader["dbname"].ToString();
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            return Str_Return;
        }
        #endregion

        #region 取得DB連結字串(for 管理)
        public String GetSetting2(String SiteID)
        {
            String Str_Return = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select dbname,dbusername,CONVERT(nvarchar(50), dbpassword) dbpassword from cocker_cust where id=@id", conn);
                int u = 0;
                if (int.TryParse(SiteID, out u))
                {
                    cmd.Parameters.Add(new SqlParameter("@id", SiteID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Str_Return = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_" + reader["dbusername"].ToString() + "; password=i_" + reader["dbpassword"].ToString() + "; database=" + reader["dbname"].ToString();
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            return Str_Return;
        }
        #endregion

        #region 取得DB連結字串 By orgName (for 管理)
        public String GetSetting3(String orgName)
        {
            String Str_Return = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select dbname,dbusername,CONVERT(nvarchar(50), dbpassword) dbpassword from cocker_cust where crm_org=@orgName", conn);
                cmd.Parameters.Add(new SqlParameter("@orgName", orgName));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Str_Return = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_" + reader["dbusername"].ToString() + "; password=i_" + reader["dbpassword"].ToString() + "; database=" + reader["dbname"].ToString() + ";Max Pool Size=30000;Connection Timeout=180";
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return Str_Return;
        }
        #endregion

        #region 確認talken是否有效
        public string checkToken(TokenItem obj)
        {
            string setting = "";
            string setting2 = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();
            using (SqlConnection conn = new SqlConnection(setting2))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from talken where talken=@talken and ip=@ip and DateDiff(MINUTE,GETDATE(),CONVERT(datetime,end_time))>=0", conn);
                cmd.Parameters.Add(new SqlParameter("@talken", obj.token));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        setting = GetSetting3(reader["orgName"].ToString());
                        using (SqlConnection conn2 = new SqlConnection(setting2))
                        {
                            conn2.Open();
                            SqlCommand cmd2 = new SqlCommand();
                            cmd2.CommandText = "sp_updateTalken";
                            cmd2.CommandType = CommandType.StoredProcedure;
                            cmd2.Connection = conn2;
                            cmd2.Parameters.Add(new SqlParameter("@orgname", reader["orgName"].ToString()));
                            cmd2.Parameters.Add(new SqlParameter("@userid", reader["ManagerID"].ToString()));
                            cmd2.Parameters.Add(new SqlParameter("@talken", obj.token));
                            cmd2.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                            SqlParameter SPOutput = cmd2.Parameters.Add("@ReturnCode", SqlDbType.NVarChar, 7);
                            SPOutput.Direction = ParameterDirection.Output;
                            string ReturnCode = null;
                            try
                            {
                                cmd2.ExecuteNonQuery();
                                ReturnCode = SPOutput.Value.ToString();
                                obj.id = reader["ManagerID"].ToString();
                                obj.orgName = reader["orgName"].ToString();
                            }
                            catch { setting = "error1"; }
                        }
                    }
                }
                catch (Exception ex) { setting = "error2" + ex.Message; }
            }
            return setting;
        }
        public string checkToken(string token)
        {
            string setting = "";
            string setting2 = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();
            using (SqlConnection conn = new SqlConnection(setting2))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from talken where talken=@talken and ip=@ip and DateDiff(MINUTE,GETDATE(),CONVERT(datetime,end_time))>=0", conn);
                cmd.Parameters.Add(new SqlParameter("@talken", token));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        setting = GetSetting3(reader["orgName"].ToString());
                        using (SqlConnection conn2 = new SqlConnection(setting2))
                        {
                            conn2.Open();
                            SqlCommand cmd2 = new SqlCommand();
                            cmd2.CommandText = "sp_updateTalken";
                            cmd2.CommandType = CommandType.StoredProcedure;
                            cmd2.Connection = conn2;
                            cmd2.Parameters.Add(new SqlParameter("@orgname", reader["orgName"].ToString()));
                            cmd2.Parameters.Add(new SqlParameter("@userid", reader["ManagerID"].ToString()));
                            cmd2.Parameters.Add(new SqlParameter("@talken", token));
                            cmd2.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                            SqlParameter SPOutput = cmd2.Parameters.Add("@ReturnCode", SqlDbType.NVarChar, 7);
                            SPOutput.Direction = ParameterDirection.Output;
                            string ReturnCode = null;
                            try
                            {
                                cmd2.ExecuteNonQuery();
                                ReturnCode = SPOutput.Value.ToString();
                            }
                            catch { setting = "error1"; }
                        }
                    }
                }
                catch (Exception ex) { setting = "error2" + ex.Message; }
            }
            return setting;
        }
        public string checkTokenByServer(TokenItem obj)
        {
            string setting = "";
            string setting2 = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();
            using (SqlConnection conn = new SqlConnection(setting2))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from talken where talken=@talken and DateDiff(MINUTE,GETDATE(),CONVERT(datetime,end_time))>=0", conn);
                cmd.Parameters.Add(new SqlParameter("@talken", obj.token));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        setting = GetSetting3(reader["orgName"].ToString());
                        using (SqlConnection conn2 = new SqlConnection(setting2))
                        {
                            conn2.Open();
                            SqlCommand cmd2 = new SqlCommand();
                            cmd2.CommandText = "sp_updateTalken";
                            cmd2.CommandType = CommandType.StoredProcedure;
                            cmd2.Connection = conn2;
                            cmd2.Parameters.Add(new SqlParameter("@orgname", reader["orgName"].ToString()));
                            cmd2.Parameters.Add(new SqlParameter("@userid", reader["ManagerID"].ToString()));
                            cmd2.Parameters.Add(new SqlParameter("@talken", obj.token));
                            cmd2.Parameters.Add(new SqlParameter("@ip", reader["ip"].ToString()));
                            SqlParameter SPOutput = cmd2.Parameters.Add("@ReturnCode", SqlDbType.NVarChar, 7);
                            SPOutput.Direction = ParameterDirection.Output;
                            string ReturnCode = null;
                            try
                            {
                                cmd2.ExecuteNonQuery();
                                ReturnCode = SPOutput.Value.ToString();
                                obj.id = reader["ManagerID"].ToString();
                                obj.orgName = reader["orgName"].ToString();
                            }
                            catch { setting = "error1"; }
                        }
                    }
                }
                catch (Exception ex) { setting = "error2" + ex.Message; }
            }
            return setting;
        }
        #endregion

        #region 取得預設網址
        public String GetDefaultURL(String SiteID)
        {
            String Str_Return = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select web_url from cocker_cust where id=@id", conn);
                int u = 0;
                if (int.TryParse(SiteID, out u))
                {
                    cmd.Parameters.Add(new SqlParameter("@id", SiteID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Str_Return = reader[0].ToString();
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            return Str_Return;
        }
        #endregion

        #region 取得預設網址 By orgName
        public String GetDefaultURL2(String OrgName)
        {
            String Str_Return = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select web_url from cocker_cust where crm_org = @crm_org;", conn);

                cmd.Parameters.Add(new SqlParameter("@crm_org", OrgName));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Str_Return = reader[0].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return Str_Return;
        }
        #endregion

        #region 取得預設完整網址
        public String GetAllLinkString(string orgName, string url, string Lng, string type)
        {
            Lng = GetLanString(Lng);
            string defaultLink = GetDefaultURL2(orgName);
            string linkString = (CheckStringIsNotNull(orgName) == "") ? "" : ((defaultLink.IndexOf("http") < 0) ? "http://" + defaultLink : defaultLink);
            switch (type)
            {
                case "Image":
                    {
                        linkString = (CheckStringIsNotNull(url) == "") ? "" : ((url.IndexOf("http") < 0) ? linkString + url : url);
                        break;
                    }
                case "Link":
                    {
                        linkString = (CheckStringIsNotNull(url) == "") ? "" : ((url.IndexOf("http") < 0) ? linkString + "/" + Lng + "/" + url : url);
                        break;
                    }
                default:
                    {
                        linkString = (CheckStringIsNotNull(url) == "") ? "" : ((url.IndexOf("http") < 0) ? linkString + "/" + Lng + url : url);
                        break;
                    }
            }
            return linkString;
        }
        #endregion

        #region 取得完整語系 By SiteID
        public string GetFullLanString(String SiteID)
        {
            String Str_Return = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select language from cocker_cust where id=@id", conn);
                int u = 0;
                if (int.TryParse(SiteID, out u))
                {
                    cmd.Parameters.Add(new SqlParameter("@id", SiteID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Str_Return = reader[0].ToString();
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            return Str_Return;
        }
        #endregion

        #region 取得完整語系 By orgName
        public string GetFullLanString2(String OrgName)
        {
            String Str_Return = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select web_url from cocker_cust where crm_org = @crm_org;", conn);

                cmd.Parameters.Add(new SqlParameter("@crm_org", OrgName));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Str_Return = reader[0].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return Str_Return;
        }
        #endregion

        #region 取得語系
        public string GetLanString(String Language)
        {
            String FolderStr = "";
            switch (Language.ToLower())
            {
                case "en-us":
                    FolderStr = "en";
                    break;
                case "zh-cn":
                    FolderStr = "cn";
                    break;
                case "zh-tw":
                    FolderStr = "tw";
                    break;
                case "vi-vn":
                    FolderStr = "vn";
                    break;
                case "ja-jp":
                    FolderStr = "jp";
                    break;
                default:
                    FolderStr = "tw";
                    break;
            }
            return FolderStr;
        }
        #endregion

        #region 取得付款方式
        public String GetPayType(string setting, String StrType)
        {
            String StrPayType = "";
            switch (StrType)
            {
                case "WebATM":
                    StrPayType = "WebATM";
                    break;
                case "POD":
                    StrPayType = "貨到付款";
                    break;
                case "ccb_credit":
                    StrPayType = "線上刷卡";
                    break;
                case "cbbInstallment3":
                    StrPayType = "線上刷卡3期分期付款";
                    break;
                case "cbbInstallment6":
                    StrPayType = "線上刷卡6期分期付款";
                    break;
                case "cbbInstallment12":
                    StrPayType = "線上刷卡12期分期付款";
                    break;
                case "cbbInstallment24":
                    StrPayType = "線上刷卡24期分期付款";
                    break;
                case "Credit":
                    StrPayType = "線上刷卡";
                    break;
                case "Installment3":
                    StrPayType = "線上刷卡3期分期付款";
                    break;
                case "Installment6":
                    StrPayType = "線上刷卡6期分期付款";
                    break;
                case "Installment12":
                    StrPayType = "線上刷卡12期分期付款";
                    break;
                case "Installment24":
                    StrPayType = "線上刷卡24期分期付款";
                    break;
                case "CVS":
                    StrPayType = "超商繳費";
                    break;
                case "Tenpay":
                    StrPayType = "財付通";
                    break;
                case "Alipay":
                    StrPayType = "支付寶";
                    break;
                case "BARCODE":
                    StrPayType = "超商條碼";
                    break;
                case "ATM":
                    StrPayType = "虛擬帳號(ATM轉帳)";
                    break;
                case "getandpay":
                    StrPayType = "貨到付款";
                    break;
                case "ezShip":
                    StrPayType = "超商取貨付款";
                    break;
                case "esafeWebatm":
                    StrPayType = "WebATM";
                    break;
                case "esafeCredit":
                    StrPayType = "線上刷卡";
                    break;
                case "esafePay24":
                    StrPayType = "超商代收";
                    break;
                case "esafePaycode":
                    StrPayType = "超商代碼付款";
                    break;
                case "esafeAlipay":
                    StrPayType = "支付寶";
                    break;
                case "chtHinet":
                    StrPayType = "Hinet帳單";
                    break;
                case "chteCard":
                    StrPayType = "Hinet點數卡";
                    break;
                case "chtld":
                    StrPayType = "行動839";
                    break;
                case "Chtn":
                    StrPayType = "市話輕鬆付";
                    break;
                case "chtCredit":
                    StrPayType = "線上刷卡";
                    break;
                case "chtATM":
                    StrPayType = "虛擬帳號(ATM轉帳)";
                    break;
                case "chtWEBATM":
                    StrPayType = "WebATM";
                    break;
                case "chtUniPresident":
                    StrPayType = "超商代收";
                    break;
                case "chtAlipay":
                    StrPayType = "支付寶";
                    break;
                case "chinatrust_credit":
                    StrPayType = "線上刷卡";
                    break;
                case "focus":
                    StrPayType = "線上刷卡";
                    break;
                case "EsunCredit":
                    StrPayType = "線上刷卡";
                    break;
                case "ezship0":
                    StrPayType = "超商取貨付款";
                    break;
                case "ezship1":
                    StrPayType = "超商取貨不付款";
                    break;
                case "tcb_allpay":
                    StrPayType = "合作金庫支付寶";
                    break;
                case "fisc_Credit":
                    StrPayType = "線上刷卡";
                    break;
                case "mPP":
                    StrPayType = "線上刷卡";
                    break;
                case "ezpay_ATM":
                    StrPayType = "虛擬帳號(ATM轉帳)";
                    break;
                case "ezpay_WEBATM":
                    StrPayType = "WebATM";
                    break;
                case "ezpay_CS":
                    StrPayType = "超商代收";
                    break;
                case "ezpay_MMK":
                    StrPayType = "超商條碼繳費";
                    break;
                case "ezpay_ALIPAY":
                    StrPayType = "支付寶";
                    break;
                case "ezpay_ALIPAY_WAP":
                    StrPayType = "支付寶";
                    break;
                case "shop":
                    StrPayType = "門市付款";
                    break;
                case "POST":
                    StrPayType = "郵政劃撥";
                    break;
                case "NcccCredit":
                    StrPayType = "聯合信用卡付款";
                    break;
                case "NcccCUPCredit":
                    StrPayType = "聯合信用卡銀聯卡付款";
                    break;
                case "PchomePayCARD":
                    StrPayType = "信用卡付款";
                    break;
                case "PchomePayATM":
                    StrPayType = "ATM付款";
                    break;
                case "PchomePayACCT":
                    StrPayType = "支付連餘額付款";
                    break;
                case "PchomePayEACH":
                    StrPayType = "支付連銀行支付付款";
                    break;
                default:
                    if (StrType != "")
                    {
                        StrPayType = getThirdPayTitle(setting, StrType);
                    }
                    else StrPayType = "ATM";
                    break;
            }
            return StrPayType;
        }
        #endregion

        #region 取得第三方金流付款方式名稱
        private string getThirdPayTitle(string setting, string key)
        {
            string StrPayType = "ATM";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select title from paymenttype where code=@key", conn);
                cmd.Parameters.Add(new SqlParameter("@key", key));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.Read())
                    {
                        StrPayType = reader[0].ToString();
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return StrPayType;
        }
        #endregion

        #region 取得付款方式
        public bool CanResetPay(string paymenType)
        {
            bool re = false;
            switch (paymenType)
            {
                case "paypal":  //綠界 線上刷卡
                case "Credit":  //綠界 線上刷卡
                //case "web_atm":  //綠界 WEB-ATM
                //case "vacc":  //綠界  虛擬帳號
                //case "cvs":  //綠界  超商代碼
                //case "barcode":  //綠界  超商條碼
                case "Installment3":  //綠界  線上刷卡3期分期付款
                case "Installment6":  //綠界  線上刷卡6期分期付款
                case "Installment12":  //綠界  線上刷卡12期分期付款
                case "Installment24":  //綠界  線上刷卡24期分期付款
                case "PchomePayCARD":   //PchomePay(支付連) 信用卡
                case "PchomePayATM":    //PchomePay(支付連) ATM
                case "PchomePayACCT":   //PchomePay(支付連) 餘額付款
                case "PchomePayEACH":   //PchomePay(支付連) 銀行支付
                case "LinePay":
                    re = true;
                    break;
            }
            return re;
        }
        #endregion

        #region 取得產品顏色及尺寸
        public string GetSpec(String setting, String TableName, int SearchID)
        {
            String ReturnStr = "";
            String Str_sql = "select title from " + TableName + " where id=@id";
            try
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(Str_sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@id", SearchID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            ReturnStr = reader[0].ToString();
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            catch
            {
                ReturnStr = "";
            }


            return ReturnStr;
        }
        #endregion

        #region 判斷產品是否有顏色或尺寸
        public bool GetSpec(String setting, String TableName)
        {
            String Str_sql = "select * from dbo.prod_Stock where " + TableName + ">0";
            try
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(Str_sql, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            catch
            {
                return true;
            }
        }
        #endregion

        #region 訂單狀態
        public String GetOrderState(String ID)
        {

            String returnstr = "";
            switch (ID)
            {
                case "1":
                    returnstr = "處理審核中";
                    break;
                case "2":
                    returnstr = "已付款";
                    break;
                case "3":
                    returnstr = "已出貨";
                    break;
                case "4":
                    returnstr = "已取消";
                    break;
                case "5":
                    returnstr = "付款失敗";
                    break;
                case "6":
                    returnstr = "出貨中";
                    break;
                case "7":
                    returnstr = "已成立";
                    break;
                case "8":
                    returnstr = "修改";
                    break;
            }
            return returnstr;

        }
        #endregion        

        #region 取得規格名稱
        public string GetSpecTitle(String setting, String SearchID)
        {
            String ReturnStr = "-";
            String Str_sql = "select title from Specification where id=@id";
            try
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(Str_sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@id", SearchID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            ReturnStr = reader[0].ToString();
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            catch
            {
                ReturnStr = "-";
            }

            return ReturnStr;
        }
        #endregion

        #region 取得Orgname
        public string GetOrgName(String setting)
        {
            String ReturnStr = "";
            String Str_sql = "select crm_org from head";
            try
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(Str_sql, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            ReturnStr = reader[0].ToString();
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            catch
            {
                ReturnStr = "";
            }
            return ReturnStr;
        }
        #endregion

        #region 字串加*號
        public String Rename(String Str, Int16 Count)
        {
            String ReturnStr = "";
            for (int i = 0; i < Str.Length; i++)
            {
                if (i < Count)
                {
                    ReturnStr += Str.Substring(i, 1);
                }
                else
                {
                    ReturnStr += "*";
                }
            }
            return ReturnStr;
        }
        #endregion

        #region 字串保留前後幾碼加○號
        public String Rename2(String Str, int before, int end)
        {
            String ReturnStr = "";
            int e = Str.Length - end;
            if (e == before) e = Str.Length;
            for (int i = 0; i < Str.Length; i++)
            {
                if (i < before || i >= e)
                {
                    ReturnStr += Str.Substring(i, 1);
                }
                else
                {
                    ReturnStr += "○";
                }
            }
            return ReturnStr;
        }
        #endregion

        #region 字串過濾
        public String ReplaceStr(String Str)
        {

            Str = Str.Replace("<", "");
            Str = Str.Replace(">", "");
            Str = Str.Replace("&lt;", "");
            Str = Str.Replace("&gt;", "");

            return Str;
        }
        #endregion

        #region 字串預設空
        public string CheckStringIsNotNull(string TargetString)
        {
            if (String.IsNullOrEmpty(TargetString) || TargetString == "")
            {
                return "";
            }
            else
            {
                return TargetString;
            }
        }
        #endregion

        #region MD5加密
        public string MD5Endode(String Str)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                Str = GetMd5Hash(md5Hash, Str);
            }

            return Str;
        }
        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }
        #endregion

        #region MD5驗證
        public bool MD5Check(String Str, String MD5Str)
        {

            using (MD5 md5Hash = MD5.Create())
            {
                Str = GetMd5Hash(md5Hash, Str);
            }

            if (Str == MD5Str)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public string MD5Check(String Str)
        {
            string md5Str;
            using (MD5 md5Hash = MD5.Create())
            {
                md5Str = GetMd5Hash(md5Hash, Str);
            }
            return md5Str;
        }
        #endregion

        #region Base64加密
        //加密
        public string Base64Encode(string AStr)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(AStr));
        }
        #endregion

        #region Base64解密
        //解密
        public string Base64Decode(string ABase64)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(ABase64));
        }
        #endregion

        #region 號碼轉國碼
        public string InternationalPrefix(String PhoneNumber)
        {
            if (PhoneNumber == null || PhoneNumber.StartsWith("886")) return CheckStringIsNotNull(PhoneNumber);
            String[] pNumber = PhoneNumber.Split(new char[] { '#', '轉' });
            PhoneNumber = new System.Text.RegularExpressions.Regex(@"\D").Replace(pNumber[0], string.Empty).TrimStart('0');
            if (PhoneNumber.Length < 1) return "";
            String number = "";
            if (PhoneNumber.StartsWith("9"))
            {
                number = String.Format("886-{0}-{1}", PhoneNumber.Substring(0, 1), PhoneNumber.Substring(1));
            }
            else
            {
                number = String.Format("886-{0}-{1}", PhoneNumber.Substring(0, 1), PhoneNumber.Substring(1));
            }
            number = (pNumber.Length > 1) ? String.Format("{0}#{1}", number,
                    new System.Text.RegularExpressions.Regex(@"\D").Replace(pNumber[1], string.Empty)
                    ) : number;

            return number;
        }
        #endregion

        #region 字串轉unicode
        public string StringToUnicode(String Str)
        {
            String Dst = "";
            char[] src = Str.ToCharArray();
            for (int i = 0; i < src.Length; i++)
            {
                byte[] bytes = Encoding.Unicode.GetBytes(src[i].ToString());
                string str = @"\u" + bytes[1].ToString("X2") + bytes[0].ToString("X2");
                Dst += str;
            }
            return Dst;
        }
        #endregion

        #region 字串轉utf-8
        public string StringToUTF8(String Str)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            Byte[] encodedBytes = utf8.GetBytes(Str);
            return utf8.GetString(encodedBytes);
        }
        #endregion

        #region 字串轉htmlEcode
        public string HtmlEncode(string text)
        {
            char[] chars = text.ToCharArray();
            StringBuilder result = new StringBuilder(text.Length + (int)(text.Length * 0.1));

            foreach (char c in chars)
            {
                int value = Convert.ToInt32(c);
                if (value > 127)
                    result.AppendFormat("&#{0};", value);
                else
                    result.Append(c);
            }
            return result.ToString();
        }
        #endregion

        #region 字串轉數字
        public int StringToInt(string text, int def)
        {
            Regex NumberPattern = new Regex("^[0-9]*$");
            return string.IsNullOrEmpty(text) || !NumberPattern.IsMatch(text) ? def : int.Parse(text);
        }
        #endregion

        #region 數字轉byte單位
        public string toByte(double q, int c)
        {
            if (q > 1000) return toByte(q / 1000, ++c);
            else return q.ToString("#0.00") + byteUnit[c];
        }
        #endregion

        #region 取得生肖
        public String GetAnimal(String Year)
        {
            String[] Animal = { "鼠", "牛", "虎", "兔", "龍", "蛇", "馬", "羊", "猴", "雞", "狗", "豬" };

            int birthpet;
            try
            {
                birthpet = Convert.ToInt32(Year) % 12;           //西元年除12取餘數            
                birthpet = birthpet - 3;        //餘數-3            
                if (birthpet < 0)               //判斷餘數是否大於0，小於0必須+12
                {
                    birthpet = birthpet + 12;
                }

                return Animal[birthpet - 1].ToString();
            }
            catch
            {
                return "";
            }
        }
        #endregion

        #region 轉換農曆日期
        private String GetLDate(DateTime Date)
        {
            TaiwanLunisolarCalendar tlc = new TaiwanLunisolarCalendar();
            //tlc.MaxSupportedDateTime.ToShortDateString();       // 取得目前支援的農曆日曆到幾年幾月幾日( 2051-02-10 );
            // 取得今天的農曆年月日
            String LDate = tlc.GetYear(Date).ToString() + "/" + tlc.GetMonth(Date).ToString() + "/" + tlc.GetDayOfMonth(Date).ToString();
            return LDate;
        }
        #endregion

        #region 取得餐點狀態
        public String GetMealType(String TypeID)
        {

            switch (TypeID)
            {
                case "1":
                    return "內用";
                case "2":
                    return "外帶";
                case "3":
                    return "外送";
                default:
                    return "內用";
            }
        }
        #endregion

        #region 取得某模組的網址
        public string GetUseModuleURL(String setting, String moduleID)
        {
            String ReturnStr = "";
            String AuID = "";
            String SubID = "";
            #region 找模組
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select id,authors_id from menu_sub where use_module=@use_module and disp_opt='Y'", conn);
                cmd.Parameters.Add(new SqlParameter("@use_module", moduleID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            AuID = reader[1].ToString();
                            SubID = reader[0].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            #endregion

            #region 找父節點
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_MenuFindFatherNode", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@id", AuID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader["authors_id"].ToString() == "0")
                            {
                                AuID = reader["id"].ToString();
                            }
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            #endregion

            ReturnStr = "index.asp?au_id=" + AuID + "&sub_id=" + SubID;
            return ReturnStr;
        }
        #endregion

        #region 取得SMS資訊
        public String GetSMSErrorMsg(String Code)
        {
            String ReturnStr = "";
            switch (Code)
            {
                case "000":
                    ReturnStr = "成功";
                    break;
                case "001":
                    ReturnStr = "參數錯誤";
                    break;
                case "002":
                    ReturnStr = "預約時間參數錯誤";
                    break;
                case "003":
                    ReturnStr = "預約時間過期";
                    break;
                case "004":
                    ReturnStr = "訊息長度過長";
                    break;
                case "005":
                    ReturnStr = "帳號密碼錯誤";
                    break;
                case "006":
                    ReturnStr = "IP無法存取";
                    break;
                case "007":
                    ReturnStr = "收件者人數為0";
                    break;
                case "008":
                    ReturnStr = "收件人超過250人";
                    break;
                case "009":
                    ReturnStr = "點數不足";
                    break;
                default:
                    ReturnStr = "其他錯誤";
                    break;
            }
            return ReturnStr;
        }
        #endregion        

        #region 儲存USERLOG
        public void SaveLog(String Setting, String UserID, String ProgName, String JobName, String Title, String TableID, String Detail, String FileName)
        {
            using (SqlConnection connlog = new SqlConnection(Setting))
            {
                connlog.Open();

                SqlCommand cmdlog = new SqlCommand();
                cmdlog.CommandText = "sp_userlogAdd";
                cmdlog.CommandType = CommandType.StoredProcedure;
                cmdlog.Connection = connlog;
                cmdlog.Parameters.Add(new SqlParameter("@id", UserID));
                cmdlog.Parameters.Add(new SqlParameter("@prog_name", ProgName));
                cmdlog.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmdlog.Parameters.Add(new SqlParameter("@title", Title));
                cmdlog.Parameters.Add(new SqlParameter("@table_id", TableID));
                cmdlog.Parameters.Add(new SqlParameter("@detail", Detail));
                cmdlog.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmdlog.Parameters.Add(new SqlParameter("@filename", FileName));

                cmdlog.ExecuteNonQuery();
            }
        }
        #endregion

        #region Get IP
        public string GetIPAddress()
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

        #region HttpWebRequest送出資料
        public String SendForm(String TradePostUrl, String param)
        {
            byte[] bs = Encoding.ASCII.GetBytes(param);

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(TradePostUrl);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = bs.Length;
            string result = null;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }
            using (WebResponse wr = req.GetResponse())
            {
                StreamReader sr = new StreamReader(wr.GetResponseStream(), System.Text.Encoding.GetEncoding("utf-8"));
                result = sr.ReadToEnd();
                sr.Close();
            }

            return result;
        }
        #endregion

        #region 回傳輸出字串並結束
        public void ResponseWriteEnd(HttpContext context, string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }
        #endregion

        #region 產生隨機字串
        public string GetRandomString(int length)
        {
            var next = new Random();
            var builder = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                builder.Append(str[next.Next(0, str.Length)]);
            }
            return builder.ToString();
        }
        #endregion

        #region 長字串計算唯一檢查碼
        public char checkSunChar(string str)
        {
            int code = 0;
            foreach (char c in str)
            {
                int unicode = c;
                if (unicode < 128)
                {
                    code += unicode;
                }
            }
            return str[code % str.Length];
        }
        #endregion

        #region 取得左邊字串
        public string Left(string param, int length)
        {
            string result = param.Substring(0, length);
            return result;
        }
        #endregion

        #region 取得右邊字串
        public string Right(string param, int length)
        {
            string result = param.Substring(param.Length - length, length);
            return result;
        }
        #endregion

        #region 取得中間字串
        public string Mid(string param, int startIndex, int length)
        {
            string result = param.Substring(startIndex, length);
            return result;
        }
        #endregion

        #region 取得中間字串開始到結束
        public string Mid(string param, int startIndex)
        {

            string result = param.Substring(startIndex);
            return result;
        }
        #endregion

        #region 檢查是否有權限
        public bool hasPwoer(string setting, string jobName, string power, string id)
        {
            bool chk = false;
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"sp_getEmplThePower @emplID,@jobName", conn);
                cmd.Parameters.Add(new SqlParameter("@emplID", id));
                cmd.Parameters.Add(new SqlParameter("@jobName", jobName));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (reader[power].ToString() == "Y") chk = true;
                    }
                }
                catch
                {
                    chk = false;
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            return chk;
        }
        #endregion

        #region insert log
        public void InsertLog(String Setting, String id, String progName, String JobName, String JobTitle, String Detail, String filename)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_userlogAdd";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@id", id));
                cmd.Parameters.Add(new SqlParameter("@prog_name", progName));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", filename));
                cmd.ExecuteNonQuery();
            }
        }
        #endregion
    }
}