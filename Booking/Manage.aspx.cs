using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Booking
{
    public partial class Manage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session.Abandon();
            //Response.Write("LOGON_USER=" + System.Security.Principal.WindowsIdentity.GetCurrent().Name);
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
                    
                    //確認ORGNAME
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("select * from cocker_cust where comp_en_name = @orgname Collate Chinese_Taiwan_Stroke_CS_AI", conn);
                        cmd.Parameters.Add(new SqlParameter("@orgname", this.orgname.Text));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    setting2 = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=" + reader["dbusername"].ToString() + "; password=" + reader["dbpassword"].ToString() + "; database=" + reader["dbname"].ToString();
                                }
                            }
                            else
                            {
                                this.Label6.Text = "登入失敗1";
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }

                    if (this.Label6.Text == "")
                    {
                        using (SqlConnection conn = new SqlConnection(setting))
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand("select * from IDManagement where orgname=@orgname and manager_id=@userid and Manager_PWD = sys.fn_VarBinToHexStr(hashbytes('MD5', convert(nvarchar,@userpwd))) and End_Date >='" + DateTime.Now.ToString("yyyy-MM-dd") + "' Collate Chinese_Taiwan_Stroke_CS_AI ", conn);
                            cmd.Parameters.Add(new SqlParameter("@orgname", this.orgname.Text));
                            cmd.Parameters.Add(new SqlParameter("@userid", this.UserID.Text));
                            cmd.Parameters.Add(new SqlParameter("@userpwd", this.UserPwd.Text));

                            SqlDataReader reader = cmd.ExecuteReader();
                            try
                            {
                                if (reader.HasRows)
                                {
                                    WebJobs(setting2, this.orgname.Text, this.UserID.Text, this.UserPwd.Text);
                                    Session["Orgname"] = this.orgname.Text;
                                    Session["LoginID"] = this.UserID.Text;
                                    Response.Redirect("system_login1.asp?username=" + this.UserID.Text + "&orgName=" + this.orgname.Text);
                                }
                                else
                                {
                                    this.Label6.Text = "登入失敗3 ";
                                }
                            }
                            finally
                            {
                                reader.Close();
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

        private void WebJobs(String setting, String Orgname, String userid, String userpwd)
        {            
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
                    SqlCommand cmd = new SqlCommand("select * from webjobs where type='booking'", conn);
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