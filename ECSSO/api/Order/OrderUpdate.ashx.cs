using ECSSO.Library;
using ECSSO.Library.EmailCont;
using ECSSO.Library.Order;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace ECSSO.api.Order
{
    /// <summary>
    /// OrderUpdate 的摘要描述
    /// </summary>
    public class OrderUpdate : IHttpHandler
    {
        private GetStr GS;
        private responseJson response;
        private TokenItem token;
        private CheckToken checkToken;
        private HttpContext context;
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;// SecurityProtocolType.Tls1.2;
            checkToken = new CheckToken();
            try
            {
                checkToken.check(context);
                response = checkToken.response;
                token = checkToken.token;
                GS = checkToken.GS;
                if (response.RspnCode == "200")
                {
                    FinishOrder order = new FinishOrder(checkToken, context.Request.Form["id"]);
                    switch (context.Request.Form["type"])
                    {
                        case "change":
                            int status = int.Parse(context.Request.Form["status"]);
                            switch (status)
                            {
                                case 1://審核中
                                    order.underReviewOrder();
                                    break;
                                case 2://已收款
                                    order.paidForOrder();
                                    break;
                                case 3://已出貨
                                    order.ShippedOrder();
                                    break;
                                case 4://已取消
                                    order.cancelOrder();
                                    break;
                                case 6://出貨中
                                    order.ShippingOrder();
                                    break;
                                case 7://已完成
                                    order.finishOrder();
                                    break;
                                case 8://修改
                                    order.MemoOrder();
                                    break;
                                default:
                                    throw new Exception("狀態錯誤");
                            }
                            response.RspnCode = "200";
                            if (response.RspnMsg == "") response.RspnMsg = "儲存成功";
                            break;
                        case "MarketTaiwan":
                            MarketTaiwan marketTaiwan = order.marketTaiwan;
                            if (marketTaiwan.enable)
                            {
                                if (context.Request.Form["status"] == "7" || context.Request.Form["status"] == "4")
                                {
                                    if (context.Request.Form["status"] == "7") marketTaiwan.state = true;
                                    if (context.Request.Form["status"] == "4") marketTaiwan.state = false;
                                    marketTaiwan.setMarketTaiwanPrice();
                                    if (marketTaiwan.state || marketTaiwan.stateCode == 7)
                                        marketTaiwan.submitMarketTaiwan();
                                    response.RspnCode = "200";
                                }
                                else throw new Exception("非傳送美安之訂單狀態");
                            }
                            else throw new Exception("並未啟動美安系統");
                            break;
                        case "Affiliates":
                            Affiliates affiliates = order.affiliates;
                            if (affiliates.enable)
                            {
                                if (context.Request.Form["status"] == "7" || context.Request.Form["status"] == "4")
                                {
                                    if (context.Request.Form["status"] == "7")
                                        affiliates.status = AffiliatesAtatusList.Confirm;
                                    if (context.Request.Form["status"] == "4")
                                        affiliates.status = AffiliatesAtatusList.Return;
                                    affiliates.setAffiliates();
                                    affiliates.submitAffiliates();
                                    response.RspnCode = "200";
                                }
                                else throw new Exception("非傳送聯盟網之訂單狀態");
                            }
                            else throw new Exception("並未啟動聯盟網系統");
                            break;
                        case "Inquire":
                            int price = 0;
                            if (int.TryParse(context.Request.Form["price"], out price))
                            {
                                order.setOrderPrice(price);
                            }
                            else throw new Exception("詢價金額不可為空");
                            break;
                        case "Store2Code":
                            order.setOrderStore2Code(context.Request.Form["code"]);
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
                context.Response.Write(checkToken.printMsg());
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