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
        public string token { get; set; }
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
                try {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Str_Token = reader[0].ToString();
                        }
                    }
                }
                finally {
                    reader.Close();
                }
            }
            return Str_Token;
        }
        #region 建立或更新使用者token
        public string updateToken(String setting, string MemID,string ip)
        {
            string s="";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_updateToken";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@token", ""));
                cmd.Parameters.Add(new SqlParameter("@ManagerID", MemID));
                cmd.Parameters.Add(new SqlParameter("@ip", ip));
                SqlParameter SPOutput = cmd.Parameters.Add("@ReturnCode", SqlDbType.NVarChar, 50);
                SPOutput.Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                s = SPOutput.Value.ToString();
                token = s;
            }
            return s;
        }
        #endregion
        #region 檢查token是否有效
        public bool checkToken(string setting, string token) {
            bool r = false;
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from token as t
                    where t.id = @token and DateDiff(MINUTE,GETDATE(),CONVERT(datetime,end_time))>=0
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@token", token));
                SqlDataReader reader = cmd.ExecuteReader();
                r = reader.HasRows;
                if (r) this.token = token;
            }
            return r;
        }
        public Library.Member.Data checkTokenAndGetMember(string setting,string token) {
            Library.Member.Data member = new Library.Member.Data();
            member.setSData(setting, token);
            if (member.token == null) throw new Exception("token過期");
            return member;
        }
        #endregion
    }
}