using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Threading;
using Microsoft.Security.Application;

namespace ECSSO
{
    public partial class orderqa : System.Web.UI.Page
    {
        private string str_language = string.Empty;
        #region  語系變換
        protected override void InitializeCulture()
        {
            //此currentculture來自default.aspx頁面上兩個超連結的連結位址,見html部分            
            if (Request.QueryString["language"] != null)
            {
                str_language = Request.QueryString["language"].ToString();
            }
            else
            {
                if (Request.Form["language"] != null)
                {
                    str_language = Request.Form["language"].ToString();
                }
            }
            if (str_language == "")
            {
                str_language = "zh-tw";
            }

            if (!String.IsNullOrEmpty(str_language))
            {
                //Nation - 決定了採用哪一種當地語系化資源，也就是使用哪種語言
                //Culture - 決定各種資料類型是如何組織，如數位與日期
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(str_language);
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(str_language);
            }
        }
        #endregion
        protected void Page_Init(object sender, EventArgs e)
        {
            HtmlGenericControl objLink = new HtmlGenericControl("link");
            objLink.Attributes.Add("rel", "stylesheet");
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/orderqa.css");
            objLink.Attributes.Add("type", "text/css");
            this.Page.Header.Controls.Add(objLink);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            this.language.Value = str_language;
            if (!IsPostBack)
            {
                #region 檢查必要參數
                if (Request.Params["OrderNo"] != null)
                {
                    this.OrderNo.Value = Request.Params["OrderNo"].ToString();
                }

                if (Request.Params["SiteID"] != null)
                {
                    this.siteid.Value = Request.Params["SiteID"].ToString();
                }
                String CheckM = "";
                if (Request.Params["CheckM"] != null)
                {
                    CheckM = Encoder.HtmlEncode(Request.Params["CheckM"].ToString());
                }
                #endregion
                GetStr GS = new GetStr();
                if (GS.MD5Check(this.siteid.Value + GS.GetOrgName(GS.GetSetting(this.siteid.Value)) + this.OrderNo.Value, CheckM))
                {
                    String Setting = GS.GetSetting(this.siteid.Value);

                    using (SqlConnection conn1 = new SqlConnection(Setting))
                    {
                        conn1.Open();

                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandText = "sp_OrderQAChk";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn1;
                        cmd.Parameters.Add(new SqlParameter("@orderno", this.OrderNo.Value));
                        cmd.ExecuteNonQuery();
                    }

                    OrderQA.InnerHtml = GetFile(Setting);
                }
            }            
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            GetStr GS = new GetStr();
            String Setting = GS.GetSetting(this.siteid.Value);

            if (this.QuestionText.Text != "")
            {
                using (SqlConnection conn1 = new SqlConnection(Setting))
                {
                    conn1.Open();

                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "sp_OrderQAAdd";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn1;
                    cmd.Parameters.Add(new SqlParameter("@orderno", this.OrderNo.Value));
                    cmd.Parameters.Add(new SqlParameter("@question", Server.HtmlEncode(this.QuestionText.Text)));
                    cmd.ExecuteNonQuery();
                }
                this.QuestionText.Text = "";
            }
            OrderQA.InnerHtml = GetFile(Setting);            
        }

        private String GetFile(String Setting) {
            String ReturnStr = "";
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select Question,Qdate,Answer,Adate from orders_QA where order_no=@order_no order by id", conn);
                cmd.Parameters.Add(new SqlParameter("@order_no", this.OrderNo.Value));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.FieldCount > 0)
                    {
                        while (reader.Read())
                        {
                            ReturnStr += "<div class='col-md-12 question'>" + reader[0].ToString() + "<div class='time'>(發問時間:" + reader[1].ToString() + ")</div></div>";
                            if (reader[2].ToString() != "")
                            {
                                ReturnStr += "<div class='col-md-12 answer'><br />" + 
                                    reader[2].ToString().Replace("\r\n", "<br />")
                                       .Replace(Environment.NewLine, "<br />")
                                       .Replace("\n", "<br />") + 
                                    "<div class='time'>(回覆時間:" + reader[3].ToString() + ")</div></div>";
                            }
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return ReturnStr;
        }
    }
}