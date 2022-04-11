using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using ECSSO.Library;
using Newtonsoft.Json;

namespace ECSSO.api.Analytics
{
    /// <summary>
    /// SaveCustTable 的摘要描述
    /// </summary>
    public class SaveCustTable : IHttpHandler
    {
        GetStr GS;
        TokenItem token;
        responseJson responseJson;
        string setting;
        public void ProcessRequest(HttpContext context)
        {
            string code, message, type, myType, start, end;
            int value;
            code = "404";
            message = "not fount";
            token = null;
            GS = new GetStr();
            responseJson = new responseJson();
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
                        int id,result;
                        switch (type)
                        {
                            case "new":
                                if (!string.IsNullOrEmpty(context.Request.Form["title"]) &&
                                    context.Request.Form["title"].Trim() != ""
                                ) newTable(context.Request.Form["title"].Trim());
                                else throw new Exception("標題不可為空");
                                GS.InsertLog(
                                    this.setting,
                                    token.id, "自訂報表", "新增報表", context.Request.Form["title"].Trim(),
                                    "",
                                    "api/Analytics/SaveCustTable.ashx"
                                );
                                break;
                            case "del":
                                try
                                {
                                    id = int.Parse(context.Request.Form["id"]);
                                }
                                catch
                                {
                                    throw new Exception("資料格式錯誤");
                                }
                                delTable(id);
                                GS.InsertLog(
                                    this.setting,
                                    token.id, "自訂報表", "刪除報表", id.ToString(),
                                    "delete custFlowTable where id=@id",
                                    "api/Analytics/SaveCustTable.ashx"
                                );
                                break;
                            case "edit":
                                try
                                {
                                    id = int.Parse(context.Request.Form["id"]);
                                    result = int.Parse(context.Request.Form["result"]);
                                    myType= context.Request.Form["myType"];
                                    start = context.Request.Form["start"];
                                    end= context.Request.Form["end"];
                                    value = int.Parse(context.Request.Form["value"]);
                                }
                                catch
                                {
                                    throw new Exception("資料格式錯誤");
                                }
                                editTable(id, context.Request.Form["title"], result, myType, start, end, value);
                                GS.InsertLog(
                                    this.setting,
                                    token.id, "自訂報表", "修改報表", id.ToString(),
                                    "{title:'"+ context.Request.Form["title"] + 
                                    "',result:'"+ result +
                                    "',type:'" + myType +
                                    "',start:'" + start +
                                    "',end:'" + end +
                                    "',value:'" + value +
                                    "'}",
                                    "api/Analytics/SaveCustTable.ashx"
                                );
                                break;
                            default:
                                throw new Exception("操作不存在");
                        }
                        code = "200";
                        message = "儲存成功";
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
        private void editTable(int id,string title,int result, string type, string start, string end, int value)
        {
            if (GS.hasPwoer(setting, "G002", "canedit", token.id))
            {
                if (string.IsNullOrEmpty(title)) {
                    throw new Exception("資料錯誤");
                }
                using (SqlConnection conn = new SqlConnection(setting)) {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        update custFlowTable set 
                            title=@title,type=@type,[start]=@start,[end]=@end,[value]=@value,
                            result=@result,eUser=@user,edate=getdate()
                    where id=@id
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@title", title));
                    cmd.Parameters.Add(new SqlParameter("@type", type));
                    cmd.Parameters.Add(new SqlParameter("@start", start));
                    cmd.Parameters.Add(new SqlParameter("@end", end));
                    cmd.Parameters.Add(new SqlParameter("@value", value));
                    cmd.Parameters.Add(new SqlParameter("@result", result));
                    cmd.Parameters.Add(new SqlParameter("@user", token.id));
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    try
                    {
                        cmd.ExecuteReader();
                    }
                    catch
                    {
                        throw new Exception("資料錯誤:");
                    }
                }
            }else throw new Exception("權限不足");
        }
        private void delTable(int id)
        {
            if (GS.hasPwoer(setting, "G002", "candel", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        delete custFlowTable where id=@id
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    try
                    {
                        cmd.ExecuteReader();
                    }
                    catch
                    {
                        throw new Exception("資料錯誤");
                    }
                }
            }
            else throw new Exception("沒有權限");
        }
        private void newTable(string title)
        {
            if (GS.hasPwoer(setting, "G002", "canadd", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        insert into custFlowTable(title,cUser,eUser)
                        output inserted.id
                        values(@title,@user,@user)
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@title", title));
                    cmd.Parameters.Add(new SqlParameter("@user", token.id));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            responseJson.Token = reader["id"].ToString();
                        }
                    }
                    catch
                    {
                        throw new Exception("資料錯誤");
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
            else throw new Exception("沒有權限");
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
            responseJson.RspnCode = RspnCode;
            responseJson.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(responseJson);
        }
    }
}