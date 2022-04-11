using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using ECSSO.Library;
using ECSSO.Library.Power;
using Newtonsoft.Json;

namespace ECSSO.api.Power
{
    /// <summary>
    /// Empl 的摘要描述
    /// </summary>
    public class Empl : IHttpHandler
    {
        private EmplItems emplItems;
        private string setting, type;
        public void ProcessRequest(HttpContext context)
        {
            GetStr GS = new GetStr();
            string code, message;
            code = "404";
            message = "not fount";
            emplItems = new EmplItems();
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
                            case "myOption":
                                getTheEmplPower(token);
                                code = "200";
                                message = "success";
                                break;
                            case "group":
                                getGroup(token);
                                break;
                            case "group-empl":
                                if (!string.IsNullOrEmpty(context.Request.Form["groupid"]))
                                    getGroupEmpl(token, context.Request.Form["groupid"]);
                                break;
                            case "empl":
                                getEmpl(token);
                                break;
                            case "toGroup":
                                if (!string.IsNullOrEmpty(context.Request.Form["groupid"]))
                                {
                                    setToGroup(int.Parse(context.Request.Form["groupid"]),
                                        JsonConvert.DeserializeObject<List<string >>(context.Request.Form["list"])
                                    );
                                }
                                code = "200";
                                message = "success";
                                break;
                            case "leave":
                                leaveGroup(
                                    JsonConvert.DeserializeObject<List<string>>(context.Request.Form["list"])
                                );
                                code = "200";
                                message = "success";
                                break;
                        }
                        if (emplItems.list!=null && emplItems.list.Count>0) {
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

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        private void getTheEmplPower(TokenItem token)
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"sp_getEmplThePower @emplID,'P001'", conn);
                cmd.Parameters.Add(new SqlParameter("@emplID", token.id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        emplItems.add = reader["canadd"].ToString() == "Y";
                        emplItems.del = reader["candel"].ToString() == "Y";
                        emplItems.edit = reader["canedit"].ToString() == "Y";
                        emplItems.exe = reader["canexe"].ToString() == "Y";
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
        private void leaveGroup(List<string> list) {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                string sql = "delete Group_Empl where empl_id in(";
                for (int i = 0; i < list.Count; i++)
                {
                    sql += "@empl_id" + i ;
                    if (i < list.Count - 1) sql += ",";
                }
                sql += ")";
                SqlCommand cmd = new SqlCommand(sql, conn);
                for (int i = 0; i < list.Count; i++)
                {
                    cmd.Parameters.Add(new SqlParameter("@empl_id" + i, list[i]));
                }
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
        private void setToGroup(int gid,List<string> list) {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                string sql = "insert into Group_Empl(Gid,empl_id)values";
                for (int i = 0; i < list.Count; i++)
                {
                    sql += "(@gid,@empl_id" + i + ")";
                    if (i < list.Count - 1) sql += ",";
                }
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@gid", gid));
                for (int i = 0; i < list.Count; i++)
                {
                    cmd.Parameters.Add(new SqlParameter("@empl_id" + i, list[i]));
                }
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
        private void getEmpl(TokenItem token)
        {
            emplItems.checkPower(setting,token.id);
            if (emplItems.exe)
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"select * from EMPL where empl_id not in(select empl_id from Group_Empl)", conn);
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        emplItems.list = new List<EmplItem>();
                        while (reader.Read())
                        {
                            EmplItem item = new EmplItem
                            {
                                id = reader["empl_id"].ToString(),
                                name = reader["ch_name"].ToString()
                            };
                            emplItems.list.Add(item);
                        }
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
        }
        private void getGroupEmpl(TokenItem token, string gid)
        {
            emplItems.checkPower(setting, token.id);
            if (emplItems.exe)
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"select b.* from Group_Empl as a left join EMPL as b on a.empl_id=b.empl_id where a.Gid=@Gid", conn);
                    cmd.Parameters.Add(new SqlParameter("@Gid", gid));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        emplItems.list = new List<EmplItem>();
                        while (reader.Read())
                        {
                            EmplItem item = new EmplItem
                            {
                                id = reader["empl_id"].ToString(),
                                name = reader["ch_name"].ToString()
                            };
                            emplItems.list.Add(item);
                        }
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
        }
        private void getGroup(TokenItem token)
        {
            emplItems.checkPower(setting, token.id);
            if (emplItems.exe)
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"select * from Group_Head", conn);
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        emplItems.list = new List<EmplItem>();
                        while (reader.Read())
                        {
                            EmplItem item = new EmplItem
                            {
                                id = reader["Gid"].ToString(),
                                name = reader["Title"].ToString()
                            };
                            emplItems.list.Add(item);
                        }
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
        }
        private String printMsg(String RspnCode, String RspnMsg)
        {
            emplItems.RspnCode = RspnCode;
            emplItems.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(emplItems);
        }
    }
}