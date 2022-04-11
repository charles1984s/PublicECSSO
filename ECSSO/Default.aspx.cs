using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Net.Mail;


namespace ECSSO
{
    public partial class _Default : System.Web.UI.Page
    {
        public class Order
        {
            public String WebTitle;                                          //網站名稱
            public String OrgName;                                          //Orgname
            public String ReturnUrl;                                        //結帳後回到原站網址
            public string ErrorUrl;                                         //訂購失敗原站網址
            public String PayType;                                          //付款方式
                                                                            //預設值(空值):ATM轉帳
                                                                            //WebATM:歐付寶-WebATM
                                                                            //Credit:歐付寶-線上刷卡
                                                                            //CVS:歐付寶-超商繳費
                                                                            //Tenpay:歐付寶-財付通
                                                                            //Alipay:歐付寶-支付寶
                                                                            //BARCODE:歐付寶-超商條碼
                                                                            //ATM:歐付寶-虛擬帳號	
                                                                            //getandpay:貨到付款
                                                                            //ezShip:超商取貨付款		
                                                                            //esafeWebatm:紅陽-WebATM			
                                                                            //esafeCredit:紅陽-信用卡			
                                                                            //esafePay24:紅陽-超商代收			
                                                                            //esafePaycode:紅陽-超商代碼付款			
                                                                            //esafeAlipay:紅陽-支付寶		
                                                                            //chtHinet:中華支付-Hinet帳單								
                                                                            //chteCard:中華支付-Hinet點數卡
                                                                            //chtld:中華支付-行動839								
                                                                            //Chtn:中華支付-市話輕鬆付								
                                                                            //chtCredit:中華支付-信用卡								
                                                                            //chtATM:中華支付-虛擬帳號付款								
                                                                            //chtWEBATM:中華支付-WebATM								
                                                                            //chtUniPresident:中華支付-超商代收								
                                                                            //chtAlipay:中華支付-支付寶								
                                                                            	
            public String FreightAmount;                                    //運費
            public String BonusDiscount;                                    //紅利折扣
            public String BonusAmt;                                         //本次訂單獲得紅利
            public String MemID;                                            //會員編號
            public List<OrderItem> OrderItem = new List<OrderItem>();       //訂單產品
        }

        public class OrderItem
        {
            public String ProductID;                                        //產品編號
            public String ProductSize;                                      //尺寸
            public String ProductColor;                                     //顏色
            public String ProductName;                                      //名稱
            public String ProductQty;                                       //數量
            public String ProductPrice;                                     //價格
            public String ProductPosNo;                                     //pos編號
        }

