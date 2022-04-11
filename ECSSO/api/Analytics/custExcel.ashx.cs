using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using ECSSO.api.CustFormAPI;
using ECSSO.Library;
using ECSSO.Library.Analytics;
using Newtonsoft.Json;

namespace ECSSO.api.Analytics
{
    /// <summary>
    /// custExcel 的摘要描述
    /// </summary>
    public class custExcel : IHttpHandler
    {
        HttpContext context;
        GetStr GS;
        TokenItem token;
        responseJson responseJson;
        string setting;
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            string code, message, type;
            code = "404";
            message = "not fount";
            token = null;
            GS = new GetStr();
            responseJson = new responseJson();
            try
            {
                if (context.Request.Form["token"] != null)
                {
                    token = new TokenItem
                    {
                        token = context.Request.Form["token"]
                    };
                    this.setting = GS.checkToken(token);
                    type = context.Request.Form["type"];
                    if (this.setting.IndexOf("error") < 0)
                    {
                        int id=0;
                        try
                        {
                            id = int.Parse(context.Request.Form["id"]);
                        }
                        catch {
                            throw new Exception("資料格式錯誤");
                        }
                        Flow2 f = submitRequest(id, type);
                        if (f.RspnCode == "200")
                        {
                            setResponse(context.Response, report(f));
                            code = "200";
                        }
                        else {
                            code = f.RspnCode;
                            message = f.RspnMsg;
                        }
                    }
                    else
                    {
                        code = "401";
                        message = "Token已過期";
                    }
                }
                else
                {
                    code = "401";
                    message = "Token不可為空";
                }
            }
            catch (Exception ex)
            {
                code = "500";
                message = ex.Message + ex.TargetSite;
            }
            finally
            {
                if (code != "200")
                {
                    context.Response.Write(printMsg(code, message));
                }
                else {
                    context.Response.Flush();
                    context.Response.End();
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
        private StringBuilder report(Flow2 f) {
            FlowData2 t= f.table;
            StringBuilder sb = new StringBuilder();
            sb.Append("<table width='100%' cellspacing='0' cellpadding='2'>");
            sb.Append("<tr><td><b>Date: </b>" + DateTime.Now + " </td></tr>");
            sb.Append("</table>");
            sb.Append("<table border = '1'>");
            sb.Append("<tr>");
            for (int i=0; i< t.columns.Count; i++) {
                Library.CustFormLibary.FormColumn c = t.columns[i];
                sb.Append("<th style = 'background-color: #D20B0C;color:#ffffff'>");
                sb.Append(c.title);
                sb.Append("</th>");
            }
            sb.Append("</tr>");
            for (int i = 0; i < t.rows.Count; i++)
            {
                ReportTableItem r = t.rows[i].value;
                sb.Append("<tr>");
                for (int j = 0; j < t.columns.Count; j++)
                {
                    Library.CustFormLibary.FormColumn c = t.columns[j];
                    sb.Append("<td>");
                    switch (c.name) {
                        case "age":
                            sb.Append(r.age);
                            break;
                        case "sex":
                            sb.Append(r.sex);
                            break;
                        case "marry":
                            sb.Append(r.marry);
                            break;
                        case "org":
                            sb.Append(r.org);
                            break;
                        case "chf":
                            sb.Append(r.chf);
                            break;
                        case "crk":
                            sb.Append(r.crk);
                            break;
                        case "POPESN":
                            sb.Append(r.POPESN);
                            break;
                        case "TSER_day":
                            sb.Append(r.TSER_day);
                            break;
                        case "ETP":
                            sb.Append(r.ETP);
                            break;
                        case "ESU":
                            sb.Append(r.ESU);
                            break;
                        case "ELV":
                            sb.Append(r.ELV);
                            break;
                        case "menuSub":
                            sb.Append(r.menuSub);
                            break;
                        case "menu":
                            sb.Append(r.menu);
                            break;
                        case "tag":
                            sb.Append(r.tag);
                            break;
                        case "name":
                            sb.Append(r.name);
                            break;
                        case "peopleTh":
                            sb.Append(r.peopleTh);
                            break;
                        case "orgLevel":
                            sb.Append(r.orgLevel);
                            break;
                        case "count":
                            sb.Append(r.count);
                            break;
                        case "countOfperson":
                            sb.Append(r.countOfperson);
                            break;
                        case "zip":
                            sb.Append(r.zip);
                            break;
                    }
                    sb.Append("</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            return sb;
        }
        private Flow2 submitRequest(int id,string type)
        {
            Flow2 responseFromServer;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                        ConfigurationManager.AppSettings["Protocol"].ToString() + "://" +
                        ConfigurationManager.AppSettings["Server_Host"].ToString() +
                        "/api/Analytics/Analytics.ashx");
                request.Method = "POST";
                request.Timeout = 130 * 1000;
                string postData = "token=" + token.token + "&type=" + type + "&id=" + id;
                //context.Response.Write("postData:"+postData);
                byte[]  byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();

                dataStream = response.GetResponseStream();
                StreamReader reader2 = new StreamReader(dataStream);
                string str = reader2.ReadToEnd();
                responseFromServer = JsonConvert.DeserializeObject<Flow2>(str);
                reader2.Close();
                dataStream.Close();
                response.Close();
            }
            catch (WebException even)
            {
                using (StreamReader sr =
                    new StreamReader(even.Response.GetResponseStream()))
                {
                    throw new Exception(sr.BaseStream.ToString());
                }
            }
            return responseFromServer;
        }
        private void setResponse(HttpResponse response,StringBuilder sb) {
            response.Clear();
            response.Buffer = true;
            response.AddHeader("content-disposition", "attachment;filename=Export("+ DateTime.Now + ").xls");
            response.Charset = "utf-8";
            response.ContentType = "application/vnd.ms-excel";
            string style = @"<style> .textmode { } </style>";
            response.Write("<meta http-equiv=Content-Type content=text/html;charset=utf-8>");
            response.Write(style);
            response.Output.Write(sb.ToString());
        }
        private String printMsg(String RspnCode, String RspnMsg)
        {
            responseJson.RspnCode = RspnCode;
            responseJson.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(responseJson);
        }
    }
}