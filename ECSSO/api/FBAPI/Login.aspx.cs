using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ECSSO.api.FBAPI
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Form["VerCode"] != null)
            {             
                Session["VerCode"] = Request.Form["VerCode"].ToString();                
                Session["OrgName"] = GetOrgName(Session["VerCode"].ToString());

                if (Session["Orgname"].ToString() != "" && Session["VerCode"].ToString() != "")
                {
                    String ClientID = "";
                    String RedirectUrL = "";
                    String URL = "";

                    using (SqlConnection conn = new SqlConnection(GetSetting(Session["OrgName"].ToString())))
                    {
                        conn.Open();
                        SqlCommand cmd;

                        cmd = new SqlCommand("select Game_AppID,Game_AppURL from head", conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    ClientID = reader[0].ToString();
                                    RedirectUrL = reader[1].ToString();
                                }
                            }
                        }
                        finally { reader.Close(); }
                    }
                    URL = "https://www.facebook.com/dialog/oauth?client_id=" + ClientID + "&scope=email,public_profile,user_friends,user_birthday,user_location&redirect_uri=" + RedirectUrL;

                    Response.Redirect(URL);
                }
            }        
        }
        #region 取得Orgname連結字串
        private String GetSetting(String OrgName)
        {
            return "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
        }
        #endregion    

        #region 由Vercode取得Orgname
        private String GetOrgName(String VerCode)
        {
            String OrgName = "";
            String Str_Sql = "select orgname from Device where stat='Y' and getdate() between start_date and end_date and VerCode=@VerCode";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand(Str_Sql, conn);
                cmd.Parameters.Add(new SqlParameter("@VerCode", VerCode));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            OrgName = reader[0].ToString();
                        }
                    }
                }
                finally { reader.Close(); }
            }
            return OrgName;
        }
        #endregion
    }
}