        protected void Page_Load(object sender, EventArgs e)
        {            
            if (!IsPostBack) {
                if (Request.Form["orderData"] == null)
                {
                    Response.Write("Error");
                    Response.End();
                }
                else {
                    if (Request.Form["orderData"].ToString() == "") {
                        Response.Write("Error");
                        Response.End();
                    }
                }
                string output = "";

                // Json Encoding            
                //output = JsonConvert.SerializeObject(order);
                output = Request.Form["orderData"].ToString();
                Order rlib = JsonConvert.DeserializeObject<Order>(output);
                jsonStr.Value = output;
                //Response.Write("OK");
                //Response.End();
                Page.Title = rlib.WebTitle;
                String OrgName = rlib.OrgName;
                String setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
                //--------------------------------------------------------------
                // Json Decoding
                String TableDiv = "";
                int totalamt = 0;
                TableDiv += "<div class='row-fluid'>";
                TableDiv += "   <div class='col-md-12' style='font-size:xx-large; font-weight:bold; padding:15px 0px; color:#555555;'>";
                TableDiv += rlib.WebTitle;
                TableDiv += "   </div>";
                TableDiv += "</div>";                
                TableDiv += "<div class='row-fluid'>";
                TableDiv += "   <div class='col-md-12' style='background-color:#E0EDF3; padding:10px 5px;'>";
                TableDiv += "購物明細";
                TableDiv += "   </div>";
                TableDiv += "</div>";
                TableDiv += "<div class='row-fluid'>";
                TableDiv += "<table class='table table-bordered'>";
                TableDiv += "   <thead>";
                TableDiv += "       <tr>";
                TableDiv += "           <th width='40%'>&nbsp;</th>";
                TableDiv += "           <th width='15%'>顏色</th>";
                TableDiv += "           <th width='15%'>尺寸</th>";
                TableDiv += "           <th width='10%'>數量</th>";
                TableDiv += "           <th width='10%'>單價</th>";
                TableDiv += "           <th width='10%'>小計</th>";
                TableDiv += "       </tr>";
                TableDiv += "   </thead>";
                TableDiv += "   <tbody>";
                
                foreach (OrderItem Orders in rlib.OrderItem)
                {
                    TableDiv += "       <tr>";
                    TableDiv += "           <td>" + Orders.ProductName + "</td>";
                    TableDiv += "           <td align='center'>" + GetSpec(setting, "prod_color", Orders.ProductColor) + "</td>";
                    TableDiv += "           <td align='center'>" + GetSpec(setting, "prod_size", Orders.ProductSize) + "</td>";
                    TableDiv += "           <td align='center'>" + Orders.ProductQty + "</td>";
                    TableDiv += "           <td class='fontright shoppingred'>" + Orders.ProductPrice + "</td>";
                    TableDiv += "           <td class='fontright shoppingred'>" + Convert.ToInt16(Orders.ProductPrice) * Convert.ToInt16(Orders.ProductQty) + "</td>";
                    TableDiv += "       </tr>";                    
                    totalamt += Convert.ToInt16(Orders.ProductPrice) * Convert.ToInt16(Orders.ProductQty);
                }
                TableDiv += "   </tbody>";
                TableDiv += "</table>";
                TableDiv += "<table class='table'>";
                TableDiv += "   <tbody>";
                TableDiv += "       <tr>";
                TableDiv += "           <td width='90%' class='fontright shoppingline' style='border-top:none; border-bottom:none; padding:3px 0px'>合計：</td>";
                TableDiv += "           <td width='10%' class='shoppingred fontright shoppingline' style='border-top:none; border-bottom:none; padding:3px 8px'>" + totalamt + "</td>";
                TableDiv += "       </tr>";
                TableDiv += "       <tr>";
                TableDiv += "           <td class='fontright' style='border-top:none; padding:3px 0px'>運費：</td>";
                TableDiv += "           <td class='shoppingred fontright' style='border-top:none; padding:3px 8px'>" + rlib.FreightAmount + "</td>";
                TableDiv += "       </tr>";

                if (Convert.ToInt16(rlib.BonusDiscount) > 0)
                {
                    TableDiv += "       <tr>";
                    TableDiv += "           <td class='fontright' style='border-top:none; padding:3px 0px'>紅利可扣抵金額：</td>";
                    TableDiv += "           <td class='shoppingred fontright' style='border-top:none; padding:3px 8px;'>" + rlib.BonusDiscount + "</td>";
                    TableDiv += "       </tr>";                    
                    TableDiv += "       <tr>";
                    TableDiv += "           <td class='fontright' valign='middle' style='border-top:1px solid #d4d4d4; padding:10px 0px'>消費總金額：</td>";
                    TableDiv += "           <td class='shoppingred fontright' style='border-top:1px solid #d4d4d4; padding:3px 8px; font-size:x-large;'>" + (totalamt - Convert.ToInt16(rlib.BonusDiscount) + Convert.ToInt16(rlib.FreightAmount)) + "</td>";
                    TableDiv += "       </tr>";
                }
                else 
                {                    
                    TableDiv += "       <tr>";
                    TableDiv += "           <td class='fontright' valign='middle' style='border-top:1px solid #d4d4d4; padding:10px 0px'>消費總金額：</td>";
                    TableDiv += "           <td class='shoppingred fontright' style='border-top:1px solid #d4d4d4; padding:3px 8px; font-size:x-large;'>" + (totalamt + Convert.ToInt16(rlib.FreightAmount)) + "</td>";
                    TableDiv += "       </tr>";                
                }
                TableDiv += "   </tbody>";
                TableDiv += "</table>";
                TableDiv += "</div>";

                TableDiv += "<div class='row-fluid'>";
                TableDiv += "   <div class='col-md-12' style='padding:5px 5px; border-top:1px solid #D4D4D4; background-color:#FBF2EA;'>";
                TableDiv += "本次購物可獲得紅利：<font class='shoppingred'>" + rlib.BonusAmt + "</font>";
                TableDiv += "   </div>";                
                TableDiv += "</div>";
                TableDiv += "<div class='row-fluid'>";
                TableDiv += "   <div class='col-md-12' style='padding:5px 5px; border-bottom:1px solid #D4D4D4; background-color:#FBF2EA; font-size:larger; font-weight:bold;'>";
                String StrPayType = "";
                switch (rlib.PayType)
                {
                    case "WebATM":
                        StrPayType = "WebATM";
                        break;
                    case "Credit":
                        StrPayType = "線上刷卡";
                        break;
                    case "CVS":
                        StrPayType = "超商繳費";
                        break;
                    case "Tenpay":
                        StrPayType = "財付通";
                        break;
                    case "Alipay":
                        StrPayType = "支付寶";
                        break;
                    case "BARCODE":
                        StrPayType = "超商條碼";
                        break;
                    case "ATM":
                        StrPayType = "虛擬帳號";
                        break;
                    case "getandpay":
                        StrPayType = "貨到付款";
                        break;
                    case "ezShip":
                        StrPayType = "超商取貨付款";
                        break;
                    case "esafeWebatm":
                        StrPayType = "WebATM";
                        break;
                    case "esafeCredit":
                        StrPayType = "信用卡";
                        break;
                    case "esafePay24":
                        StrPayType = "超商代收";
                        break;
                    case "esafePaycode":
                        StrPayType = "超商代碼付款";
                        break;
                    case "esafeAlipay":
                        StrPayType = "支付寶";
                        break;
                    case "chtHinet":
                        StrPayType = "Hinet帳單";
                        break;
                    case "chteCard":
                        StrPayType = "Hinet點數卡";
                        break;
                    case "chtld":
                        StrPayType = "行動839";
                        break;
                    case "Chtn":
                        StrPayType = "市話輕鬆付";
                        break;
                    case "chtCredit":
                        StrPayType = "信用卡";
                        break;
                    case "chtATM":
                        StrPayType = "虛擬帳號付款";
                        break;
                    case "chtWEBATM":
                        StrPayType = "WebATM";
                        break;
                    case "chtUniPresident":
                        StrPayType = "超商代收";
                        break;
                    case "chtAlipay":
                        StrPayType = "支付寶";
                        break;
                    default:
                        StrPayType = "ATM";
                        break;
                }
                TableDiv += "付款方式：" + StrPayType;
                TableDiv += "   </div>";
                TableDiv += "</div>";

                if (rlib.MemID != "")
                {                    
                    //String OrgName = rlib.OrgName;
                    //String setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        String Str_sql = "select ch_name,email,tel,sex,cell_phone from cust where mem_id='" + rlib.MemID + "'";
                        SqlCommand cmd = new SqlCommand(Str_sql, conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            this.o_cell.Text = reader["cell_phone"].ToString();
                            this.o_name.Text = reader["ch_name"].ToString();
                            this.o_tel.Text = reader["tel"].ToString();
                            this.mail.Text = reader["email"].ToString();
                            this.o_sex.SelectedIndex = Convert.ToInt16(reader["sex"].ToString())-1;
                        }
                    }
                }                
                shoppingcar.InnerHtml = TableDiv;                
            }                  
        }               

