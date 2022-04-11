using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ECSSO.CockerAdmin;
using System.Configuration;
using System.Data;
using System.Xml;

namespace ECSSO
{
    public partial class News : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            CockerAdmin.CockerAdmin CA = new CockerAdmin.CockerAdmin();
            String Orgname = "";

            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings["sqlDB"];
            SqlDataSource conn = new SqlDataSource();
            conn.ConnectionString = setting.ConnectionString;
            String str_sql = "select comp_en_name from head";
            conn.SelectCommand = str_sql;
            DataView DV = (DataView)conn.Select(DataSourceSelectArguments.Empty);
            if (DV.Table.Rows.Count > 0)
            {
                Orgname = DV.Table.Rows[0][0].ToString();
            }
            Response.Write("<div>網站到期日：" + CA.GetExpiration(Orgname) + "</div>");

            //取得根節點內的子節點
            XmlDocument doc = new XmlDocument();
            if (Request.QueryString["ID"] != null)
            {
                doc.LoadXml(CA.GetNews(Request.QueryString["ID"].ToString()));
            }
            else
            {
                doc.LoadXml(CA.GetNews(""));
            }


            //選擇節點
            XmlNodeList main = doc.SelectNodes("News/NewsNode");
            foreach (XmlElement element in main)
            {
                Response.Write("<div style='width:400px; height:30px; border-bottom:#cccccc 1px dotted;'>");
                //Response.Write("<div style='float:left; width:20px;'><a href='news.aspx?ID=" + element["ID"].InnerText + "'>" + element["ID"].InnerText + "</a></div>");
                Response.Write("<div style='float:left; width:100px; padding-top:5px;'>" + element["NoteDate"].InnerText + "</div>");
                Response.Write("<div style='float:left; width:300px; padding-top:5px;'><a href='news.aspx?ID=" + element["ID"].InnerText + "'>" + element["Title"].InnerText + "</a></div>");
                //Response.Write(element["ID"].InnerText + "<Br>");
                //Response.Write(element["Title"].InnerText + "<Br>");
                //Response.Write(element["NoteDate"].InnerText + "<Br>");            
                Response.Write("</div>");
                if (Request.QueryString["ID"] != null)
                {
                    Response.Write("<div style='width:400px;'>" + element["Cont"].InnerText + "</div>");
                    Response.Write("<div style='width:400px; text-align:center;'><a href='news.aspx'>回上頁</a></div>");
                }
            }
        }
    }
}