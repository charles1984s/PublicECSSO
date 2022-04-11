using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using ECSSO.Library;
using ECSSO.Library.Analytics;
using Newtonsoft.Json;

namespace ECSSO.api.Analytics
{
    /// <summary>
    /// saveAims 的摘要描述
    /// </summary>
    public class saveAims : IHttpHandler
    {
        GetStr GS;
        TokenItem token;
        responseJson responseJson;
        string setting;
        public void ProcessRequest(HttpContext context)
        {
            string code, message, type;
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
                        int id;
                        switch (type)
                        {
                            case "new":
                                Aims aims = JsonConvert.DeserializeObject<Aims>(context.Request.Form["Items"]);
                                for (int i = 0; i < aims.items.Count; i++)
                                {
                                    addAims(aims.id, aims.items[i]);
                                    GS.InsertLog(
                                        this.setting,
                                        token.id, "自訂報表", "新增目標", aims.id.ToString(),
                                        JsonConvert.SerializeObject(aims.items[i]),
                                        "api/Analytics/saveAims.ashx"
                                    );
                                }
                                responseJson = aims;
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
                                delAims(id);
                                GS.InsertLog(
                                    this.setting,
                                    token.id, "自訂報表", "刪除目標", id.ToString(),
                                    "delete CustTableAime where id=@id",
                                    "api/Analytics/saveAims.ashx"
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

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        private void addAims(int tid, CustTableAime item)
        {
            if (GS.hasPwoer(setting, "G002", "canadd", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
if not exists(select * from CustTableAime where tid=@tid and [type]=@type) begin
	insert into CustTableAime(tid,title,[type])
	output inserted.id
	values(@tid,@title,@type)
end else begin
	select id from CustTableAime where tid=@tid and [type]=@type
end
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@tid", tid));
                    cmd.Parameters.Add(new SqlParameter("@title", item.title));
                    cmd.Parameters.Add(new SqlParameter("@type", item.type));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        if (reader.Read()) item.id = int.Parse(reader["id"].ToString());
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
        private void delAims(int id)
        {
            if (GS.hasPwoer(setting, "G002", "candel", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        delete CustTableAime where id=@id
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
        private String printMsg(String RspnCode, String RspnMsg)
        {
            responseJson.RspnCode = RspnCode;
            responseJson.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(responseJson);
        }
    }
}