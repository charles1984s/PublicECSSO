using ECSSO.Library;
using ECSSO.Library.Enumeration;
using ECSSO.Library.Report;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace ECSSO.api.Order.Report
{
    /// <summary>
    /// OrderReportOfList 的摘要描述
    /// </summary>
    public class OrderReportOfList : IHttpHandler
    {
        private NPOIReportExcel nPOI { get; set; }
        private GetStr gs { get; set; }
        private CheckToken checkToken;
        private string setting;
        public void ProcessRequest(HttpContext context)
        {
            nPOI = new NPOIReportExcel();
            string fileName = "條列式訂單匯出-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xls"; // 檔案名稱
            checkToken = new CheckToken();
            try
            {
                checkToken.check(context);
                gs = checkToken.GS;
                setting = checkToken.setting;
                if (setting.IndexOf("error") >= 0) throw new Exception("Token已過期");
                else if (string.IsNullOrEmpty(context.Request.Form["start"])) throw new Exception("請輸入搜尋開始日期");
                else if (string.IsNullOrEmpty(context.Request.Form["end"])) throw new Exception("請輸入搜尋結束日期");
                else
                {
                    string start = context.Request.Form["start"] + " " + (context.Request.Form["startTime"] ?? "");
                    string end = context.Request.Form["end"] + " " + (context.Request.Form["endTime"] ?? "");
                    byte[] cont = nPOI.EntityListToExcel2003(
                        getHeader(),
                        GetListOrders(start, end),
                        context.Request.Form["start"] + "-" + context.Request.Form["end"],
                        new byte[3] { 189, 215, 238 },
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
        private List<ListOrderReport> GetListOrders(string start, string end)
        {
            List<ListOrderReport> lists = new List<ListOrderReport>();
            List<string> log = new List<string>();
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select 
	                    convert(nvarchar,hd.cdate,112) cdate,hd.ser_id serNo,hd.[state],hd.id,
	                    hd.mem_id,hd.o_tel,hd.o_cell,hd.o_name,hd.invoice_title,hd.ident,
	                    pt.title PtTitle,isnull(ol.Temperature,1) Temperature,hd.o_addr,lt.title LTitle,
	                    hd.amt orderAmt,hd.discount_amt,hd.bonus_discount,hd.bonus_amt,
	                    hd.name,hd.cell,hd.tel,hd.addr,hd.mail,hd.notememo,
	                    isnull(Coupon.title,'') CouponTitle,hd.couponDiscount,hd.freightamount,
	                    o.prod_name,isnull(c.title,'') cTitle,isnull(s.title,'') sTitle,
	                    o.ser_no,o.price,o.qty,o.amt,o.memo,o.virtual,o.discount 
                    from orders  as o
                    left join orders_hd as hd on hd.id=o.order_no
                    left join prod_color as c on c.id=o.colorid
                    left join prod_size as s on s.id = o.sizeid
                    left join orders_Logistics as ol on ol.order_no=hd.id
                    left join Logisticstype as lt on ol.LogisticstypeID = lt.id
                    left join paymenttype as pt on pt.code = hd.payment_type
                    left join Coupon on Coupon.id=hd.couponID
                    where 
                        CONVERT(datetime,hd.cdate) between CONVERT(datetime,@start) and CONVERT(datetime,@end)
                    order by hd.id,o.ser_no
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
                        if (string.IsNullOrEmpty(log.Find(e => e == reader["id"].ToString()))) log.Add(reader["id"].ToString());
                        ListOrderReport order = new ListOrderReport
                        {
                            A = reader["cdate"].ToString(),
                            B = reader["serNo"].ToString(),
                            C = Enum.GetName(typeof(ORderStatusEnum), int.Parse(reader["state"].ToString())),
                            D = reader["id"].ToString(),
                            E = reader["mem_id"].ToString(),
                            F = reader["o_name"].ToString(),
                            G = reader["o_tel"].ToString(),
                            H = reader["o_cell"].ToString(),
                            I = reader["invoice_title"].ToString(),
                            J = reader["ident"].ToString(),
                            K = reader["PtTitle"].ToString(),
                            L = Enum.GetName(typeof(TemperatureEnum), int.Parse(reader["Temperature"].ToString())),
                            M = reader["o_addr"].ToString(),
                            N = reader["LTitle"].ToString(),
                            O = (
                                    int.Parse(reader["orderAmt"].ToString()) - int.Parse(reader["discount_amt"].ToString()) - int.Parse(reader["bonus_amt"].ToString()) -
                                    int.Parse(reader["bonus_discount"].ToString()) + int.Parse(reader["freightamount"].ToString())
                                ).ToString(),
                            P = reader["ser_no"].ToString(),
                            Q = reader["prod_name"].ToString(),
                            R = reader["memo"].ToString() + "/",
                            S = reader["price"].ToString(),
                            T = reader["qty"].ToString(),
                            U = reader["discount"].ToString(),
                            V = (int.Parse(reader["amt"].ToString()) - int.Parse(reader["discount"].ToString())).ToString(),
                            W = reader["discount_amt"].ToString(),
                            X = reader["bonus_discount"].ToString(),
                            Y = reader["bonus_amt"].ToString(),
                            Z = reader["freightamount"].ToString(),
                            AA = reader["orderAmt"].ToString(),
                            AB = reader["name"].ToString(),
                            AC = reader["tel"].ToString(),
                            AD = reader["cell"].ToString(),
                            AE = reader["addr"].ToString(),
                            AF = reader["mail"].ToString(),
                            AG = reader["notememo"].ToString(),
                        };
                        if (!string.IsNullOrEmpty(reader["cTitle"].ToString())) order.R += reader["cTitle"].ToString() + "/";
                        if (!string.IsNullOrEmpty(reader["sTitle"].ToString())) order.R += reader["sTitle"].ToString() + "/";
                        order.R = order.R.Substring(0, order.R.Length - 1);
                        lists.Add(order);
                    }
                }
                finally
                {
                    gs.InsertLog(
                        setting, checkToken.token.id,
                        "訂單管理", "訂單列印", "0",
                        JsonConvert.SerializeObject(log),
                        "api/Order/Report/OrderReportOfList.ashx"
                    );
                    reader.Close();
                }
            }
            return lists;
        }

        private Dictionary<string, string> getHeader()
        {
            Dictionary<string, string> pairs = new Dictionary<string, string> {
                { "A", "訂購日期"},{ "B", "受訂單號"},
                { "C", "訂單狀態"},{ "D", "訂單編號"},
                { "E", "訂購人會員編號"},{ "F", "訂購人"},
                { "G", "訂購人電話"},{ "H", "訂購人手機"},
                { "I", "發票抬頭"},{ "J", "發票統編"},
                { "K", "付款方式"},{ "L", "溫層方式"},
                { "M", "發票地址"},{ "N", "貨運方式"},
                { "O", "總計"},{ "P", "序號"},
                { "Q", "產品名稱"},{ "R", "產品備註"},
                { "S", "單價"},{ "T", "數量"},{ "U", "折扣"},
                { "V", "小計"},{ "W", "訂單折扣"},{ "X", "優惠券折扣"},
                { "Y", "紅利折扣"},{ "Z", "運費"},
                { "AA", "訂單金額"},{ "AB", "收件人"},
                { "AC", "收件人電話"},{ "AD", "收件人手機"},
                { "AE", "收件人地址"},{ "AF", "收件人Email"},
                { "AG", "訂單備註"}
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