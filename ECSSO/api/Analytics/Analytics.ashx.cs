using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web;
using ECSSO.Library;
using ECSSO.Library.Analytics;
using ECSSO.Library.Coupon;
using ECSSO.Library.CustFormLibary;
using Newtonsoft.Json;

namespace ECSSO.api.Analytics
{
    /// <summary>
    /// Analytics 的摘要描述
    /// </summary>
    public class Analytics : IHttpHandler
    {
        private string setting, start, end;
        private Flow flow;
        private GetStr GS;
        TokenItem token;
        public void ProcessRequest(HttpContext context)
        {
            GS = new GetStr();
            string code, message, serverIP;
            code = "404";
            message = "not fount";
            serverIP = "10.250.250.57";
            try
            {
                flow = new Flow();
                if (context.Request.Form["token"] != null)
                {
                    token = new TokenItem
                    {
                        token = context.Request.Form["token"]
                    };
                    if (GS.GetIPAddress() == serverIP) this.setting = GS.checkTokenByServer(token);
                    else this.setting = GS.checkToken(token);
                    if (this.setting.IndexOf("error") < 0)
                    {
                        switch (context.Request.Form["type"])
                        {
                            case "all":
                                if (!string.IsNullOrEmpty(context.Request.Form["start"]) &&
                                    !string.IsNullOrEmpty(context.Request.Form["end"]))
                                {
                                    start = context.Request.Form["start"];
                                    end = context.Request.Form["end"];
                                    loadAllData();
                                }
                                else throw new Exception("參數錯誤");
                                break;
                            case "custTable":
                                loadCustTable();
                                break;
                            case "custTableReport":
                                loadCustTableReport(int.Parse(context.Request.Form["id"]));
                                break;
                            default:
                                throw new Exception("工作不存在");
                        }
                        code = "200";
                        message = "success";
                    }
                    else
                    {
                        code = "401";
                        message = "Token已過期";
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

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        private void loadCustTable()
        {
            flow.table = new FlowData
            {
                sorting = new FormSort
                {
                    enabled = true
                },
                empty = "查無資料",
                columns = crateCustTableColumn(),
                rows = new List<FlowItem>()
            };
            if (GS.hasPwoer(setting, "G002", "canexe", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select
	                        t.id,t.title,e1.ch_name cName,e2.ch_name eName,
	                        convert(nvarchar,t.cdate,120) cdate,convert(nvarchar,t.edate,120) edate
                        from custFlowTable as t
                        left join EMPL as e1 on t.cUser=e1.empl_id
                        left join EMPL as e2 on t.eUser=e2.empl_id
                        order by t.edate
                    ", conn);
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            flow.table.rows.Add(new FlowItem
                            {
                                options = new FormOptions
                                {
                                    classes = "cell",
                                    expanded = true
                                },
                                value = new CustTableListItem
                                {
                                    id = int.Parse(reader["id"].ToString()),
                                    title = reader["title"].ToString(),
                                    cdate = reader["cdate"].ToString(),
                                    edate = reader["edate"].ToString(),
                                    cuser = reader["cName"].ToString(),
                                    euser = reader["eName"].ToString()
                                }
                            });
                        }
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
            else flow.table.empty = "沒有權限";
        }

        private void loadCustTableReport(int id)
        {
            flow.table = new FlowData
            {
                sorting = new FormSort
                {
                    enabled = true
                },
                empty = "查無資料",
                columns = crateCustTableReportColumn(id),
                rows = new List<FlowItem>()
            };
            if (GS.hasPwoer(setting, "G002", "canexe", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_getCustTableAnalytics";
                    cmd.Connection = conn;
                    cmd.CommandTimeout = 300;
                    cmd.Parameters.Add(new SqlParameter("@tid", id));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            flow.table.rows.Add(new FlowItem
                            {
                                options = new FormOptions
                                {
                                    classes = "cell",
                                    expanded = true
                                },
                                value = new ReportTableItem
                                {
                                    age = int.Parse(reader["age"].ToString()),
                                    sex = reader["sex"].ToString() == "0" ? "女" : (reader["sex"].ToString() == "1"?"男":"其他"),
                                    marry = reader["marry"].ToString() == "1" ? "已婚" : "未婚",
                                    org = reader["org"].ToString(),
                                    chf = reader["chf"].ToString(),
                                    crk = reader["crk"].ToString(),
                                    POPESN = reader["POPESN"].ToString(),
                                    TSER_day = int.Parse(reader["TSER_day"].ToString()),
                                    ETP = reader["ETP"].ToString(),
                                    ESU = reader["ESU"].ToString(),
                                    ELV = reader["ELV"].ToString(),
                                    menuSub = reader["menuSub"].ToString(),
                                    menu = reader["menu"].ToString(),
                                    tag = reader["tag"].ToString(),
                                    orgLevel = reader["orgLevel"].ToString(),
                                    name = reader["name"].ToString() + "(" + reader["account"].ToString() + ")",
                                    peopleTh = int.Parse(reader["peopleTh"].ToString()),
                                    zip = reader["zipName"].ToString(),
                                    count = int.Parse(reader["n"].ToString()),
                                    countOfperson = int.Parse(reader["n2"].ToString())
                                }
                            });
                        }
                    }
                    catch (Exception e) {
                        throw new Exception("sql 等待過久");
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
            else flow.table.empty = "沒有權限";
        }
        private void loadAllData()
        {
            flow.table = new FlowData
            {
                sorting = new FormSort
                {
                    enabled = true
                },
                empty = "查無資料",
                columns = crateAllColumn(),
                rows = new List<FlowItem>()
            };
            if (GS.hasPwoer(setting, "G001", "canexe", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select xdate,COUNT(xdate) n,
                        (
	                        select count(*) from(
		                        select ip from [remote] as r2 where r2.xdate=r.xdate group by r2.ip,r2.mem_id
	                        ) a
                        ) n2
                        from [remote] as r
                        where convert(datetime,xdate) between convert(datetime,@start) and convert(datetime,@end)
                        group by xdate
                        order by convert(datetime,xdate) desc
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@start", start));
                    cmd.Parameters.Add(new SqlParameter("@end", end));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        int i = 0;
                        while (reader.Read())
                        {
                            flow.table.rows.Add(new FlowItem
                            {
                                options = new FormOptions
                                {
                                    classes = "cell",
                                    expanded = true
                                },
                                value = new AlltableItem
                                {
                                    id = ++i,
                                    count = int.Parse(reader["n"].ToString()),
                                    countOfperson = int.Parse(reader["n2"].ToString()),
                                    date = reader["xdate"].ToString()
                                }
                            });
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            else flow.table.empty = "沒有權限";
        }
        private List<FormColumn> crateAllColumn()
        {
            List<FormColumn> col = new List<FormColumn>();
            col.Add(new FormColumn
            {
                name = "id",
                title = "編號",
                type = "number",
                breakpoints = null,
                style = new TableStyle { width = 80, maxWidth = 80 }
            });
            col.Add(new FormColumn
            {
                name = "count",
                title = "瀏覽人次",
                type = "number",
                breakpoints = null
            });
            col.Add(new FormColumn
            {
                name = "countOfperson",
                title = "瀏覽人數",
                type = "number",
                breakpoints = null
            });
            col.Add(new FormColumn
            {
                name = "date",
                title = "日期",
                type = "date",
                breakpoints = null
            });
            return col;
        }
        private List<FormColumn> crateCustTableColumn()
        {
            List<FormColumn> col = new List<FormColumn>();
            col.Add(new FormColumn
            {
                name = "id",
                title = "編號",
                type = "number",
                breakpoints = null,
                style = new TableStyle { width = 80, maxWidth = 80 }
            });
            col.Add(new FormColumn
            {
                name = "title",
                title = "表單名稱",
                type = "text",
                breakpoints = null
            });
            col.Add(new FormColumn
            {
                name = "cuser",
                title = "建立人",
                type = "text",
                breakpoints = null
            });
            col.Add(new FormColumn
            {
                name = "cdate",
                title = "建立時間",
                type = "text",
                breakpoints = null
            });
            col.Add(new FormColumn
            {
                name = "euser",
                title = "最後修改人",
                type = "text",
                breakpoints = null
            });
            col.Add(new FormColumn
            {
                name = "edate",
                title = "最後修改時間",
                type = "text",
                breakpoints = null
            });
            return col;
        }
        private List<FormColumn> crateCustTableReportColumn(int id)
        {
            List<FormColumn> col = new List<FormColumn>();
            using (SqlConnection conn = new SqlConnection(setting))
            {
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
                        string key = "";
                        switch (reader["type"].ToString())
                        {
                            case "1":
                                key = "age";
                                break;
                            case "2":
                                key = "sex";
                                break;
                            case "3":
                                key = "marry";
                                break;
                            case "4":
                                key = "org";
                                break;
                            case "5":
                                key = "chf";
                                break;
                            case "6":
                                key = "crk";
                                break;
                            case "7":
                                key = "POPESN";
                                break;
                            case "8":
                                key = "TSER_day";
                                break;
                            case "9":
                                key = "ETP";
                                break;
                            case "10":
                                key = "ESU";
                                break;
                            case "11":
                                key = "ESU";
                                break;
                            case "12":
                                key = "ELV";
                                break;
                            case "13":
                                key = "menuSub";
                                break;
                            case "14":
                                key = "menu";
                                break;
                            case "15":
                                key = "tag";
                                break;
                            case "16":
                                key = "name";
                                break;
                            case "17":
                                key = "orgLevel";
                                break;
                            case "18":
                                key = "peopleTh";
                                break;
                            case "19":
                                key = "zip";
                                break;
                        }
                        col.Add(new FormColumn
                        {
                            name = key,
                            title = reader["title"].ToString(),
                            type = "text",
                            breakpoints = null
                        });
                    }
                    col.Add(new FormColumn
                    {
                        name = "count",
                        title = "瀏覽人次",
                        type = "number",
                        breakpoints = null
                    });
                    col.Add(new FormColumn
                    {
                        name = "countOfperson",
                        title = "瀏覽人數",
                        type = "number",
                        breakpoints = null
                    });
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            return col;
        }
        private String printMsg(String RspnCode, String RspnMsg)
        {
            flow.RspnCode = RspnCode;
            flow.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(flow);
        }
    }
}