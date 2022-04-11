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

namespace TAT
{
    public class GetTATStr
    {
        #region 取得DB連結字串
        public String GetSetting(String SiteID)
        {
            String Str_Return = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["TATsqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select dbname,dbusername,dbpassword from cocker_cust where id=@id", conn);
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

        #region 取得預設網址
        public String GetDefaultURL(String SiteID)
        {
            String Str_Return = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["TATsqlDB"].ToString()))
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

        #region 字串轉utf-8
        public string StringToUTF8(String Str)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            Byte[] encodedBytes = utf8.GetBytes(Str);
            return utf8.GetString(encodedBytes);
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

        #region 由Setting取得Orgname
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

        #region 發送簡訊
        public String SendSMS(String Setting, String Tel, String SMSID, String SMSPwd, String SMSCont)
        {
            GetTATStr GS = new GetTATStr();

            String URL = @"http://sms-get.com/api_send.php?username=" + SMSID + "&password=" + SMSPwd + "&method=1&sms_msg=" + GS.StringToUTF8(SMSCont) + "&phone=" + Tel + "&send_date=&hour=&min=";
            Uri urlCheck = new Uri(URL);
            WebRequest request = WebRequest.Create(urlCheck);
            request.Timeout = 10000;
            using (WebResponse wr = request.GetResponse())
            {
                using (StreamReader myStreamReader = new StreamReader(wr.GetResponseStream()))
                {
                    Library.Account.SMSData postf = JsonConvert.DeserializeObject<Library.Account.SMSData>(myStreamReader.ReadToEnd());
                    using (SqlConnection conn = new SqlConnection(Setting))
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand();
                            cmd.CommandText = "AddSmsLog";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = conn;
                            cmd.Parameters.Add(new SqlParameter("@tel", Tel));
                            cmd.Parameters.Add(new SqlParameter("@SMSID", postf.error_msg.Split('|')[0].ToString()));
                            cmd.Parameters.Add(new SqlParameter("@SMSCode", postf.error_code));
                            cmd.Parameters.Add(new SqlParameter("@SMSMsg", GS.GetSMSErrorMsg(postf.error_code)));
                            cmd.Parameters.Add(new SqlParameter("@ContMsg", SMSCont)); 
                        cmd.ExecuteNonQuery();
                        }

                    return postf.error_code;
                }
            }
            
        }
        #endregion
    }
}