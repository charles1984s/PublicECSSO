using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Cors;

namespace WebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            string settingSite = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();
            string urlList = "";
            using (SqlConnection conn = new SqlConnection(settingSite))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select web_url from cocker_cust where stat='Y' and GETDATE() between convert(datetime,[start_date]) and convert(datetime,[end_date])", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        string url = reader["web_url"].ToString().Trim().ToLower();
                        Regex reg = new Regex("^http");
                        if (!reg.IsMatch(url))
                        {
                            url = "http://" + url;
                        }
                        urlList += url + ",";
                    }
                    if (urlList != "")
                    {
                        urlList = urlList.Substring(0, urlList.Length - 1);
                    }
                }
                catch { 
                
                }
            }
            if (urlList != "")
                config.EnableCors(new EnableCorsAttribute(urlList, "*", "*"));
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            //移除 XML Formatter
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            //JSON 縮排
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;

            // 取消註解以下程式碼行以啟用透過 IQueryable 或 IQueryable<T> 傳回類型的動作查詢支援。
            // 為了避免處理未預期或惡意佇列，請使用 QueryableAttribute 中的驗證設定來驗證傳入的查詢。
            // 如需詳細資訊，請造訪 http://go.microsoft.com/fwlink/?LinkId=279712。
            //config.EnableQuerySupport();

            // 若要停用您應用程式中的追蹤，請將下列程式碼行標記為註解或加以移除
            // 如需詳細資訊，請參閱: http://www.asp.net/web-api
            config.EnableSystemDiagnosticsTracing();
        }
    }
}
