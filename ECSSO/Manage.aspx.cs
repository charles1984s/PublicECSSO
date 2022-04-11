using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using ECSSO.CRMebgWebService;
using ECSSO.CRMLoginWebService;
using SpeechLib;

namespace ECSSO
{
    public partial class Manage : System.Web.UI.Page
    {
        //private String Pos = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            Session.Abandon();
            if (ConfigurationManager.AppSettings["Server_Host"].ToString() != Request.Url.Host ||
                ConfigurationManager.AppSettings["Protocol"].ToString() != Request.Url.Scheme)
            {
                Response.Status = "301 Moved Permanently";
                Response.AddHeader("Location", ConfigurationManager.AppSettings["Protocol"].ToString() + "://" + ConfigurationManager.AppSettings["Server_Host"].ToString());
                Response.End();
            }
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select img from cocker_ad", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Image2.ImageUrl = reader[0].ToString();
                        }
                    }
                    else
                    {
                        Image2.ImageUrl = "img/ad_03.png";
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        #region 登入
        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            if (this.orgname.Text == "")
            {
                this.CHKorgname.Text = "請輸入網站代碼";
            }
            else
            {
                this.CHKorgname.Text = "";
            }

            if (this.UserID.Text == "")
            {
                this.CHKUserID.Text = "請輸入帳號";
            }
            else
            {
                this.CHKUserID.Text = "";
            }

            if (this.UserPwd.Text == "")
            {
                this.CHKUserPwd.Text = "請輸入密碼";
            }
            else
            {
                this.CHKUserPwd.Text = "";
            }
            if (this.CHKorgname.Text == "" && this.CHKUserID.Text == "" && this.CHKUserPwd.Text == "")
            {
                if (Session["CheckCode"] != null && String.Compare(Session["CheckCode"].ToString(), this.TextBox1.Text, true) == 0)
                {
                    this.Label6.Text = "";
                    String setting = ConfigurationManager.ConnectionStrings["sqlDB"].ToString();
                    String setting2 = "";
                    String CrmVersion = "";
                    //確認ORGNAME
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("select CONVERT(nvarchar(50), dbpassword) dbpasswordR,* from cocker_cust where comp_en_name = @orgname Collate Chinese_Taiwan_Stroke_CS_AI", conn);
                        cmd.Parameters.Add(new SqlParameter("@orgname", this.orgname.Text));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    setting2 = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=" + reader["dbusername"].ToString() + "; password=" + reader["dbpasswordR"].ToString() + "; database=" + reader["dbname"].ToString();
                                }
                            }
                            else
                            {
                                this.Label6.Text = "無此網站";
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }

                    if (this.Label6.Text == "")
                    {
                        using (SqlConnection conn = new SqlConnection(setting2))
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand("select crm_version from head", conn);
                            SqlDataReader reader = cmd.ExecuteReader();
                            try
                            {
                                while (reader.Read())
                                {
                                    CrmVersion = reader[0].ToString();
                                }
                            }
                            finally
                            {
                                reader.Close();
                            }
                        }
                        if (CrmVersion != "N")
                        {
                            Service crmservice = new Service();
                            //EbgCrmWebServiceApi crmservice = new EbgCrmWebServiceApi();
                            String id = this.UserID.Text + "@" + this.orgname.Text + ".hisales.hinet.net";
                            String pwd = this.UserPwd.Text;
                            //Result result = crmservice.CrmAuthentication(id, pwd, this.orgname.Text);
                            //if (result.success)
                            if (crmservice.IsAuthenticated(id, pwd))
                            {
                                WebJobs(setting2, this.orgname.Text, this.UserID.Text, this.UserPwd.Text, CrmVersion);
                                Session["Orgname"] = this.orgname.Text;
                                Session["LoginID"] = this.UserID.Text;
                                String[] returnurl = new string[] { "system_login1.asp?Crm_version=" + CrmVersion + "&username=" + this.UserID.Text + "&orgName=" + this.orgname.Text };
                                Response.Redirect(returnurl[0]);
                            }
                            else
                            {
                                this.Label6.Text = "登入失敗2";
                            }
                        }
                        else
                        {
                            GetStr GS = new GetStr();
                            using (SqlConnection conn = new SqlConnection(setting))
                            {
                                conn.Open();
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "sp_login";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = conn;
                                cmd.Parameters.Add(new SqlParameter("@orgname", this.orgname.Text));
                                cmd.Parameters.Add(new SqlParameter("@userid", this.UserID.Text));
                                cmd.Parameters.Add(new SqlParameter("@userpwd", this.UserPwd.Text));
                                cmd.Parameters.Add(new SqlParameter("@ip", GS.GetIPAddress()));
                                SqlParameter SPOutput = cmd.Parameters.Add("@ReturnCode", SqlDbType.NVarChar, 50);
                                SPOutput.Direction = ParameterDirection.Output;

                                string ReturnCode = null;
                                try
                                {
                                    cmd.ExecuteNonQuery();
                                    ReturnCode = SPOutput.Value.ToString();
                                    if (ReturnCode == "error:0")
                                    {
                                        this.Label6.Text = "帳號密碼錯誤超過三次，帳號已被鎖定，請等候15分鐘後再做嘗試。";
                                    } else if (ReturnCode == "error:1") {
                                        this.Label6.Text = "帳號密碼錯誤 ";
                                    }
                                    else
                                    {
                                        WebJobs(setting2, this.orgname.Text, this.UserID.Text, this.UserPwd.Text, CrmVersion);
                                        Session["Orgname"] = this.orgname.Text;
                                        Session["LoginID"] = this.UserID.Text;
                                        switch (this.orgname.Text) {
                                            case "360prod":
                                                Response.Redirect("http://360prod.ezsale.tw/check?talken=" + ReturnCode);
                                                break;
                                            case "e-catalog":
                                                Response.Redirect("http://e-catalog.ezsale.tw/check?talken=" + ReturnCode);
                                                break;
                                            default:
                                                Response.Redirect("/admin/system_login1.asp?talken=" + ReturnCode + "&username=" + this.UserID.Text + "&orgName=" + this.orgname.Text);
                                                break;
                                        }
                                    }
                                }
                                catch
                                {
                                    this.Label6.Text = "登入失敗3 ";
                                }

                                /*try
                                {                                    
                                    if (reader.HasRows)
                                    {
                                                                            
                                        Response.Redirect("system_login1.asp?Crm_version=" + CrmVersion + "&username=" + this.UserID.Text + "&orgName=" + this.orgname.Text);
                                    }
                                    else
                                    {
                                        this.Label6.Text = "登入失敗3 ";
                                    }
                                }
                                finally
                                {
                                    reader.Close();
                                }*/
                            }
                        }
                    }
                }
                else
                {
                    this.Label6.Text = "請輸入正確驗證碼";
                }
            }
        }
        #endregion
        private void WebJobs(String setting, String Orgname, String userid, String userpwd, String CrmVersion)
        {

            if (CrmVersion != "N")
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select * from IDManagement where orgname=@orgname and manager_id=@userid Collate Chinese_Taiwan_Stroke_CS_AI", conn);
                    cmd.Parameters.Add(new SqlParameter("@orgname", Orgname));
                    cmd.Parameters.Add(new SqlParameter("@userid", userid));
                    SqlDataReader reader1 = cmd.ExecuteReader();
                    try
                    {
                        if (reader1.HasRows)
                        {
                            using (SqlConnection conn1 = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
                            {
                                conn1.Open();
                                String sql = "update IDManagement set Manager_pwd=sys.fn_VarBinToHexStr(hashbytes('MD5', convert(nvarchar,@userpwd))) where orgname=@orgname and manager_id=@userid";
                                SqlCommand cmd2 = new SqlCommand(sql, conn1);
                                cmd2.Parameters.Add(new SqlParameter("@userpwd", userpwd));
                                cmd2.Parameters.Add(new SqlParameter("@userid", userid));
                                cmd2.Parameters.Add(new SqlParameter("@orgname", Orgname));
                                cmd2.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            using (SqlConnection conn1 = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
                            {
                                conn1.Open();
                                String sql = "INSERT INTO [IDManagement]([orgName],[Manager_ID],[Manager_PWD],[End_Date],[email]) ";
                                sql += " VALUES (@OrgName, @Manager_ID, sys.fn_VarBinToHexStr(hashbytes('MD5', convert(nvarchar,@Manager_PWD))), '2100-12-31', '')";
                                SqlCommand cmd2 = new SqlCommand(sql, conn1);
                                cmd2.Parameters.Add(new SqlParameter("@Manager_PWD", userpwd));
                                cmd2.Parameters.Add(new SqlParameter("@Manager_ID", userid));
                                cmd2.Parameters.Add(new SqlParameter("@OrgName", Orgname));
                                cmd2.ExecuteNonQuery();
                            }
                        }
                    }
                    finally
                    {
                        reader1.Close();
                    }
                }
            }
            bool hasjobs = false;
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from authors where empl_id=@userid", conn);
                cmd.Parameters.Add(new SqlParameter("@userid", userid));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        hasjobs = true;
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            if (!hasjobs)
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select * from webjobs", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        SqlCommand cmd2;
                        while (reader.Read())
                        {
                            if (reader["job_id"].ToString() == "P001" || reader["job_id"].ToString() == "Z999")
                            {
                                using (SqlConnection conn1 = new SqlConnection(setting))
                                {
                                    conn1.Open();
                                    cmd2 = new SqlCommand();
                                    cmd2.CommandText = "sp_AddAuthors";
                                    cmd2.CommandType = CommandType.StoredProcedure;
                                    cmd2.Connection = conn1;
                                    cmd2.Parameters.Add(new SqlParameter("@userid", userid));
                                    cmd2.Parameters.Add(new SqlParameter("@jobid", reader["job_id"].ToString()));
                                    cmd2.Parameters.Add(new SqlParameter("@canadd", "N"));
                                    cmd2.Parameters.Add(new SqlParameter("@canedit", "N"));
                                    cmd2.Parameters.Add(new SqlParameter("@candel", "N"));
                                    cmd2.Parameters.Add(new SqlParameter("@canqry", "N"));
                                    cmd2.Parameters.Add(new SqlParameter("@canexe", "N"));
                                    cmd2.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                if (reader["job_id"].ToString() == "E009")
                                {
                                    if (CrmVersion == "P")
                                    {
                                        using (SqlConnection conn1 = new SqlConnection(setting))
                                        {
                                            conn1.Open();
                                            cmd2 = new SqlCommand();
                                            cmd2.CommandText = "sp_AddAuthors";
                                            cmd2.CommandType = CommandType.StoredProcedure;
                                            cmd2.Connection = conn1;
                                            cmd2.Parameters.Add(new SqlParameter("@userid", userid));
                                            cmd2.Parameters.Add(new SqlParameter("@jobid", reader["job_id"].ToString()));
                                            cmd2.Parameters.Add(new SqlParameter("@canadd", reader["canadd"].ToString()));
                                            cmd2.Parameters.Add(new SqlParameter("@canedit", reader["canedit"].ToString()));
                                            cmd2.Parameters.Add(new SqlParameter("@candel", reader["candel"].ToString()));
                                            cmd2.Parameters.Add(new SqlParameter("@canqry", reader["canqry"].ToString()));
                                            cmd2.Parameters.Add(new SqlParameter("@canexe", reader["canexe"].ToString()));
                                            cmd2.ExecuteNonQuery();
                                        }
                                    }
                                    else
                                    {
                                        using (SqlConnection conn1 = new SqlConnection(setting))
                                        {
                                            conn1.Open();
                                            cmd2 = new SqlCommand();
                                            cmd2.CommandText = "sp_AddAuthors";
                                            cmd2.CommandType = CommandType.StoredProcedure;
                                            cmd2.Connection = conn1;
                                            cmd2.Parameters.Add(new SqlParameter("@userid", userid));
                                            cmd2.Parameters.Add(new SqlParameter("@jobid", reader["job_id"].ToString()));
                                            cmd2.Parameters.Add(new SqlParameter("@canadd", "N"));
                                            cmd2.Parameters.Add(new SqlParameter("@canedit", "N"));
                                            cmd2.Parameters.Add(new SqlParameter("@candel", "N"));
                                            cmd2.Parameters.Add(new SqlParameter("@canqry", "N"));
                                            cmd2.Parameters.Add(new SqlParameter("@canexe", "N"));
                                            cmd2.ExecuteNonQuery();
                                        }
                                    }
                                }
                                else
                                {
                                    using (SqlConnection conn1 = new SqlConnection(setting))
                                    {
                                        conn1.Open();
                                        cmd2 = new SqlCommand();
                                        cmd2.CommandText = "sp_AddAuthors";
                                        cmd2.CommandType = CommandType.StoredProcedure;
                                        cmd2.Connection = conn1;
                                        cmd2.Parameters.Add(new SqlParameter("@userid", userid));
                                        cmd2.Parameters.Add(new SqlParameter("@jobid", reader["job_id"].ToString()));
                                        cmd2.Parameters.Add(new SqlParameter("@canadd", reader["canadd"].ToString()));
                                        cmd2.Parameters.Add(new SqlParameter("@canedit", reader["canedit"].ToString()));
                                        cmd2.Parameters.Add(new SqlParameter("@candel", reader["candel"].ToString()));
                                        cmd2.Parameters.Add(new SqlParameter("@canqry", reader["canqry"].ToString()));
                                        cmd2.Parameters.Add(new SqlParameter("@canexe", reader["canexe"].ToString()));
                                        cmd2.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
        }
    }
}