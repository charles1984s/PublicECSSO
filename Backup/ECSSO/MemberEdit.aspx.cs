using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Globalization;

namespace ECSSO
{
    public partial class MemberEdit : System.Web.UI.Page
    {
        private string str_language = string.Empty;
        //語系變換
        protected override void InitializeCulture()
        {
            //此currentculture來自default.aspx頁面上兩個超連結的連結位址,見html部分            
            if (Request.QueryString["language"] != null)
            {
                str_language = Request.QueryString["language"].ToString();
            }
            else
            {
                if (Request.Form["language"] != null) {
                    str_language = Request.Form["language"].ToString();
                }
                
            }
            if (str_language == "") {
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
        protected void Page_Load(object sender, EventArgs e)
        {
            this.language.Value = str_language;
            if (!IsPostBack)
            {
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
                    this.returnurl.Value = Request.Form["ReturnUrl"].ToString();
                }
                else
                {
                    if (Request.QueryString["ReturnUrl"].ToString() != null)
                    {
                        this.returnurl.Value = Request.QueryString["ReturnUrl"].ToString();
                    }
                }

                if (Request.Form["MemID"] != null)
                {
                    this.MemberID.Text = Request.Form["MemID"].ToString();
                }
                else
                {
                    if (Request.QueryString["MemID"].ToString() != null)
                    {
                        this.MemberID.Text = Request.QueryString["Url"].ToString();
                    }                    
                }

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select dbname,dbusername,dbpassword,web_url from cocker_cust where id=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", this.siteid.Value));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {                                
                                if (this.returnurl.Value == "")
                                {
                                    this.returnurl.Value = "http://" + reader["web_url"].ToString();
                                }
                                setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=" + reader["dbusername"].ToString() + "; password=" + reader["dbpassword"].ToString() + "; database=" + reader["dbname"].ToString();
                            }
                        }
                        else
                        {
                            Response.Write("<script type='text/javascript'>history.go(-1);</script>");
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
                

                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select title from head", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            this.WebTitle.Text = reader[0].ToString();
                            Page.Title = reader[0].ToString();
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select * from cust where mem_id=@mem_id", conn);
                    cmd.Parameters.Add(new SqlParameter("@mem_id", this.MemberID.Text));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.FieldCount > 0)
                        {
                            while (reader.Read())
                            {
                                this.UserID.Value = reader["id"].ToString();
                                this.CHName.Text = reader["ch_name"].ToString();
                                this.Sex.SelectedIndex = Convert.ToInt16(reader["sex"].ToString()) - 1;
                                this.Tel.Text = reader["tel"].ToString();
                                this.Email.Text = reader["email"].ToString();
                                this.CellPhone.Text = reader["cell_phone"].ToString();
                                this.BirthDay.Text = reader["birth"].ToString();
                                this.Address.Text = reader["addr"].ToString();
                                this.bonusTotal.Text = reader["bonus_total"].ToString();
                                switch (reader["vip"].ToString()) { 
                                    case "1":
                                        this.VIP.Text = "一般會員";
                                        break;
                                    case "2":
                                        this.VIP.Text = "VIP會員";
                                        break;
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
            else
            {
                Page.Title = this.WebTitle.Text;
            }
        }
        
        public Boolean isright(string s, String right) //定義正則表達式函數
        {
            Regex Regex1 = new Regex(right, RegexOptions.IgnoreCase);
            return Regex1.IsMatch(s);
        }
        
        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            if (this.CHName.Text == "")
            {
                this.CheckName.Text = "請輸入您的姓名";
            }
            else
            {
                this.CheckName.Text = "";
            }
            String RegStr = @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$";
            if (!isright(this.Email.Text, RegStr))
            {
                this.CheckEmail.Text = "Email格式錯誤";
            }
            else
            {
                this.CheckEmail.Text = "";
            }
            DateTime dt = Convert.ToDateTime("1911-01-01");
            RegStr = @"\d{4}\-\d{2}\-\d{2}";
            if (!DateTime.TryParse(this.BirthDay.Text, out dt) || !isright(this.BirthDay.Text, RegStr))
            {
                this.CheckBirthDay.Text = "請輸入正確日期(格式:yyyy-MM-dd)";
            }
            else
            {
                this.CheckBirthDay.Text = "";
            }
            RegStr = @"\d{2,3}\-\d{6,8}";
            if (!isright(this.Tel.Text, RegStr))
            {
                this.CheckTel.Text = "聯絡電話格式錯誤(格式:xx-xxxxxxx)";
            }
            else
            {
                this.CheckTel.Text = "";
            }
            RegStr = @"09\d{2}\-\d{6}";
            if (!isright(this.CellPhone.Text, RegStr))
            {
                this.CheckCellPhone.Text = "行動電話格式錯誤(格式:09xx-xxxxxx)";
            }
            else
            {
                this.CheckCellPhone.Text = "";
            }
            if (this.Address.Text == "")
            {
                this.CheckAddress.Text = "請輸入地址";
            }
            else
            {
                this.CheckAddress.Text = "";
            }

            if (CheckBox1.Checked)
            {
                RegStr = @"^[0-9a-zA-Z_]{4,10}$";
                if (!isright(this.NewPwd.Text, RegStr))
                {
                    Label1.Text = "密碼長度請輸入4至10個英文或數字";
                }
                else
                {
                    if (this.NewPwd.Text != this.ChkNewPwd.Text)
                    {
                        Label1.Text = "新密碼與確認新密碼不同";
                    }
                    else
                    {
                        Label1.Text = "";
                    }
                }
            }

            if (this.CheckName.Text == "" && this.CheckBirthDay.Text == "" && this.CheckEmail.Text == "" && this.CheckTel.Text == "" && this.CheckCellPhone.Text == "" && this.CheckAddress.Text == "" && this.Label1.Text == "")
            {
                String str_ChName = this.CHName.Text;
                String str_SEX = this.Sex.SelectedItem.Value;
                String str_Birth = this.BirthDay.Text;
                String str_Email = this.Email.Text;
                String str_Tel = this.Tel.Text;
                String str_CellPhone = this.CellPhone.Text;
                String str_Addr = this.Address.Text;
                GetStr getstr = new GetStr();
                String setting = getstr.GetSetting(this.siteid.Value);

                if (CheckBox1.Checked)
                {
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandText = "sp_ResetPassword";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn;
                        cmd.Parameters.Add(new SqlParameter("@id", this.UserID.Value));
                        cmd.Parameters.Add(new SqlParameter("@pwd", this.NewPwd.Text));
                        cmd.ExecuteNonQuery();
                        Label1.Text = "";
                    }
                }
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "sp_EditMember2";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@ch_name", str_ChName));
                    cmd.Parameters.Add(new SqlParameter("@sex", str_SEX));
                    cmd.Parameters.Add(new SqlParameter("@ident", ""));
                    cmd.Parameters.Add(new SqlParameter("@birth", Convert.ToDateTime(str_Birth).ToString("yyyy-MM-dd")));
                    cmd.Parameters.Add(new SqlParameter("@tel", str_Tel));
                    cmd.Parameters.Add(new SqlParameter("@cell_phone", str_CellPhone));
                    cmd.Parameters.Add(new SqlParameter("@email", str_Email));
                    cmd.Parameters.Add(new SqlParameter("@addr", str_Addr));
                    cmd.Parameters.Add(new SqlParameter("@id", this.MemberID.Text));
                    cmd.ExecuteNonQuery();
                }
                Response.Write("<script type='text/javascript'>alert('修改成功');</script>");
            }
        }

        protected void LinkButton2_Click(object sender, EventArgs e)
        {
            Response.Redirect(this.returnurl.Value);
        }

        protected void LinkButton3_Click(object sender, EventArgs e)
        {
            Session.Clear();

            String StrUrl = this.returnurl.Value; 
            string[] strs = StrUrl.Split(new string[] { "/tw/" }, StringSplitOptions.RemoveEmptyEntries);            
            Response.Redirect(strs[0] + "/tw/logout.asp");            
        }
    }
}