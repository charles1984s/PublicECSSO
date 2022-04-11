using ECSSO.Library;
using ECSSO.Library.Order;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ECSSO.api.Order.Report
{
    /// <summary>
    /// OrderReport 的摘要描述
    /// </summary>
    public class OrderReport : IHttpHandler
    {
        private GetStr GS;
        private responseJson response;
        private TokenItem token;
        private CheckToken checkToken;
        private string payStr,orderID;
        public void ProcessRequest(HttpContext context)
        {
            checkToken = new CheckToken();
            checkToken.check(context);
            response = checkToken.response;
            token = checkToken.token;
            GS = checkToken.GS;
            try
            {
                if (response.RspnCode == "200")
                {
                    switch (context.Request.Form["type"])
                    {
                        case "theOrder":
                            int id = 0;
                            orderID = context.Request.Form["id"];
                            try
                            {
                                id = int.Parse(context.Request.Form["id"]);
                            }
                            catch
                            {
                                throw new Exception("資料格式錯誤");
                            }
                            setResponse(context.Response, report(context.Request.Form["id"]));
                            GS.InsertLog(
                                checkToken.setting,
                                token.id, "訂單管理", "訂單單筆列印", context.Request.Form["id"],
                                "theOrder",
                                "api/Order/Report/OrderReport.ashx"
                            );
                            response.RspnCode = "200";
                            
                            break;
                        default:
                            response.RspnCode = "404";
                            response.RspnMsg = "操作不存在";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                response.RspnCode = "500";
                response.RspnMsg = ex.Message;
            }
            finally
            {

                if (response.RspnCode != "200")
                {
                    context.Response.Write(checkToken.printMsg());
                }
                else
                {
                    context.Response.Flush();
                    context.Response.End();
                }
            }
        }
        private void setResponse(HttpResponse response, StringBuilder sb)
        {
            response.Clear();
            response.Buffer = true;
            response.AddHeader("content-disposition", "attachment;filename=訂單下載(" + orderID + ").xls");
            response.Charset = "utf-8";
            response.ContentType = "application/vnd.ms-excel";
            string style = @"<style> .textmode { } </style>";
            response.Write("<meta http-equiv=Content-Type content=text/html;charset=utf-8>");
            response.Write(style);
            response.Output.Write(sb.ToString());
        }
        private StringBuilder report(string id)
        {
            OrderReportData t = loadOrders(id);
            StringBuilder sb = new StringBuilder();
            sb.Append("<table width='100%' cellspacing='0' cellpadding='2'>");
            sb.Append("<tr><td><b>Date: </b>" + DateTime.Now + " </td></tr>");
            sb.Append("</table>");
            sb.Append("<table border = '1'>");
            sb.Append(@"<tr>
				<td>訂購日期</td>
				<td>受訂單號</td>
				<td>訂單金額</td>	
				<td>收件人</td>
				<td>收件人電話</td>
				<td>收件人手機</td>
				<td>收件人Email</td>
				<td colspan='2'></td>
			</tr>");
            sb.Append(@"<tr style='background-color:#eeeeee;'>
			    <td style='font-weight:bold;'>&nbsp;" + t.cdate + @"</td>
				<td style='font-weight:bold;'>&nbsp;" + t.serID + @"</td>
				<td style='font-weight:bold; color:#c00000; font:14pt'>" + t.price + @"</td>
				<td style='font-weight:bold;'>&nbsp;" + t.recipient + @"</td>
				<td style='font-weight:bold;'>&nbsp;" + t.recipientTel + @"</td>
				<td style='font-weight:bold;'>&nbsp;" + t.recipientPhone + @"</td>
				<td>" + t.recipientEmail + @"</td>
				<td colspan='2'></td>	
			</tr>");
            sb.Append(@"<tr>
                <td colspan='5'>收件人地址</td>
                <td colspan='4'>發票地址</td>
            </tr>");
            sb.Append(@"<tr>
                <td colspan='5'>" + t.recipientArr + @"</td>
                <td colspan='4' style='font-weight:bold;'>" + t.senderArr + @"</td>
            </tr>");
            sb.Append(@"<tr>
				<td>發票抬頭</td>
				<td>發票統編</td>
				<td>訂單狀態</td>
				<td>訂單編號</td>
                <td>訂購人會員編號</td>
				<td>訂購人</td>
				<td>訂購人電話</td>
				<td>訂購人手機</td>
				<td>付款方式</td>
            </tr >");
            sb.Append(@"<tr style='background-color:#eeeeee;'>
			    <td>&nbsp;" + t.invoiceTitle + @"</td>
				<td>&nbsp;" + t.invoiceNo + @"</td>
				<td>" + t.status + @"</td>
				<td>&nbsp;" + t.orderID + @"</td>
                <td>&nbsp;" + t.senderMemID + @"</td>
				<td>&nbsp;" + t.sender + @"</td>
				<td>&nbsp;" + t.senderTel + @"</td>
				<td>&nbsp;" + t.senderPhone + @"</td>		
				<td style='font-weight:bold; font:14pt'>" + t.payType + @"</td>
			</tr>");
            sb.Append(@"<tr>
                <td>溫層方式</td>
                <td>貨運方式</td>
                <td colspan='7'></td>
            </tr>");
            for (int j = 0; j < t.Temperature.Count; j++)
            {
                string temperature = t.Temperature[j];
                string logistics = t.logistics[j];
                sb.Append(@"<tr>
                    <td style='font-weight:bold; font:14pt'>" + temperature + @"</td>
                    <td style='font-weight:bold; font:14pt'>" + logistics + @"</td>
                    <td colspan='7'></td>
                </tr>");
            }
            sb.Append(@"<tr>
                <td colspan='9'>訂單備註</td>
            </tr>");
            sb.Append(@"<tr>
                <td colspan='9'>&nbsp;" + t.memo + @"</td>
            </tr>");
            sb.Append(@"<tr>
				<td>序號</td>
                <td>料號</td>
				<td colspan='3'>產品名稱</td>
				<td>單價</td>
				<td>數量</td>
				<td>折扣</td>
				<td>小計</td>
			</tr>");
            for (int j = 0; j < t.prods.Count; j++)
            {
                OrderProd c = t.prods[j];
                sb.Append(@"<tr style='background-color:#FFFFCC;'>
				<td>&nbsp;" + c.serNo + @"</td>
                <td>" + c.itemNo + @"</td>
				<td colspan='3'>" + c.name +
                    (c.memo != "" ? "(" + c.memo + ")" : "") +
                    (
                        c.size != "" && c.color != "" ? "(" + c.size + "/" + c.color + ")" : (
                            c.size != "" ? "(" + c.size + ")" : (
                                c.color != "" ? "(" + c.color + ")" : ""
                            )
                        )
                    ) + @"</td>
				<td>" + c.price + @"</td>
				<td>" + c.qty + @"</td>
				<td>" + c.discount + @"</td>
				<td>" + (Convert.ToInt32((c.qty * c.price)+0.001) - c.discount) + @"</td>
			</tr>");
            }
            sb.Append(@"<tr style='background-color:#FFFFCC;'>
			    <td align='right' colspan='9'>小計：" + t.oPrice + @"</td>
			</tr>");
            if (t.bonus != 0)
            {
                sb.Append(@"<tr style='background-color:#FFFFCC;'>
				    <td align='right' colspan='9'>本次訂單扣抵紅利：" + t.bonus + @"</td>
			    </tr>");
            }
            if (t.discont != 0)
            {
                sb.Append(@"<tr style='background-color:#FFFFCC;'>
					<td align='right' colspan='9'>滿額折扣：" + t.discont + @"</td>
				</tr>");
            }
            if (t.coupon != 0)
            {
                sb.Append(@"<tr style='background-color:#FFFFCC;'>
					<td align='right' colspan='9'>優惠券(" + t.couponTitle + ")：" + t.coupon + @"</td>
				</tr>");
            }
            sb.Append(@"<tr style='background-color:#FFFFCC;'>
				<td align='right' colspan='9'>運費：" + t.freight + @"</td>
			</tr>");
            sb.Append(@"<tr style='background-color:#FFFFCC;'>
				<td align='right' colspan='9' style='color:#c00000; font-size:16pt; font-weight:bold;'>總計：" +
                    t.price + @"</td>
			</tr>");
            sb.Append("</table>");
            return sb;
        }
        private OrderReportData loadOrders(string orderId)
        {
            OrderReportData data = null;
            setBigPayType();
            if (GS.hasPwoer(checkToken.setting, "E004", "canexe", token.id))
            {
                using (SqlConnection conn = new SqlConnection(checkToken.setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        SELECT 
                        ROW_NUMBER() OVER(partition by a.id ORDER BY a.id desc) idNum,isnull(d.title,'') couponTitle,b.Temperature,
                            a.*,b.LogisticsSubType,c.title as LogisticsTypeName,convert(nvarchar, a.cdate, 120) as order_date 
                        FROM orders_hd as a
                        left join orders_Logistics as b on a.id = b.order_no
                        left join Logisticstype as c on b.LogisticstypeID=c.id
                        left join Coupon as d on a.couponID=d.id
                        where a.id=@orderId and [a].[state] not in(3,4,5,7)
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@orderId", orderId));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            if (data == null)
                            {
                                data = new OrderReportData
                                {
                                    orderID = reader["id"].ToString(),
                                    cdate = reader["order_date"].ToString(),
                                    recipient = reader["name"].ToString(),
                                    recipientArr = reader["addr"].ToString(),
                                    recipientEmail = reader["mail"].ToString(),
                                    recipientPhone = reader["cell"].ToString(),
                                    recipientTel = reader["tel"].ToString(),
                                    sender = reader["o_name"].ToString(),
                                    senderArr = reader["o_addr"].ToString(),
                                    senderMemID = reader["mem_id"].ToString(),
                                    senderPhone = reader["o_cell"].ToString(),
                                    senderTel = reader["o_tel"].ToString(),
                                    oPrice = GS.StringToInt(reader["amt"].ToString(), 0),
                                    bonus = GS.StringToInt(reader["bonus_discount"].ToString(), 0),
                                    discont = GS.StringToInt(reader["discount_amt"].ToString(), 0),
                                    coupon = GS.StringToInt(reader["couponDiscount"].ToString(), 0),
                                    freight = GS.StringToInt(reader["freightamount"].ToString(), 0),
                                    status = getStatusStr(GS.StringToInt(reader["state"].ToString(), 0)),
                                    payType = getPaymentTypeStr(reader["payment_type"].ToString()),
                                    logistics = new List<string>(),
                                    Temperature = new List<string>(),
                                    memo = reader["notememo"].ToString(),
                                    invoiceNo = reader["ident"].ToString(),
                                    invoiceTitle = reader["invoice_title"].ToString(),
                                    couponTitle = reader["couponTitle"].ToString(),
                                    serID = reader["ser_id"].ToString(),
                                    prods = getOrderProd(orderId),
                                };
                                data.price = data.oPrice - data.bonus - data.discont - data.coupon;
                                if (data.price < 0) data.price = data.freight;
                                else data.price += data.freight;
                                int status = GS.StringToInt(reader["state"].ToString(), 0);
                            }
                            data.logistics.Add(
                                reader["LogisticsTypeName"].ToString() + " " +
                                getLogisticsStr(reader["LogisticsSubType"].ToString())
                            );
                            data.Temperature.Add(getTemperatureStr(reader["Temperature"].ToString()));
                        }
                        if (data == null) throw new Exception("資料不存在");
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
            return data;
        }
        private List<OrderProd> getOrderProd(string orderId) {
            List<OrderProd> orderProd = new List<OrderProd>();
            using (SqlConnection conn = new SqlConnection(checkToken.setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                        select a.*,b.title as colortitle,c.title as sizetitle,d.itemno 
                        from orders as a 
                        left join prod_color as b on a.colorid=b.id 
                        left join prod_size as c on a.sizeid=c.id 
                        left join prod as d on a.productid=d.id
                        where order_no=@orderId
                    ", conn);
                cmd.Parameters.Add(new SqlParameter("@orderId", orderId));
                SqlDataReader reader = null;
                try {
                    reader = cmd.ExecuteReader();
                    while (reader.Read()) {
                        OrderProd prod = new OrderProd
                        {
                            serNo = reader["ser_no"].ToString(),
                            memo = reader["memo"].ToString(),
                            itemNo = reader["itemno"].ToString(),
                            color = reader["colortitle"].ToString(),
                            size = reader["sizetitle"].ToString(),
                            price = double.Parse(reader["price"].ToString()),
                            discount = long.Parse(reader["discount"].ToString()),
                            qty = int.Parse(reader["qty"].ToString()),
                            name = reader["prod_name"].ToString()
                        };
                        prod.subtotal = (long)((prod.price * prod.qty)+0.001) - prod.discount;
                        orderProd.Add(prod);
                    }
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            return orderProd;
        }
        private string getLogisticsStr(string code)
        {
            string logisticsStr = "";
            switch (code)
            {
                case "TCAT":
                    logisticsStr += " 黑貓";
                    break;
                case "ECAN":
                    logisticsStr += " 宅配通";
                    break;
            }
            return logisticsStr;
        }
        private string getStatusStr(int status)
        {
            string statusStr;
            switch (status)
            {
                case 2:
                    statusStr = "已收款";
                    break;
                case 3:
                    statusStr = "已出貨";
                    break;
                case 4:
                    statusStr = "已取消";
                    break;
                case 5:
                    statusStr = "付款失敗";
                    break;
                case 6:
                    statusStr = "出貨中";
                    break;
                case 7:
                    statusStr = "已完成(已成立)";
                    break;
                case 8:
                    statusStr = "註記中";
                    break;
                default:
                    statusStr = "審核中";
                    break;
            }
            return statusStr;
        }
        private string getPaymentTypeStr(string code)
        {
            string paymentTypeStr;
            switch (code)
            {
                case "WebATM":
                    paymentTypeStr = payStr + "-WebATM";
                    break;
                case "POD":
                    paymentTypeStr = payStr + "貨到付款";
                    break;
                case "Credit":
                    paymentTypeStr = payStr + "-線上刷卡";
                    break;
                case "Installment3":
                    paymentTypeStr = payStr + "-線上刷卡3期分期付款";
                    break;
                case "Installment6":
                    paymentTypeStr = payStr + "-線上刷卡6期分期付款";
                    break;
                case "Installment12":
                    paymentTypeStr = payStr + "-線上刷卡12期分期付款";
                    break;
                case "Installment24":
                    paymentTypeStr = payStr + "-線上刷卡24期分期付款";
                    break;
                case "CVS":
                    paymentTypeStr = payStr + "-超商繳費";
                    break;
                case "Tenpay":
                    paymentTypeStr = payStr + "-財付通";
                    break;
                case "Alipay":
                    paymentTypeStr = payStr + "-支付寶";
                    break;
                case "BARCODE":
                    paymentTypeStr = payStr + "-超商條碼";
                    break;
                case "ATM":
                    paymentTypeStr = payStr + "-虛擬帳號(ATM轉帳)";
                    break;
                case "getandpay":
                    paymentTypeStr = "貨到付款";
                    break;
                case "ezShip":
                    paymentTypeStr = "超商取貨付款";
                    break;
                case "ezship0":
                    paymentTypeStr = "超商取貨付款";
                    break;
                case "ezship1":
                    paymentTypeStr = "超商取貨不付款";
                    break;
                case "esafeWebatm":
                    paymentTypeStr = "紅陽-WebATM";
                    break;
                case "esafeCredit":
                    paymentTypeStr = "紅陽-信用卡";
                    break;
                case "esafePay24":
                    paymentTypeStr = "紅陽-超商代收";
                    break;
                case "esafePaycode":
                    paymentTypeStr = "紅陽-超商代碼付款";
                    break;
                case "esafeAlipay":
                    paymentTypeStr = "紅陽-支付寶";
                    break;
                case "chtHinet":
                    paymentTypeStr = "中華支付-Hinet帳單";
                    break;
                case "chteCard":
                    paymentTypeStr = "中華支付-Hinet點數卡";
                    break;
                case "chtld":
                    paymentTypeStr = "中華支付-行動839";
                    break;
                case "Chtn":
                    paymentTypeStr = "中華支付-市話輕鬆付";
                    break;
                case "chtCredit":
                    paymentTypeStr = "中華支付-信用卡";
                    break;
                case "chtATM":
                    paymentTypeStr = "中華支付-虛擬帳號(ATM轉帳)付款";
                    break;
                case "chtWEBATM":
                    paymentTypeStr = "中華支付-WebATM";
                    break;
                case "chtUniPresident":
                    paymentTypeStr = "中華支付-超商代收";
                    break;
                case "chtAlipay":
                    paymentTypeStr = "中華支付-支付寶";
                    break;
                case "focus":
                    paymentTypeStr = "合作金庫-線上刷卡";
                    break;
                case "tcb_allpay":
                    paymentTypeStr = "合作金庫-支付寶";
                    break;
                case "chinatrust_credit":
                    paymentTypeStr = "中國信託-線上刷卡";
                    break;
                case "EsunCredit":
                    paymentTypeStr = "玉山銀行-線上刷卡";
                    break;
                case "NcccCredit":
                    paymentTypeStr = "聯合信用卡-線上刷卡";
                    break;
                case "NcccCUPCredit":
                    paymentTypeStr = "聯合信用卡銀聯卡-線上刷卡";
                    break;
                case "fisc_Credit":
                    paymentTypeStr = "第一銀行-線上刷卡";
                    break;
                case "mPP":
                    paymentTypeStr = "藍新-線上刷卡";
                    break;
                case "ccb_credit":
                    paymentTypeStr = "國泰世華-線上刷卡";
                    break;
                case "cbbInstallment3":
                    paymentTypeStr = "國泰世華-線上刷卡3期分期付款";
                    break;
                case "cbbInstallment6":
                    paymentTypeStr = "國泰世華-線上刷卡6期分期付款";
                    break;
                case "cbbInstallment12":
                    paymentTypeStr = "國泰世華-線上刷卡12期分期付款";
                    break;
                case "cbbInstallment24":
                    paymentTypeStr = "國泰世華-線上刷卡24期分期付款";
                    break;
                case "ezpay_ATM":
                    paymentTypeStr = "虛擬帳號(ATM轉帳)";
                    break;
                case "ezpay_WEBATM":
                    paymentTypeStr = "WebATM";
                    break;
                case "ezpay_CS":
                    paymentTypeStr = "超商代收";
                    break;
                case "ezpay_MMK":
                    paymentTypeStr = "超商條碼繳費";
                    break;
                case "ezpay_ALIPAY":
                    paymentTypeStr = "支付寶";
                    break;
                case "ezpay_ALIPAY_WAP":
                    paymentTypeStr = "支付寶";
                    break;
                case "PchomePayCARD":
                    paymentTypeStr = "PchomePay 信用卡付款";
                    break;
                case "PchomePayATM":
                    paymentTypeStr = "PchomePay ATM付款";
                    break;
                case "PchomePayACCT":
                    paymentTypeStr = "PchomePay 支付連餘額付款";
                    break;
                case "PchomePayEACH":
                    paymentTypeStr = "PchomePay 支付連銀行支付付";
                    break;
                default:
                    paymentTypeStr = "ATM轉帳";
                    break;
            }
            return paymentTypeStr;
        }
        private string getTemperatureStr(string no)
        {
            string str = "";
            switch (no) {
                case "0001":
                    str = "常溫";
                    break;
                case "0002":
                    str = "冷藏";
                    break;
                case "0003":
                    str = "冷凍";
                    break;
            }
            return str;
        }
        private void setBigPayType()
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select top 1 allpay_url from head
                ", conn);
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (reader["allpay_url"].ToString().IndexOf("allpay") > 0) payStr = "歐付寶";
                        else payStr = "綠界";
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                finally
                {
                    if (reader != null) reader.Close();
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
    }
}