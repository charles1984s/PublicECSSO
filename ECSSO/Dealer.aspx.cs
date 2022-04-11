using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Net.Mail;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Globalization;
using Microsoft.Security.Application;

namespace ECSSO
{
    public partial class Dealer : System.Web.UI.Page
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
        protected void Page_Init(object sender, EventArgs e)
        {
            HtmlGenericControl objLink = new HtmlGenericControl("link");
            objLink.Attributes.Add("rel", "stylesheet");
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/Dealer.css");
            objLink.Attributes.Add("type", "text/css");
            this.Page.Header.Controls.Add(objLink);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            this.language.Value = str_language;
            if (!IsPostBack)
            {               
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
                else
                {
                    if (Request.QueryString["ReturnUrl"] != null)
                    {
                        this.returnurl.Value = Server.UrlDecode(Request.QueryString["ReturnUrl"].ToString());
                    }
                }

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
                                this.returnurl.Value = "http://" + reader["web_url"].ToString() + "?" + this.returnurl.Value.Split('?')[1];
                            }
                        }
                    }
                    catch
                    {

                    }
                    finally { reader.Close(); }
                }

                if (Request.Form["MemID"] != null)
                {
                    this.mem_id.Value = Request.Form["MemID"].ToString();                    
                }
                else
                {
                    if (Request.QueryString["MemID"] != null)
                    {
                        this.mem_id.Value = Request.QueryString["MemID"].ToString();
                    }
                }

                String CheckM = "";
                if (Request.Form["CheckM"] != null)
                {
                    CheckM = Encoder.HtmlEncode(Request.Form["CheckM"].ToString());
                }
                else
                {
                    if (Request.QueryString["CheckM"] != null)
                    {
                        CheckM = Encoder.HtmlEncode(Request.QueryString["CheckM"].ToString());
                    }
                }

                GetStr getstr = new GetStr();
                if (getstr.MD5Check(this.siteid.Value + this.mem_id.Value, CheckM))
                {
                    String setting = getstr.GetSetting(this.siteid.Value);
                    SqlDataSource1.ConnectionString = setting;

                    LinkButton1.Attributes.Add("OnClick", "return confirm('"+GetLocalResourceObject("StringResource1")+"');");
                }                
            }
       }
        
        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            GetStr getstr = new GetStr();
            String setting = getstr.GetSetting(this.siteid.Value);            

            if (ChkNum())   //判斷是否有訂購產品
            {

                #region 將選購產品暫存到List
                List<Shoppingcar.OrderList> List1 = new List<Shoppingcar.OrderList>();
                List<Shoppingcar.OrderItem> List2 = new List<Shoppingcar.OrderItem>();
                List<Shoppingcar.OrderSpec> List3;

                String ProdiD = "";
                String Posno = "";
                String ProdTitle = "";
                int Colorid = 0;
                int Sizeid = 0;
                int Qty = 0;
                String Str_Msg = "";

                for (int i = 0; i < GridView1.Rows.Count; i++)
                {
                    Qty = int.Parse(((TextBox)this.GridView1.Rows[i].Cells[6].FindControl("QTY")).Text);
                    if (Qty != 0 && Convert.ToInt32(Qty) > 0)
                    {
                        if (!CheckStock(setting, ((HiddenField)this.GridView1.Rows[i].Cells[0].FindControl("ProdID")).Value, int.Parse(((HiddenField)this.GridView1.Rows[i].Cells[4].FindControl("SizeID")).Value), int.Parse(((HiddenField)this.GridView1.Rows[i].Cells[4].FindControl("ColorID")).Value), Qty))
                        {
                            Str_Msg +=　"【" + ((HiddenField)this.GridView1.Rows[i].Cells[3].FindControl("Title")).Value + "】";
                        }
                        else {
                            List3 = new List<Shoppingcar.OrderSpec>();
                            List3.Add(new Shoppingcar.OrderSpec()
                            {
                                Size = int.Parse(((HiddenField)this.GridView1.Rows[i].Cells[4].FindControl("SizeID")).Value),
                                Color = int.Parse(((HiddenField)this.GridView1.Rows[i].Cells[4].FindControl("ColorID")).Value),
                                Qty = Qty,
                                Price = 0,
                                FinalPrice = 0
                            });
                            List2.Add(new Shoppingcar.OrderItem()
                            {
                                ID = ((HiddenField)this.GridView1.Rows[i].Cells[0].FindControl("ProdID")).Value,
                                Name = ((HiddenField)this.GridView1.Rows[i].Cells[3].FindControl("Title")).Value,
                                PosNo = ((HiddenField)this.GridView1.Rows[i].Cells[0].FindControl("PosNo")).Value,
                                Virtual = "N",
                                UseTime = "0",
                                OrderSpecs = List3
                            });
                        }                        
                    }
                }
                #endregion
                if (Str_Msg == "")
                {
                    if (List2.Count > 0)
                    {
                        SqlCommand cmd;
                        #region 取得訂單ID
                        String OrderID = "";
                        using (SqlConnection conn = new SqlConnection(setting))
                        {
                            conn.Open();

                            cmd = new SqlCommand("select isnull(max(id),'') from orders_hd", conn);
                            SqlDataReader reader = cmd.ExecuteReader();
                            try
                            {
                                while (reader.Read())
                                {
                                    if (reader[0].ToString() == "")
                                    {
                                        OrderID = "000000001";
                                    }
                                    else
                                    {
                                        OrderID = (Convert.ToInt32(reader[0]) + 1).ToString().PadLeft(9, '0');
                                    }
                                }
                            }
                            finally
                            {
                                reader.Close();
                            }
                        }
                        #endregion

                        if (OrderID != "")
                        {
                            #region 取得會員資料
                            String str_ChName = "";
                            String str_SEX = "";
                            String str_Email = "";
                            String str_Tel = "";
                            String str_CellPhone = "";
                            String str_Addr = "";
                            String Cust_id = "";
                            using (SqlConnection conn = new SqlConnection(setting))
                            {
                                //20140331有更新此預存程序!!!!(新增郵遞區號,縣市,鄉鎮區)
                                conn.Open();

                                cmd = new SqlCommand("select * from cust where mem_id=@mem_id", conn);
                                cmd.Parameters.Add(new SqlParameter("@mem_id", this.mem_id.Value));
                                SqlDataReader reader = cmd.ExecuteReader();
                                try
                                {
                                    while (reader.Read())
                                    {
                                        str_ChName = reader["ch_name"].ToString();
                                        str_SEX = reader["sex"].ToString();
                                        str_Email = reader["email"].ToString();
                                        str_Tel = reader["tel"].ToString();
                                        str_CellPhone = reader["cell_phone"].ToString();
                                        str_Addr = reader["addr"].ToString();
                                        Cust_id = reader["id"].ToString();
                                    }
                                }
                                finally
                                {
                                    conn.Close();
                                    reader.Close();
                                }
                            }
                            #endregion

                            #region save訂單表頭
                            using (SqlConnection conn = new SqlConnection(setting))
                            {
                                //20140331有更新此預存程序!!!!(新增郵遞區號,縣市,鄉鎮區)
                                conn.Open();

                                cmd = new SqlCommand();
                                cmd.CommandText = "sp_orderhd";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = conn;
                                cmd.Parameters.Add(new SqlParameter("@id", OrderID));
                                cmd.Parameters.Add(new SqlParameter("@name", str_ChName));
                                cmd.Parameters.Add(new SqlParameter("@sex", Convert.ToInt32(str_SEX)));
                                cmd.Parameters.Add(new SqlParameter("@tel", str_Tel));
                                cmd.Parameters.Add(new SqlParameter("@cell", str_CellPhone));
                                cmd.Parameters.Add(new SqlParameter("@addr", str_Addr));
                                cmd.Parameters.Add(new SqlParameter("@mail", str_Email));
                                cmd.Parameters.Add(new SqlParameter("@notememo", ""));
                                cmd.Parameters.Add(new SqlParameter("@mem_id", this.mem_id.Value));
                                cmd.Parameters.Add(new SqlParameter("@item2", ""));
                                cmd.Parameters.Add(new SqlParameter("@item3", ""));
                                cmd.Parameters.Add(new SqlParameter("@item4", ""));
                                cmd.Parameters.Add(new SqlParameter("@payment_type", ""));
                                cmd.Parameters.Add(new SqlParameter("@o_name", str_ChName));
                                cmd.Parameters.Add(new SqlParameter("@o_tel", str_Tel));
                                cmd.Parameters.Add(new SqlParameter("@o_cell", str_CellPhone));
                                cmd.Parameters.Add(new SqlParameter("@o_addr", ""));
                                cmd.Parameters.Add(new SqlParameter("@bonus_amt", "0"));
                                cmd.Parameters.Add(new SqlParameter("@bonus_discount", "0"));
                                cmd.Parameters.Add(new SqlParameter("@freightamount", "0"));
                                cmd.Parameters.Add(new SqlParameter("@c_no", GetPOSID(setting, this.mem_id.Value.ToString())));
                                cmd.Parameters.Add(new SqlParameter("@ship_city", ""));
                                cmd.Parameters.Add(new SqlParameter("@ship_zip", ""));
                                cmd.Parameters.Add(new SqlParameter("@ship_countryname", ""));
                                cmd.Parameters.Add(new SqlParameter("@discount_amt", "0"));
                                cmd.Parameters.Add(new SqlParameter("@RID", ""));
                                cmd.Parameters.Add(new SqlParameter("@Click_ID", ""));
                                cmd.Parameters.Add(new SqlParameter("@prod_bonus", "0"));
                                cmd.ExecuteNonQuery();
                            }
                            #endregion

                            #region 訂單表身
                            int orderitem = 1;
                            foreach (Shoppingcar.OrderItem Items in List2)
                            {
                                ProdiD = Items.ID;
                                Posno = Items.PosNo;
                                ProdTitle = Items.Name;

                                foreach (Shoppingcar.OrderSpec OrderSpecs in Items.OrderSpecs)
                                {
                                    Colorid = OrderSpecs.Color;
                                    Sizeid = OrderSpecs.Size;
                                    Qty = OrderSpecs.Qty;

                                    if (!CheckStock(setting, ProdiD, Sizeid, Colorid, Convert.ToInt32(Qty)))
                                    {
                                        //庫存不足
                                    }
                                    else
                                    {
                                        #region 新增表身

                                        using (SqlConnection conn = new SqlConnection(setting))
                                        {
                                            conn.Open();
                                            //新增表身
                                            cmd = new SqlCommand();
                                            cmd.CommandText = "sp_order";
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.Connection = conn;
                                            cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                                            cmd.Parameters.Add(new SqlParameter("@ser_no", orderitem.ToString().PadLeft(3, '0')));
                                            cmd.Parameters.Add(new SqlParameter("@prod_name", ProdTitle));
                                            cmd.Parameters.Add(new SqlParameter("@price", "0"));
                                            cmd.Parameters.Add(new SqlParameter("@qty", Qty));
                                            cmd.Parameters.Add(new SqlParameter("@amt", "0"));
                                            cmd.Parameters.Add(new SqlParameter("@productid", ProdiD));
                                            cmd.Parameters.Add(new SqlParameter("@colorid", Colorid));
                                            cmd.Parameters.Add(new SqlParameter("@sizeid", Sizeid));
                                            cmd.Parameters.Add(new SqlParameter("@posno", ""));
                                            cmd.Parameters.Add(new SqlParameter("@memo", ""));
                                            cmd.Parameters.Add(new SqlParameter("@virtual", "N"));
                                            cmd.Parameters.Add(new SqlParameter("@usetime", ""));
                                            cmd.Parameters.Add(new SqlParameter("@usedate", ""));
                                            cmd.Parameters.Add(new SqlParameter("@discount", "0"));
                                            cmd.Parameters.Add(new SqlParameter("@discription", ""));
                                            cmd.Parameters.Add(new SqlParameter("@bonus", "0"));
                                            cmd.ExecuteNonQuery();

                                            //庫存更新
                                            cmd = new SqlCommand();
                                            cmd.CommandText = "sp_stocks";
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.Connection = conn;
                                            cmd.Parameters.Add(new SqlParameter("@prod_id", Convert.ToInt32(ProdiD)));
                                            cmd.Parameters.Add(new SqlParameter("@qty", Convert.ToInt32(Qty)));
                                            cmd.Parameters.Add(new SqlParameter("@prod_color", ProdiD));
                                            cmd.Parameters.Add(new SqlParameter("@prod_size", Sizeid));
                                            cmd.ExecuteNonQuery();
                                        }
                                        orderitem = orderitem + 1;
                                        #endregion
                                    }
                                }
                            }
                            #endregion

                            #region 儲存總金額
                            using (SqlConnection conn = new SqlConnection(setting))
                            {
                                conn.Open();

                                cmd = new SqlCommand();
                                cmd.CommandText = "sp_order_freight";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = conn;
                                cmd.Parameters.Add(new SqlParameter("@id", OrderID));
                                cmd.Parameters.Add(new SqlParameter("@amt", "0"));
                                cmd.ExecuteNonQuery();
                            }
                            #endregion

                            #region 寄通知信
                            String service_mail = "";
                            String mail_title = "";
                            using (SqlConnection conn = new SqlConnection(setting))
                            {
                                conn.Open();
                                cmd = new SqlCommand("select service_mail,title from head", conn);
                                SqlDataReader reader = cmd.ExecuteReader();
                                try
                                {
                                    while (reader.Read())
                                    {
                                        service_mail = reader[0].ToString();
                                        if (service_mail == "")
                                        {
                                            service_mail = "service@ether.com.tw";
                                        }
                                        mail_title = reader[1].ToString();
                                    }
                                }
                                finally
                                {
                                    reader.Close();
                                }
                            }

                            String mail_cont = "<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />";
                            mail_cont += "<div style='font-family:微軟正黑體, Arial, 新細明體, Helvetica, sans-serif'>";
                            mail_cont += "<font size='4' color='#ff0000'><b>" + GetLocalResourceObject("StringResource2") + "</b></font><br>";
                            mail_cont += "<b>" + GetLocalResourceObject("StringResource3") + "</b>";
                            mail_cont += "<hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px; margin-top:8px; margin-bottom:20px;'>";

                            mail_cont += "<p style='color:#e30000; font-weight:bold; max-width:600px; margin:5px 10px;'>" + GetLocalResourceObject("StringResource4") + "：<span id='order_id' style='font-weight:normal; color:#000;'>" + OrderID + "</span></p>";

                            mail_cont += "<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0' style='border:none;border-width: 0px;margin-left:10px;margin-right:10px; width:600px; font-size:9pt; font-family:'microsoft jhenghei', sans-serif;'>";
                            mail_cont += "  <tr>";
                            mail_cont += "    <th colspan='4' align='left' valign='middle' scope='col' style='background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;border-right-width:0px;'>" + GetLocalResourceObject("StringResource5") + "：" + getstr.Rename(str_ChName, 1) + "(" + Cust_id + ")</th>";
                            mail_cont += "  </tr>";
                            mail_cont += "  <tr>";
                            mail_cont += "    <td align='right' valign='middle' style='width:15%;background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GetLocalResourceObject("StringResource6") + "</td>";
                            mail_cont += "    <td align='left' valign='middle' style='width:35%;background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'><span style='color:#333;'>" + getstr.Rename(str_CellPhone, 4) + "</span></td>";
                            mail_cont += "    <td align='right' valign='middle' style='width:15%;background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GetLocalResourceObject("StringResource7") + "</td>";
                            mail_cont += "    <td align='left' valign='middle' style='width:35%;background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'><span style='color:#333;'>" + getstr.Rename(str_Tel, 4) + "</span></td>";
                            mail_cont += "  </tr>";
                            mail_cont += "  <tr>";
                            mail_cont += "    <td align='right' valign='middle' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GetLocalResourceObject("StringResource8") + "</td>";
                            mail_cont += "    <td colspan='3' align='left' valign='middle' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'>" + getstr.Rename(str_Email, 5) + "</td>";
                            mail_cont += "  </tr>";
                            mail_cont += "</table>";
                            mail_cont += "<br>";

                            mail_cont += "<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0' style='border:none;border-width: 0px;margin-left:10px;margin-right:10px; width:600px; font-size:9pt; font-family:'microsoft jhenghei', sans-serif;'>";
                            mail_cont += "  <tr>";
                            mail_cont += "    <th align='left' scope='col' style='width:30%;background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;'>" + GetLocalResourceObject("StringResource9") + "</th>";
                            mail_cont += "    <th align='center' scope='col' style='width:10%;background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;'>" + GetLocalResourceObject("StringResource10") + "</th>";
                            mail_cont += "    <th align='center' scope='col' style='width:10%;background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;'>" + getstr.GetSpecTitle(setting, "1") + "</th>";
                            mail_cont += "    <th align='center' scope='col' style='width:10%;background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;'>" + getstr.GetSpecTitle(setting, "2") + "</th>";
                            mail_cont += "    <th align='center' scope='col' style='width:10%;background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;'>" + GetLocalResourceObject("StringResource11") + "</th>";
                            mail_cont += "  </tr>";

                            for (int i = 0; i < List2.Count; i++)
                            {
                                ProdiD = List2[i].ID;
                                Posno = List2[i].PosNo;
                                ProdTitle = List2[i].Name;

                                for (int j = 0; j < List2[i].OrderSpecs.Count; j++)
                                {
                                    Colorid = List2[i].OrderSpecs[j].Color;
                                    Sizeid = List2[i].OrderSpecs[j].Size;
                                    Qty = List2[i].OrderSpecs[j].Qty;
                                    mail_cont += "  <tr>";
                                    mail_cont += "    <td align='left' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + ProdTitle + "</td>";
                                    mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + Posno + "</td>";
                                    mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + getstr.GetSpec(setting, "prod_color", Colorid) + "</td>";
                                    mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + getstr.GetSpec(setting, "prod_size", Sizeid) + "</td>";
                                    mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + Qty + "</td>";
                                    mail_cont += "  </tr>";
                                }
                            }
                            mail_cont += "</table>";
                            mail_cont += "<br>";
                            mail_cont += "<hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px;'>";
                            mail_cont += "<span style='color:#ff0000;'>" + GetLocalResourceObject("StringResource12") + "</span><br>";
                            mail_cont += "<span style='color:#ff0000;'>" + GetLocalResourceObject("StringResource13") + "</span>";
                            mail_cont += "<hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px;'>";

                            send_email(mail_cont, GetLocalResourceObject("StringResource14") + " 【" + mail_title + "】", service_mail, str_Email);//呼叫send_email函式測試    
                            #endregion

                            Response.Write("<script type='text/javascript'>alert('" + GetLocalResourceObject("StringResource15") + "');</script>");
                            SqlDataSource1.ConnectionString = setting;
                            this.GridView1.DataBind();
                        }
                    }
                }
                else {
                    Response.Write("<script type='text/javascript'>alert('" + Str_Msg  + GetLocalResourceObject("StringResource16") + "');</script>");
                }
            }
            else {
                Response.Write("<script type='text/javascript'>alert('" + GetLocalResourceObject("StringResource17") + "');</script>");                
            }
        }

        protected void LinkButton2_Click(object sender, EventArgs e)
        {
            Response.Write("<script language='javascript'>top.location.href='" + this.returnurl.Value + "';</script>");
            //Response.Redirect(this.returnurl.Value);            
        }

        protected void LinkButton3_Click(object sender, EventArgs e)
        {
            Session.Clear();
            GetStr GS = new GetStr();

            String StrUrl = this.returnurl.Value;
            string[] strs = StrUrl.Split(new string[] { "/" + GS.GetLanString(str_language) + "/" }, StringSplitOptions.RemoveEmptyEntries);
            Response.Write("<script language='javascript'>top.location.href='" + strs[0] + "/" + GS.GetLanString(str_language) + "/logout.asp';</script>");
            //Response.Redirect(strs[0] + "/tw/logout.asp");
        }

        #region 發送email
        public void send_email(string msg, string mysubject, string sender, string mail)
        {
            MailMessage message = new MailMessage();//MailMessage(寄信者, 收信者)
            message.From = new MailAddress(sender, GetLocalResourceObject("StringResource18").ToString());
            message.Bcc.Add(sender);
            message.To.Add(mail);

            message.IsBodyHtml = true;
            message.BodyEncoding = System.Text.Encoding.UTF8;//E-mail編碼
            message.Subject = mysubject;//E-mail主旨
            message.Body = msg;//E-mail內容

            //SmtpClient smtpClient = new SmtpClient("msa.hinet.net");//設定E-mail Server和port
            SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings.Get("smtpServer"), Int32.Parse(ConfigurationManager.AppSettings.Get("smtpPort")));//設定E-mail Server和port
            if (ConfigurationManager.AppSettings.Get("CredentialUser") != "")
            {
                smtpClient.Credentials = new System.Net.NetworkCredential(
                        ConfigurationManager.AppSettings.Get("CredentialUser"),
                        ConfigurationManager.AppSettings.Get("CredentialPW")
                );
            }
            try
            {
                smtpClient.Send(message);
            }
            catch
            {
                smtpClient.Send(message);
            }

        }
        #endregion        

        #region 確認庫存
        private bool CheckStock(String setting, String ProdID, int ProdSize, int ProdColor, int Qty)
        {
            String Str_sql = "select isnull(stock,0) as stock from prod_stock where prod_id=@prod_id and colorid = @colorid and sizeid=@sizeid";
            int Stock = 0;
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                cmd.Parameters.Add(new SqlParameter("@prod_id", ProdID));
                cmd.Parameters.Add(new SqlParameter("@colorid", ProdColor));
                cmd.Parameters.Add(new SqlParameter("@sizeid", ProdSize));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        Stock = Convert.ToInt32(reader[0]) - Qty;
                    }
                }
                finally
                {
                    conn.Close();
                    reader.Close();
                }
            }

            if (Stock >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion        

        #region 抓會員POS ID

        private String GetPOSID(String setting, String MemberID)
        {
            String POSID = "";
            if (MemberID != "" && MemberID != null)
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand("select isnull(C_NO,'') as C_NO from cust where mem_id=@mem_id", conn);
                    cmd.Parameters.Add(new SqlParameter("@mem_id", MemberID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            if (reader[0].ToString() == "")
                            {
                                POSID = "";
                            }
                            else
                            {
                                POSID = reader[0].ToString();
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            return POSID;
        }

        #endregion

        #region 檢查是否有選購商品
        public bool ChkNum() {
            Int32 Qty = 0;

            for (int i = 0; i < GridView1.Rows.Count; i++)
            {
                try{
                    Qty += Convert.ToInt32(((TextBox)this.GridView1.Rows[i].Cells[3].FindControl("QTY")).Text);
                }
                catch { }                
            }

            if (Qty > 0)
            {
                return true;
            }
            else {
                return false;
            }
        }
        #endregion        

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow) {
                Image Img1 = (Image)e.Row.FindControl("Image1");
                if (Img1.ImageUrl == "")
                {
                    Img1.Visible = false;
                }
                else {
                    Img1.Visible = true;
                }
            }
        }
    }
}