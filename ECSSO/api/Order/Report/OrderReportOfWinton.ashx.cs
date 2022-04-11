using ECSSO.Library.Enumeration;
using ECSSO.Library.Report;
using ECSSO.Library.Winton;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.api.Order.Report
{
    /// <summary>
    /// OrderReportOfWinton 的摘要描述
    /// </summary>
    public class OrderReportOfWinton : IHttpHandler
    {
        private NPOIReportExcel nPOI { get; set; }
        private GetStr gs { get; set; }
        private string setting;
        public void ProcessRequest(HttpContext context)
        {
            gs = new GetStr();
            nPOI = new NPOIReportExcel();
            string fileName = "文中格式訂單匯出-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xls"; // 檔案名稱
            try
            {
                setting = gs.checkToken(context.Request.Form["token"]);
                if (setting.IndexOf("error") >= 0) throw new Exception("Token已過期");
                else if (string.IsNullOrEmpty(context.Request.Form["start"])) throw new Exception("請輸入搜尋開始日期");
                else if (string.IsNullOrEmpty(context.Request.Form["end"])) throw new Exception("請輸入搜尋結束日期");
                else
                {
                    string start = context.Request.Form["start"] + " " + (context.Request.Form["startTime"] ?? "");
                    string end = context.Request.Form["end"] + " " + (context.Request.Form["endTime"] ?? "");
                    fileName = $"文中格式訂單匯出-{start}~{end}.xls"; // 檔案名稱
                    byte[] cont = nPOI.EntityListToExcel2003(
                        getHeader(),
                        GetWintonOrders(start, end),
                        "銷貨",
                        new byte[3] { 255, 255, 255 },
                        new byte[3] { 255, 255, 255 }
                    );
                    context.Response.AddHeader("Content-Disposition", string.Format("attachment; filename=" + fileName));
                    context.Response.BinaryWrite(cont);
                }
            }
            catch (Exception e)
            {
                context.Response.Write(e.Message);
            }
        }
        private void updateOrderSerNo(string start, string end)
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    SET NOCOUNT ON;
                    declare @starNum int;
                    if exists(select * from orders_hd where importNo like CONVERT(nvarchar,GETDATE(),112)+'%') begin
                        select @starNum=COUNT(*) from orders_hd where importNo like CONVERT(nvarchar,GETDATE(),112)+'%'
                    end else begin
                        set @starNum = 0
                    end

                    select * into #temp1
                    from orders_hd as hd
                        where 
                            hd.[state] not in(4,5) and 
                            CONVERT(datetime,hd.cdate) between CONVERT(datetime,@start) and CONVERT(datetime,@end) and
                            ISNULL(hd.importNo,'')='' and not FK_CheckId is null

                    update #temp1 set cdate=o.cdate from(
                        select a.id,a.mem_id,
                            (select top 1 cdate from #temp1 where mem_id=a.mem_id) cdate
                        from #temp1 as a
                    ) as o
                    update orders_hd set importNo= a.importNo from(
                        select 
                            (dense_rank() OVER(order by hd.mem_id)) NoOf,
                            case 
                                when importNo is null then
                                    CONVERT(nvarchar,GETDATE(),112) +
                                    RIGHT(
                                        REPLICATE('0', 4) + 
                                        CAST((dense_rank() OVER(order by hd.mem_id)+@starNum) as NVARCHAR), 4)
                                else importNo
                            end as importNo
                            ,id,mem_id
                        from #temp1 as hd
                    ) a where a.id=orders_hd.id
                    drop table #temp1;
                    SET NOCOUNT OFF;
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@start", start));
                cmd.Parameters.Add(new SqlParameter("@end", end));
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        private List<WintonOrder> GetWintonOrders(string start, string end)
        {
            List<WintonOrder> wintonOrders = new List<WintonOrder>();
            updateOrderSerNo(start, end);
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    SET NOCOUNT ON;
                    select
                        hd.id orderID,hd.mem_id,hd.importNo,body.qty,body.sizeid,body.colorid,body.price,body.productid,body.cdate,FK_CheckId,prod.taxType,prod.BtoBShipMode
                        into #temp1
                    from orders_hd as hd
                    left join orders as body on hd.id=body.order_no
                    left join prod on body.productid =prod.id
                    where 
                        hd.[state] not in(4,5) and not hd.FK_CheckId is null and
                        CONVERT(datetime,hd.cdate) between CONVERT(datetime,@start) and CONVERT(datetime,@end)
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
                        prod.itemno,hd.productid,
                        case 
                            when prod.BtoBhPayType =1 then 0 
                            when not cf.id is null then cf.price
                            else hd.price 
                        end price,
                        hd.qty,prod.taxType,
                        Cust.custID,hd.importNo,
                        winton.*
                    from #temp2 as hd
                    left join #temp4 as t4 on t4.importNo=hd.importNo and t4.taxType=hd.taxType
                    left join Cust on hd.mem_id=Cust.mem_id
                    left join prod on hd.productid=prod.id
                    left join check_prod as cf on cf.check_id=hd.FK_CheckId and cf.prod_id=prod.id and cf.size_id=hd.sizeid and cf.color_id=hd.colorid
                    left join winton on 1=1
                    order by hd.importNo,prod.taxType,hd.ser_no
                    drop table #temp1;
                    drop table #temp2;
                    drop table #temp3;
                    drop table #temp4;
                    SET NOCOUNT OFF;
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@start", start));
                cmd.Parameters.Add(new SqlParameter("@end", end));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    DateTime dateTime = DateTime.Now;
                    string dateTimeStr =
                            (dateTime.Year - 1911).ToString() + "/" +
                            dateTime.Month.ToString().PadLeft(2, '0') + "/" +
                            dateTime.Day.ToString().PadLeft(2, '0');

                    while (reader.Read())
                    {
                        string serNo = (int.Parse(gs.Left(reader["importNo"].ToString(), 4)) - 1911) + gs.Right(reader["importNo"].ToString(), 8)+"-"+ reader["taxType"].ToString();
                        string OrderSerNo = int.Parse(reader["ser_no"].ToString()).ToString().PadLeft(4, '0');
                        int qty = int.Parse(reader["qty"].ToString() ?? "0");
                        double unitPrice = double.Parse(reader["price"].ToString() ?? "0");
                        int taxType = 5;
                        switch (int.Parse(reader["taxType"].ToString())) {
                            case 2:
                                taxType = 9;
                                break;
                            case 3:
                                taxType = 6;
                                break;
                            default:
                                taxType = 5;
                                break;
                        }
                        string orderID = reader["orderID"].ToString();
                        string BtoBShipMode = ((BtoBShipModeEnum)int.Parse(reader["BtoBShipMode"].ToString())).ToString();
                        WintonOrder order = new WintonOrder
                        {
                            B = "20" + serNo,
                            C = reader["custID"].ToString(),
                            D = reader["sellNo"].ToString(),
                            E = reader["itemno"].ToString(),
                            F = qty.ToString(),
                            G = dateTimeStr,
                            H = orderID.Length>10?"": orderID,
                            K = OrderSerNo,
                            M = unitPrice.ToString(),
                            N = "N",
                            Q = reader["depNo"].ToString(),
                            S = taxType.ToString(),
                            V = "0",
                            W = "0",
                            Y = reader["priceHaveTax"].ToString(),
                            Z = reader["subNo"].ToString(),
                            AL = "N",
                            AT = reader["custID"].ToString(),
                            CJ = reader["incNo"].ToString(),
                            AX = BtoBShipMode.IndexOf("總部")>=0?"": BtoBShipMode,
                            AW = $"訂單編號：{orderID}"
                        };
                        wintonOrders.Add(order);
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return wintonOrders;
        }

        private Dictionary<string, string> getHeader()
        {
            Dictionary<string, string> pairs = new Dictionary<string, string> {
                { "A", "庫別代號(明細)"},{ "B", "銷貨單號(表頭)"},{ "C", "客供商代號(表頭)"},
                { "D", "業務員代號(表頭)"},{ "E", "產品代號(明細)"},{ "F", "數量(明細)"},
                { "G", "銷貨日期(表頭)"},{ "H", "訂單單號(表頭)"},{ "I", "訂單序號(明細)"},
                { "J", "刪除記號"},{ "K", "銷貨單序號(明細)"},{ "L", "訂購未交量(明細)"},
                { "M", "標準售價(明細)"},{ "N", "關聯機號否(明細)"},{ "O", "批號(明細)"},
                { "P", "備註二(HEAT NO)(明細)"},{ "Q", "部門代號(表頭)"},{ "R", "付款方式(表頭)"},
                { "S", "稅別(表頭)"},{ "T", "付現金額(表頭)"},{ "U", "刷卡金額(表頭)"},
                { "V", "總含稅金額(表頭)"},{ "W", "總稅額(表頭)"},{ "X", "其他請款費用(表頭)"},
                { "Y", "單價含稅否(表頭)"},{ "Z", "傳票類別(表頭)"},{ "AA", "未稅單價(明細)"},
                { "AB", "未稅金額(明細)"},{ "AC", "稅額(明細)"},{ "AD", "外幣未稅單價(明細)"},
                { "AE", "外幣未稅金額(明細)"},{ "AF", "外幣稅額(明細)"},{ "AG", "序號(客供商統編)"},
                { "AH", "序號(客供商地址)"},{ "AI", "外幣代號(表頭)"},{ "AJ", "匯率(表頭)"},
                { "AK", "發票號碼(表頭)"},{ "AL", "列印註記(表頭)"},{ "AM", "專案\\項目編號(表頭)"},
                { "AN", "報價單號(表頭)"},{ "AO", "內聯單號(表頭)"},{ "AP", "建立時間(表頭)"},
                { "AQ", "修改時間(表頭)"},{ "AR", "建立人員(表頭)"},{ "AS", "修改人員(表頭)"},
                { "AT", "請款客戶(表頭)"},{ "AU", "顏色(明細)"},{ "AV", "明細備註(明細)"},
                { "AW", "備註(M)250"},{ "AX", "主檔自定義欄位一"},{ "AY", "主檔自定義欄位二"},
                { "AZ", "主檔自定義欄位三"},{ "BA", "主檔自定義欄位四"},{ "BB", "主檔自定義欄位五"},
                { "BC", "主檔自定義欄位六"},{ "BD", "主檔自定義欄位七"},{ "BE", "主檔自定義欄位八"},
                { "BF", "主檔自定義欄位九"},{ "BG", "主檔自定義欄位十"},{ "BH", "主檔自定義欄位十一"},
                { "BI", "主檔自定義欄位十二"},{ "BJ", "明細自定義欄位一"},{ "BK", "明細自定義欄位二"},
                { "BL", "明細自定義欄位三"},{ "BM", "明細自定義欄位四"},{ "BN", "明細自定義欄位五"},
                { "BO", "明細自定義欄位六"},{ "BP", "明細自定義欄位七"},{ "BQ", "明細自定義欄位八"},
                { "BR", "明細自定義欄位九"},{ "BS", "明細自定義欄位十"},{ "BT", "明細自定義欄位十一"},
                { "BU", "明細自定義欄位十二"},{ "BV", "產品描述"},{ "BW", "發票捐贈註記"},
                { "BX", "發票捐贈對象"},{ "BY", "電子發票註記"},{ "BZ", "列印紙本電子發票註記"},
                { "CA", "載具類別號碼"},{ "CB", "載具顯碼id"},{ "CC", "載具隱碼id"},
                { "CD", "愛心碼"},{ "CE", "支票金額(表頭)"},{ "CF", "匯款金額(表頭)"},
                { "CG", "外銷方式"},{ "CH", "發票日期(表頭)"},{ "CI", "對方品名/品名備註"},
                { "CJ", "收款單帳款科目類別"},{ "CK", "提貨券金額"},{ "CL", "序號(客供商聯絡人)"},
                { "CM", "庫別10碼"},{ "CN", "物流商編號"},{ "CO", "網購平台代號"},
                { "CP", "網購單號"},{ "CQ", "網購實際訂單日期"},{ "CR", "網購訂購人名稱"},
                { "CS", "網購訂購人抬頭"},{ "CT", "網購訂購人統編"},{ "CU", "網購訂購人聯絡電話"},
                { "CV", "網購訂購人聯絡地址"},{ "CW", "網購平台已開立發票號碼"},{ "CX", "網購收貨人名稱"},
                { "CY", "網購收貨人手機號碼"},{ "CZ", "網購收貨人聯絡電話"},{ "DA", "網購收貨人聯絡地址"},
                { "DB", "網購訂單備註"},{ "DC", "網購貨運公司"},{ "DD", "網購物流單號"},
                { "DE", "結帳日期"},{ "DF", "收款日期"},{ "DG", "對帳日期"},
                { "DH", "指送地址"},{ "DI", "對方品號"}
            };
            return pairs;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

    }
}