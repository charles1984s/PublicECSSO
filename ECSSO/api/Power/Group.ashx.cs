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
    /// Group 的摘要描述
    /// </summary>
    public class Group : IHttpHandler
    {
        private string setting, type;
        private GroupResponse response;
        public void ProcessRequest(HttpContext context)
        {
            GetStr GS = new GetStr();
            string code, message;
            code = "404";
            message = "not fount";
            TokenItem token = null;
            response = new GroupResponse();
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
                            case "new":
                                if (context.Request.Form["name"] != null)
                                {
                                    response.name = context.Request.Form["name"];
                                    addGroup(token);
                                    GS.InsertLog(
                                        GS.GetSetting3(token.orgName),
                                        token.id, "群組管理", "新增", response.id.ToString(),
                                        "insert", "api/Power/Group.ashx");
                                    code = "200";
                                    message = "success";
                                }
                                break;
                            case "edit":
                                if (context.Request.Form["id"] != null && context.Request.Form["name"] != null)
                                {
                                    response.id = int.Parse(context.Request.Form["id"]);
                                    response.name = context.Request.Form["name"];
                                    editGroup(token);
                                    GS.InsertLog(
                                        GS.GetSetting3(token.orgName),
                                        token.id, "群組管理", "修改", response.id.ToString(),
                                        "update", "api/Power/Group.ashx");
                                    code = "200";
                                    message = "success";
                                }
                                break;
                            case "del":
                                if (context.Request.Form["id"] != null)
                                {
                                    response.id = int.Parse(context.Request.Form["id"]);
                                    delGroup(token);
                                    GS.InsertLog(
                                        GS.GetSetting3(token.orgName),
                                        token.id, "群組管理", "刪除", response.id.ToString(),
                                        "update", "api/Power/Group.ashx");
                                    code = "200";
                                    message = "success";
                                }
                                break;
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
        private void editGroup(TokenItem token)
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"update Group_Head set Title=@name where Gid=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@name", response.name));
                cmd.Parameters.Add(new SqlParameter("@id", response.id));
                SqlDataReader reader = null;
                try
                {
                    cmd.ExecuteReader();
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
        private void addGroup(TokenItem token) {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    insert into Group_Head(Title)
                    OUTPUT Inserted.Gid
                    values(@name)", conn);
                cmd.Parameters.Add(new SqlParameter("@name", response.name));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read()) response.id = int.Parse(reader["Gid"].ToString());
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
        private void delGroup(TokenItem token)
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    delete authors where [type]='g' and cid=@Gid;
                    delete Group_authority where Gid=@Gid;
                    delete Group_Empl where Gid=@Gid;
                    delete Group_Head where Gid=@Gid;
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@Gid", response.id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read()) response.id = int.Parse(reader["Gid"].ToString());
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        private String printMsg(String RspnCode, String RspnMsg)
        {
            response.RspnCode = RspnCode;
            response.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(response);
        }
    }
}