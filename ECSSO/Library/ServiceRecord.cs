using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    public class ServiceRecord
    {
        public String memID { get; set; }
        public List<ServiceRecordItem> items { get; set; }
        public List<ServiceRecordItem> getMemRecode(String setting, String orgname, String type, String memID)
        {
            if (items == null) items = new List<ServiceRecordItem>();
            else if (items.Count != 0) items.Clear();
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from serviceRecord where type=@type and bindID=@bindID order by convert(datetime,notedate) desc", conn);
                cmd.Parameters.Add(new SqlParameter("@type", type));
                cmd.Parameters.Add(new SqlParameter("@bindID", memID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        this.memID = memID;
                        while (reader.Read())
                        {
                            ServiceRecordItem item = new ServiceRecordItem{
                                ID=reader["id"].ToString(),
                                title = reader["title"].ToString(),
                                notedate = reader["notedate"].ToString(),
                                status = reader["status"].ToString(),
                                question = reader["question"].ToString(),
                                handle = reader["handle"].ToString(),
                                file = reader["file"].ToString().Replace("/upload", "/upload/" + orgname)
                            };
                            items.Add(item);
                        }
                    }
                }
                catch { 
                    
                }
            }
            return items;
        }
    }
    public class ServiceRecordItem{
        public String ID { get; set; }
        public String title { get; set; }
        public String status { get; set; }
        public String notedate { get; set; }
        public String question { get; set; }
        public String handle { get; set; }
        public String file { get; set; }
    }
}