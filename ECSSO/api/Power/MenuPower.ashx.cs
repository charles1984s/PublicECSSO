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
    /// MenuPower 的摘要描述
    /// </summary>
    public class MenuPower : IHttpHandler
    {
        private EmplItems EmplItems;
        private EmplObject emplObject;
        private string setting, type;
        public void ProcessRequest(HttpContext context)
        {
            GetStr GS = new GetStr();
            string code, message;
            code = "404";
            message = "not fount";
            EmplItems = new EmplItems();
            emplObject = new EmplObject();
            TokenItem token = null;
            try {
                if (context.Request.Form["token"] != null) {
                    token = new TokenItem
                    {
                        token = context.Request.Form["token"]
                    };
                    this.setting = GS.checkToken(token);
                    if (this.setting.IndexOf("error") < 0) {
                        type = context.Request.Form["type"];
                        switch (type) {
                            case "group":
                                getGroupList(token,int.Parse(context.Request.Form["id"]));
                                break;
                            case "groupCanAdd":
                                getGroupCanAddList(token, int.Parse(context.Request.Form["id"]));
                                break;
                            case "empl":
                                getEmplList(token,int.Parse(context.Request.Form["id"]));
                                break;
                            case "emplCanAdd":
                                getEmplCanAddList(token, int.Parse(context.Request.Form["id"]));
                                break;
                            case "groupAdd":
                                addMenuGroup(token, int.Parse(context.Request.Form["id"]), int.Parse(context.Request.Form["gid"]));
                                GS.InsertLog(
                                    GS.GetSetting3(token.orgName),
                                    token.id, "選單群組權限設定", "修改", context.Request.Form["id"],
                                    "insert "+ context.Request.Form["gid"], "api/Power/MenuPower.ashx");
                                break;
                            case "emplAdd":
                                addMenuEmpl(token, int.Parse(context.Request.Form["id"]), context.Request.Form["empl_id"]);
                                GS.InsertLog(
                                    GS.GetSetting3(token.orgName),
                                    token.id, "選單帳號權限設定", "修改", context.Request.Form["id"],
                                    "insert "+ context.Request.Form["empl_id"], "api/Power/MenuPower.ashx");
                                break;
                            case "groupDel":
                                delMenuGroup(token, int.Parse(context.Request.Form["id"]), int.Parse(context.Request.Form["gid"]));
                                GS.InsertLog(
                                   GS.GetSetting3(token.orgName),
                                   token.id, "選單群組權限設定", "刪除", context.Request.Form["id"],
                                   "insert " + context.Request.Form["gid"], "api/Power/MenuPower.ashx");
                                break;
                            case "emplDel":
                                delMenuEmpl(token, int.Parse(context.Request.Form["id"]), context.Request.Form["empl_id"]);
                                GS.InsertLog(
                                    GS.GetSetting3(token.orgName),
                                    token.id, "選單帳號權限設定", "刪除", context.Request.Form["id"],
                                    "insert " + context.Request.Form["empl_id"], "api/Power/MenuPower.ashx");
                                break;
                        }
                        code = "200";
                        message = "success";
                    }
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
        private void getGroupCanAddList(TokenItem token, int id)
        {
            emplObject.checkPower(setting, token.id);
            if (emplObject.exe)
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select * from Group_Head where Gid not in(
                            select empl_id from limit where [type]='G' and menu_id=@menu_id and menu_type='1'
                        )
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@menu_id", id));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        EmplItems.list = new List<EmplItem>();
                        while (reader.Read())
                        {
                            EmplItem EmplItem = new EmplItem
                            {
                                id = reader["Gid"].ToString(),
                                name = reader["title"].ToString(),
                            };
                            EmplItems.list.Add(EmplItem);
                        }
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
        }
        private void getGroupList(TokenItem token, int id)
        {
            emplObject.checkPower(setting, token.id);
            if (emplObject.exe)
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select * from Group_Head where Gid in(
                            select empl_id from limit where [type]='G' and menu_id=@menu_id and menu_type='1'
                        )
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@menu_id", id));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        EmplItems.list = new List<EmplItem>();
                        while (reader.Read())
                        {
                            EmplItem EmplItem = new EmplItem
                            {
                                id = reader["Gid"].ToString(),
                                name = reader["title"].ToString(),
                            };
                            EmplItems.list.Add(EmplItem);
                        }
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
        }
        private void getEmplList(TokenItem token,int id)
        {
            emplObject.checkPower(setting, token.id);
            if (emplObject.exe) {
                using (SqlConnection conn = new SqlConnection(setting)) {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select empl_id,ch_name from EMPL where empl_id in(
                            select empl_id from limit where [type]='M' and menu_type='1' and menu_id=@menu_id
                        )
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@menu_id", id));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        EmplItems.list = new List<EmplItem>();
                        while (reader.Read()) {
                            EmplItem EmplItem = new EmplItem
                            {
                                id = reader["empl_id"].ToString(),
                                name = reader["ch_name"].ToString(),
                            };
                            EmplItems.list.Add(EmplItem);
                        }
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
        }
        private void getEmplCanAddList(TokenItem token, int id)
        {
            emplObject.checkPower(setting, token.id);
            if (emplObject.exe)
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select empl_id,ch_name from EMPL where empl_id not in(
                            select empl_id from limit where [type]='M' and menu_type='1' and menu_id=@menu_id
                        )
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@menu_id", id));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        EmplItems.list = new List<EmplItem>();
                        while (reader.Read())
                        {
                            EmplItem EmplItem = new EmplItem
                            {
                                id = reader["empl_id"].ToString(),
                                name = reader["ch_name"].ToString(),
                            };
                            EmplItems.list.Add(EmplItem);
                        }
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
        }
        private void addMenuGroup(TokenItem token, int id, int Gid) {
            emplObject.checkPower(setting, token.id);
            if (emplObject.add)
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        insert into limit(empl_id,menu_id,menu_type,canedit,canexe,[type])values
                        (@empl_id,@menu_id,1,'Y','Y','G')
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@menu_id", id));
                    cmd.Parameters.Add(new SqlParameter("@empl_id", Gid));
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
        }
        private void addMenuEmpl(TokenItem token, int id, string empl_id)
        {
            emplObject.checkPower(setting, token.id);
            if (emplObject.add)
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        insert into limit(empl_id,menu_id,menu_type,canedit,canexe,[type])values
                        (@empl_id,@menu_id,1,'Y','Y','M')
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@menu_id", id));
                    cmd.Parameters.Add(new SqlParameter("@empl_id", empl_id));
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
        }
        private void delMenuGroup(TokenItem token, int id, int Gid)
        {
            emplObject.checkPower(setting, token.id);
            if (emplObject.del)
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        delete limit where empl_id=@empl_id and menu_id=@menu_id and menu_type=1 and [type]='G'
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@menu_id", id));
                    cmd.Parameters.Add(new SqlParameter("@empl_id", Gid));
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
        }
        private void delMenuEmpl(TokenItem token, int id, string empl_id)
        {
            emplObject.checkPower(setting, token.id);
            if (emplObject.del)
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        delete limit where empl_id=@empl_id and menu_id=@menu_id and menu_type=1 and [type]='M'
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@menu_id", id));
                    cmd.Parameters.Add(new SqlParameter("@empl_id", empl_id));
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
        }

        private String printMsg(String RspnCode, String RspnMsg)
        {
            EmplItems.RspnCode = RspnCode;
            EmplItems.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(EmplItems);
        }
    }
}