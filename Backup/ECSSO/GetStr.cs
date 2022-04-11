using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace ECSSO
{
    public class GetStr
    {
        public String GetSetting(String SiteID) {
            String Str_Return = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select dbname,dbusername,dbpassword from cocker_cust where id=@id", conn);
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
                finally {
                    reader.Close();
                }
            }
            return Str_Return;
        }

        public string GetLanString(String Language) {
            String FolderStr = "";
            switch (Language)
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
                default:
                    FolderStr = "tw";
                    break;
            }
            return FolderStr;
        }
    }
}