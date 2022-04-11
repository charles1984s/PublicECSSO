using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Security.Application;

namespace ECSSO.Customer
{
    public partial class BankStatement : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            #region 檢查必要參數
            String setting = "";

            if (Request.Form["SiteID"] != null)
            {
                this.siteid.Value = Request.Form["SiteID"].ToString();
            }
            else
            {
                if (Request.QueryString["SiteID"] != null)
                {
                    this.siteid.Value = Request.QueryString["SiteID"].ToString();
                }
            }

            if (Request.Form["ReturnUrl"] != null)
            {
                this.returnurl.Value = Server.UrlDecode(Request.Form["ReturnUrl"].ToString());
            }
            else if (Request.QueryString["ReturnUrl"] != null)
            {
                this.returnurl.Value = Server.UrlDecode(Request.QueryString["ReturnUrl"].ToString());
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
                {
                    conn.Open();
                    SqlCommand cmd;
                    cmd = new SqlCommand("select web_url from cocker_cust where id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", this.siteid.Value));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                this.returnurl.Value = "https://" + reader["web_url"].ToString() + "?" + HttpContext.Current.Server.UrlDecode(Request.QueryString["ReturnUrl"].ToString()).Split('?')[1];
                            }
                        }
                    }
                    catch
                    {

                    }
                    finally { reader.Close(); }
                }
            }

            if (Request.Form["MemID"] != null)
            {
                this.MemID.Value = Request.Form["MemID"].ToString();
            }
            else
            {
                if (Request.QueryString["MemID"] != null)
                {
                    MemID.Value= Request.QueryString["MemID"].ToString();
                }
            }
            #endregion
        }
    }
}