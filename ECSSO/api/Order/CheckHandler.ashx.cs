using ECSSO.Library;
using ECSSO.Library.Order.Check;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.api.Order
{
    /// <summary>
    /// CheckHandler 的摘要描述
    /// </summary>
    public class CheckHandler : IHttpHandler
    {
        private CheckToken checkToken;
        private responseJson data;
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Form["type"].ToUpper().IndexOf("PROD") >= 0)
            {
                data = new CheckArgProdList();
            }
            else if (context.Request.Form["type"].ToUpper().IndexOf("ORDER") >= 0)
            {
                data = new CheckOrder();
            }
            else
            {
                data = new CheckList();
            }
            checkToken = new CheckToken(data);
            try
            {
                data.RspnCode = "500";
                checkToken.check(context);
                if (data.RspnCode == "200")
                {
                    data.RspnCode = "500.1";
                    Method(context);
                    data.RspnCode = "200";
                }
                else throw new Exception("Token不存在");
            }
            catch (Exception ex)
            {
                data.RspnMsg = ex.Message;
            }
            finally
            {
                context.Response.Write(checkToken.printMsg());
            }
        }
        private void Method(HttpContext context)
        {
            switch (context.Request.Form["type"].ToUpper())
            {
                case "LIST":
                    ((CheckList)data).getCheckList();
                    break;
                case "NEWRANGE":
                    ((CheckList)data).getNotCheckDate();
                    break;
                case "AGRPRODLIST":
                    ((CheckArgProdList)data).getProds();
                    break;
                case "ORDERLIST":
                    ((CheckOrder)data).getOrders();
                    break;
                case "ORDERLISTVIEW":
                    ((CheckOrder)data).getOrdersView(int.Parse(context.Request["id"]));
                    break;
                case "NEW":
                    ((CheckList)data).AddCheck(JsonConvert.DeserializeObject<CheckAddDto>(context.Request.Form["data"]));
                    break;
                case "DELETE":
                    ((CheckList)data).Delete(int.Parse(context.Request["id"]));
                    break;
                default:
                    data.RspnCode = "500.9";
                    throw new Exception("執行目標不明確");
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