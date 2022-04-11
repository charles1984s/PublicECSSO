using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
//using System.Net.Http;
using System.Text.RegularExpressions;
//using System.Web.Http;
using System.Security.Cryptography;

namespace ECSSO.common
{
    public class APIandDB
    {                
        
        GetStr getstr = new GetStr();
        String setting = "";
        String SnAndId = null;

        //寫完之後這裡要修改，初始化的時候就把setting設定好用以提升效能
        //public APIandDB(string siteid)
        //{
        //    setting = getstr.GetSetting(siteid);
        //}

        //特別用出來給會有無email情形的OpenID用
        public bool LoginOrReg(string siteid, string OAuthemail, string OAuthSnAndId)
        {
            //OAuth2WhenNoEmail裡面是儲存snAndid的，在email是使用者自己輸入的時候要把snAndid存到額外的資料庫欄位
            SnAndId = OAuthSnAndId;
            return LoginOrReg(siteid, OAuthemail);
        }

        //特別用出來給會有無email情形的OpenID用
        public bool RegMember(string siteid, string OAuthemail, string OAuthSnAndId)
        {
            //OAuth2WhenNoEmail裡面是儲存snAndid的，在email是使用者自己輸入的時候要把snAndid存到額外的資料庫欄位
            SnAndId = OAuthSnAndId;
            return RegMember(siteid, OAuthemail);
        }
        public bool LoginOrReg(string siteid, string OAuthemail)
        {
            setting = getstr.GetSetting(siteid);

            //如果OAuth2SnAndId == null 則代表SnAndId是空值，即此OpenID來源不會有抓不到email的問題
            //如果OAuth2SnAndId != null 則代表SnAndId有值，即此OpenID來源可能會有抓不到email的問題，所以要修改SQL語法
            if (SnAndId != null)
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand("select mem_id,chk from Cust where (id=@id or SnAndId=@OAuth2SnAndId) and (chk='Y' or chk='O')", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", OAuthemail));
                    cmd.Parameters.Add(new SqlParameter("@OAuth2SnAndId", SnAndId));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read()) {
                                if (reader[1].ToString() == "O")
                                {
                                    chToY(reader[0].ToString());                                    
                                }                                
                            }
                            return true;
                        }
                        else
                        {
                            return RegMember(siteid, OAuthemail);
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            else        //搜尋看有沒有這個會員的SQL語法
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand("select mem_id,chk from Cust where id=@id and (chk='Y' or chk='O')", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", OAuthemail));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (reader[1].ToString() == "O")
                                {
                                    chToY(reader[0].ToString());                                    
                                }                                                                
                            }
                            return true;
                        }
                        else
                        {
                            return RegMember(siteid, OAuthemail);
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }


