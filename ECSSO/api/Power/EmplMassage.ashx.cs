using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using ECSSO.Library;
using ECSSO.Library.Power;
using Newtonsoft.Json;

namespace ECSSO.api.Power
{
    /// <summary>
    /// EmplMassage 的摘要描述
    /// </summary>
    public class EmplMassage : IHttpHandler
    {
        private EmplData emplData;
        private string setting, settingM, type;
        private string code, message;
        public void ProcessRequest(HttpContext context)
        {
            GetStr GS = new GetStr();
            code = "404";
            message = "not fount";
            emplData = new EmplData();
            TokenItem token = null;
            try
            {
                if (context.Request.Form["token"] != null)
                {
                    token = new TokenItem
                    {
                        token = context.Request.Form["token"]
                    };
                    this.setting = GS.checkToken(token);
                    type = context.Request.Form["type"];
                    if (this.setting.IndexOf("error") < 0)
                    {
                        switch (type)
                        {
                            case "data":
                                getEmplData(token, context.Request.Form["id"]);
                                break;
                            case "edit":
                                settingM = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();
                                emplData.item = new EmplItem
                                {
                                    id = context.Request.Form["empl_id"],
                                    name = context.Request.Form["name"],
                                    email = context.Request.Form["email"],
                                    manager = context.Request.Form["manager"] == "Y"
                                };
                                if (checkOtherEmpl())
                                {
                                    code = "401";
                                    message = "Email重複";
                                    emplData.item = null;
                                }
                                else if (!emplData.item.manager && !checkNotLastManager())
                                {
                                    code = "401";
                                    message = "不可取消最後的管理者權限";
                                    emplData.item = null;
                                }
                                else
                                {
                                    saveEmplData(token);
                                    GS.InsertLog(
                                        GS.GetSetting3(token.orgName),
                                        token.id, "使用者管理", "修改", emplData.item.id,
                                        "sp_updateEMPL", "api/Power/EmplMassage.ashx");
                                }
                                break;
                            case "password":
                                settingM = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();
                                emplData.item = new EmplItem
                                {
                                    id = context.Request.Form["empl_id"]
                                };
                                if (
                                    context.Request.Form["pwd"] != null && context.Request.Form["repwd"] != null &&
                                    context.Request.Form["pwd"] == context.Request.Form["repwd"]
                                )
                                {
                                    updattePassWord(token, context.Request.Form["pwd"]);
                                    if (code != "500")
                                    {
                                        GS.InsertLog(
                                            GS.GetSetting3(token.orgName),
                                            token.id, "使用者密碼", "修改", emplData.item.id,
                                            "sp_updateEMPLPassword", "api/Power/EmplMassage.ashx");
                                    } else emplData.item = null;
                                }
                                else
                                {
                                    code = "401";
                                    message = "密碼與密碼確認不相符";
                                    emplData.item = null;
                                }
                                break;
                            case "del":
                                settingM = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();
                                emplData.item = new EmplItem
                                {
                                    id = context.Request.Form["empl_id"]
                                };
                                if (!checkNotLastManager())
                                {
                                    code = "401";
                                    message = "不可取消最後的管理者權限";
                                    emplData.item = null;
                                }
                                else if (token.id == emplData.item.id)
                                {
                                    code = "401";
                                    message = "不可刪除自身帳號";
                                    emplData.item = null;
                                }
                                else
                                {
                                    delEmpl(token);
                                    GS.InsertLog(
                                        GS.GetSetting3(token.orgName),
                                        token.id, "使用者管理", "刪除", emplData.item.id,
                                        "delete all", "api/Power/EmplMassage.ashx");
                                }
                                break;
                            case "new":
                                settingM = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();
                                emplData.item = new EmplItem
                                {
                                    id = context.Request.Form["empl_id"],
                                    gid = context.Request.Form["gid"] ==null?0: int.Parse(context.Request.Form["gid"]),
                                    name = context.Request.Form["name"],
                                    email = context.Request.Form["email"],
                                    manager = context.Request.Form["manager"] == "Y"
                                };
                                if (context.Request.Form["pwd"] == null || context.Request.Form["repwd"] == null ||
                                    context.Request.Form["pwd"] != context.Request.Form["repwd"])
                                {
                                    code = "401";
                                    message = "密碼與密碼確認不相符";
                                    emplData.item = null;
                                } else if (checkEmpl()) {
                                    code = "401";
                                    message = "帳號或Email重複";
                                    emplData.item = null;
                                }
                                else
                                {
                                    InsertEmpl(token, context.Request.Form["pwd"]);
                                    GS.InsertLog(
                                        GS.GetSetting3(token.orgName),
                                        token.id, "使用者管理", "新增", emplData.item.id,
                                        "sp_insertEMPL", "api/Power/EmplMassage.ashx");
                                }
                                break;
                        }
                        if (emplData.item != null)
                        {
                            code = "200";
                            message = "success";
                        }
                    }
                    else
                    {
                        code = "401";
                        message = "Token已過期";
                    }
                }
                else
                {
                    code = "401";
                    message = "Token不可為空";
                }
            }
            catch (Exception ex)
            {
                code = "500";
                message = ex.Message;
            }
            finally
            {
                context.Response.Write(printMsg(code, message));
            }
        }
        private void getEmplData(TokenItem token, string id)
        {
            emplData.checkPower(setting, token.id);
            if (emplData.exe)
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"select * from empl where empl_id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            emplData.item = new EmplItem
                            {
                                id = reader["empl_id"].ToString(),
                                name = reader["ch_name"].ToString(),
                                email = reader["email"].ToString(),
                                manager = reader["manager"].ToString() == "Y"
                            };
                        }
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
        }
        private void saveEmplData(TokenItem token)
        {
            emplData.checkPower(setting, token.id);
            if (emplData.edit)
            {
                using (SqlConnection conn = new SqlConnection(settingM))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "sp_updateEMPL";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@orgName", token.orgName));
                    cmd.Parameters.Add(new SqlParameter("@id", emplData.item.id));
                    cmd.Parameters.Add(new SqlParameter("@email", emplData.item.email));
                    cmd.Parameters.Add(new SqlParameter("@name", emplData.item.name));
                    cmd.Parameters.Add(new SqlParameter("@manager", (emplData.item.manager ? "Y" : "N")));
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private bool checkNotLastManager()
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"select count(*) from empl where manager='Y'", conn);
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (int.Parse(reader[0].ToString()) > 1) check = true;
                    }
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            return check;
        }
        private void updattePassWord(TokenItem token, string pwd)
        {
            emplData.checkPower(setting, token.id);
            if (emplData.edit)
            {
                using (SqlConnection conn = new SqlConnection(settingM))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "sp_updateEMPLPassword";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@orgName", token.orgName));
                    cmd.Parameters.Add(new SqlParameter("@id", emplData.item.id));
                    cmd.Parameters.Add(new SqlParameter("@pwd", pwd));
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read()) {
                        code = "500";
                        message = $"該密碼已於{(DateTime.Parse(reader["logtime"].ToString()).ToString("yyyy/MM/dd"))}時被使用，密碼需與前三次設定不同。";
                    }
                    
                }
            }
        }
        private void delEmpl(TokenItem token)
        {
            emplData.checkPower(setting, token.id);
            if (emplData.del) {
                using (SqlConnection conn = new SqlConnection(settingM))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "sp_delEMPL";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@orgName", token.orgName));
                    cmd.Parameters.Add(new SqlParameter("@id", emplData.item.id));
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private bool checkOtherEmpl() {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"select * from empl where email=@email and empl_id!=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@id", emplData.item.id));
                cmd.Parameters.Add(new SqlParameter("@email", emplData.item.email));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        check = true;
                    }
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            return check;
        }
        private bool checkEmpl() {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"select * from empl where empl_id=@id or email=@email", conn);
                cmd.Parameters.Add(new SqlParameter("@id", emplData.item.id));
                cmd.Parameters.Add(new SqlParameter("@email", emplData.item.email));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        check = true;
                    }
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            return check;
        }
        private void InsertEmpl(TokenItem token, string pwd)
        {
            emplData.checkPower(setting, token.id);
            if (emplData.add)
            {
                using (SqlConnection conn = new SqlConnection(settingM))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "sp_insertEMPL";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@orgName", token.orgName));
                    cmd.Parameters.Add(new SqlParameter("@id", emplData.item.id));
                    cmd.Parameters.Add(new SqlParameter("@gid", emplData.item.gid));
                    cmd.Parameters.Add(new SqlParameter("@email", emplData.item.email));
                    cmd.Parameters.Add(new SqlParameter("@name", emplData.item.name));
                    cmd.Parameters.Add(new SqlParameter("@manager", (emplData.item.manager ? "Y" : "N")));
                    cmd.Parameters.Add(new SqlParameter("@pwd", pwd));
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private String printMsg(String RspnCode, String RspnMsg)
        {
            emplData.RspnCode = RspnCode;
            emplData.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(emplData);
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}