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
    /// getTheCustTable 的摘要描述
    /// </summary>
    public class getTheCustTable : IHttpHandler
    {
        private string setting;
        private CustTable table;
        private GetStr GS;
        TokenItem token;
        public void ProcessRequest(HttpContext context)
        {
            GS = new GetStr();
            string code, message;
            code = "404";
            message = "not fount";
            try {
                table = new CustTable();
                if (context.Request.Form["token"] != null) {
                    token = new TokenItem
                    {
                        token = context.Request.Form["token"]
                    };
                    this.setting = GS.checkToken(token);
                    if (this.setting.IndexOf("error") < 0) {
                        if (context.Request.Form["id"]!=null) {
                            getCustTable(int.Parse(context.Request.Form["id"]));
                            code = "200";
                            message = "success";
                        }
                        else throw new Exception("工作不存在");
                    }
                    else
                    {
                        code = "401";
                        message = "Token已過期" + this.setting;
                    }
                }
                else throw new Exception("Token不存在");
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
        public void getCustTable(int id) {
            if (GS.hasPwoer(setting, "G002", "canedit", token.id)) {
                using (SqlConnection conn = new SqlConnection(setting)) {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select *,convert(nvarchar,[start],111) startT,convert(nvarchar,[end],111) endT 
                        from custFlowTable where id=@id
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = null;
                    try {
                        reader = cmd.ExecuteReader();
                        if (reader.Read()) {
                            table.title = reader["title"].ToString();
                            table.result = int.Parse(reader["result"].ToString());
                            table.type = reader["type"].ToString();
                            table.start = reader["startT"].ToString();
                            table.end = reader["endT"].ToString();
                            table.value = reader["value"].ToString();
                        }
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
                table.aime = getCustTableAime(id);
                table.condition = getCustTableCondition(id);
            }
            else throw new Exception("沒有權限");
        }
        private List<CustTableAime> getCustTableAime(int id) {
            List<CustTableAime> Aime = new List<CustTableAime>();
            using (SqlConnection conn = new SqlConnection(setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                        select * from CustTableAime where tid=@id
                    ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Aime.Add(new CustTableAime {
                            id= int.Parse(reader["id"].ToString()),
                            title= reader["title"].ToString(),
                            type= reader["type"].ToString()
                        });
                    }
                }
                finally
                {
                    if(reader!=null) reader.Close();
                }
            }
            return Aime;
        }
        private List<CustTableCondition> getCustTableCondition(int id)
        {
            List<CustTableCondition> Condition = new List<CustTableCondition>();
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                        select * from CustTableCondition where tid=@id
                    ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Condition.Add(new CustTableCondition
                        {
                            id = int.Parse(reader["id"].ToString()),
                            title = reader["title"].ToString(),
                            type = reader["type"].ToString(),
                            Condition = getCustTableConditionItem(int.Parse(reader["id"].ToString())),
                            nextCondition = reader["next"].ToString()
                        });
                    }
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            return Condition;
        }
        private List<CustTableConditionItem> getCustTableConditionItem(int id)
        {
            List<CustTableConditionItem> ConditionItem = new List<CustTableConditionItem>();
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                        select * from CustTableConditionItem where cid=@id
                    ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        ConditionItem.Add(new CustTableConditionItem
                        {
                            id = int.Parse(reader["id"].ToString()),
                            title = reader["title"].ToString(),
                            type = reader["type"].ToString(),
                            value = reader["value"].ToString(),
                            next = reader["next"].ToString()
                        });
                    }
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            return ConditionItem;
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
            table.RspnCode = RspnCode;
            table.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(table);
        }
    }
}