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

namespace Meal.Library
{
    public class GetMealStr
    {
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

        #region 由Setting取得Orgname
        public string GetOrgName2(String setting)
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

        #region 由Vercode取得Orgname
        public String GetOrgName(String VerCode)
        {
            String OrgName = "";
            String Str_Sql = "select orgname from Device where stat='Y' and getdate() between start_date and end_date and VerCode=@VerCode";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB2"].ToString()))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand(Str_Sql, conn);
                cmd.Parameters.Add(new SqlParameter("@VerCode", VerCode));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            OrgName = reader[0].ToString();
                        }
                    }
                }
                finally { reader.Close(); }
            }
            return OrgName;
        }
        #endregion

        #region 取得Orgname連結字串
        public String GetSetting(String OrgName)
        {
            return "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
        }
        #endregion

        #region 取得DB連結字串
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
    }
}