            #region 原碼檢測修改前
            /*
            using (SqlConnection conn = new SqlConnection(setting))
            {
                //搜尋看有沒有這個會員的SQL語法
                String Str_sql = "select mem_id from Cust where id=@id and chk='Y'";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                cmd.Parameters.Add(new SqlParameter("@id", OAuthemail));

                //如果OAuth2SnAndId == null 則代表SnAndId是空值，即此OpenID來源不會有抓不到email的問題
                //如果OAuth2SnAndId != null 則代表SnAndId有值，即此OpenID來源可能會有抓不到email的問題，所以要修改SQL語法
                if (SnAndId != null)
                {
                    Str_sql = "select mem_id from Cust where (id=@id or SnAndId=@OAuth2SnAndId) and chk='Y'";
                    cmd = new SqlCommand(Str_sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@id", OAuthemail));
                    cmd.Parameters.Add(new SqlParameter("@OAuth2SnAndId", SnAndId));
                }

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    return true;
                }
                else
                {
                    return RegMember(siteid, OAuthemail);
                }
            }
             */ 
            #endregion
            
        }
        private bool RegMember(string siteid, string OAuthemail)
        {
            String MemID = "";
            setting = getstr.GetSetting(siteid);

            //Check ID repeat

            //先檢查OAuthemail是不是正確的email，如果不是就退回去，傳進來正確的email才能註冊(因為OAuth的email有可能會有null的情況)
            if (!Regex.IsMatch(OAuthemail, @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$"))
            {
                return false;
            }
            else {
                //如果OAuth2SnAndId == null 則代表SnAndId是空值，即此OpenID來源不會有抓不到email的問題
                //如果OAuth2SnAndId != null 則代表SnAndId有值，即此OpenID來源可能會有抓不到email的問題，所以要修改SQL語法
                if (SnAndId != null)
                {
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        String Str_sql = "select mem_id from Cust where id=@id or SnAndId=@OAuth2SnAndId";
                        SqlCommand cmd = new SqlCommand(Str_sql, conn);
                        cmd.Parameters.Add(new SqlParameter("@id", OAuthemail));
                        cmd.Parameters.Add(new SqlParameter("@OAuth2SnAndId", SnAndId));
                        //假設一開始使用者用沒email的OpenID註冊，那就會比對SnAndId這個欄位，假設事後又在OpenID補上Email，還是變成以比對SnAndId為主
                        //以上相似的SQL語法用這麼多這麼嚴謹，就是為了要很確定真的沒有這個會員
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                return false;   //有會員id但chk不是Y所以 return false
                            }
                            else
                            {
                                //setting Mem_id
                                MemID = GetMemID(setting);
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }                    
                }
                else {
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        //基本參數
                        String Str_sql = "select mem_id from Cust where id=@id";
                        SqlCommand cmd = new SqlCommand(Str_sql, conn);
                        cmd.Parameters.Add(new SqlParameter("@id", OAuthemail));
                        //假設一開始使用者用沒email的OpenID註冊，那就會比對SnAndId這個欄位，假設事後又在OpenID補上Email，還是變成以比對SnAndId為主
                        //以上相似的SQL語法用這麼多這麼嚴謹，就是為了要很確定真的沒有這個會員
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                return false;   //有會員id但chk不是Y所以 return false
                            }
                            else
                            {
                                //setting Mem_id
                                MemID = GetMemID(setting);
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                }
                #region 原碼修改前
                /*
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    //基本參數
                    String Str_sql = "select mem_id from Cust where id=@id";
                    SqlCommand cmd = new SqlCommand(Str_sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@id", OAuthemail));

                    //如果OAuth2SnAndId == null 則代表SnAndId是空值，即此OpenID來源不會有抓不到email的問題
                    //如果OAuth2SnAndId != null 則代表SnAndId有值，即此OpenID來源可能會有抓不到email的問題，所以要修改SQL語法
                    if (SnAndId != null)
                    {
                        Str_sql = "select mem_id from Cust where id=@id or SnAndId=@OAuth2SnAndId";
                        cmd = new SqlCommand(Str_sql, conn);
                        cmd.Parameters.Add(new SqlParameter("@id", OAuthemail));
                        cmd.Parameters.Add(new SqlParameter("@OAuth2SnAndId", SnAndId));
                    }

                    //假設一開始使用者用沒email的OpenID註冊，那就會比對SnAndId這個欄位，假設事後又在OpenID補上Email，還是變成以比對SnAndId為主
                    //以上相似的SQL語法用這麼多這麼嚴謹，就是為了要很確定真的沒有這個會員

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            return false;   //有會員id但chk不是Y所以 return false
                        }
                        else
                        {
                            //setting Mem_id
                            MemID = GetMemID(setting);
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
                 * */
                #endregion
                
            }

            
            //try
            {
                if (MemID != "")
                {
                    //Insert Cust
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandText = "sp_NewMember2";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn;

                        string pwd = "這裡是用OAuth來登入的密碼,啦啦啦~!@#$%&*";                        
                        pwd += DateTime.Now.ToString("yyyyMMddHHmmss");   //加上一些亂數yo~

                        cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                        cmd.Parameters.Add(new SqlParameter("@id", OAuthemail));
                        cmd.Parameters.Add(new SqlParameter("@pwd", pwd));
                        cmd.Parameters.Add(new SqlParameter("@ch_name", "網站會員" + MemID));
                        cmd.Parameters.Add(new SqlParameter("@sex", "1"));  //性別暫時都使用1
                        cmd.Parameters.Add(new SqlParameter("@email", OAuthemail));
                        cmd.Parameters.Add(new SqlParameter("@birth", ""));
                        cmd.Parameters.Add(new SqlParameter("@tel", ""));
                        cmd.Parameters.Add(new SqlParameter("@cell_phone", ""));
                        cmd.Parameters.Add(new SqlParameter("@addr", ""));
                        cmd.Parameters.Add(new SqlParameter("@ident", ""));
                        cmd.Parameters.Add(new SqlParameter("@id2", ""));
                        cmd.Parameters.Add(new SqlParameter("@C_ZIP", ""));

                        //如果OAuth2SnAndId == null 則代表SnAndId是空值，即此OpenID來源不會有抓不到email的問題
                        //如果OAuth2SnAndId != null 則代表SnAndId有值，即此OpenID來源可能會有抓不到email的問題，跑到這邊也代表著email是使用者自己輸入的，所以得把SnAndID存到額外的資料庫欄位
                        if (SnAndId != null)
                        {
                            cmd.Parameters.Add(new SqlParameter("@SnAndId", SnAndId));
                        }
                        else {
                            cmd.Parameters.Add(new SqlParameter("@SnAndId", ""));
                        }
                        cmd.Parameters.Add(new SqlParameter("@chk", "Y"));
                        cmd.ExecuteNonQuery();

                        chToY(MemID);

                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            //catch
            //{
            //    return false;
            //}
        }
        private void chToY(string MemID)
        {
            string mem_id = MemID;
            using (SqlConnection conn = new SqlConnection(setting))
            {
                //成功註冊後,直接把chk欄未改成Y
                //(要先取得bonus的初始值)
                conn.Open();
                string bonus = "0";
                SqlCommand getbonus = new SqlCommand("select bonus_first from head", conn);
                SqlDataReader reader3 = getbonus.ExecuteReader();
                try
                {
                    if (reader3.HasRows)
                    {
                        while (reader3.Read())
                        {
                            if (reader3[0].ToString() != "")
                            {
                                bonus = reader3[0].ToString();
                            }                            
                        }
                    }
                }
                finally {
                    reader3.Close();
                }

                SqlCommand changeChk = new SqlCommand();
                changeChk.CommandText = "sp_CheckMail";
                changeChk.CommandType = CommandType.StoredProcedure;
                changeChk.Connection = conn;
                changeChk.Parameters.Add(new SqlParameter("@mem_id", mem_id));
                changeChk.Parameters.Add(new SqlParameter("@bonus", bonus));
                changeChk.ExecuteNonQuery();
            }
        }
        public string getEmail(string siteid, string OAuthSnAndId)
        {
            setting = getstr.GetSetting(siteid);

            using (SqlConnection conn = new SqlConnection(setting))
            {
                string reString = "noThisEmail";
                //搜尋看有沒有這個會員的SQL語法
                String Str_sql = "select id from Cust where SnAndId=@OAuthSnAndId and ( chk='Y' or chk='O')";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                cmd.Parameters.Add(new SqlParameter("@OAuthSnAndId", OAuthSnAndId));

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            reString = reader["id"].ToString();
                        }
                    }
                }
                finally {
                    reader.Close();
                }
                return reString;
            }
        }
        public bool updateSnAndId(string siteid, string CheckedEmail, string OAuthSnAndId)
        {
            setting = getstr.GetSetting(siteid);

            using (SqlConnection conn = new SqlConnection(setting))
            {
                //搜尋看有沒有這個會員的SQL語法
                String Str_sql = "select id from Cust where SnAndId=@OAuthSnAndId";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                cmd.Parameters.Add(new SqlParameter("@OAuthSnAndId", OAuthSnAndId));

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    return false;
                }
                else
                {
                    reader.Close();
                    //先取得改會員email的mem_id
                    Str_sql = "select mem_id from Cust where id=@CheckedEmail";
                    cmd = new SqlCommand(Str_sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@CheckedEmail", CheckedEmail));  //要查找的email
                    reader = cmd.ExecuteReader();
                    try {
                        if (reader.HasRows)
                        {
                            cmd = new SqlCommand();
                            cmd.CommandText = "sp_UpdateSnAndId";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = conn;

                            string reString = "0";
                            while (reader.Read())
                            {
                                reString = reader["mem_id"].ToString();
                            }
                            cmd.Parameters.Add(new SqlParameter("@mem_id", reString)); //查詢到的mem_id
                            cmd.Parameters.Add(new SqlParameter("@SnAndId", OAuthSnAndId)); //要寫入的SnAndId

                            reader.Close(); //要先把佔用conn的reader關掉才能執行下一個命令

                            cmd.ExecuteNonQuery();
                            return true;
                        }
                    }
                    finally { 
                        reader.Close(); 
                    }                    
                    return false;
                }
            }
        }
        private String GetMemID(String setting) {
            String MemID = "";
            //setting Mem_id
            using (SqlConnection conn2 = new SqlConnection(setting))
            {
                conn2.Open();
                SqlCommand cmd2 = new SqlCommand("select isnull(max(mem_id),'') from Cust", conn2);
                SqlDataReader reader2 = cmd2.ExecuteReader();
                try
                {
                    while (reader2.Read())
                    {
                        if (reader2[0].ToString() != "")
                        {
                            MemID = (Convert.ToInt16(reader2[0].ToString()) + 1).ToString().PadLeft(6, '0');
                        }
                        else
                        {
                            MemID = "000001";
                        }
                    }
                }
                finally
                {
                    reader2.Close();
                }
            }
            return MemID;
        }
    }
}