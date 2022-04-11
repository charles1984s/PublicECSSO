using ECSSO.Library.Enumeration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Order.BankStatement
{
    public class BankStatementOutput: FooTable
    {
        public string memid { get; set; }
        public void getMaster() {
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
                columns = crateMasterTableColumn(),
                rows = new List<FooTabkeRow>()
            };
            RspnCode = "500.4";
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(@"
                    select
	                     ROW_NUMBER() OVER(order by importNo desc,taxType) [index], 
	                    importNo,taxType,mem_id 
                    from orders_hd
                    left join orders on orders_hd.id=orders.order_no
                    left join prod on orders.productid=prod.id
                    where not importNo is null and ISNULL(FK_CheckId,0)!=0 and mem_id=@memid
                    group by importNo,taxType,mem_id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@memid", memid));
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
                            value = new BankStatementMaster
                            {
                                id = int.Parse(reader["index"].ToString()),
                                importNo = reader["importNo"].ToString(),
                                date = $"{checkToken.GS.Left(reader["importNo"].ToString(), 4)}/{checkToken.GS.Mid(reader["importNo"].ToString(), 4,2)}/{checkToken.GS.Mid(reader["importNo"].ToString(), 6, 2)}",
                                taxType = ((ProdTaxEnum)int.Parse(reader["taxType"].ToString())).ToString()
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    RspnCode = "500";
                    throw e;
                }
            }
        }
        private void GetimportNo(int id,out string importNo, out int taxType) {
            getMaster();
            FooTabkeRow rows = table.rows.Find(e => e.value.id == id);
            BankStatementMaster value = (BankStatementMaster)rows.value;
            importNo = value.importNo;
            taxType = (int)Enum.Parse(typeof(ProdTaxEnum), value.taxType);
        }
        public void getDatail(int id)
        {
            string importNo;
            int taxType;
            GetimportNo(id,out importNo,out taxType);
            RspnCode = "500.3";
            table = null;
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
                columns = crateDatailTableColumn(),
                rows = new List<FooTabkeRow>()
            };
            RspnCode = "500.4";
            using (SqlConnection conn = new SqlConnection(checkToken.setting)) {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand(@"
                    SET NOCOUNT ON;
                    select
                        hd.id orderID,hd.mem_id,hd.importNo,body.qty,body.sizeid,body.colorid,body.price,body.productid,body.cdate,FK_CheckId,prod.taxType,prod.BtoBShipMode
                        into #temp1
                    from orders_hd as hd
                    left join orders as body on hd.id=body.order_no
                    left join prod on body.productid =prod.id
                    where 
                        hd.[state] not in(4,5) and not hd.FK_CheckId is null and
                        importNo=@importNo and prod.taxType=@taxType 
                    order by hd.importNo,convert(int,body.productid)

                    select 
                        MIN(convert(int,orderID)) id,taxType,
                        importNo,productid,SUM(qty) qty,MAX(price) price,MAX(mem_id) mem_id,
                        MAX(FK_CheckId) FK_CheckId,sizeid,colorid,
                        ROW_NUMBER() OVER(Partition by t.importNo,t.taxType order by t.BtoBShipMode,t.productid) ser_no
                        into #temp2
                    from #temp1 as t
                    group by importNo,productid,sizeid,colorid,taxType,BtoBShipMode

                    select orderID,importNo,taxType into #temp3 from #temp1 group by orderID,importNo,taxType

                    select 
                        importNo,taxType,
                        STUFF(
                            (
                                SELECT  ','+[orderID] FROM #temp3 
                                WHERE [importNo] = t.[importNo] and taxType = t.taxType FOR XML PATH('')
                            ),1,1,''
                        ) orderID
                        into #temp4
                    from #temp3 as t group by importNo,taxType
                    select 
                        t4.orderID,cf.id,prod.BtoBhPayType,hd.ser_no,prod.taxType,prod.BtoBShipMode,
                        prod.itemno,hd.productid,prod.title,price.sizeTitle,price.colorTitle,prod_list.title subTitle,
                        case 
                            when prod.BtoBhPayType =1 then 0 
                            when not cf.id is null then cf.price
                            else hd.price 
                        end price,
                        hd.qty,prod.taxType,
                        Cust.custID,hd.importNo
                    from #temp2 as hd
                    left join #temp4 as t4 on t4.importNo=hd.importNo and t4.taxType=hd.taxType
                    left join Cust on hd.mem_id=Cust.mem_id
                    left join prod on hd.productid=prod.id
                    left join prod_list on prod.sub_id=prod_list.id
                    left join check_prod as cf on cf.check_id=hd.FK_CheckId and cf.prod_id=prod.id and cf.size_id=hd.sizeid and cf.color_id=hd.colorid
                    left join prodPrice as price on price.id=prod.id and price.sizeID=hd.sizeid and price.colorID=hd.colorid
                    order by hd.importNo,prod.taxType,hd.ser_no
                    drop table #temp1;
                    drop table #temp2;
                    drop table #temp3;
                    drop table #temp4;
                    SET NOCOUNT OFF;
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@importNo", importNo));
                cmd.Parameters.Add(new SqlParameter("@taxType", taxType));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        string spec = "";
                        if (!string.IsNullOrEmpty(reader["colorTitle"].ToString()))
                        {
                            spec = spec + "/" + reader["colorTitle"].ToString();
                        }
                        if (!string.IsNullOrEmpty(reader["sizeTitle"].ToString())) {
                            spec = spec + "/" + reader["sizeTitle"].ToString();
                        }
                        if (!string.IsNullOrEmpty(spec)) spec = checkToken.GS.Mid(spec, 1);

                        table.rows.Add(new FooTabkeRow
                        {
                            options = new RowOptions
                            {
                                classes = "cell",
                                expanded = false
                            },
                            value = new BankStatementDetial
                            {
                                productId = reader["productId"].ToString(),
                                productName = reader["title"].ToString()+(string.IsNullOrEmpty(spec)?"":$"({spec})"),
                                price = int.Parse(reader["price"].ToString()),
                                qty = int.Parse(reader["qty"].ToString()),
                                orderNumber = reader["orderID"].ToString(),
                                amt = int.Parse(reader["price"].ToString()) * int.Parse(reader["qty"].ToString()),
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    RspnCode = "500";
                }
            }
        }
        private List<FooTableColumn> crateMasterTableColumn()
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
                name = "date",
                title = "成立日期",
                type = "text",
                sortable = true,
                breakpoints = "xs",
                style = null
            });
            col.Add(new FooTableColumn
            {
                name = "taxType",
                title = "稅別",
                type = "text",
                sortable = true,
                breakpoints = "xs",
                style = null
            });
            return col;
        }
        private List<FooTableColumn> crateDatailTableColumn() {
            List<FooTableColumn> col = new List<FooTableColumn>();
            col.Add(new FooTableColumn
            {
                name = "productId",
                title = "產品編號",
                type = "int",
                sortable = true,
                breakpoints = "xs",
                style = new FooTableStyle { maxWidth = 160, width = 160 }
            });
            col.Add(new FooTableColumn
            {
                name = "productName",
                title = "產品名稱",
                type = "text",
                sortable = true,
                breakpoints = "xs",
                style = null
            });
            col.Add(new FooTableColumn
            {
                name = "price",
                title = "價格",
                type = "int",
                sortable = true,
                breakpoints = "xs",
                style = null
            });
            col.Add(new FooTableColumn
            {
                name = "qty",
                title = "數量",
                type = "int",
                sortable = true,
                breakpoints = "xs",
                style = null
            });
            col.Add(new FooTableColumn
            {
                name = "amt",
                title = "小計",
                type = "int",
                sortable = true,
                breakpoints = "xs",
                style = null
            });
            col.Add(new FooTableColumn
            {
                name = "orderNumber",
                title = "訂單編號(合併)",
                type = "txt",
                sortable = true,
                breakpoints = "lg",
                style = null
            });
            return col;
        }
    }
}