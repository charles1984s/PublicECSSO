using System;
using System.Collections.Generic;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace TAT.api
{
    /// <summary>
    /// Account 的摘要描述
    /// </summary>
    public class Account : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Params["Type"] == null) ResponseWriteEnd(context, "error:4");
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, "error:4");
            if (context.Request.Params["SiteID"] == null) ResponseWriteEnd(context, "error:4");
            if (context.Request.Params["Items"] == null) ResponseWriteEnd(context, "error:4");

            if (context.Request.Params["Type"].ToString() == "") ResponseWriteEnd(context, "error:4");
            if (context.Request.Params["SiteID"].ToString() == "") ResponseWriteEnd(context, "error:4");
            if (context.Request.Params["CheckM"].ToString() == "") ResponseWriteEnd(context, "error:4");
            if (context.Request.Params["Items"].ToString() == "") ResponseWriteEnd(context, "error:4");

            String ChkM = context.Request.Params["CheckM"].ToString();
            String SiteID = context.Request.Params["SiteID"].ToString();
            String Type = context.Request.Params["Type"].ToString();

            GetTATStr GS = new GetTATStr();
            String Setting = GS.GetSetting(SiteID);
            if (!GS.MD5Check(Type + SiteID + GS.GetOrgName(Setting), ChkM)) 
            {
                ErrorMsg("error", "error:3", Setting);
                ResponseWriteEnd(context, "error:3");     //驗證碼錯誤
            } 


            Library.Account.InputData postf = JsonConvert.DeserializeObject<Library.Account.InputData>(context.Request.Params["Items"]);


            String ID = "";
            String Pwd = "";
            String Name = "";
            String Birth = "";
            String Gender = "";
            String PhoneCode = "";

            if (postf.ID == null || postf.ID == "") ResponseWriteEnd(context, "error:4");
            ID = postf.ID;

            switch (Type)
            {
                case "1":   //註冊+取得會員驗證碼

                    String SMSCode = Register(Setting, ID);
                    ResponseWriteEnd(context, SMSCode);

                    break;

                case "2":   //開通帳號+修改資料

                    if (postf.Pwd == null) ResponseWriteEnd(context, "error:4");
                    if (postf.Name == null) ResponseWriteEnd(context, "error:4");
                    if (postf.Birth == null) ResponseWriteEnd(context, "error:4");
                    if (postf.Gender == null) ResponseWriteEnd(context, "error:4");
                    if (postf.Pwd == "") ResponseWriteEnd(context, "error:4");
                    
                    Pwd = postf.Pwd;
                    Name = postf.Name;
                    Birth = postf.Birth;
                    Gender = postf.Gender;

                    ResponseWriteEnd(context, UpdateCust(Setting, ID, Pwd, Name, int.Parse(Gender), Birth));

                    break;
                case "3":   //會員登入

                    if (postf.Pwd == null) ResponseWriteEnd(context, "error:4");
                    if (postf.Pwd == "") ResponseWriteEnd(context, "error:4");

                    Pwd = postf.Pwd;

                    ResponseWriteEnd(context, Login(Setting, ID, Pwd));
                    break;
                case "4":   //會員驗證碼檢查

                    if (postf.PhoneCode == null) ResponseWriteEnd(context, "error:4");
                    if (postf.PhoneCode == "") ResponseWriteEnd(context, "error:4");

                    PhoneCode = postf.PhoneCode;

                    ResponseWriteEnd(context, ChkPhoneCode(Setting, ID, PhoneCode));
                    break;
            }
        }

        private String UpdateCust(String Setting, String ID, String Pwd, String ChName, int Sex, String Birth) 
        {
            String MemID = "";
            String ReturnCode = "";

            #region 確認是否有會員
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select mem_id from cust where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@id", ID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            MemID = reader[0].ToString();
                        }
                    }
                    else
                    {
                        ReturnCode = "error:1";
                        return ReturnCode;
                    }
                }
                catch
                {
                    ReturnCode = "error:2";
                    return ReturnCode;
                }
                finally
                {
                    reader.Close();
                }
            }
            #endregion
            
            #region 開通帳號
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_CheckMail";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                cmd.Parameters.Add(new SqlParameter("@bonus", ""));
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch 
                {
                    ReturnCode = "error:2";
                    return ReturnCode;
                }
                
            }
            #endregion

            #region 改密碼
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_ResetPassword";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@id", ID));
                cmd.Parameters.Add(new SqlParameter("@pwd", Pwd));
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    ReturnCode = "error:2";
                    return ReturnCode;
                }
            }
            #endregion

            #region 改其他資料
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_EditMember2";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@ch_name", ChName));
                cmd.Parameters.Add(new SqlParameter("@sex", Sex));
                cmd.Parameters.Add(new SqlParameter("@ident", ""));
                cmd.Parameters.Add(new SqlParameter("@birth", Birth));
                cmd.Parameters.Add(new SqlParameter("@tel", ""));
                cmd.Parameters.Add(new SqlParameter("@cell_phone", ""));
                cmd.Parameters.Add(new SqlParameter("@email", ""));
                cmd.Parameters.Add(new SqlParameter("@addr", "")); 
                cmd.Parameters.Add(new SqlParameter("@id", MemID));
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    ReturnCode = "error:2";
                    return ReturnCode;
                }
            }
            #endregion

            return "success";
        }

        private String Register(String Setting,String ID) 
        {
            String ReturnCode = "";

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_GetSMSCode";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;

                cmd.Parameters.Add(new SqlParameter("@ID", ID));
                SqlParameter SPOutput = cmd.Parameters.Add("@ReturnCode", SqlDbType.NVarChar, 7);
                SPOutput.Direction = ParameterDirection.Output;
                try
                {
                    cmd.ExecuteNonQuery();
                    ReturnCode = SPOutput.Value.ToString();
                }
                catch
                {
                    ReturnCode = "error:2";
                }
            }

            if (ReturnCode.Length == 4) 
            { 
                //發簡訊~
                String SMSStr = "";
                String SMSID = "";
                String SMSPwd = "";

                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select top 1 title,SMSID,SMSPwd from head", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                SMSStr = "【" + reader[0].ToString() + "】感謝您的註冊，請輸入驗證碼" + ReturnCode + "完成註冊，若您並未提出申請，請忽略此簡訊。";
                                SMSID = reader[1].ToString();
                                SMSPwd = reader[2].ToString();
                            }
                            if (SMSID != "" && SMSPwd != "")
                            {
                                GetTATStr GS = new GetTATStr();
                                GS.SendSMS(Setting, ID, SMSID, SMSPwd, SMSStr);
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            return ReturnCode;
        }

        private String Login(String Setting, String ID, String Pwd)
        {
            String ReturnCode = "";

            #region 登入
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_CheckPassWord";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@pwd", Pwd));
                cmd.Parameters.Add(new SqlParameter("@id", ID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ReturnCode = reader["mem_id"].ToString();
                        }
                    }
                    else 
                    {
                        ReturnCode = "error:1";
                    }

                    if (ReturnCode == "")
                    {
                        return "error:1";
                    }
                    else 
                    {
                        return ReturnCode;
                    }
                }
                catch
                {
                    ReturnCode = "error:2";
                    return ReturnCode;
                }
                finally
                {
                    reader.Close();
                }
            }
            #endregion
        }

        private String ChkPhoneCode(String Setting, String ID, String PhoneCode)
        {
            String ReturnCode = "";
            String MemID = "";

            #region 檢查簡訊驗證碼
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select mem_id from cust where id=@id and smscode=@smscode and SMSCodeExpire>=getdate()", conn);
                cmd.Parameters.Add(new SqlParameter("@id", ID));
                cmd.Parameters.Add(new SqlParameter("@smscode", PhoneCode));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            MemID = reader[0].ToString();
                        }
                    }
                    else
                    {
                        ReturnCode = "error:1";
                        return ReturnCode;
                    }
                }
                catch
                {
                    ReturnCode = "error:2";
                    return ReturnCode;
                }
                finally
                {
                    reader.Close();
                }
            }
            #endregion
            
            #region 開通帳號
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_CheckMail";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                cmd.Parameters.Add(new SqlParameter("@bonus", ""));
                try
                {
                    cmd.ExecuteNonQuery();
                    ReturnCode = "success";
                    return ReturnCode;
                }
                catch
                {
                    ReturnCode = "error:2";
                    return ReturnCode;
                }
            }
            #endregion
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

            Library.Account.ErrorObject root = new Library.Account.ErrorObject();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);

        }
        #endregion

        private void ResponseWriteEnd(HttpContext context, string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }


    }
}