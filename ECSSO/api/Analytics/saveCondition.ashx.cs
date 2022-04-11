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
    /// saveCondition 的摘要描述
    /// </summary>
    public class saveCondition : IHttpHandler
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
            try {
                if (context.Request.Form["token"] != null) {
                    token = new TokenItem
                    {
                        token = context.Request.Form["token"]
                    };
                    this.setting = GS.checkToken(token);
                    type = context.Request.Form["type"];
                    if (this.setting.IndexOf("error") < 0) {
                        int id;
                        switch (type)
                        {
                            case "new":
                                Condition condition = JsonConvert.DeserializeObject<Condition>(context.Request.Form["Items"]);
                                for (int i = 0; i < condition.items.Count; i++)
                                {
                                    CustTableCondition item = condition.items[i];
                                    addCondition(condition.id, item);
                                    for (int j=0; j< item.Condition.Count; j++) {
                                        CustTableConditionItem condItem = item.Condition[j];
                                        if (item.Condition.Count == 1) condItem.next = "or";
                                        addConditionItem(item.id, condItem);
                                    }
                                }
                                responseJson = condition;
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
                                delConditionItem(id);
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
        public void addCondition(int tid, CustTableCondition item) {
            if (GS.hasPwoer(setting, "G002", "canadd", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
if not exists(select * from CustTableCondition where tid=@tid and [type]=@type) begin
	insert into CustTableCondition(tid,title,[type])
	output inserted.id
	values(@tid,@title,@type)
end else begin
	select id from CustTableCondition where tid=@tid and [type]=@type
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
        public void addConditionItem(int cid, CustTableConditionItem item)
        {
            if (GS.hasPwoer(setting, "G002", "canadd", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
if not exists(select * from CustTableConditionItem where cid=@cid and [value]=@value and [type]=@type) begin
	insert into CustTableConditionItem(cid,title,[type],value,[next])
	output inserted.id
	values(@cid,@title,@type,@value,@next)
end else begin
	select id from CustTableConditionItem where cid=@cid and [type]=@type
end
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@cid", cid));
                    cmd.Parameters.Add(new SqlParameter("@title", item.title));
                    cmd.Parameters.Add(new SqlParameter("@type", item.type));
                    cmd.Parameters.Add(new SqlParameter("@value", item.value));
                    cmd.Parameters.Add(new SqlParameter("@next", item.next));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        if (reader.Read()) item.id = int.Parse(reader["id"].ToString());
                    }
                    catch(Exception e)
                    {
                        throw new Exception("資料錯誤:"+e.Message);
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
            else throw new Exception("沒有權限");
        }
        public void delConditionItem(int id)
        {
            if (GS.hasPwoer(setting, "G002", "candel", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        delete CustTableConditionItem where cid in(select id from CustTableCondition where id=@id)
                        delete CustTableCondition where id=@id
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