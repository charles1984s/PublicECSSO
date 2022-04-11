using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml.Serialization;
using ECSSO.Library;

namespace ECSSO.Concatenation
{
    /// <summary>
    /// IKPDLogin 的摘要描述
    /// </summary>
    public class IKPDLogin : IHttpHandler
    {
        string APReqEncodedData = null, portalServerIP, APName;
        string memberID = null, GCode = null;
        HttpContext context = null;
        public void ProcessRequest(HttpContext context)
        {
            string token = "";
            IKPDCust instance = null;
            string xmlstring = "";
            this.context = context;
            portalServerIP = "https://ikpd.kcg.gov.tw";
            APReqEncodedData = context.Request.Form["APReqEncodedData"];
            APName = "kpdcenter";
            if (APReqEncodedData == "" || APReqEncodedData == null) ResponseWriteEnd("參數錯誤");
            else
            {
                try
                {
                    try
                    {
                        xmlstring = HttpUtility.HtmlDecode(submitCoupon()).Trim().Replace("verify-service", "IKPDCust").Replace(System.Environment.NewLine, "");
                        StringReader readerXML = new StringReader(xmlstring);//xmlstring 是傳入 XML 格式的 string
                        XmlSerializer serializer = new XmlSerializer(typeof(IKPDCust));//documents 是 paste xml as class 來的類別
                        instance = (IKPDCust)serializer.Deserialize(readerXML);
                        token = loginForID(instance.user_account);
                        //token = loginForID("admin");
                    }
                    catch(Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                    if (token == "" || token.IndexOf("error") >= 0) throw new Exception("該帳號沒有權限");
                    else {
                        context.Response.Redirect("iKPDLoginVerification.aspx?token="+ token);
                    }
                }
                catch (Exception ex)
                {
                    ResponseWriteEnd(ex.Message);
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
        private string loginForID(string id) {
            string token = "";
            String setting = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();
            using (SqlConnection conn = new SqlConnection(setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "iKPDLogin";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@userid", id));
                cmd.Parameters.Add(new SqlParameter("@ip", (new GetStr()).GetIPAddress()));
                SqlParameter SPOutput = cmd.Parameters.Add("@ReturnCode", SqlDbType.NVarChar, 50);
                SPOutput.Direction = ParameterDirection.Output;
                try
                {
                    cmd.ExecuteNonQuery();
                    token = SPOutput.Value.ToString();
                }
                catch { }
            }
            return token;
        }
        private string submitCoupon()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(portalServerIP + "/admin/ApTicket_Verify.aspx");
            request.Method = "POST";
            string postData = "apticket=" + APReqEncodedData + "&apid=" + APName;
            //context.Response.Write("postData:"+postData);
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();

            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();

            dataStream = response.GetResponseStream();
            StreamReader reader2 = new StreamReader(dataStream);
            string responseFromServer = reader2.ReadToEnd();

            reader2.Close();
            dataStream.Close();
            response.Close();
            return responseFromServer;
        }
        private void ResponseWriteEnd(string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }
    }
}