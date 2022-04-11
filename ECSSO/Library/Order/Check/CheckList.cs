using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Order.Check
{
    public class CheckList : FooTable
    {
        public DateTime d1 { get; set; }
        public DateTime d2 { get; set; }
        public void getCheckList()
        {
            RspnCode = "500.3";
            table = new FooTableDetail
            {
                sorting = new FooTableSort
                {
                    enabled = true
                },
                paging = new FooTablePaging
                {
                    enabled = false,
                    limit = 3
                },
                empty = "查無資料",
                columns = crateCheckTableColumn(),
                rows = new List<FooTabkeRow>()
            };
            RspnCode = "500.4";
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select id,convert(nvarchar(10),sdate,111) sdate,convert(nvarchar(10),edate,111) edate from check_hd order by id desc
                ", conn);
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        table.rows.Add(new FooTabkeRow
                        {
                            options = new RowOptions
                            {
                                classes = "cell",
                                expanded = false
                            },
                            value = new OrderCheck
                            {
                                id = int.Parse(reader["id"].ToString()),
                                title = $"{reader["sdate"]}~{reader["edate"]}"
                            }
                        });
                    }
                    checkToken.GS.InsertLog(
                        checkToken.setting,
                        checkToken.token.id, "對帳單", "取得對帳單列表", "",
                        $"",
                        "api/Order/CheckHandler.ashx"
                    );
                }
                catch (Exception e)
                {
                    RspnMsg = e.Message;
                    success = false;
                }
            }
        }
        public void getNotCheckDate() {
            d2 = DateTime.Now;
            d1 = DateTime.Now.AddDays(-7);
            using (SqlConnection conn = new SqlConnection(checkToken.setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select convert(nvarchar,MAX(cdate),126) max,convert(nvarchar,Min(cdate),126) min from orders_hd where FK_CheckId is null and [state] not in(4,5)
                ", conn);
                try {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read()) {
                        d2 = DateTime.Parse(reader["max"].ToString());
                        d1 = DateTime.Parse(reader["min"].ToString());
                    }
                    checkToken.GS.InsertLog(
                        checkToken.setting,
                        checkToken.token.id, "對帳單", "取得尚未對帳起訖日期", "",
                        $"",
                        "api/Order/CheckHandler.ashx"
                    );
                }
                catch (Exception e)
                {
                    RspnMsg = e.Message;
                    success = false;
                }
            }
        }
        public void AddCheck(CheckAddDto addDto)
        {
            RspnCode = "500.2";
            RspnCode = "500.2";
            d1 = DateTime.Parse(checkToken.context.Request.Form["d1"]);
            d2 = DateTime.Parse(checkToken.context.Request.Form["d2"]);
            int id = Create();
            addDto.id = id;
            addDto.save(checkToken.setting);
            checkToken.GS.InsertLog(
                checkToken.setting,
                checkToken.token.id, "對帳單", "新增對帳單", $"{d1.ToString("yyyy-MM-dd HH:mm:ss")}~{d2.ToString("yyyy-MM-dd HH:mm:ss")}",
                JsonConvert.SerializeObject(addDto),
                "api/Order/CheckHandler.ashx"
            );
        }
        private int Create()
        {
            int id = 0;
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    insert into check_hd(sdate,edate) output INSERTED.id values(@d1,@d2)
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@d1", d1.ToString("yyyy-MM-dd HH:mm:ss")));
                cmd.Parameters.Add(new SqlParameter("@d2", d2.ToString("yyyy-MM-dd HH:mm:ss")));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read()) id = int.Parse(reader["id"].ToString());
                }
                catch (Exception e)
                {
                    RspnMsg = e.Message;
                    success = false;
                    throw new Exception(RspnMsg);
                }
            }
            return id;
        }
        public void Delete(int id) {
            FlushOrderChech(id);
            deleteCheckOrder(id);
            deleteCheckProd(id);
            using (SqlConnection conn = new SqlConnection(checkToken.setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    delete check_hd where id=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                try
                {
                    cmd.ExecuteReader();
                    checkToken.GS.InsertLog(
                        checkToken.setting,
                        checkToken.token.id, "對帳單", "刪除對帳單", id.ToString(),
                        "",
                        "api/Order/CheckHandler.ashx"
                    );
                }
                catch (Exception e)
                {
                    RspnMsg = e.Message;
                    success = false;
                    throw new Exception(RspnMsg);
                }
            }
        }
        private void deleteCheckProd(int id)
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    delete check_prod where check_id=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    RspnMsg = e.Message;
                    success = false;
                    throw new Exception(RspnMsg);
                }
            }
        }
        private void deleteCheckOrder(int id) {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    delete check_orders where check_id=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    RspnMsg = e.Message;
                    success = false;
                    throw new Exception(RspnMsg);
                }
            }
        }
        private void FlushOrderChech(int id) {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    update orders_hd set FK_CheckId=null where FK_CheckId=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    RspnMsg = e.Message;
                    success = false;
                    throw new Exception(RspnMsg);
                }
            }
        }
        private List<FooTableColumn> crateCheckTableColumn()
        {
            List<FooTableColumn> col = new List<FooTableColumn>();
            col.Add(new FooTableColumn
            {
                name = "id",
                title = "序號",
                type = "int",
                sortable = true,
                breakpoints = "xs",
                style = new FooTableStyle { maxWidth = 80, width = 80 }
            });
            col.Add(new FooTableColumn
            {
                name = "title",
                title = "對帳單時間區間",
                type = "text",
                sortable = true,
                breakpoints = null
            });
            col.Add(new FooTableColumn
            {
                name = "view",
                title = "瀏覽",
                type = "view",
                breakpoints = "xs",
                sortable = false,
                style = new FooTableStyle { width = 70, maxWidth = 70 }
            });/*
            col.Add(new FooTableColumn
            {
                name = "export",
                title = "匯出",
                type = "export",
                breakpoints = "xs",
                sortable = false,
                style = new FooTableStyle { width = 70, maxWidth = 70 }
            });*/
            col.Add(new FooTableColumn
            {
                name = "delete",
                title = "刪除",
                type = "delete",
                breakpoints = null,
                sortable = false,
                style = new FooTableStyle { width = 70, maxWidth = 70 }
            });
            return col;
        }
    }
}