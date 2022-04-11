using ECSSO.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace ECSSO.api
{
    /// <summary>
    /// User2 的摘要描述
    /// </summary>
    public class User2 : IHttpHandler
    {
        private HttpContext context;
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            GetStr GS = new GetStr();

            int statement = 0;
            string returnMsg = "Something has wrong!!", tableName = "Cust";
            try
            {
                if (context.Request.Params["Type"] == null || context.Request.Params["Type"].ToString() == "") statement = 1;
                if (context.Request.Params["Items"] == null || context.Request.Params["Items"].ToString() == "") statement = 2;
                if (context.Request.Params["CheckSum"] == null || context.Request.Params["CheckSum"].ToString() == "") statement = 3;
                if (context.Request.Params["Token"] == null || context.Request.Params["Token"].ToString() == "") statement = 4;

                switch (statement)
                {
                    case 0:
                        {
                            String ChkM = context.Request.Params["CheckSum"].ToString();
                            String Type = context.Request.Params["Type"].ToString();
                            String Token = context.Request.Params["Token"].ToString();
                            CheckUser2 items = JsonConvert.DeserializeObject<CheckUser2>(context.Request.Params["Items"]);
                            string Items = context.Request.Params["Items"].ToString();
                            string strSqlConnection = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();

                            if (GS.MD5Check(Type + Items, ChkM))
                            {
                                CommandSetting cmdSetting = new CommandSetting();
                                DataListSetting dataList = new DataListSetting();
                                dataList.Data = new List<object>();

                                if (items.Login == "")
                                {
                                    returnMsg = ErrorMsg("error", "Login不得為空", "");
                                    break;
                                }
                                else
                                {
                                    string strLogin = GS.Base64Decode(items.Login);
                                    string[] strs = strLogin.Split(new char[] { ',' });
                                    string LoginId = "";
                                    string LoginPwd = "";
                                    string LoginType = "";
                                    try
                                    {
                                        LoginId = strs[0];
                                        LoginPwd = strs[1];
                                        LoginType = (strs.Length < 3 || GS.CheckStringIsNotNull(strs[2]) == "") ? "0" : strs[2];
                                    }
                                    catch
                                    {
                                        returnMsg = ErrorMsg("error", "帳號密碼輸入格式錯誤", "");
                                        break;
                                    }
                                    if (LoginId == "")
                                    {
                                        returnMsg = ErrorMsg("error", "帳號不得為空", "");
                                        break;
                                    }
                                    if (LoginPwd == "" && LoginType == "0")
                                    {
                                        returnMsg = ErrorMsg("error", "密碼不得為空", "");
                                        break;
                                    }

                                    switch (cmdSetting.isVerityState(GS.GetIPAddress(), Token, strSqlConnection))
                                    {
                                        case 0:
                                            UserLogin userLogin = new UserLogin();
                                            string MemID = "";
                                            if (Type == "Login")
                                            {
                                                using (SqlConnection conn = new SqlConnection(cmdSetting._strSqlConnection))
                                                {
                                                    if (conn.State == ConnectionState.Closed) conn.Open();
                                                    if (LoginType == "1")
                                                    {
                                                        using (SqlCommand cmd = new SqlCommand("select mem_id From Cust Where id = @id", conn))
                                                        {
                                                            cmd.Parameters.Add(new SqlParameter("@id", LoginId));

                                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                                            {
                                                                if (reader.HasRows)
                                                                {
                                                                    while (reader.Read())
                                                                    {
                                                                        MemID = reader["mem_id"].ToString();
                                                                        userLogin.LoginState = "3";
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    userLogin.LoginState = "4";
                                                                }
                                                            }

                                                        }
                                                    }
                                                    else if (LoginType == "0")
                                                    {
                                                        using (SqlCommand cmd = new SqlCommand("sp_CheckPassWord @pwd,@id", conn))
                                                        {
                                                            cmd.Parameters.Add(new SqlParameter("@pwd", LoginPwd));
                                                            cmd.Parameters.Add(new SqlParameter("@id", LoginId));

                                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                                            {
                                                                if (reader.HasRows)
                                                                {
                                                                    while (reader.Read())
                                                                    {
                                                                        MemID = reader["mem_id"].ToString();
                                                                        userLogin.LoginState = "3";
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    userLogin.LoginState = "4";
                                                                }
                                                            }

                                                        }
                                                    }
                                                }
                                                userLogin.MemID = MemID;

                                                dataList.Data.Add(userLogin);
                                                returnMsg = JsonConvert.SerializeObject(dataList);
                                            }
                                            else if (Type == "SignIn")
                                            {
                                                if (GS.CheckStringIsNotNull(items.UUID) == "")
                                                {
                                                    returnMsg = ErrorMsg("error", "UUID必填", "");
                                                    break;
                                                }
                                                using (SqlConnection conn = new SqlConnection(cmdSetting._strSqlConnection))
                                                {
                                                    if (conn.State == ConnectionState.Closed) conn.Open();
                                                    SqlCommand cmd = new SqlCommand("select mem_id from cust where id=@id", conn);
                                                    cmd.Parameters.Add(new SqlParameter("@id", LoginId));
                                                    SqlDataReader reader = cmd.ExecuteReader();
                                                    try
                                                    {
                                                        if (reader.HasRows)
                                                        {
                                                            userLogin.LoginState = "0";
                                                        }
                                                        else
                                                        {
                                                            //setting Mem_id
                                                            using (SqlConnection conn2 = new SqlConnection(cmdSetting._strSqlConnection))
                                                            {
                                                                if (conn2.State == ConnectionState.Closed) conn2.Open();
                                                                SqlCommand cmd2 = new SqlCommand("select isnull(max(mem_id),'') from cust", conn2);
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
                                                        }
                                                    }
                                                    finally
                                                    {
                                                        reader.Close();
                                                    }
                                                }
                                                userLogin.MemID = MemID;
                                                if (MemID != "")
                                                {
                                                    //Insert Cust
                                                    try
                                                    {
                                                        using (SqlConnection conn = new SqlConnection(cmdSetting._strSqlConnection))
                                                        {
                                                            if (conn.State == ConnectionState.Closed) conn.Open();
                                                            using (SqlCommand cmd = new SqlCommand("sp_NewMember2", conn))
                                                            {
                                                                cmd.CommandType = CommandType.StoredProcedure;

                                                                cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                                                                cmd.Parameters.Add(new SqlParameter("@id", LoginId));
                                                                cmd.Parameters.Add(new SqlParameter("@pwd", LoginPwd));
                                                                cmd.Parameters.Add(new SqlParameter("@ch_name", LoginId));
                                                                cmd.Parameters.Add(new SqlParameter("@sex", 1));
                                                                cmd.Parameters.Add(new SqlParameter("@email", ""));
                                                                cmd.Parameters.Add(new SqlParameter("@birth", Convert.ToDateTime("1911-01-01").ToString("yyyy-MM-dd")));
                                                                cmd.Parameters.Add(new SqlParameter("@tel", ""));
                                                                cmd.Parameters.Add(new SqlParameter("@cell_phone", ""));
                                                                cmd.Parameters.Add(new SqlParameter("@addr", ""));
                                                                cmd.Parameters.Add(new SqlParameter("@ident", ""));
                                                                cmd.Parameters.Add(new SqlParameter("@id2", ""));
                                                                cmd.Parameters.Add(new SqlParameter("@C_ZIP", ""));
                                                                cmd.Parameters.Add(new SqlParameter("@SnAndId", ""));
                                                                cmd.Parameters.Add(new SqlParameter("@chk", "Y"));
                                                                cmd.ExecuteNonQuery();
                                                            }
                                                            using (SqlCommand cmd = new SqlCommand("sp_updateCustUUID", conn))
                                                            {
                                                                cmd.CommandType = CommandType.StoredProcedure;

                                                                cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                                                                cmd.Parameters.Add(new SqlParameter("@UUID", items.UUID));
                                                                cmd.ExecuteNonQuery();
                                                            }
                                                        }
                                                        userLogin.LoginState = "1";
                                                    }
                                                    catch
                                                    {
                                                        userLogin.LoginState = "2";
                                                    }
                                                }

                                                dataList.Data.Add(userLogin);
                                                returnMsg = JsonConvert.SerializeObject(dataList);
                                            }
                                            else
                                            {
                                                returnMsg = ErrorMsg("error", "Type不存在", "");
                                            }
                                            break;
                                        case 1:
                                            returnMsg = ErrorMsg("error", "Token不存在", "");
                                            break;
                                        case 2:
                                            returnMsg = ErrorMsg("error", "Token權限出問題", "");
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                returnMsg = ErrorMsg("error", "CheckSum驗證失敗", "");
                            }
                            break;
                        }
                    case 1:
                        {
                            returnMsg = ErrorMsg("error", "Type必填", "");
                            break;
                        }
                    case 2:
                        {
                            returnMsg = ErrorMsg("error", "Items必填", "");
                            break;
                        }
                    case 3:
                        {
                            returnMsg = ErrorMsg("error", "CheckSum必填", "");
                            break;
                        }
                    case 4:
                        {
                            returnMsg = ErrorMsg("error", "Token必填", "");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                returnMsg = ErrorMsg("error", ex.ToString(), "");
            }
            //context.Response.ContentType = "json/application";
            context.Response.Write(returnMsg);
            context.Response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "User2 error", "", RspnMsg);
            }

            ContextErrorMessager root = new ContextErrorMessager();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);

        }
        #endregion
        #region insert log
        private void InsertLog(String Setting, String JobName, String JobTitle, String Detail)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_userlogAdd";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@id", "guest"));
                cmd.Parameters.Add(new SqlParameter("@prog_name", "帳號"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " User2.ashx"));

                cmd.ExecuteNonQuery();
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