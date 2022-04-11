using System;
using System.Collections.Generic;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Meal.Library;

namespace Meal.api
{
    /// <summary>
    /// Report 的摘要描述
    /// </summary>
    public class Report : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");

            /*每日營業額
            select SUM(Convert(int,amt)) as amt,CONVERT(VARCHAR(10) , cdate, 111 ) as cdate 
            from orders_hd 
            where state in ('2','3','7')
            group by CONVERT(VARCHAR(10) , cdate, 111 )             
             */

            /*期間產品銷售量
            select a.productid,a.prod_name,sum(a.qty) as salesnum 
            from orders as a left join orders_hd as b on a.order_no=b.id 
            where b.state in ('2','3','7')
            and a.Cdate between '2015-12-01 00:00:00' and '2016-03-01 23:59:59'
            group by a.prod_name,a.productid              
             */
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {

            Library.Account.ErrorObject root = new Library.Account.ErrorObject();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);

        }
        #endregion

        private void ResponseWriteEnd(HttpContext context, string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }
    }
}