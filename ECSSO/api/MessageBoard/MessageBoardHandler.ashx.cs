using ECSSO.Library;
using ECSSO.Library.MessageBoard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ECSSO.api.MessageBoard
{
    /// <summary>
    /// MessageBoardHandler 的摘要描述
    /// </summary>
    public class MessageBoardHandler : IHttpHandler
    {
        private CheckToken checkToken;
        public void ProcessRequest(HttpContext context)
        {
            checkToken = new CheckToken();
            checkToken.check(context);
            try
            {
                if (checkToken.response.RspnCode != "200") throw new Exception("Token不存在");
                IMessageBoardList list = new IMessageBoardList(checkToken.setting);
                setResponse(context.Response, list.getHtmlString());
            }
            catch (Exception e)
            {
                checkToken.response.RspnCode = "500";
                checkToken.response.RspnMsg = e.Message;
            }
            finally {
                if (checkToken.response.RspnCode != "200")
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
            response.AddHeader("content-disposition", "attachment;filename=留言板匯出.doc");
            response.Charset = "utf-8";
            response.ContentType = "application/octet-stream";
            string style = @"<style> .textmode { } </style>";
            response.Write("<meta http-equiv=Content-Type content=text/html;charset=utf-8>");
            response.Write(style);
            response.Output.Write(sb.ToString());
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