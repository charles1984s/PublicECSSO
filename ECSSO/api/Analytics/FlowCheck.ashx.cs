using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using ECSSO.Library;
using ECSSO.Library.Analytics;
using ECSSO.Library.CustFormLibary;
using Newtonsoft.Json;

namespace ECSSO.api.Analytics
{
    /// <summary>
    /// FlowCheck 的摘要描述
    /// </summary>
    public class FlowCheck : IHttpHandler
    {
        private string setting;
        private Flow flow;
        private GetStr GS;
        private TokenItem token;
        public void ProcessRequest(HttpContext context)
        {
            GS = new GetStr();
            string code, message;
            code = "404";
            message = "not fount";
            try {
                flow = new Flow();
                if (context.Request.Form["token"] != null) {
                    token = new TokenItem
                    {
                        token = context.Request.Form["token"]
                    };
                    this.setting = GS.checkToken(token);
                    if (this.setting.IndexOf("error") < 0) {
                        code = "200";
                        message = "success";
                        switch (context.Request.Form["type"]) {
                            case "year":
                                loadYearData();
                                break;
                            case "month":
                                if (context.Request.Form["year"] == null) {
                                    code = "401";
                                    message = "年分不可為空";
                                }
                                else
                                    loadMonthData(int.Parse(context.Request.Form["year"]));
                                break;
                            case "detail":
                                if (context.Request.Form["year"] == null)
                                {
                                    code = "401";
                                    message = "年分不可為空";
                                }
                                else if (context.Request.Form["month"] == null)
                                {
                                    code = "401";
                                    message = "月分不可為空";
                                }
                                else {
                                    int y = int.Parse(context.Request.Form["year"]);
                                    int m = int.Parse(context.Request.Form["month"]);
                                    loadDetailData(y, m);
                                }
                                break;
                            default :
                                code = "404";
                                message = "no type";
                                break;
                        }
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
                message = ex.StackTrace;
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
        private List<FormColumn> crateCustTableColumn(int type)
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
            switch (type) {
                case 1:
                    col.Add(new FormColumn
                    {
                        name = "year",
                        title = "年分",
                        type = "text",
                        breakpoints = null
                    });
                    break;
                case 2:
                    col.Add(new FormColumn
                    {
                        name = "month",
                        title = "月份",
                        type = "number",
                        breakpoints = null
                    });
                    break;
                case 3:
                    col.Add(new FormColumn
                    {
                        name = "date",
                        title = "日期",
                        type = "text",
                        breakpoints = null
                    });
                    break;
            }
            col.Add(new FormColumn
            {
                name = "upload",
                title = "上傳流量",
                type = "text",
                breakpoints = null
            });
            col.Add(new FormColumn
            {
                name = "download",
                title = "下載流量",
                type = "text",
                breakpoints = null
            });
            return col;
        }
        private void initTable(int type) {
            flow.table = new FlowData
            {
                sorting = new FormSort
                {
                    enabled = true
                },
                empty = "查無資料",
                columns = crateCustTableColumn(type),
                rows = new List<FlowItem>()
            };
        }
        private void loadDetailData(int year,int month) {
            initTable(3);
            if (GS.hasPwoer(setting, "G003", "canexe", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select * from flowMeter where DATEPART(YEAR, [date]) = @year and DATEPART(Month, [date]) = @month order by [DATE] desc
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@year", year));
                    cmd.Parameters.Add(new SqlParameter("@month", month));
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
                                value = new FlowCheckDetailItem
                                {
                                    id = ++i,
                                    date = DateTime.Parse(reader["date"].ToString()).ToString("yyyy/MM/dd"),
                                    upload = GS.toByte(long.Parse(reader["Upload"].ToString()), 0),
                                    download = GS.toByte(long.Parse(reader["download"].ToString()), 0)
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
        private void loadMonthData(int year) {
            initTable(2);
            if (GS.hasPwoer(setting, "G003", "canexe", token.id)) {
                using (SqlConnection conn = new SqlConnection(setting)) {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        SELECT DATEPART(month, [date]) AS Closing_Month, isnull(sum([Upload]),0) as [Upload],isnull(sum(download),0) AS download FROM flowMeter where DATEPART(YEAR, [date]) = @year GROUP BY DATEPART(month, [date])
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@year", year));
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
                                value = new FlowCheckMonthItem
                                {
                                    id = ++i,
                                    month = int.Parse(reader["Closing_Month"].ToString()),
                                    upload = GS.toByte(long.Parse(reader["Upload"].ToString()), 0),
                                    download = GS.toByte(long.Parse(reader["download"].ToString()), 0)
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
        private void loadYearData() {
            initTable(1);
            if (GS.hasPwoer(setting, "G003", "canexe", token.id)) {
                using (SqlConnection conn = new SqlConnection(setting)) {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        SELECT DATEPART(YEAR, [date]) AS Closing_YEAR,MAX(DATEPART(MONTH, [date])) maxMonth,min(DATEPART(MONTH, [date])) minMonth, isnull(sum([Upload]),0) as [Upload],isnull(sum(download),0) AS download FROM flowMeter GROUP BY DATEPART(YEAR, [date])
                    ", conn);
                    SqlDataReader reader = null;
                    try {
                        reader = cmd.ExecuteReader();
                        int i = 0;
                        while (reader.Read()) {
                            flow.table.rows.Add(new FlowItem
                            {
                                options = new FormOptions
                                {
                                    classes = "cell",
                                    expanded = true
                                },
                                value = new FlowCheckYearItem
                                {
                                    id = ++i,
                                    year =  reader["Closing_YEAR"].ToString() + "("+
                                            reader["minMonth"].ToString() + "~" +
                                            reader["maxMonth"].ToString() + ")",
                                    upload = GS.toByte(long.Parse(reader["Upload"].ToString()),0),
                                    download = GS.toByte(long.Parse(reader["download"].ToString()), 0)
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
        private String printMsg(String RspnCode, String RspnMsg)
        {
            flow.RspnCode = RspnCode;
            flow.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(flow);
        }
    }
}