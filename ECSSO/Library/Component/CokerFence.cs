using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Component
{
    public class CokerFence : Component
    {
        public int bid { get; set; }
        private int colNum { get; set; }
        private int padding { get; set; }
        private int dispType { get; set; }
        public void setFence(string setting,int bid)
        {
            GetStr GS = new GetStr();
            this.bid = bid;
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from fence where id=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", bid));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        colNum = GS.StringToInt(reader["colNum"].ToString(), 1);
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
        }
    }
}