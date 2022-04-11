using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Power
{
    public class EmplObject : responseJson
    {
        public bool exe { get; set; }
        public bool add { get; set; }
        public bool edit { get; set; }
        public bool del { get; set; }
        public void checkPower(string setting, string id)
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"sp_getEmplThePower @emplID,'P001'", conn);
                cmd.Parameters.Add(new SqlParameter("@emplID", id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        this.exe = reader["canexe"].ToString() == "Y";
                        this.edit = reader["canedit"].ToString() == "Y";
                        this.del = reader["candel"].ToString() == "Y";
                        this.add = reader["canadd"].ToString() == "Y";
                    }
                }
                catch
                {
                    throw new Exception("權限不存在");
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
    }
    public class EmplItems : EmplObject {
        public List<EmplItem> list { get; set; }
    }
    public class EmplData : EmplObject
    {
        public EmplItem item { get; set; }
    }
    public class EmplItem
    {
        public string id { get; set; }
        public int gid { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public bool manager { get; set; }
    }
}