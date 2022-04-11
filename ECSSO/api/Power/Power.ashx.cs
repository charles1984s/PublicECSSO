using System;
using System.Collections.Generic;
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
    /// Power 的摘要描述
    /// </summary>
    public class Power : IHttpHandler
    {
        private PowerItems powerItems;
        private EmplObject emplObject;
        private string setting, type;
        public void ProcessRequest(HttpContext context)
        {
            GetStr GS = new GetStr();
            string code, message;
            code = "404";
            message = "not fount";
            powerItems = new PowerItems();
            emplObject = new EmplObject();
            TokenItem token = null;
            try {
                if (context.Request.Form["token"] != null) {
                    token = new TokenItem
                    {
                        token = context.Request.Form["token"]
                    };
                    this.setting = GS.checkToken(token);
                    type = context.Request.Form["type"];
                    if (this.setting.IndexOf("error") < 0) {
                        switch (type) {
                            case "data":
                                int gid = checkGroup(context.Request.Form["id"]);
                                if (gid == 0) getData(token, context.Request.Form["id"]);
                                else getGroupEmplData(token, gid, context.Request.Form["id"]);
                                if (powerItems.group != null && powerItems.group.Count > 0)
                                {
                                    code = "200";
                                    message = "success";
                                }
                                break;
                            case "save":
                                List<savePowerGroup> savePowerGroup = JsonConvert.DeserializeObject<List<savePowerGroup>>(context.Request.Form["data"]);
                                saveData(token, context.Request.Form["id"], savePowerGroup);
                                GS.InsertLog(
                                    GS.GetSetting3(token.orgName),
                                    token.id, "帳號權限設定", "修改", context.Request.Form["id"],
                                    "sp_UpdatePowerTable", "api/Power/Power.ashx");
                                code = "200";
                                message = "success";
                                break;
                            case "group":
                                getGroupData(token, int.Parse(context.Request.Form["id"]));
                                if (powerItems.group != null && powerItems.group.Count > 0)
                                {
                                    code = "200";
                                    message = "success";
                                }
                                break;
                            case "saveGroup":
                                List<savePowerGroup> saveGroup = JsonConvert.DeserializeObject<List<savePowerGroup>>(context.Request.Form["data"]);
                                saveGroupData(token, int.Parse(context.Request.Form["id"]), saveGroup);
                                GS.InsertLog(
                                    GS.GetSetting3(token.orgName),
                                    token.id, "群組權限設定", "修改", context.Request.Form["id"],
                                    "sp_UpdateGroupPowerTable", "api/Power/Power.ashx");
                                code = "200";
                                message = "success";
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
        private DataTable initSaveTable() {
            string[] colFields = { "job_id", "canexe", "canedit", "canadd", "candel" };
            DataTable saveData = new DataTable();
            foreach (string column in colFields)
            {
                DataColumn datecolumn = new DataColumn(column);
                datecolumn.AllowDBNull = true;
                saveData.Columns.Add(datecolumn);
            }
            return saveData;
        }
        private void getAuList(DataTable saveData, List<savePowerGroup> attr,int gid, string id)
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                string sql = "";
                if (gid == 0) sql="select * from authors where empl_id=@id and job_id in(";
                else sql = "select * from authors where cid=@gid and [type]='g' and job_id in(";
                for (int i = 0; i < attr.Count; i++)
                {
                    sql += "@job" + i;
                    if (i < attr.Count - 1) sql += ",";
                }
                sql += ")";
                SqlCommand cmd = new SqlCommand(sql, conn);
                if (gid == 0) cmd.Parameters.Add(new SqlParameter("@id", id));
                else cmd.Parameters.Add(new SqlParameter("@gid", gid));
                for (int i = 0; i < attr.Count; i++)
                {
                    cmd.Parameters.Add(new SqlParameter("@job" + i, attr[i].job));
                }
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string[] row = {
                                reader["job_id"].ToString(),
                                reader["canexe"].ToString(),
                                reader["canedit"].ToString(),
                                reader["canadd"].ToString(),
                                reader["candel"].ToString()
                            };
                        saveData.Rows.Add(row);
                    }
                    for (int i = 0; i < attr.Count; i++)
                    {
                        for (int j = 0; j < saveData.Rows.Count; j++)
                        {
                            if (attr[i].job == saveData.Rows[j][0].ToString())
                            {
                                switch (attr[i].key)
                                {
                                    case "add":
                                        saveData.Rows[j].SetField<string>(3, attr[i].run ? "Y" : "N");
                                        break;
                                    case "edit":
                                        saveData.Rows[j].SetField<string>(2, attr[i].run ? "Y" : "N");
                                        break;
                                    case "del":
                                        saveData.Rows[j].SetField<string>(4, attr[i].run ? "Y" : "N");
                                        break;
                                    case "exe":
                                        saveData.Rows[j].SetField<string>(1, attr[i].run ? "Y" : "N");
                                        break;
                                }
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
        
        private void saveGroupData(TokenItem token, int gid, List<savePowerGroup> attr) {
            emplObject.checkPower(setting, token.id);
            if (emplObject.edit)
            {
                DataTable saveData = initSaveTable();
                getAuList(saveData, attr, gid, "");
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_UpdateGroupPowerTable";
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@id", gid));
                    SqlParameter tvparam = cmd.Parameters.AddWithValue("@table", saveData);
                    tvparam.SqlDbType = SqlDbType.Structured;
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void saveData(TokenItem token, string id, List<savePowerGroup> attr) {
            emplObject.checkPower(setting, token.id);
            if (emplObject.edit) {
                DataTable saveData = initSaveTable();
                getAuList(saveData, attr, 0, id);
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_UpdatePowerTable";
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlParameter tvparam = cmd.Parameters.AddWithValue("@table", saveData);
                    tvparam.SqlDbType = SqlDbType.Structured;
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private int checkGroup(string id) {
            int gid = 0;
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                            select * from Group_Empl where empl_id=@id
                        ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read()) gid = int.Parse(reader["Gid"].ToString());
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            return gid;
        }
        private void initPowerData() {
            powerItems.group = new List<PowerGroup>();
            PowerGroup head = new PowerGroup
            {
                key = "A",
                name = "版型視覺",
                list = new List<PowerItem>()
            };
            PowerGroup map = new PowerGroup
            {
                key = "C",
                name = "網站地圖",
                list = new List<PowerItem>()
            };
            PowerGroup store = new PowerGroup
            {
                key = "E",
                name = "商店管理",
                list = new List<PowerItem>()
            };
            PowerGroup member = new PowerGroup
            {
                key = "F",
                name = "會員管理",
                list = new List<PowerItem>()
            };
            PowerGroup flow = new PowerGroup
            {
                key = "G",
                name = "流量分析",
                list = new List<PowerItem>()
            };
            PowerGroup system = new PowerGroup
            {
                key = "P",
                name = "系統管理",
                list = new List<PowerItem>()
            };
            powerItems.group.Add(head);
            powerItems.group.Add(map);
            powerItems.group.Add(store);
            powerItems.group.Add(member);
            powerItems.group.Add(flow);
            powerItems.group.Add(system);
        }
        private void emplDataInit(string id) {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_initPower";
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@id", id));
                cmd.ExecuteNonQuery();
            }
        }
        private void emplDataInit(int gid)
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_initGroupPower";
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@id", gid));
                cmd.ExecuteNonQuery();
            }
        }
        private void getData(TokenItem token,string id) {
            emplObject.checkPower(setting, token.id);
            if (emplObject.exe)
            {
                emplDataInit(id);
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select a.job_id,a.job_name,a.canadd,a.canedit,a.candel,a.canexe,
                            isnull(b.canadd,'N') as empl_canadd,
                            isnull(b.canedit,'N') as empl_canedit,
                            isnull(b.candel,'N') as empl_candel,
                            isnull(b.canexe,'N') as empl_canexe
                        from webjobs as a 
                        left join authors as b on a.job_id=b.job_id 
                        where a.job_id not like 'M%' and b.empl_id=@id and a.canexe='Y'
                        order by a.job_id
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        initPowerData();
                        while (reader.Read())
                        {
                            PowerItem item = new PowerItem {
                                job = reader["job_id"].ToString(),
                                name = reader["job_name"].ToString(),
                                isSelf = false,
                                add = new PowerCheckItem {
                                    enable = reader["canadd"].ToString() == "Y",
                                    run = (
                                        reader["canadd"].ToString() == "Y" ?
                                        reader["empl_canadd"].ToString() == "Y" :
                                        false
                                    )
                                },
                                edit = new PowerCheckItem
                                {
                                    enable = reader["canedit"].ToString() == "Y",
                                    run = (
                                        reader["canedit"].ToString() == "Y" ?
                                        reader["empl_canedit"].ToString() == "Y" :
                                        false
                                    )
                                },
                                del = new PowerCheckItem
                                {
                                    enable = reader["candel"].ToString() == "Y",
                                    run = (
                                        reader["candel"].ToString() == "Y" ?
                                        reader["empl_candel"].ToString() == "Y" :
                                        false
                                    )
                                },
                                exe = new PowerCheckItem
                                {
                                    enable = reader["canexe"].ToString() == "Y",
                                    run = (
                                        reader["canexe"].ToString() == "Y" ?
                                        reader["empl_canexe"].ToString() == "Y" :
                                        false
                                    )
                                }
                            };
                            for (int i = 0; i< powerItems.group.Count;i++) {
                                if (powerItems.group[i].key == item.job.Substring(0, 1))
                                {
                                    powerItems.group[i].list.Add(item);
                                    break;
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
        }
        private void getGroupData(TokenItem token,int gid) {
            emplObject.checkPower(setting, token.id);
            if (emplObject.exe) {
                emplDataInit(gid);
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select a.job_id,a.job_name,a.canadd,a.canedit,a.candel,a.canexe,
                            isnull(b.canadd,'N') as empl_canadd,
                            isnull(b.canedit,'N') as empl_canedit,
                            isnull(b.candel,'N') as empl_candel,
                            isnull(b.canexe,'N') as empl_canexe
                        from webjobs as a 
                        left join authors as b on a.job_id=b.job_id 
                        where a.job_id not like 'M%' and b.cid=@id and b.[type]='g' and a.canexe='Y'
                        order by a.job_id
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", gid));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        initPowerData();
                        while (reader.Read())
                        {
                            PowerItem item = new PowerItem
                            {
                                job = reader["job_id"].ToString(),
                                name = reader["job_name"].ToString(),
                                isSelf = false,
                                add = new PowerCheckItem
                                {
                                    enable = reader["canadd"].ToString() == "Y",
                                    run = (
                                        reader["canadd"].ToString() == "Y" ?
                                        reader["empl_canadd"].ToString() == "Y" :
                                        false
                                    )
                                },
                                edit = new PowerCheckItem
                                {
                                    enable = reader["canedit"].ToString() == "Y",
                                    run = (
                                        reader["canedit"].ToString() == "Y" ?
                                        reader["empl_canedit"].ToString() == "Y" :
                                        false
                                    )
                                },
                                del = new PowerCheckItem
                                {
                                    enable = reader["candel"].ToString() == "Y",
                                    run = (
                                        reader["candel"].ToString() == "Y" ?
                                        reader["empl_candel"].ToString() == "Y" :
                                        false
                                    )
                                },
                                exe = new PowerCheckItem
                                {
                                    enable = reader["canexe"].ToString() == "Y",
                                    run = (
                                        reader["canexe"].ToString() == "Y" ?
                                        reader["empl_canexe"].ToString() == "Y" :
                                        false
                                    )
                                }
                            };
                            for (int i = 0; i < powerItems.group.Count; i++)
                            {
                                if (powerItems.group[i].key == item.job.Substring(0, 1))
                                {
                                    powerItems.group[i].list.Add(item);
                                    break;
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
        }
        private void getGroupEmplData(TokenItem token, int gid, string id)
        {
            getGroupData(token, gid);
            if (emplObject.exe)
            {
                emplDataInit(id);
                using (SqlConnection conn = new SqlConnection(setting)) {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select a.job_id,a.job_name,a.canadd,a.canedit,a.candel,a.canexe,
                            isnull(b.canadd,'N') as empl_canadd,
                            isnull(b.canedit,'N') as empl_canedit,
                            isnull(b.candel,'N') as empl_candel,
                            isnull(b.canexe,'N') as empl_canexe
                        from webjobs as a 
                        left join authors as b on a.job_id=b.job_id 
                        where a.job_id not like 'M%' and b.empl_id=@id and b.[dafault]='N' and a.canexe='Y'
                        order by a.job_id
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = null;
                    try {
                        reader = cmd.ExecuteReader();
                        while (reader.Read()) {
                            for (int i = 0; i < powerItems.group.Count; i++)
                            {
                                string jobID = reader["job_id"].ToString();
                                if (powerItems.group[i].key == jobID.Substring(0, 1))
                                {
                                    List<PowerItem> list = powerItems.group[i].list;
                                    for (int j = 0; j < list.Count; j++)
                                    {
                                        if (list[j].job == jobID) {
                                            list[j].isSelf = true;
                                            if (list[j].del.enable) {
                                                list[j].del.run = (
                                                    reader["candel"].ToString() == "Y" ?
                                                    reader["empl_candel"].ToString() == "Y" :
                                                    false
                                                );
                                            }
                                            if (list[j].add.enable)
                                            {
                                                list[j].add.run = (
                                                    reader["canadd"].ToString() == "Y" ?
                                                    reader["empl_canadd"].ToString() == "Y" :
                                                    false
                                                );
                                            }
                                            if (list[j].exe.enable)
                                            {
                                                list[j].exe.run = (
                                                    reader["canexe"].ToString() == "Y" ?
                                                    reader["empl_canexe"].ToString() == "Y" :
                                                    false
                                                );
                                            }
                                            if (list[j].edit.enable)
                                            {
                                                list[j].edit.run = (
                                                    reader["canedit"].ToString() == "Y" ?
                                                    reader["empl_canedit"].ToString() == "Y" :
                                                    false
                                                );
                                            }
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
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
            powerItems.RspnCode = RspnCode;
            powerItems.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(powerItems);
        }
    }
}