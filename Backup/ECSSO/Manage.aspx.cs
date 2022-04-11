﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using ECSSO.CRMebgWebService;


namespace ECSSO
{
    public partial class Manage : System.Web.UI.Page
    {
        //private String Pos = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            { 
                conn.Open();
                SqlCommand cmd = new SqlCommand("select img from cocker_ad",conn);
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
                finally {
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
                    String CrmVersion = "";
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
                        if (CrmVersion == "Y")
                        {
                            EbgCrmWebServiceApi crmservice = new EbgCrmWebServiceApi();
                            String id = this.UserID.Text + "@" + this.orgname.Text + ".hisales.hinet.net";
                            String pwd = this.UserPwd.Text;
                            Result result = crmservice.CrmAuthentication(id, pwd, this.orgname.Text);
                            if (result.success)
                            {
                                WebJobs(setting2, this.orgname.Text, this.UserID.Text, this.UserPwd.Text, CrmVersion);
                                Session["Orgname"] = this.orgname.Text;
                                Response.Redirect("system_login1.asp?Crm_version=" + CrmVersion + "&username=" + this.UserID.Text + "&orgName=" + this.orgname.Text);
                            }
                            else
                            {
                                this.Label6.Text = "登入失敗2";
                            }
                        }
                        else
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
                                        WebJobs(setting2, this.orgname.Text, this.UserID.Text, this.UserPwd.Text, CrmVersion);
                                        Session["Orgname"] = this.orgname.Text;
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
                                }
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

        private void WebJobs(String setting, String Orgname, String userid, String userpwd, String CrmVersion)
        {

            if (CrmVersion == "Y")
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
                                if (reader["job_id"].ToString()=="E009"){
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