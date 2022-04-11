using ECSSO.Extension;
using ECSSO.Library;
using ECSSO.Library.Enumeration;
using ECSSO.Library.Order;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.api.Order
{
    /// <summary>
    /// OrderHandler 的摘要描述
    /// </summary>
    public class OrderHandler : IHttpHandler
    {
        private string setting, payStr, servicePriceType;
        private bool othersLink;
        private GetStr GS;
        private FooTable order;
        private TokenItem token;
        public void ProcessRequest(HttpContext context)
        {
            GS = new GetStr();
            string code, message;
            code = "404";
            message = "not fount";
            othersLink = false;
            payStr = "";
            try {
                order = new FooTable();
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
                            case "orderHd":
                                string searchStr, searchQA, orderBy;
                                int page, size;
                                List<int> stat = new List<int>();
                                if (context.Request.Form["searchStr"] != null && context.Request.Form["searchStr"] != "") {
                                    searchStr = "%" + context.Request.Form["searchStr"] + "%";
                                } else searchStr = "%%";

                                if (context.Request.Form["searchStat"] != null && context.Request.Form["searchStat"] != "")
                                {
                                    string[] attr = context.Request.Form["searchStat"].Split(',');
                                    for (int i = 0; i < attr.Length; i++)
                                    {
                                        try
                                        {
                                            stat.Add(int.Parse(attr[i]));
                                        }catch{}
                                    }
                                }
                                if (context.Request.Form["searchQA"] != null && context.Request.Form["searchQA"] != "")
                                {
                                    searchQA = context.Request.Form["searchQA"];
                                }
                                else searchQA = "";
                                if (context.Request.Form["orderBy"] != null && context.Request.Form["orderBy"] != "")
                                {
                                    orderBy = context.Request.Form["orderBy"];
                                }
                                else orderBy = "id desc";
                                if (context.Request.Form["page"] != null && context.Request.Form["page"] != "")
                                {
                                    try
                                    {
                                        page = int.Parse(context.Request.Form["page"]);
                                    }
                                    catch
                                    {
                                        page = 1;
                                    }
                                }
                                else page = 1;
                                if (context.Request.Form["size"] != null && context.Request.Form["size"] != "")
                                {
                                    try
                                    {
                                        size = int.Parse(context.Request.Form["size"]);
                                    }
                                    catch
                                    {
                                        size = 20;
                                    }
                                }
                                else size = 20;
                                loadOrderHd(searchStr, stat, searchQA, orderBy, page, size);
                                break;
                            default:
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
        private List<FooTableColumn> crateCustTableColumn()
        {
            List<FooTableColumn> col = new List<FooTableColumn>();
            col.Add(new FooTableColumn
            {
                name = "orderID",
                title = "訂單編號",
                type = "text",
                sortable = true,
                breakpoints = "xs"
            });
            col.Add(new FooTableColumn
            {
                name = "cdate",
                title = "訂單成立時間",
                type = "date",
                sortable = true,
                breakpoints = null,
                style = new FooTableStyle { maxWidth = 100}
            });
            col.Add(new FooTableColumn
            {
                name = "recipient",
                title = "收件人",
                type = "text",
                sortable = true,
                breakpoints = "xs"
            });
            col.Add(new FooTableColumn
            {
                name = "price",
                title = "結帳金額",
                type = "price",
                sortable = true,
                breakpoints = null
            });
            col.Add(new FooTableColumn
            {
                name = "status",
                title = "訂單狀態",
                type = "text",
                sortable = true,
                breakpoints = null
            });
            if (othersLink)
            {
                col.Add(new FooTableColumn
                {
                    name = "othersLink",
                    title = "第三方串接",
                    type = "othersLink",
                    sortable = true,
                    breakpoints = null,
                    style = new FooTableStyle { maxWidth = 100 }
                });
            }
            col.Add(new FooTableColumn
            {
                name = "view",
                title = "瀏覽",
                type = "view",
                breakpoints = "view",
                sortable = false,
                style = new FooTableStyle { width = 70, maxWidth = 70 }
            });
            col.Add(new FooTableColumn
            {
                name = "export",
                title = "匯出",
                type = "export",
                breakpoints = "xs",
                sortable = false,
                style = new FooTableStyle { width = 70, maxWidth = 70 }
            });
            col.Add(new FooTableColumn
            {
                name = "finish",
                title = @"改為
                <select name='state'>
                    <option value='6'>出貨中</option>
                    <option value='3'>已出貨</option>
                    <option value='7' selected=''>已完成</option>
                </select>",
                type = "finish",
                breakpoints = "xs",
                sortable = false,
                style = new FooTableStyle { width = 120, maxWidth = 120 }
            });
            col.Add(new FooTableColumn
            {
                name = "oPrice",
                title = "商品金額",
                type = "price",
                breakpoints = "xs sm lg"
            });
            if (servicePriceType != "N")
            {
                col.Add(new FooTableColumn
                {
                    name = "servicePrice",
                    title = "服務費",
                    type = "text",
                    breakpoints = "xs sm lg"
                });
            }
            col.Add(new FooTableColumn
            {
                name = "bonus",
                title = "紅利折抵",
                type = "price",
                breakpoints = "xs sm lg"
            });
            col.Add(new FooTableColumn
            {
                name = "discont",
                title = "滿額折抵",
                type = "price",
                breakpoints = "xs sm lg"
            });
            col.Add(new FooTableColumn
            {
                name = "coupon",
                title = "優惠券現金折抵",
                type = "price",
                breakpoints = "xs sm lg"
            });
            col.Add(new FooTableColumn
            {
                name = "freight",
                title = "運費",
                type = "price",
                breakpoints = "xs sm lg"
            });
            col.Add(new FooTableColumn
            {
                name = "price2",
                title = "結帳金額",
                type = "price",
                breakpoints = "xs sm lg"
            });
            col.Add(new FooTableColumn
            {
                name = "payType",
                title = "付款方式",
                type = "text",
                breakpoints = "xs sm lg"
            });
            col.Add(new FooTableColumn
            {
                name = "logistics",
                title = "運費方式",
                type = "text",
                breakpoints = "xs sm lg"
            });
            return col;
        }
        private void initTable()
        {
            order.table = new FooTableDetail
            {
                sorting = new FooTableSort
                {
                    enabled = true
                },
                paging = new FooTablePaging { 
                    enabled = false,
                    limit = 3
                },
                empty = "查無資料",
                columns = crateCustTableColumn(),
                rows = new List<FooTabkeRow>()
            };
        }
        private void loadOrderHd(string searchStr, List<int> searchStat,string searchQA,string orderBy,int page,int size) {
            setBigPayType();
            initTable();
            if (GS.hasPwoer(setting, "E004", "canexe", token.id)) {
                using (SqlConnection conn = new SqlConnection(setting)) {
                    bool insertDeliveryDateCol = false;
                    conn.Open();
                    order.table.paging.size = size;
                    SqlCommand cmd = new SqlCommand(@"
                        select * from(
                            select ROW_NUMBER() OVER(
                                order by 
                                    case WHEN @orderBy = 'orderID ASC' Then id ELSE null END ASC,
                                    case WHEN @orderBy = 'orderID DESC' Then id ELSE null END DESC,
                                    case WHEN @orderBy = 'cdate ASC' Then CONVERT(datetime,cdate) ELSE null END ASC,
                                    case WHEN @orderBy = 'cdate DESC' Then CONVERT(datetime,cdate) ELSE null END DESC,
                                    case WHEN @orderBy = 'recipient ASC' Then name ELSE null END ASC,
                                    case WHEN @orderBy = 'recipient DESC' Then name ELSE null END DESC,
                                    case WHEN @orderBy = 'price ASC' Then convert(int,price) ELSE null END ASC,
                                    case WHEN @orderBy = 'price DESC' Then convert(int,price) ELSE null END DESC,
                                    case WHEN @orderBy = 'status ASC' Then [state] ELSE null END ASC,
                                    case WHEN @orderBy = 'status DESC' Then [state] ELSE null END DESC
                            ) rowNumber,(select count(*) from orders_hd) [count],* from (
                                select case WHEN reg>0 then reg+freightamount else freightamount end price,* from(
                                    SELECT
                                        case when isnull(a.affi_id,'') = '' then
					                        case when isnull(a.RID,'') = '' then '無' else '美安' end
				                        else '聯盟網' end thirdShop,
				                        ROW_NUMBER() OVER(partition by a.id ORDER BY a.id desc) idNum,
                                        amt-convert(int,bonus_discount)-discount_amt-couponDiscount reg,p.title paymentTitle,
                                        a.*,b.LogisticsSubType,c.title as LogisticsTypeName,convert(nvarchar, cdate, 120) as order_date 
                                    FROM orders_hd as a
                                    left join orders_Logistics as b on a.id = b.order_no
                                    left join Logisticstype as c on b.LogisticstypeID=c.id
                                    left join paymenttype as p on p.code=a.payment_type
                                    where
                                        case 
                                            when a.id like '%' + @searchstr + '%' then 1
                                            when o_name like '%' + @searchstr + '%' then 1
                                            when name like '%' + @searchstr + '%' then 1
                                            when replace(o_tel,'-','') like '%' + replace(@searchstr,'-','') + '%' then 1
                                            when replace(o_cell,'-','') like '%' + replace(@searchstr,'-','') + '%' then 1
                                            when mail like '%' + @searchstr + '%' then 1
                                            when @searchstr='' then 1
                                            else 0
                                        end = 1 and
                                        case
                                            when exists(select * from @searchStat where id = [state] and (a.[type]<>'S' or amt<>0)) then 1
                                            when exists(select * from @searchStat where id = -1 and a.[type]='S' and amt=0) then 1
                                            when not exists(select * from @searchStat) then 1
                                            else 0
                                        end =1 and
                                        case
                                            when @searchQA='Y' and a.id in(select distinct(order_no) from orders_QA) then 1
                                            when @searchQA='A' and a.id in(select distinct(order_no) from orders_QA where DATALENGTH(Answer) <= 1) then 1
                                            when @searchQA='' then 1
                                            else 0
                                        end = 1
                                ) as a where idNum = 1
                            ) as b
                        ) as c
                        where rowNumber>(@page-1)*@count and rowNumber<=@page*@count
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@searchstr", searchStr));
                    cmd.Parameters.Add(new SqlParameter("@searchQA", searchQA));
                    cmd.Parameters.Add(new SqlParameter("@orderBy", orderBy));
                    cmd.Parameters.Add(new SqlParameter("@page", page));
                    cmd.Parameters.Add(new SqlParameter("@count", size));
                    cmd.AddParameter("@searchStat", searchStat);
                    SqlDataReader reader = null;
                    try {
                        reader = cmd.ExecuteReader();
                        while (reader.Read()) {
                            OrderHead orderHead = new OrderHead
                            {
                                orderID = reader["id"].ToString(),
                                cdate = reader["order_date"].ToString(),
                                recipient = reader["payment_type"].ToString()=="PCHomeIPL7"?
                                    "支付連超取付" :
                                    GS.Rename2(reader["name"].ToString(), 1, 1),
                                oPrice = GS.StringToInt(reader["amt"].ToString(), 0),
                                bonus = GS.StringToInt(reader["bonus_discount"].ToString(), 0),
                                discont = GS.StringToInt(reader["discount_amt"].ToString(), 0),
                                coupon = GS.StringToInt(reader["couponDiscount"].ToString(), 0),
                                freight = GS.StringToInt(reader["freightamount"].ToString(), 0),
                                servicePrice = GS.StringToInt(reader["servicePrice"].ToString(), 0),
                                status = Enum.GetName(typeof(ORderStatusEnum),
                                    (reader["type"].ToString() == "S" && GS.StringToInt(reader["amt"].ToString(), 0) == 0) ?
                                    -1: GS.StringToInt(reader["state"].ToString(), 0)
                                ),
                                payType = reader["paymentTitle"].ToString(),
                                logistics = reader["LogisticsTypeName"].ToString() + " " + 
                                        getLogisticsStr(reader["LogisticsSubType"].ToString()),
                                view = reader["id"].ToString(),
                                export = reader["id"].ToString(),
                                finish = reader["id"].ToString(),
                                deliveryDate = reader["deliveryDate"].ToString(),
                                othersLink = reader["thirdShop"].ToString()
                            };
                            if (reader["type"].ToString() == "S") orderHead.servicePrice = 0;
                            orderHead.price = orderHead.oPrice + orderHead.servicePrice - orderHead.bonus - orderHead.discont - orderHead.coupon;
                            if (orderHead.price < 0) orderHead.price = orderHead.freight;
                            else orderHead.price += orderHead.freight;
                            if (string.IsNullOrEmpty(orderHead.payType)) getPaymentTypeStr(reader["payment_type"].ToString());
                            if (!insertDeliveryDateCol && !String.IsNullOrEmpty(orderHead.deliveryDate)) {
                                insertDeliveryDateCol = true;
                                order.table.columns.Add(new FooTableColumn {
                                    name = "deliveryDate",
                                    title = "到貨日",
                                    type = "text",
                                    breakpoints = "xs sm lg"
                                });
                            }
                            orderHead.price2 = orderHead.price;
                            int status = GS.StringToInt(reader["state"].ToString(),0);
                            string gray = "";
                            if (status == 4 || status == 5 || status == 7) {
                                gray = " gray";
                            }
                            order.table.rows.Add(new FooTabkeRow
                            {
                                options = new RowOptions
                                {
                                    classes = "cell"+ gray,
                                    expanded = false
                                },
                                value = orderHead
                            });
                            order.table.total = GS.StringToInt(reader["count"].ToString(), 0);
                        }
                    }
                    finally
                    {
                        if(reader!=null) reader.Close();
                    }
                }
            }
            else order.table.empty = "沒有權限";
        }
        private string getLogisticsStr(string code) {
            string logisticsStr="";
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
        private string getPaymentTypeStr(string code) {
            string paymentTypeStr = "";
            switch (code) {
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
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(@"
                            select title from paymenttype where code=@code
                        ", conn);
                        cmd.Parameters.Add(new SqlParameter("@code", code));
                        SqlDataReader reader = null;
                        try {
                            reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                paymentTypeStr = reader["title"].ToString();
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception(e.Message);
                        }
                        finally
                        {
                            if (reader != null) reader.Close();
                            if(paymentTypeStr=="") paymentTypeStr = "ATM轉帳";
                        }
                    }
                    break;
            }
            return paymentTypeStr;
        }
        private void setBigPayType() {
            using (SqlConnection conn = new SqlConnection(setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select top 1 allpay_url,Affi_Site_ID,market_taiwan,servicePriceType from head
                ", conn);
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (reader["allpay_url"].ToString().IndexOf("allpay") > 0) payStr = "歐付寶";
                        else payStr = "綠界";
                        if (reader["market_taiwan"].ToString() == "Y" || reader["Affi_Site_ID"].ToString()!="")
                            othersLink = true;
                        servicePriceType = reader["servicePriceType"].ToString();
                    }
                }
                catch (Exception e) {
                    throw new Exception(e.Message);
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
        }
        private String printMsg(String RspnCode, String RspnMsg)
        {
            order.RspnCode = RspnCode;
            order.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(order);
        }
    }
}