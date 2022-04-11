using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Order.Check
{
    public class CheckOrder : FooTable
    {
        public DateTime d1 { get; set; }
        public DateTime d2 { get; set; }
        public void getOrders()
        {
            RspnCode = "500.2";
            d1 = DateTime.Parse(checkToken.context.Request.Form["d1"]);
            d2 = DateTime.Parse(checkToken.context.Request.Form["d2"]);
            table = new FooTableDetail
            {
                filtering = new FooTableFiltering
                {
                    enabled = true
                },
                sorting = new FooTableSort
                {
                    enabled = true
                },
                paging = new FooTablePaging
                {
                    enabled = true,
                    limit = 3,
                    size = 20
                },
                empty = "查無資料",
                columns = crateCheckTableColumn(),
                rows = new List<FooTabkeRow>()
            };
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select 
	                    hd.id orderId,hd.cdate,Cust.id custId,Cust.ch_name
                    from orders_hd as hd
                    left join Cust on hd.mem_id=Cust.mem_id
                    where 
                    FK_CheckId is null and 
                    CONVERT(datetime,hd.cdate) between CONVERT(datetime,@d1) and CONVERT(datetime,@d2)
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@d1", d1.ToString("yyyy-MM-dd HH:mm:ss")));
                cmd.Parameters.Add(new SqlParameter("@d2", d2.ToString("yyyy-MM-dd HH:mm:ss")));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    RspnCode = "500.3";
                    while (reader.Read())
                    {
                        table.rows.Add(new FooTabkeRow
                        {
                            options = new RowOptions
                            {
                                classes = "cell",
                                expanded = false
                            },
                            value = new OrderHdItem
                            {
                                id = reader["orderId"].ToString(),
                                Date = DateTime.Parse(reader["cdate"].ToString()).ToString("yyyy/MM/dd HH:mm:ss"),
                                CustID = reader["custId"].ToString(),
                                CustName = reader["ch_name"].ToString()
                            }
                        });
                    }
                    checkToken.GS.InsertLog(
                        checkToken.setting,
                        checkToken.token.id, "對帳單", "取得對帳訂單", "",
                        $"{d1.ToString("yyyy-MM-dd HH:mm:ss")}~{d2.ToString("yyyy-MM-dd HH:mm:ss")}",
                        "api/Order/CheckHandler.ashx"
                    );
                }
                catch (Exception e)
                {
                    RspnMsg = e.Message;
                    success = false;
                    throw new Exception(e.Message);
                }
            }
        }
        public void getOrdersView(int id)
        {
            RspnCode = "500.2";
            table = new FooTableDetail
            {
                filtering = new FooTableFiltering
                {
                    enabled = true
                },
                sorting = new FooTableSort
                {
                    enabled = true
                },
                paging = new FooTablePaging
                {
                    enabled = true,
                    limit = 3,
                    size = 20
                },
                empty = "查無資料",
                columns = crateCheckViewTableColumn(),
                rows = new List<FooTabkeRow>()
            };
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from(
	                    select 
		                    order_no,c.ch_name,o.prod_name,s.title sizeTitle,color.title colorTitle,o.qty,o.memo,hd.cdate,
		                    case
			                    when cp.id is null then o.price
			                    else cp.price
		                    end price
	                    from check_hd as ch
	                    left join check_orders as co on co.check_id=ch.id
	                    left join orders_hd as hd on hd.id=co.order_id
	                    left join orders as o on hd.id=o.order_no
	                    left join prod_size as s on s.id = o.sizeid
	                    left join prod_color as color on color.id=o.colorid
	                    left join check_prod as cp on cp.prod_id=o.productid and cp.color_id=color.id and cp.size_id=s.id
	                    left join Cust as c on hd.mem_id=c.mem_id
	                    where ch.id=@id
                    ) as a
                    group by order_no,ch_name,prod_name,sizeTitle,colorTitle,qty,memo,price,cdate
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    RspnCode = "500.3";
                    while (reader.Read())
                    {
                        table.rows.Add(new FooTabkeRow
                        {
                            options = new RowOptions
                            {
                                classes = "cell",
                                expanded = false
                            },
                            value = new OrderHdViewItem
                            {
                                id = reader["order_no"].ToString(),
                                Date = DateTime.Parse(reader["cdate"].ToString()).ToString("yyyy/MM/dd HH:mm:ss"),
                                CustName = reader["ch_name"].ToString(),
                                Price = double.Parse(reader["price"].ToString()),
                                Qty = int.Parse(reader["qty"].ToString()),
                                ProudName = reader["prod_name"].ToString(),
                                Spec = $@"{reader["sizeTitle"]}
                                        {(
                                            string.IsNullOrEmpty(reader["sizeTitle"].ToString()) &&
                                            string.IsNullOrEmpty(reader["colorTitle"].ToString()) ? "" : "/")
                                        }{reader["colorTitle"]}",
                                Memo = reader["memo"].ToString()
                            }
                        });
                    }
                    checkToken.GS.InsertLog(
                        checkToken.setting,
                        checkToken.token.id, "對帳單", "取得對帳訂單", "",
                        $"{d1.ToString("yyyy-MM-dd HH:mm:ss")}~{d2.ToString("yyyy-MM-dd HH:mm:ss")}",
                        "api/Order/CheckHandler.ashx"
                    );
                }
                catch (Exception e)
                {
                    RspnMsg = e.Message;
                    success = false;
                    throw new Exception(e.Message);
                }
            }
        }
        private List<FooTableColumn> crateCheckViewTableColumn()
        {
            List<FooTableColumn> col = new List<FooTableColumn>();
            col.Add(new FooTableColumn
            {
                name = "id",
                title = "訂單編號",
                type = "text",
                sortable = true,
                breakpoints = "xs",
                style = new FooTableStyle { maxWidth = 160, width = 160 }
            });
            col.Add(new FooTableColumn
            {
                name = "CustName",
                title = "客戶名稱",
                type = "text",
                sortable = true,
                breakpoints = null
            });
            col.Add(new FooTableColumn
            {
                name = "ProudName",
                title = "產品名稱",
                type = "text",
                sortable = true,
                breakpoints = null,
                style = new FooTableStyle { maxWidth = 150, width = 150 }
            });
            col.Add(new FooTableColumn
            {
                name = "Spec",
                title = "規格",
                type = "text",
                sortable = true,
                breakpoints = null,
                style = new FooTableStyle { maxWidth = 150, width = 150 }
            });
            col.Add(new FooTableColumn
            {
                name = "Qty",
                title = "數量",
                type = "text",
                sortable = true,
                breakpoints = null
            });
            col.Add(new FooTableColumn
            {
                name = "Price",
                title = "價格",
                type = "text",
                sortable = true,
                breakpoints = null
            });
            col.Add(new FooTableColumn
            {
                name = "Memo",
                title = "備註",
                type = "text",
                sortable = true,
                breakpoints = null
            });
            col.Add(new FooTableColumn
            {
                name = "Date",
                title = "訂單建立時間",
                type = "text",
                sortable = true,
                breakpoints = null
            });
            return col;
        }
        private List<FooTableColumn> crateCheckTableColumn()
        {
            List<FooTableColumn> col = new List<FooTableColumn>();
            col.Add(new FooTableColumn
            {
                name = "check",
                title = "選擇",
                type = "check",
                sortable = true,
                filterable = false,
                breakpoints = "xs",
                style = new FooTableStyle { maxWidth = 80, width = 80 }
            });
            col.Add(new FooTableColumn
            {
                name = "id",
                title = "訂單編號",
                type = "text",
                sortable = true,
                breakpoints = "xs",
                style = new FooTableStyle { maxWidth = 200, width = 200 }
            });
            col.Add(new FooTableColumn
            {
                name = "CustID",
                title = "客戶編號",
                type = "text",
                sortable = true,
                breakpoints = null
            });
            col.Add(new FooTableColumn
            {
                name = "CustName",
                title = "客戶名稱",
                type = "text",
                sortable = true,
                breakpoints = null
            });
            col.Add(new FooTableColumn
            {
                name = "Date",
                title = "訂單建立時間",
                type = "text",
                sortable = true,
                breakpoints = null
            });
            return col;
        }
    }
}