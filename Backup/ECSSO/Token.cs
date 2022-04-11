using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace ECSSO
{
    public class Token
    {
        public string LoginToken(string custid, string setting)
        {
            String Str_Token = "";            

            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                String Str_sql = "select sys.fn_VarBinToHexStr(hashbytes('MD5', convert(nvarchar,id+pwd+logintime))) from cust where id=@id";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                cmd.Parameters.Add(new SqlParameter("@id", custid));
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Str_Token = reader[0].ToString();
                    }
                }
            }

            return Str_Token;
        }
    }
}