        protected void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBox1.Checked) {
                this.name.Text = this.o_name.Text;
                this.tel.Text = this.o_tel.Text;
                this.cell.Text = this.o_cell.Text;
                this.sex.SelectedIndex = this.o_sex.SelectedIndex;
            }
        }

        //確認庫存
        private bool CheckStock(String setting, String ProdID, String ProdSize, String ProdColor, int Qty)
        {
            String Str_sql = "select stock from prod_stock where prod_id=@prod_id and colorid = @colorid and sizeid=@sizeid";
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
                        Stock = Convert.ToInt16(reader[0]) - Qty;
                    }
                }
                finally
                {
                    conn.Close();
                    reader.Close();
                }
            }

            if (Stock > 0)
            {
                return true;
            }
            else {
                return false;
            }            
        }
        //抓顏色及尺寸用
        private string GetSpec(String setting, String TableName, String SearchID) {
            
            String ReturnStr = "";
            String Str_sql = "select title from " + TableName + " where id=@id";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                cmd.Parameters.Add(new SqlParameter("@id", SearchID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        ReturnStr = reader[0].ToString();
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            return ReturnStr;
        }

        //發送email
        public void send_email(string msg, string mysubject, string sender, string mail)
        {
            MailMessage message = new MailMessage();//MailMessage(寄信者, 收信者)
            message.From = new MailAddress(sender, "客服中心");
            message.Bcc.Add(sender);
            message.To.Add(mail);

            message.IsBodyHtml = true;
            message.BodyEncoding = System.Text.Encoding.UTF8;//E-mail編碼
            message.Subject = mysubject;//E-mail主旨
            message.Body = msg;//E-mail內容

            SmtpClient smtpClient = new SmtpClient("msa.hinet.net");//設定E-mail Server和port
            try
            {
                smtpClient.Send(message);
            }
            catch
            {
                smtpClient.Send(message);
            }

        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            Order rlib = JsonConvert.DeserializeObject<Order>(this.jsonStr.Value);
            String OrgName = rlib.OrgName;
            String ErrorUrl = rlib.ErrorUrl;
            String Str_Error = "";
            String setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;

            //檢查庫存
            foreach (OrderItem Orders in rlib.OrderItem)
            {
                if (!CheckStock(setting, Orders.ProductID, Orders.ProductSize, Orders.ProductColor, Convert.ToInt16(Orders.ProductQty)))
                {
                    Str_Error += "【" + Orders.ProductName + "】";
                }
            }

            if (Str_Error != "")        //庫存不足
            {
                Response.Write("<script type='text/javascript'>alert('" + Str_Error + "庫存不足,請重新選購');window.location.href='" + ErrorUrl + "';</script>");
                Response.End();
            }
            else
            {
                //訂單變數
                String BonusAmt = rlib.BonusAmt;
                String BonusDiscount = rlib.BonusDiscount;
                String FreightAmount = rlib.FreightAmount;
                String MemID = rlib.MemID;
                String PayType = rlib.PayType;
                String ReturnUrl = rlib.ReturnUrl;

                String OName = this.o_name.Text;
                String OTel = this.o_tel.Text;
                String OCell = this.o_cell.Text;
                String OSex = this.o_sex.SelectedItem.Value;
                String OEmail = this.mail.Text;

                String SName = this.name.Text;
                String STel = this.tel.Text;
                String SCell = this.cell.Text;
                String SSex = this.sex.SelectedItem.Value;
                String City = this.ddlCity.SelectedItem.Text;
                String Country = this.ddlCountry.SelectedItem.Text;
                String Zip = this.ddlzip.SelectedItem.Text;
                String Address = City + Country + this.address.Text;
                String Notememo = this.notememo.Text;
                String OrderID = "";

                SqlCommand cmd;

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
                            else {
                                OrderID = (Convert.ToInt16(reader[0]) + 1).ToString().PadLeft(9, '0');
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }

                if (OrderID != "")
                {
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        //20140331有更新此預存程序!!!!(新增郵遞區號,縣市,鄉鎮區)
                        conn.Open();
                        cmd = new SqlCommand();
                        cmd.CommandText = "sp_orderhd";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn;
                        cmd.Parameters.Add(new SqlParameter("@id", OrderID));
                        cmd.Parameters.Add(new SqlParameter("@name", SName));
                        cmd.Parameters.Add(new SqlParameter("@sex", Convert.ToInt16(SSex)));
                        cmd.Parameters.Add(new SqlParameter("@tel", STel));
                        cmd.Parameters.Add(new SqlParameter("@cell", SCell));
                        cmd.Parameters.Add(new SqlParameter("@addr", Address));
                        cmd.Parameters.Add(new SqlParameter("@mail", OEmail));
                        cmd.Parameters.Add(new SqlParameter("@notememo", Notememo));
                        cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                        cmd.Parameters.Add(new SqlParameter("@item2", ""));
                        cmd.Parameters.Add(new SqlParameter("@item3", ""));
                        cmd.Parameters.Add(new SqlParameter("@item4", ""));
                        cmd.Parameters.Add(new SqlParameter("@payment_type", PayType));
                        cmd.Parameters.Add(new SqlParameter("@o_name", OName));
                        cmd.Parameters.Add(new SqlParameter("@o_tel", OTel));
                        cmd.Parameters.Add(new SqlParameter("@o_cell", OCell));
                        cmd.Parameters.Add(new SqlParameter("@o_addr", ""));
                        cmd.Parameters.Add(new SqlParameter("@bonus_amt", BonusAmt));
                        cmd.Parameters.Add(new SqlParameter("@bonus_discount", BonusDiscount));
                        cmd.Parameters.Add(new SqlParameter("@freightamount", FreightAmount));
                        cmd.Parameters.Add(new SqlParameter("@c_no", ""));
                        cmd.Parameters.Add(new SqlParameter("@ship_city", City));
                        cmd.Parameters.Add(new SqlParameter("@ship_zip", Zip));
                        cmd.Parameters.Add(new SqlParameter("@ship_countryname", Country));
                        cmd.ExecuteNonQuery();
                    }


                    int i = 1;
                    int order_totalamt = 0;
                    foreach (OrderItem Orders in rlib.OrderItem)
                    {
                        using (SqlConnection conn = new SqlConnection(setting))
                        {
                            conn.Open();
                            //新增表身
                            cmd = new SqlCommand();
                            cmd.CommandText = "sp_order";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = conn;
                            cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                            cmd.Parameters.Add(new SqlParameter("@ser_no", i.ToString().PadLeft(3, '0')));
                            cmd.Parameters.Add(new SqlParameter("@prod_name", Orders.ProductName));
                            cmd.Parameters.Add(new SqlParameter("@price", Convert.ToInt16(Orders.ProductPrice)));
                            cmd.Parameters.Add(new SqlParameter("@qty", Convert.ToInt16(Orders.ProductQty)));
                            cmd.Parameters.Add(new SqlParameter("@amt", Convert.ToInt16(Orders.ProductQty) * Convert.ToInt16(Orders.ProductPrice)));
                            cmd.Parameters.Add(new SqlParameter("@productid", Orders.ProductID));
                            cmd.Parameters.Add(new SqlParameter("@colorid", Orders.ProductColor));
                            cmd.Parameters.Add(new SqlParameter("@sizeid", Orders.ProductSize));
                            cmd.Parameters.Add(new SqlParameter("@posno", Orders.ProductPosNo));
                            cmd.ExecuteNonQuery();
                            order_totalamt += Convert.ToInt16(Orders.ProductQty) * Convert.ToInt16(Orders.ProductPrice);
                            //庫存更新
                            cmd = new SqlCommand();
                            cmd.CommandText = "sp_stocks";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = conn;
                            cmd.Parameters.Add(new SqlParameter("@prod_id", Convert.ToInt16(Orders.ProductID)));
                            cmd.Parameters.Add(new SqlParameter("@qty", Convert.ToInt16(Orders.ProductQty)));
                            cmd.Parameters.Add(new SqlParameter("@prod_color", Convert.ToInt16(Orders.ProductColor)));
                            cmd.Parameters.Add(new SqlParameter("@prod_size", Convert.ToInt16(Orders.ProductSize)));
                            cmd.ExecuteNonQuery();
                            i = i + 1;
                        }
                    }

                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        cmd = new SqlCommand();
                        cmd.CommandText = "sp_order_freight";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn;
                        cmd.Parameters.Add(new SqlParameter("@id", OrderID));
                        cmd.Parameters.Add(new SqlParameter("@amt", order_totalamt));
                        cmd.ExecuteNonQuery();
                    }

                    String service_mail = "";
                    String mer_id = "";
                    String remit_money1 = "";
                    String remit_money2 = "";
                    String remit_money3 = "";
                    String title = "";
                    String freight_range = "";
                    //發送通知信
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        cmd = new SqlCommand("select service_mail,mer_id,remit_money1,remit_money2,remit_money3,title,freight_range from head", conn);
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
                                mer_id = reader[1].ToString();
                                remit_money1 = reader[2].ToString();
                                remit_money2 = reader[3].ToString();
                                remit_money3 = reader[4].ToString();
                                title = reader[5].ToString();
                                freight_range = reader[6].ToString();
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }

                    String mail_cont = "<table width='688' border='0' cellpadding='0' cellspacing='0' align='center'>";
                    mail_cont = mail_cont + "  <tr>";
                    mail_cont = mail_cont + "    <td><img src='http://www.cocker.com.tw/images/bg_01.gif'></td>";
                    mail_cont = mail_cont + "  </tr>";
                    mail_cont = mail_cont + "  <tr>";
                    mail_cont = mail_cont + "    <td background='http://www.cocker.com.tw/images/bg_04.gif'><table width='600' border='0' align='center' cellpadding='0' cellspacing='0'>";
                    mail_cont = mail_cont + "      <tr>";
                    mail_cont = mail_cont + "        <td width='54'><img src='http://www.cocker.com.tw/images/icon_03.gif'></td>";
                    mail_cont = mail_cont + "        <td width='546'><span style='color:#e30000; font-weight:bold; font-size:11pt;'>訂單編號：</span>";
                    mail_cont = mail_cont + OrderID;
                    mail_cont = mail_cont + "		</td>";
                    mail_cont = mail_cont + "      </tr>";
                    mail_cont = mail_cont + "    </table>";
                    mail_cont = mail_cont + "        <br />";
                    mail_cont = mail_cont + "        <table width='580' border='0' align='center' cellpadding='3' cellspacing='3' style='border-bottom:#CCCCCC 1px solid; border-left:#CCCCCC 1px solid;border-right:#CCCCCC 1px solid;border-top:#CCCCCC 1px solid;'>";
                    mail_cont = mail_cont + "          <tr style='background:#f7f7f7;'>";
                    mail_cont = mail_cont + "            <td colspan='2'>【訂&nbsp;&nbsp;購&nbsp;&nbsp;人】";
                    mail_cont = mail_cont + OName;
                    mail_cont = mail_cont + "			</td>";
                    mail_cont = mail_cont + "          </tr>";
                    mail_cont = mail_cont + "          <tr style='background:#f7f7f7;'>";
                    mail_cont = mail_cont + "            <td>【手&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;機】";
                    mail_cont = mail_cont + OCell;
                    mail_cont = mail_cont + "			</td>";
                    mail_cont = mail_cont + "            <td>【電&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;話】";
                    mail_cont = mail_cont + OTel;
                    mail_cont = mail_cont + "			</td>";
                    mail_cont = mail_cont + "          </tr>";
                    mail_cont = mail_cont + "          <tr style='background:#f4f2f7;'>";
                    mail_cont = mail_cont + "            <td colspan='2'>【電子信箱】";
                    mail_cont = mail_cont + OEmail;
                    mail_cont = mail_cont + "			</td>";
                    mail_cont = mail_cont + "          </tr>";
                    mail_cont = mail_cont + "          <tr>";
                    mail_cont = mail_cont + "            <td colspan='2' height='30'></td>";
                    mail_cont = mail_cont + "          </tr>";
                    mail_cont = mail_cont + "          <tr style='background:#f7f7f7;'>";
                    mail_cont = mail_cont + "            <td>【收&nbsp;&nbsp;件&nbsp;&nbsp;人】";
                    mail_cont = mail_cont + SName;
                    mail_cont = mail_cont + "			</td>";
                    mail_cont = mail_cont + "            <td>【性&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;別】";
                    mail_cont = mail_cont + SSex;
                    mail_cont = mail_cont + "			</td>";
                    mail_cont = mail_cont + "          </tr>";
                    mail_cont = mail_cont + "          <tr style='background:#f4f2f7;'>";
                    mail_cont = mail_cont + "            <td colspan='2'>【地&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;址】";
                    mail_cont = mail_cont + Address;
                    mail_cont = mail_cont + "			</td>";
                    mail_cont = mail_cont + "          </tr>";
                    mail_cont = mail_cont + "          <tr style='background:#f7f7f7;'>";
                    mail_cont = mail_cont + "            <td>【手&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;機】";
                    mail_cont = mail_cont + SCell;
                    mail_cont = mail_cont + "			</td>";
                    mail_cont = mail_cont + "            <td>【電&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;話】";
                    mail_cont = mail_cont + STel;
                    mail_cont = mail_cont + "			</td>";
                    mail_cont = mail_cont + "          </tr>";
                    mail_cont = mail_cont + "          <tr>";
                    mail_cont = mail_cont + "            <td colspan='2' height='30'></td>";
                    mail_cont = mail_cont + "          </tr>";
                    mail_cont = mail_cont + "          <tr class='shop01'>";
                    mail_cont = mail_cont + "            <td colspan='2'>【付款方式】";

                    String payment_type = "";
                    switch (rlib.PayType)
                    {
                        case "WebATM":
                            payment_type = "WebATM";
                            break;
                        case "Credit":
                            payment_type = "線上刷卡";
                            break;
                        case "CVS":
                            payment_type = "超商繳費";
                            break;
                        case "Tenpay":
                            payment_type = "財付通";
                            break;
                        case "Alipay":
                            payment_type = "支付寶";
                            break;
                        case "BARCODE":
                            payment_type = "超商條碼";
                            break;
                        case "ATM":
                            payment_type = "虛擬帳號";
                            break;
                        case "getandpay":
                            payment_type = "貨到付款";
                            break;
                        case "ezShip":
                            payment_type = "超商取貨付款";
                            break;
                        case "esafeWebatm":
                            payment_type = "WebATM";
                            break;
                        case "esafeCredit":
                            payment_type = "信用卡";
                            break;
                        case "esafePay24":
                            payment_type = "超商代收";
                            break;
                        case "esafePaycode":
                            payment_type = "超商代碼付款";
                            break;
                        case "esafeAlipay":
                            payment_type = "支付寶";
                            break;
                        case "chtHinet":
                            payment_type = "Hinet帳單";
                            break;
                        case "chteCard":
                            payment_type = "Hinet點數卡";
                            break;
                        case "chtld":
                            payment_type = "行動839";
                            break;
                        case "Chtn":
                            payment_type = "市話輕鬆付";
                            break;
                        case "chtCredit":
                            payment_type = "信用卡";
                            break;
                        case "chtATM":
                            payment_type = "虛擬帳號付款";
                            break;
                        case "chtWEBATM":
                            payment_type = "WebATM";
                            break;
                        case "chtUniPresident":
                            payment_type = "超商代收";
                            break;
                        case "chtAlipay":
                            payment_type = "支付寶";
                            break;
                        default:
                            payment_type = "ATM";
                            break;
                    }

                    mail_cont = mail_cont + payment_type;
                    mail_cont = mail_cont + "			</td>";
                    mail_cont = mail_cont + "          </tr>";
                    mail_cont = mail_cont + "          <tr style='background:#f4f2f7;'>";
                    mail_cont = mail_cont + "            <td colspan='2'>【備&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;註】";
                    mail_cont = mail_cont + Notememo;
                    mail_cont = mail_cont + "			</td>";
                    mail_cont = mail_cont + "          </tr>";
                    mail_cont = mail_cont + "        </table>";
                    mail_cont = mail_cont + "      <br />";
                    mail_cont = mail_cont + "        <table width='600' border='0' align='center' cellpadding='0' cellspacing='0'>";
                    mail_cont = mail_cont + "          <tr>";
                    mail_cont = mail_cont + "            <td width='54'><img src='http://www.cocker.com.tw/images/icon_03.gif'></td>";
                    mail_cont = mail_cont + "            <td width='546'><span style='color:#e30000; font-weight:bold; font-size:11pt;'>訂購項目</span></td>";
                    mail_cont = mail_cont + "          </tr>";
                    mail_cont = mail_cont + "        </table>";
                    mail_cont = mail_cont + "      <br />";
                    mail_cont = mail_cont + "        <table width='580' border='0' align='center' cellpadding='3' cellspacing='3' style='border-bottom:#CCCCCC 1px solid; border-left:#CCCCCC 1px solid;border-right:#CCCCCC 1px solid;border-top:#CCCCCC 1px solid;'>";
                    mail_cont = mail_cont + "          <tr style='background-color:#f0f0f0; color:0d087f;' align='center'>";
                    mail_cont = mail_cont + "            <td width='391'>訂購項目</td>";
                    mail_cont = mail_cont + "            <td width='74'>數量</td>";
                    mail_cont = mail_cont + "            <td width='83'>單價</td>";
                    mail_cont = mail_cont + "            <td width='83'>合計</td>";
                    mail_cont = mail_cont + "          </tr>";
                    i = 0;
                    int totalamt = 0;
                    foreach (OrderItem Orders in rlib.OrderItem)
                    {
                        if (i % 2 == 0)
                        {
                            mail_cont = mail_cont + "	<tr style='background:#f4f2f7;'>";
                        }
                        else
                        {
                            mail_cont = mail_cont + "	<tr style='background:#f7f7f7;'>";
                        }

                        mail_cont = mail_cont + "				<td>";
                        mail_cont = mail_cont + Orders.ProductName;
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "				<td align='center'>";
                        mail_cont = mail_cont + Orders.ProductQty;
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "				<td align='right'>";
                        mail_cont = mail_cont + Orders.ProductPrice;
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "				<td align='right'>";
                        mail_cont = mail_cont + Convert.ToInt16(Orders.ProductQty) * Convert.ToInt16(Orders.ProductPrice);
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "			  </tr>";
                        totalamt += Convert.ToInt16(Orders.ProductPrice) * Convert.ToInt16(Orders.ProductQty);
                        i = i + 1;
                    }
                    mail_cont = mail_cont + "			  <tr style='background:#f4f2f7;'>";

                    mail_cont = mail_cont + "			  <td>運費</td><td></td><td></td><td align='right'>";
                    mail_cont = mail_cont + FreightAmount;
                    mail_cont = mail_cont + "</td></tr>";

                    mail_cont = mail_cont + "<tr>";
                    mail_cont = mail_cont + "            <td colspan='4' align='right'><span style='font-size:14pt; font-weight:bold;'>合計&nbsp;新台幣</span><span style='color:#FF0000; font-weight:bold;'>NT$";
                    mail_cont = mail_cont + (totalamt + Int32.Parse(FreightAmount));
                    //紅利扣抵
                    if (Int32.Parse(BonusDiscount.ToString()) > 0)
                    {
                        mail_cont = mail_cont + "			  <tr style='background:#f4f2f7;'>";
                        mail_cont = mail_cont + "            <td colspan='4' align='right'><span style='font-size:14pt; font-weight:bold;'>紅利扣抵金額</span><span style='color:#FF0000; font-weight:bold;'>NT$";
                        mail_cont = mail_cont + BonusDiscount;
                        mail_cont = mail_cont + "			</span></td>";
                        mail_cont = mail_cont + "          </tr>";
                        mail_cont = mail_cont + "			  <tr style='background:#f4f2f7;'>";
                        mail_cont = mail_cont + "            <td colspan='4' align='right'><span style='font-size:14pt; font-weight:bold;'>扣抵後金額(含運費)</span><span style='color:#FF0000; font-weight:bold;'>NT$";
                        mail_cont = mail_cont + (totalamt + Convert.ToInt16(FreightAmount) - Convert.ToInt16(BonusDiscount));
                        mail_cont = mail_cont + "			</span></td>";
                        mail_cont = mail_cont + "          </tr>";
                    }

                    mail_cont = mail_cont + "			</span></td>";
                    mail_cont = mail_cont + "          </tr>";
                    mail_cont = mail_cont + "        </table>";
                    if (Convert.ToInt16(FreightAmount) > 0)
                    {
                        mail_cont = mail_cont + "        <table width='580' border='0' align='center' cellpadding='0' cellspacing='0'>";
                        mail_cont = mail_cont + "          <tr>";
                        mail_cont = mail_cont + "            <td align='right' style='color:#FF0000; font-weight:bold;' valign='top'>(未滿";
                        mail_cont = mail_cont + freight_range;
                        mail_cont = mail_cont + "			元，酌收運費";
                        mail_cont = mail_cont + FreightAmount;
                        mail_cont = mail_cont + "			元)</td>";
                        mail_cont = mail_cont + "          </tr>";
                        mail_cont = mail_cont + "        </table>";
                    }
                    mail_cont = mail_cont + "      <br />";

                    if (payment_type == "ATM轉帳")
                    {
                        mail_cont = mail_cont + "		<table width='600' border='0' align='center' cellpadding='0' cellspacing='0'>";
                        mail_cont = mail_cont + "          <tr>";
                        mail_cont = mail_cont + "            <td width='54'><img src='http://www.cocker.com.tw/images/icon_03.gif'></td>";
                        mail_cont = mail_cont + "            <td width='546' style='color:#e30000; font-weight:bold; font-size:11pt;'>繳費資訊</td>";
                        mail_cont = mail_cont + "          </tr>";
                        mail_cont = mail_cont + "        </table>";
                        mail_cont = mail_cont + "      <br />";
                        mail_cont = mail_cont + "        <table width='580' border='0' align='center' cellpadding='3' cellspacing='3' style='border-bottom:#CCCCCC 1px solid; border-left:#CCCCCC 1px solid;border-right:#CCCCCC 1px solid;border-top:#CCCCCC 1px solid;'>";
                        mail_cont = mail_cont + "          <tr style='background:#f7f7f7;'>";
                        mail_cont = mail_cont + "            <td width='120' align='center' style='color:0d087f;'>轉帳銀行代號</td>";
                        mail_cont = mail_cont + "            <td width='437'>";
                        mail_cont = mail_cont + remit_money1;
                        mail_cont = mail_cont + "</td>";
                        mail_cont = mail_cont + "          </tr>";
                        mail_cont = mail_cont + "          <tr style='background:#f4f2f7;'>";
                        mail_cont = mail_cont + "            <td align='center' style='color:0d087f;'>轉&nbsp;&nbsp;&nbsp;帳&nbsp;&nbsp;&nbsp;帳&nbsp;&nbsp;號</td>";
                        mail_cont = mail_cont + "            <td>";
                        mail_cont = mail_cont + remit_money2;
                        mail_cont = mail_cont + "			</td>";
                        mail_cont = mail_cont + "          </tr>";
                        mail_cont = mail_cont + "          <tr style='background:#f7f7f7;'>";
                        mail_cont = mail_cont + "            <td align='center' style='color:0d087f;'>戶&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;名</td>";
                        mail_cont = mail_cont + "            <td>";
                        mail_cont = mail_cont + remit_money3;
                        mail_cont = mail_cont + "			</td>";
                        mail_cont = mail_cont + "          </tr>";
                        mail_cont = mail_cont + "          <tr style='background:#f4f2f7;'>";
                        mail_cont = mail_cont + "            <td align='center' style='color:0d087f;'>應&nbsp;&nbsp;&nbsp;繳&nbsp;&nbsp;&nbsp;金&nbsp;&nbsp;額</td>";
                        mail_cont = mail_cont + "            <td>新台幣&nbsp;&nbsp;&nbsp;NT$";
                        mail_cont = mail_cont + (totalamt + Convert.ToInt16(FreightAmount) - Convert.ToInt16(BonusDiscount));
                        mail_cont = mail_cont + "			</td>";
                        mail_cont = mail_cont + "          </tr>";
                        mail_cont = mail_cont + "          <tr style='background:#f7f7f7;'>";
                        mail_cont = mail_cont + "            <td align='center' style='color:0d087f;'>繳&nbsp;&nbsp;&nbsp;費&nbsp;&nbsp;&nbsp;期&nbsp;&nbsp;限</td>";
                        mail_cont = mail_cont + "            <td>";
                        mail_cont = mail_cont + DateTime.Today.AddDays(1).ToString("yyyy/MM/dd") + " 23:59</td>";
                        mail_cont = mail_cont + "          </tr>";
                        mail_cont = mail_cont + "      </table><br>";
                    }


                    mail_cont = mail_cont + "        <center>‧隨後我們也會將轉帳的資料mail一封到您指定的電子信箱：<span style='color:#FF0000; font-weight:bold;'>";
                    mail_cont = mail_cont + OEmail;
                    mail_cont = mail_cont + "			</span></center></td>";
                    mail_cont = mail_cont + "  </tr>";
                    mail_cont = mail_cont + "  <tr>";
                    mail_cont = mail_cont + "    <td><img src='http://www.cocker.com.tw/images/bg_06.jpg'></td>";
                    mail_cont = mail_cont + "  </tr>";
                    mail_cont = mail_cont + "</table>";


                    send_email(mail_cont, "訂購通知 【" + title + "】", service_mail, OEmail);//呼叫send_email函式測試                    
                    Response.Write("<script type='text/javascript'>window.location.href='" + ReturnUrl + "/tw/shop_order1.asp?id=" + OrderID + "';</script>");

                }
                else
                {
                    Response.Write("<script type='text/javascript'>alert('資料錯誤，請重新選購');window.location.href='" + ErrorUrl + "';</script>");
                    Response.End();
                }
            }
        }

    }
}