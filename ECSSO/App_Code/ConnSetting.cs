using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace ECSSO
{
    public class ConnSetting
    {
        //private string test { get; set; }
        
        public string GetByID(string SiteID)
        {
            string sqlstr = "select dbname,dbusername,CONVERT(nvarchar(50), dbpassword) dbpassword,web_url from cocker_cust where id=@id";            
            return GetSetting(sqlstr, SiteID);
        }

        public string GetByHost(string Host)
        {
            string sqlstr = "select dbname,dbusername,CONVERT(nvarchar(50), dbpassword) dbpassword,web_url from cocker_cust where web_url=@id";
            return GetSetting(sqlstr, Host);
        }

        public string GetByOrg(string orgName)
        {
            string sqlstr = "select dbname,dbusername,CONVERT(nvarchar(50), dbpassword) dbpassword,web_url from cocker_cust where crm_org=@id";
            return GetSetting(sqlstr, orgName);
        }

        private string GetSetting(string sqlstr,string value)
        {
            string connSetting = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqlstr, conn);
                cmd.Parameters.Add(new SqlParameter("@id", value));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read()) 
                        {
                            connSetting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=" + reader["dbname"] + "; password=" + reader["dbpassword"] + "; database=" + reader["dbname"];
                        }
                    }
                }
                finally {
                    reader.Close();
                }
            }            
            return connSetting;
        }
    }
}