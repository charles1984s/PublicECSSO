using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Newtonsoft.Json;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Net.Mail;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Globalization;
using Microsoft.Security.Application;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Net;
using ECSSO.Library.CustFormLibary;

namespace ECSSO
{
    public partial class Shopping : System.Web.UI.Page
    {
        private string str_language = string.Empty, folder = string.Empty;
        private string setting { get; set; }
        private string couponTitle { get; set; }
        private string disablePrice { get; set; }
        private string orderMemo { get; set; }
        private int person { get; set; }
        private int discont { get; set; }
        private Shoppingcar.RootObject rlib { get; set; }
        private GetStr GS { get; set; }
        #region 語系變換
        protected override void InitializeCulture()
        {
            //此currentculture來自default.aspx頁面上兩個超連結的連結位址,見html部分

            if (Request.Form["language"] != null)
            {
                str_language = Request.Form["language"].ToString();
            }
            else
            {
                if (Request.QueryString["language"] != null)
                {
                    str_language = Request.QueryString["language"].ToString();
                }
            }
            if (str_language == "")
            {
                str_language = "zh-tw";
            }
            if (Request.Form["folder"] != null)
            {
                folder = Request.Form["folder"];
            }
            else
            {
                if (Request.QueryString["folder"] != null)
                {
                    folder = Request.QueryString["folder"].ToString();
                }
            }
            if (folder == "") folder = "tw";
            if (!String.IsNullOrEmpty(str_language))
            {
                //Nation - 決定了採用哪一種當地語系化資源，也就是使用哪種語言
                //Culture - 決定各種資料類型是如何組織，如數位與日期
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(str_language);
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(str_language);
            }
        }
        #endregion
        
        #region 載入CSS
        protected void Page_Init(object sender, EventArgs e)
        {
            HtmlGenericControl objLink = new HtmlGenericControl("link");

            objLink.Attributes.Add("rel", "stylesheet");
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/shopping.css?t=20190218");
            objLink.Attributes.Add("type", "text/css");
            this.Page.Header.Controls.Add(objLink);
        }
        #endregion

        #region 讀取客製化CSS
        private string getCellMailCssString()
        {
            Shoppingcar.RootObject lib = JsonConvert.DeserializeObject<Shoppingcar.RootObject>(this.jsonStr.Value);
            string css = "";
            string setting = GS.GetSetting3(lib.OrderData.OrgName);
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select cellMailCss from CurrentUseHead", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.Read())
                    {
                        css = reader["cellMailCss"].ToString();
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return css;
        }
        private string getCusrCssString() {
            Shoppingcar.RootObject lib = JsonConvert.DeserializeObject<Shoppingcar.RootObject>(this.jsonStr.Value);
            string css = "";
            string setting = GS.GetSetting3(lib.OrderData.OrgName);
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select end_css from CurrentUseHead", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.Read())
                    {
                        css = reader["end_css"].ToString();
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return css;
        }
        private void getCustCss()
        {
            HtmlGenericControl objStyle = new HtmlGenericControl("style");
            if (ChkJson(this.jsonStr.Value))
            {
                StringWriter myWriter = new StringWriter();
                HttpUtility.HtmlDecode(getCusrCssString(), myWriter);
                objStyle.InnerHtml = HttpUtility.HtmlDecode(myWriter.ToString());
                this.Page.Header.Controls.Add(objStyle);
            }
        }
        #endregion

        #region 讀取訂購單內容(json)
        protected void Page_Load(object sender, EventArgs e)
        {
            HiddenDIV.Visible = false;      //此表單DIV不顯示
            disablePrice = "N";
            person = 0;
            discont = 0;
            GS = new GetStr();
            this.language.Value = str_language;
            if (IsPostBack)
            {
                CompareValidator1.EnableClientScript = CheckBox3.Checked;
                RegularExpressionValidator2.EnableClientScript = CheckBox3.Checked;
                RequiredFieldValidator8.EnableClientScript = CheckBox3.Checked;
                RequiredFieldValidator12.EnableClientScript = CheckBox3.Checked;

                if (this.password.Text == this.confirmPassword.Text)
                {
                    this.password.Attributes["Value"] = this.password.Text;
                    this.confirmPassword.Attributes["Value"] = this.confirmPassword.Text;
                }
                getCustCss();
            }
            else
            {
                if (Request.Form["orderData"] == null)
                {
                    Response.Write("Error");
                    Response.End();
                }
                else
                {
                    if (Request.Form["orderData"].ToString() == "")
                    {
                        Response.Write("Error");
                        Response.End();
                    }
                }
                string output = "";

                TimeSpan ts;
                // Json Encoding            
                //output = JsonConvert.SerializeObject(order);
                output = GS.ReplaceStr(Request.Form["orderData"].ToString());
                try
                {
                    rlib = JsonConvert.DeserializeObject<Shoppingcar.RootObject>(output);
                }
                catch
                {
                    Response.Write("購物車資料錯誤");
                    Response.End();
                }


                if (ChkJson(output))
                {
                    jsonStr.Value = output;
                    //Response.Write("OK");
                    //Response.End();
                    rlib.OrderData.doInsertCoupon = false;
                    if (rlib.OrderData.PayType == "ezship0" || rlib.OrderData.PayType == "ezship1")
                    {
                        this.Label10.Visible = false;
                        this.ddlCity.Visible = false;
                        this.ddlCountry.Visible = false;
                        this.ddlzip.Visible = false;
                        this.address.Visible = false;
                        this.RequiredFieldValidator7.Visible = false;
                        this.divaddr1.Visible = false;
                    }
                    if (str_language != "zh-tw") {
                        this.twCity.Visible = false;
                        this.RequiredFieldValidator14.Enabled = false;
                        this.RequiredFieldValidator13.Enabled = false;
                    }
                    Page.Title = rlib.OrderData.WebTitle;
                    version.Value = rlib.OrderData.version;

                    String OrgName = rlib.OrderData.OrgName;
                    setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
                    String MerchantID = "";
                    String SiteID = "";
                    getCustCss();
                    #region 自訂備註
                    FormColumns f = new FormColumns(setting, 0);
                    if (f.RspnCode == "200")
                    {
                        notememo.Visible = false;
                        formRelation.InnerHtml = HttpUtility.HtmlDecode(Server.HtmlDecode(Microsoft.Security.Application.Encoder.HtmlEncode(f.getBootstrap3Html())));
                        formRelationValidator.Enabled = true;
                    }
                    else {
                        formRelation.Visible = false;
                        formRelationValidator.Enabled = false;
                    }
                    #endregion
                    /*取貨門市選擇*/
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(@"
                            select
                                storetype,ecpay_mer_id,id,
                                invoice_disp,disablePrice,
                                bonus_range,bonus_send,OrderRemark
                            from CurrentUseHead
                        ", conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            while (reader.Read())
                            {
                                MerchantID = reader[1].ToString();
                                SiteID = reader[2].ToString();
                                if (reader[0].ToString() == "1")
                                {
                                    SqlDataSource4.ConnectionString = setting;
                                    SqlDataSource4.DataBind();
                                    PickupShop.Visible = true;
                                    orderitem3.Visible = false;

                                    this.h_o_name.Value = this.o_name.Text;
                                    this.h_o_tel.Value = this.o_tel.Text;
                                    this.h_o_cell.Value = this.o_cell.Text;
                                    this.h_mail.Value = this.mail.Text;
                                    this.h_o_sex.Value = this.o_sex.SelectedIndex.ToString();
                                    if (this.address != null)
                                    {
                                        this.h_o_addr.Value = this.o_addr.Value;
                                    }
                                }
                                HiddenDIV.Visible = true;
                                if (reader[3].ToString() != "Y")
                                {
                                    this.CheckBox2.Checked = false;
                                    this.invoice.Visible = false;
                                }
                                disablePrice = reader["disablePrice"].ToString();
                                if (string.IsNullOrEmpty(reader["OrderRemark"].ToString()))
                                {
                                    this.orderMemoText.Visible = false;
                                }
                                else {
                                    this.orderMemoText.InnerHtml =
                                        HttpUtility.HtmlEncode(reader["OrderRemark"].ToString()).Replace(
                                            char.ConvertFromUtf32(13) + char.ConvertFromUtf32(10), "<br />"
                                        );
                                }
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }

                    #region 超商取貨和自取不需輸入收件地址
                    if (rlib.OrderData.LogisticstypeID == "003" || rlib.OrderData.LogisticstypeID == "004" || rlib.OrderData.LogisticstypeID == "006")
                    {
                        divaddr1.Visible = false;
                    }

                    #endregion

                    #region 超商取貨門市選擇按鈕(綠界)
                    if ((rlib.OrderData.LogisticstypeID == "003" || rlib.OrderData.LogisticstypeID == "004") && rlib.OrderData.PayType != "PCHomeIPL7")
                    {

                        CVSdiv.Visible = true;

                        if (rlib.OrderData.LogisticApi == "ecpay")
                        {
                            LinkButton2.Visible = true;
                            LinkButton3.Visible = false;
                            LinkButton4.Visible = false;
                        } else {
                            LinkButton2.Visible = false;
                            if (rlib.OrderData.LogisticstypeID == "003")    //打開全家按鈕
                            {
                                LinkButton3.Visible = false;
                                LinkButton4.Visible = true;
                            }
                            if (rlib.OrderData.LogisticstypeID == "004")    //打開7-11按鈕
                            {
                                LinkButton3.Visible = true;
                                LinkButton4.Visible = false;
                            }
                        }

                        String MerchantTradeNo = DateTime.Now.ToString("yyyyMMddHHmmss");         //廠商交易編號
                        String LogisticsSubType = "";                                              //物流子類型
                        String IsCollection = "N";                      //是否代收貨款

                        using (SqlConnection conn = new SqlConnection(setting))
                        {
                            conn.Open();

                            SqlCommand cmd = new SqlCommand("select ecpaycode from Logisticstype where id=@id", conn);
                            cmd.Parameters.Add(new SqlParameter("@id", rlib.OrderData.LogisticstypeID));
                            SqlDataReader reader = cmd.ExecuteReader();
                            try
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        LogisticsSubType = reader[0].ToString();

                                    }
                                }
                            }
                            finally { reader.Close(); }
                        }

                        if (rlib.OrderData.PayType == "getandpay")
                        {
                            IsCollection = "Y";
                        }

                        this.LinkButton2.Attributes["onclick"] = "window.open('map.aspx?CheckM=" + GS.MD5Endode(SiteID + rlib.OrderData.OrgName) + "&SiteID=" + SiteID + "&LogisticsSubType=" + LogisticsSubType + "&IsCollection=" + IsCollection + "'); return false;";
                    }
                    else
                    {
                        CVSdiv.Visible = false;
                    }
                    #endregion


                    //--------------------------------------------------------------
                    // Json Decoding
                    String TableDiv = "";
                    int totalamt = 0;
                    int ShopType = rlib.OrderData.ShopType;
                    bool isProdSize = GS.GetSpec(setting, "sizeid");
                    bool isProdColor = GS.GetSpec(setting, "colorid");

                    #region 詢價
                    if (rlib.OrderData.ShopType == 4 || rlib.OrderData.allVirtualProd || rlib.OrderData.PayType == "PCHomeIPL7")
                    {
                        this.orderitem3.Visible = false;
                        this.RequiredFieldValidator7.Enabled = false;
                        this.RequiredFieldValidator6.Enabled = false;
                        this.RequiredFieldValidator13.Enabled = false;
                        this.RegularExpressionValidator3.Enabled = false;
                        this.orderMemData.Attributes["class"] = "col-md-12";
                        if(rlib.OrderData.ShopType == 4) disablePrice = "Y";
                    }
                    #endregion

                    TableDiv += "<div class='row-fluid'>";
                    TableDiv += "   <div class='col-md-12' style='font-size:xx-large; font-weight:bold; padding:15px 0px; color:#555555;'>";
                    TableDiv += rlib.OrderData.WebTitle;
                    TableDiv += "   </div>";
                    TableDiv += "</div>";
                    TableDiv += "<div class='row-fluid'>";
                    TableDiv += "   <div class='col-md-12' style='padding:10px 5px;'>";
                    TableDiv += GetLocalResourceObject("StringResource1");
                    TableDiv += "   </div>";
                    TableDiv += "</div>";

                    #region 購物車表頭
                    TableDiv += "<div class='row-fluid'>";
                    TableDiv += "<div class='col-md-12' style='padding:0px;'>";
                    TableDiv += $"<table id='orderTable' class='table table-bordered{(ShopType==4? " inquiry" : "")}'>";
                    TableDiv += "   <thead>";
                    TableDiv += "       <tr>";
                    TableDiv += "           <th width='40%'>&nbsp;</th>";
                    if (rlib.OrderData.ShopType == 3)
                    {
                        TableDiv += "           <th width='10%'>客製項目</th>";
                    }
                    else
                    {
                        if (isProdColor)
                            TableDiv += "           <th width='10%'>" + GS.GetSpecTitle(setting, "1") + "</th>";
                        if (isProdSize)
                            TableDiv += "           <th width='10%'>" + GS.GetSpecTitle(setting, "2") + "</th>";
                    }
                    TableDiv += "           <th width='10%'>" + GetLocalResourceObject("StringResource2") + "</th>";
                    if (disablePrice != "Y")
                    {
                        TableDiv += "           <th width='10%'>" + GetLocalResourceObject("StringResource3") + "</th>";
                        TableDiv += "           <th width='10%'>" + GetLocalResourceObject("StringResource4") + "</th>";
                        TableDiv += "           <th width='10%'>" + GetLocalResourceObject("StringResource5") + "</th>";
                    }
                    TableDiv += "       </tr>";
                    TableDiv += "   </thead>";
                    TableDiv += "   <tbody>";
                    #endregion


                    #region 購物車會員資料
                    if (rlib.OrderData.MemID != "")
                    {
                        //String OrgName = rlib.OrgName;
                        //String setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
                        using (SqlConnection conn = new SqlConnection(setting))
                        {
                            conn.Open();
                            String Str_sql = "select ch_name,email,tel,sex,cell_phone,addr,bonus_total,ident from cust where mem_id=@mem_id";
                            SqlCommand cmd = new SqlCommand(Str_sql, conn);
                            cmd.Parameters.Add(new SqlParameter("@mem_id", rlib.OrderData.MemID));
                            SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                this.o_cell.Text = reader["cell_phone"].ToString();
                                this.o_name.Text = reader["ch_name"].ToString();
                                this.o_tel.Text = reader["tel"].ToString();
                                this.o_addr.Value = reader["addr"].ToString();
                                this.mail.Text = reader["email"].ToString();
                                this.ident.Text = reader["ident"].ToString();
                                this.o_sex.SelectedIndex = Convert.ToInt32(reader["sex"].ToString()) - 1;

                                this.cell.Text = this.o_cell.Text;
                                this.name.Text = this.o_name.Text;
                                this.tel.Text = this.o_tel.Text;
                                this.address.Text = this.o_addr.Value;
                                this.sex.SelectedIndex = this.o_sex.SelectedIndex;
                                this.AddMember.Visible = false;
                                this.CheckBox3.Checked = false;
                                Session["bonus"] = reader["bonus_total"].ToString();
                            }
                        }
                    }
                    #endregion
                    Int32 BonusDiscount = 0;
                    if (rlib.OrderData.BonusDiscount != null)
                    {
                        BonusDiscount = Convert.ToInt32(rlib.OrderData.BonusDiscount);
                    }
                    if (Session["prodBonus"] == null)
                    {
                        Session["prodBonus"] = "0";
                    }
                    if (Session["bonus"] == null)
                    {
                        Session["bonus"] = "0";
                    }

                    if ((Convert.ToInt32(Session["bonus"].ToString()) - Convert.ToInt32(Session["prodBonus"].ToString()) - BonusDiscount) >= 0)
                    {
                        int couDiscont = 0;
                        if (rlib.OrderData.ShopType == 3)         //點餐
                        {
                            #region 點餐
                            foot.Visible = false;
                            LinkButton1.Text = GetLocalResourceObject("StringResource54").ToString();
                            String VerCode = rlib.OrderData.MenuLists.Vercode;
                            String TableID = rlib.OrderData.MenuLists.TableID;
                            String TakeMealType = rlib.OrderData.MenuLists.TakeMealType;

                            if (TakeMealType != "3")
                            {
                                this.h_o_name.Value = this.o_name.Text;
                                this.h_o_tel.Value = this.o_tel.Text;
                                this.h_o_cell.Value = this.o_cell.Text;
                                this.h_mail.Value = this.mail.Text;
                                this.h_o_sex.Value = this.o_sex.SelectedIndex.ToString();
                                if (this.address != null)
                                {
                                    this.h_o_addr.Value = this.o_addr.Value;
                                }
                                HiddenDIV.Visible = true;
                                OrderForm.Visible = false;
                            }

                            TableDiv += "       <tr>";
                            TableDiv += "           <td colspan='6'><b>";
                            TableDiv += GS.GetMealType(TakeMealType);

                            if (TakeMealType == "1")
                            {
                                TableDiv += "桌號：" + TableID;
                            }

                            TableDiv += "           </b></td>";
                            TableDiv += "       </tr>";

                            String ComboID = string.Empty;
                            String ComboName = string.Empty;
                            String ComboDiscount = string.Empty;
                            String ComboQty = string.Empty;
                            String MenuID = string.Empty;
                            String MenuName = string.Empty;
                            String SpecQty = string.Empty;
                            String SpecString = string.Empty;
                            Int32 SpecAmt = 0;
                            Int32 MenuPrice = 0;
                            Int32 MenuAddPrice = 0;
                            Int32 MenuDiscount = 0;

                            #region 購物車表身
                            foreach (Shoppingcar.Menu Menu in rlib.OrderData.MenuLists.Menu)
                            {
                                ComboID = Menu.ID;
                                ComboName = Menu.Name;
                                ComboDiscount = Menu.Discount;
                                ComboQty = Menu.Qty;
                                SpecAmt = 0;

                                if (ComboID == "Single")
                                {
                                    TableDiv += "       <tr class='menuGroup'>";
                                    TableDiv += "           <td colspan='6'>" + ComboName + "</td>";
                                    TableDiv += "       </tr>";
                                }
                                else
                                {
                                    SpecAmt = Convert.ToInt32((Convert.ToInt32(ComboQty) * double.Parse(GetProdPrice(ComboID, "", setting, "", ""))) + 0.001);

                                    TableDiv += "       <tr class='menuGroup'>";
                                    TableDiv += "           <td>" + ComboName + "</td>";
                                    TableDiv += "           <td></td>";
                                    TableDiv += "           <td>" + ComboQty + "</td>";
                                    if (disablePrice != "Y")
                                    {
                                        TableDiv += "           <td>" + GetProdPrice(ComboID, "", setting, "", "") + "</td>";
                                        TableDiv += "           <td>" + ComboDiscount + "</td>";
                                        TableDiv += "           <td>" + SpecAmt + "</td>";
                                    }
                                    TableDiv += "       </tr>";

                                    totalamt = totalamt + SpecAmt;
                                }

                                foreach (Shoppingcar.MenuItem MenuItems in Menu.MenuItems)
                                {
                                    MenuID = MenuItems.ID;
                                    MenuName = MenuItems.Name;
                                    MenuPrice = Convert.ToInt32(GetProdPrice(MenuID, "", setting, "", ""));

                                    if (ComboID == "Single")
                                    {
                                        MenuAddPrice = 0;
                                    }
                                    else
                                    {
                                        MenuAddPrice = Convert.ToInt32(GetProdPrice(MenuID, "", setting, "", ComboID));
                                    }

                                    foreach (Shoppingcar.MenuSpec MenuSpec in MenuItems.MenuSpec)
                                    {
                                        SpecString = "";
                                        SpecQty = MenuSpec.Qty;
                                        SpecAmt = 0;
                                        if (MenuSpec.OtherID != null)
                                        {
                                            for (int i = 0; i < MenuSpec.OtherID.Count; i++)
                                            {
                                                SpecString += GetMemoName(setting, MenuSpec.OtherID[i]) + "$" + GetProdPrice(MenuID, "", setting, MenuSpec.OtherID[i], "") + " ";
                                                SpecAmt += Convert.ToInt32(GetProdPrice(MenuID, "", setting, MenuSpec.OtherID[i], ""));
                                            }
                                        }
                                        if (MenuSpec.Memo != null)
                                        {
                                            for (int i = 0; i < MenuSpec.Memo.Count; i++)
                                            {
                                                SpecString += MenuSpec.Memo[i].ToString() + " ";
                                            }
                                        }

                                        if (ComboID == "Single")
                                        {

                                            MenuDiscount = 0 - SpecAmt;
                                        }
                                        else
                                        {
                                            MenuDiscount = MenuPrice - SpecAmt - MenuAddPrice;
                                        }


                                        TableDiv += "       <tr>";
                                        TableDiv += "           <td>" + MenuName;
                                        if (MenuAddPrice > 0)
                                        {
                                            TableDiv += "(需加價" + MenuAddPrice + ")";
                                        }
                                        TableDiv += "</td>";
                                        TableDiv += "           <td>" + SpecString + "</td>";
                                        TableDiv += "           <td>" + SpecQty + "</td>";
                                        if (disablePrice != "Y")
                                        {
                                            TableDiv += "           <td>" + MenuPrice + "</td>";
                                            TableDiv += "           <td>" + MenuDiscount + "</td>";
                                            TableDiv += "           <td>" + (MenuPrice - MenuDiscount) * Convert.ToInt32(SpecQty) + "</td>";
                                        }
                                        TableDiv += "       </tr>";

                                        totalamt = totalamt + (MenuPrice - MenuDiscount) * Convert.ToInt32(SpecQty);
                                    }
                                }
                            }
                            #endregion
                            #endregion
                        }
                        else
                        {
                            #region 優惠券
                            rlib.OrderData.CouponID = 0;
                            rlib.OrderData.couDiscont = 0;
                            if (!string.IsNullOrEmpty(rlib.OrderData.MemID))
                            {
                                if (!string.IsNullOrEmpty(rlib.OrderData.GCode) && string.IsNullOrEmpty(rlib.OrderData.VCode))
                                {
                                    using (SqlConnection conn = new SqlConnection(setting))
                                    {
                                        conn.Open();
                                        SqlCommand cmd = new SqlCommand(@"
                                            select * from cust_coupon where [stat]=1 and GCode=@GCode and memid=@memid
                                        ", conn);
                                        cmd.Parameters.Add(new SqlParameter("@GCode", rlib.OrderData.GCode));
                                        cmd.Parameters.Add(new SqlParameter("@memid", rlib.OrderData.MemID));
                                        SqlDataReader reader = null;
                                        try
                                        {
                                            reader = cmd.ExecuteReader();
                                            if (reader.Read())
                                            {
                                                rlib.OrderData.VCode = reader["VCode"].ToString();
                                            }
                                        }
                                        finally
                                        {
                                            if (reader != null) reader.Close();
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(rlib.OrderData.GCode) || !string.IsNullOrEmpty(rlib.OrderData.VCode))
                                {
                                    ChechCouponData chechCouponData = chechCoupon(setting, rlib.OrderData.VCode, rlib.OrderData.GCode, rlib.OrderData.MemID);
                                    if (chechCouponData != null)
                                    {
                                        rlib.OrderData.CouponID = chechCouponData.id;
                                        rlib.OrderData.VCode = chechCouponData.VCode;
                                        rlib.OrderData.doInsertCoupon = chechCouponData.doInsertCoupon;
                                        rlib.OrderData.GCode = chechCouponData.GCode;
                                        couponExe(
                                            chechCouponData.activeType,
                                            chechCouponData.discount,
                                            chechCouponData.gift,
                                            chechCouponData.prodStoreNo
                                        );
                                        couponTitle = chechCouponData.title;
                                        rlib.OrderData.couponTitle = couponTitle;
                                    }
                                    else
                                    {
                                        Response.Write("<script type='text/javascript'>alert('" + "優惠已被領取完畢或優惠券已到期，請重新確認優惠券。" + "');window.location.href='" + rlib.OrderData.ErrorUrl + "';</script>");
                                        Response.End();
                                    }
                                }
                            }
                            #endregion
                            #region 購物車表身
                            foreach (Shoppingcar.OrderList Orders in rlib.OrderData.OrderLists)
                            {
                                int listPrice = 0;
                                String Str_Css1 = "";
                                String Str_Css2 = "";
                                if ((rlib.OrderData.OrderLists.Count > 1 && Orders.Type != 0) || disablePrice == "S")
                                {
                                    TableDiv += "       <tr>";
                                    TableDiv += "           <td colspan='100%'><b>" + Orders.Title + "</b></td>";
                                    TableDiv += "       </tr>";
                                    Str_Css1 = "padding-left: 20px;";
                                    Str_Css2 = "padding-left: 40px;";
                                }
                                else
                                {
                                    Str_Css1 = "";
                                    Str_Css2 = "padding-left: 20px;";
                                }

                                foreach (Shoppingcar.OrderItem OrderItems in Orders.OrderItems)
                                {
                                    #region 訂購商品規格
                                    foreach (Shoppingcar.OrderSpec OrderSpecs in OrderItems.OrderSpecs)
                                    {
                                        String UseTime = "";
                                        if (OrderItems.Virtual == "Y")
                                        {
                                            if (Convert.ToInt32(OrderItems.UseTime) > 0)
                                            {
                                                ts = new TimeSpan(0, Convert.ToInt32(OrderItems.UseTime), 0);
                                                if (ts.Days > 0) UseTime = ts.Days.ToString() + GetLocalResourceObject("StringResource6");
                                                if (ts.Hours > 0) UseTime += ts.Hours.ToString() + GetLocalResourceObject("StringResource7");
                                                if (ts.Minutes > 0) UseTime += ts.Minutes.ToString() + GetLocalResourceObject("StringResource8");
                                                UseTime = "(" + UseTime + ")";
                                            }
                                        }

                                        TableDiv += "       <tr>";
                                        TableDiv += "           <td style='" + Str_Css1 + "'>" + OrderItems.Name + UseTime + "</td>";
                                        if (isProdColor)
                                            TableDiv += "           <td align='center'>" + GS.GetSpec(setting, "prod_color", OrderSpecs.Color) + "</td>";
                                        if (isProdSize)
                                            TableDiv += "           <td align='center'>" + GS.GetSpec(setting, "prod_size", OrderSpecs.Size) + "</td>";
                                        TableDiv += "           <td align='center'>" + OrderSpecs.Qty + "</td>";
                                        if (disablePrice != "Y")
                                        {
                                            TableDiv += "           <td class='fontright shoppingred'>" + OrderSpecs.Price + "</td>";
                                            TableDiv += "           <td class='fontright shoppingred'>" + OrderSpecs.Discount + "</td>";
                                            TableDiv += "           <td class='fontright shoppingred'>";
                                            if (Convert.ToInt32(OrderSpecs.Discount) != 0)
                                            {
                                                TableDiv += "           <s style='font-size:9pt; color:#bbbbbb; padding-right:3px;'>" + Convert.ToInt32((OrderSpecs.Qty * OrderSpecs.Price + 0.001)) + "</s>";
                                            }
                                            TableDiv += (Convert.ToInt32((OrderSpecs.Price * Convert.ToInt32(OrderSpecs.Qty)) + 0.001) - Convert.ToInt32(OrderSpecs.Discount));

                                            if (Convert.ToInt32(OrderSpecs.Bonus) > 0)
                                            {
                                                TableDiv += "<br>(" + GetLocalResourceObject("StringResource53") + OrderSpecs.Bonus + ")";
                                            }
                                            TableDiv += "</td>";
                                        }
                                        TableDiv += "       </tr>";
                                        int amt = Convert.ToInt32((OrderSpecs.Price * Convert.ToInt32(OrderSpecs.Qty) + 0.001)) - Convert.ToInt32(OrderSpecs.Discount);
                                        listPrice += amt;
                                        totalamt += amt;
                                    }
                                    #endregion

                                    #region 加價購
                                    foreach (Shoppingcar.AdditionalItem AdditionalItems in OrderItems.AdditionalItems)
                                    {
                                        TableDiv += "       <tr>";
                                        TableDiv += "           <td style='" + Str_Css2 + "'><span style='color:#ff0000;'>" + GetLocalResourceObject("StringResource9") + "-</span>" + AdditionalItems.Name + "</td>";
                                        if (isProdColor)
                                            TableDiv += "           <td align='center'>" + GS.GetSpec(setting, "prod_color", AdditionalItems.Color) + "</td>";
                                        if (isProdSize)
                                            TableDiv += "           <td align='center'>" + GS.GetSpec(setting, "prod_size", AdditionalItems.Size) + "</td>";
                                        TableDiv += "           <td align='center'>" + AdditionalItems.Qty + "</td>";
                                        if (disablePrice != "Y")
                                        {
                                            TableDiv += "           <td class='fontright shoppingred'>" + AdditionalItems.Price + "</td>";
                                            TableDiv += "           <td class='fontright shoppingred'>" + AdditionalItems.Discount + "</td>";
                                            TableDiv += "           <td class='fontright shoppingred'>";
                                            if (Convert.ToInt32(AdditionalItems.Discount) != 0)
                                            {
                                                TableDiv += "           <s style='font-size:9pt; color:#bbbbbb; padding-right:3px;'>" + (Convert.ToInt32(AdditionalItems.Qty) * Convert.ToInt32(AdditionalItems.Price)) + "</s>";
                                            }

                                            TableDiv += ((Convert.ToInt32(AdditionalItems.Price) * Convert.ToInt32(AdditionalItems.Qty)) - Convert.ToInt32(AdditionalItems.Discount)) + "</td>";
                                        }
                                        TableDiv += "       </tr>";
                                        int amt = Convert.ToInt32(AdditionalItems.Price) * Convert.ToInt32(AdditionalItems.Qty) - Convert.ToInt32(AdditionalItems.Discount);
                                        listPrice += amt;
                                        totalamt += amt;
                                    }
                                    #endregion
                                }
                                if (rlib.OrderData.ShopType != 4)
                                {
                                    if (disablePrice == "S")
                                    {
                                        TableDiv += "       <tr>";
                                        TableDiv += "           <td colspan='100%' class='shoppingline' style='font-size:0;text-align: right;'>";
                                        TableDiv += "               <div style='width:90%; display:inline-block;font-size:1rem;'><b>" + Orders.Title + "</b>【合計】</div>";
                                        TableDiv += "               <div style='width:10%; display:inline-block;font-size:1rem;' class='shoppingred'>" + listPrice + "</div>";
                                        TableDiv += "           </td>";
                                        TableDiv += "       </tr>";
                                    }
                                    if (rlib.OrderData.servicePriceType == "L")
                                    {
                                        TableDiv += $@"<tr>
                                        <td colspan='100%' class='shoppingline' style='font-size:0;text-align: right;'>
                                            <div style='width:90%; display:inline-block;font-size:1rem;'>
                                                <b>{GetLocalResourceObject("servicePrice")}</b>：
                                            </div>
                                            <div style='width:10%; display:inline-block;font-size:1rem;' class='shoppingred'>
                                                {rlib.OrderData.servicePrice}
                                            </div>
                                        </td>
                                    </tr>";
                                    }
                                }
                                #region 2015/1/15 此項目改為單項商品總折扣，怕重複折扣，移除此項目
                                /*
                        if (Convert.ToInt32(Orders.Discount) > 0 || Convert.ToInt32(Orders.Discount) < 0)
                        {
                            TableDiv += "       <tr>";
                            TableDiv += "           <td style='padding-left: 20px;'>" + GetLocalResourceObject("活動折扣") + "</td>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td align='center'></td>";
                            TableDiv += "           <td class='fontright shoppingred'></td>";
                            TableDiv += "           <td class='fontright shoppingred'></td>";
                            TableDiv += "           <td class='fontright shoppingred'>" + Convert.ToInt32(Orders.Discount) + "</td>";
                            TableDiv += "       </tr>";
                            totalamt = totalamt + Convert.ToInt32(Orders.Discount);
                        }     */
                                #endregion
                            }
                            #endregion
                        }

                        TableDiv += "   </tbody>";
                        TableDiv += "</table>";

                        double discount_range = 0;
                        double discount_price = 0;
                        double DiscounAmt = 0;
                        int discountType = 0;

                        #region 購物車金額
                        using (SqlConnection conn = new SqlConnection(setting))
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand("select isnull(discount_range,0),isnull(discount_price,0),isnull(discountTyp,0) from CurrentUseHead", conn);
                            SqlDataReader reader = cmd.ExecuteReader();
                            try
                            {
                                while (reader.Read())
                                {
                                    discount_range = Convert.ToInt32(reader[0].ToString());
                                    discount_price = Convert.ToInt32(reader[1].ToString());
                                    discountType = Convert.ToInt32(reader[2].ToString());
                                }
                            }
                            finally
                            {
                                reader.Close();
                            }
                        }

                        TableDiv += $"<table class='table{(rlib.OrderData.ShopType == 4 ? " hide" : "")}'>";
                        TableDiv += "   <tbody>";
                        string disablePriceCss = "";
                        if (disablePrice == "Y") disablePriceCss = " class='hide'";
                        if (discount_range > 0)
                        {
                            TableDiv += "       <tr" + disablePriceCss + ">";
                            TableDiv += "           <td width='90%' class='fontright shoppingline' style='border-top:none; border-bottom:none; padding:3px 0px'>" + GetLocalResourceObject("StringResource10") + "：";
                            if (rlib.OrderData.deliveryDate != "")
                            {
                                TableDiv += "           <span class='pull-left'>" + GetLocalResourceObject("StringResource55") + ":" + rlib.OrderData.deliveryDate + "</span>";
                            }
                            TableDiv += "           </td>";
                            TableDiv += "           <td width='10%' class='shoppingred fontright shoppingline' style='border-top:none; border-bottom:none; padding:3px 8px'>" + totalamt + "</td>";
                            TableDiv += "       </tr>";
                            if (rlib.OrderData.servicePriceSum > 0)
                            {
                                TableDiv += $@"<tr{disablePriceCss}>
                                    <td width='90%' class='fontright shoppingline' style='border-top:none; border-bottom:none; padding:3px 0px'>
                                        {((rlib.OrderData.servicePriceType=="L")? GetLocalResourceObject("servicePriceSum"):GetLocalResourceObject("servicePrice"))}：
                                    </td>
                                    <td width='10%' class='shoppingred fontright shoppingline' style='border-top:none; border-bottom:none; padding:3px 8px'>
                                        {rlib.OrderData.servicePriceSum}
                                    </td>
                                </tr>";
                            }
                            if (discountType == 0)
                                DiscounAmt = Math.Floor((Convert.ToDouble(totalamt)+ rlib.OrderData.servicePriceSum) / discount_range) * discount_price;
                            else
                            {
                                if (totalamt >= discount_range) DiscounAmt = (totalamt+ rlib.OrderData.servicePriceSum) - Math.Round((totalamt+ rlib.OrderData.servicePriceSum) * (discount_price / Math.Pow(10, discount_price.ToString().Length)));
                                else DiscounAmt = 0;
                            }
                            this.discount_amt.Value = DiscounAmt.ToString();
                            totalamt = totalamt - Convert.ToInt32(DiscounAmt);
                            TableDiv += "       <tr" + disablePriceCss + ">";
                            TableDiv += "           <td width='90%' class='fontright shoppingline' style='border-top:none; border-bottom:none; padding:3px 0px'>" + GetLocalResourceObject("StringResource11") + discount_range + (discountType == 0 ? GetLocalResourceObject("StringResource12") : GetLocalResourceObject("StringResource58")) + discount_price + (discountType == 0 ? GetLocalResourceObject("StringResource13") : GetLocalResourceObject("StringResource59")) + "</td>";
                            TableDiv += "           <td width='10%' class='shoppingred fontright shoppingline' style='border-top:none; border-bottom:none; padding:3px 8px'>-" + DiscounAmt + "</td>";
                            TableDiv += "       </tr>";
                        }
                        else
                        {
                            this.discount_amt.Value = "0";
                            TableDiv += "       <tr" + disablePriceCss + ">";
                            TableDiv += "           <td width='90%' class='fontright shoppingline' style='border-top:none; border-bottom:none; padding:3px 0px'>" + GetLocalResourceObject("StringResource10") + "：";
                            if (rlib.OrderData.deliveryDate != "")
                            {
                                TableDiv += "           <span class='pull-left'>" + GetLocalResourceObject("StringResource55") + ":" + rlib.OrderData.deliveryDate + "</span>";
                            }
                            TableDiv += "           </td>";
                            TableDiv += "           <td width='10%' class='shoppingred fontright shoppingline' style='border-top:none; border-bottom:none; padding:3px 8px'>" + totalamt + "</td>";
                            TableDiv += "       </tr>";
                            if (rlib.OrderData.servicePriceSum > 0)
                            {
                                TableDiv += $@"<tr{disablePriceCss}>
                                    <td width='90%' class='fontright shoppingline' style='border-top:none; border-bottom:none; padding:3px 0px'>
                                        {GetLocalResourceObject("servicePriceSum")}：
                                    </td>
                                    <td width='10%' class='shoppingred fontright shoppingline' style='border-top:none; border-bottom:none; padding:3px 8px'>
                                        {rlib.OrderData.servicePriceSum}
                                    </td>
                                </tr>";
                            }
                        }
                        if (discont != 0)
                        {
                            TableDiv += "       <tr" + disablePriceCss + ">";
                            TableDiv += "           <td class='fontright' valign='middle' style='padding:3px 0px'>" + "優惠券(" + couponTitle + ")折抵" + "：</td>";
                            TableDiv += "           <td class='shoppingred fontright' style='padding:3px 8px;'>-" + discont + "</td>";
                            TableDiv += "       </tr>";
                            totalamt -= discont;
                            couDiscont = discont;
                            rlib.OrderData.couDiscont = discont;
                        }
                        if (person > 0)
                        {
                            couDiscont = totalamt - (int)Math.Floor((totalamt * person) / 100.0);
                            TableDiv += "       <tr" + disablePriceCss + ">";
                            TableDiv += "           <td class='fontright' valign='middle' style='padding:3px 0px'>" + "優惠券(" + couponTitle + ")折抵" + "：</td>";
                            TableDiv += "           <td class='shoppingred fontright' style='padding:3px 8px;'>-" + couDiscont + "</td>";
                            TableDiv += "       </tr>";
                            totalamt -= couDiscont;
                            rlib.OrderData.couDiscont = couDiscont;
                        }

                        if (Convert.ToInt32(rlib.OrderData.BonusDiscount) > 0)
                        {
                            TableDiv += "       <tr" + disablePriceCss + ">";
                            TableDiv += "           <td class='fontright' style='border-top:none; padding:3px 0px'>" + GetLocalResourceObject("StringResource14") + "：</td>";
                            TableDiv += "           <td class='shoppingred fontright' style='border-top:none; padding:3px 8px;'>-" + rlib.OrderData.BonusDiscount + "</td>";
                            TableDiv += "       </tr>";
                            if (!rlib.OrderData.allVirtualProd)
                            {
                                TableDiv += "       <tr>";
                                TableDiv += "           <td class='fontright' style='border-top:none; padding:3px 0px'>" + GetLocalResourceObject("StringResource15") + "：</td>";
                                TableDiv += "           <td class='shoppingred fontright' style='border-top:none; padding:3px 8px'>" + rlib.OrderData.FreightAmount + "</td>";
                                TableDiv += "       </tr>";
                            }
                            int t = totalamt - Convert.ToInt32(rlib.OrderData.BonusDiscount) + rlib.OrderData.servicePriceSum;
                            if (t < 0) t = rlib.OrderData.FreightAmount;
                            else t += rlib.OrderData.FreightAmount;
                            TableDiv += "       <tr>";
                            TableDiv += "           <td class='fontright' width='90%' valign='middle' style='border-top:1px solid #d4d4d4; padding:10px 0px'>" + GetLocalResourceObject("StringResource16") + "：</td>";
                            TableDiv += "           <td class='shoppingred fontright' style='border-top:1px solid #d4d4d4; padding:3px 8px; font-size:x-large;'>" + t + "</td>";
                            TableDiv += "       </tr>";
                        }
                        else
                        {
                            if (!rlib.OrderData.allVirtualProd)
                            {
                                TableDiv += "       <tr>";
                                TableDiv += "           <td class='fontright' style='border-top:none; padding:3px 0px'>" + GetLocalResourceObject("StringResource15") + "：</td>";
                                TableDiv += "           <td class='shoppingred fontright' style='border-top:none; padding:3px 8px'>" + rlib.OrderData.FreightAmount + "</td>";
                                TableDiv += "       </tr>";
                            }
                            int t = totalamt + rlib.OrderData.servicePriceSum;
                            if (t < 0) t = rlib.OrderData.FreightAmount;
                            else t += rlib.OrderData.FreightAmount;
                            TableDiv += "       <tr>";
                            TableDiv += "           <td class='fontright' width='90%' valign='middle' style='border-top:1px solid #d4d4d4; padding:10px 0px'>" + GetLocalResourceObject("StringResource16") + "：</td>";
                            TableDiv += "           <td class='shoppingred fontright' style='border-top:1px solid #d4d4d4; padding:3px 8px; font-size:x-large;'>" + t + "</td>";
                            TableDiv += "       </tr>";
                        }
                        TableDiv += "   </tbody>";
                        TableDiv += "</table>";
                        TableDiv += "</div>";
                        TableDiv += "</div>";

                        if (rlib.OrderData.ShopType != 4)
                        {
                            TableDiv += "<div class='row-fluid'>";
                            if (rlib.OrderData.MemID != "" && rlib.OrderData.BonusAmt != 0)
                            {
                                TableDiv += "   <div class='col-md-12' style='padding:5px 5px; border-top:1px solid #D4D4D4;'>";
                                TableDiv += GetLocalResourceObject("StringResource17") + "：<font class='shoppingred'>" + rlib.OrderData.BonusAmt + "</font>";
                                TableDiv += "   </div>";
                            }
                            TableDiv += "</div>";
                            TableDiv += "<div class='row-fluid'>";
                            TableDiv += "   <div class='col-md-12' style='padding:5px 5px; border-bottom:1px solid #D4D4D4; font-size:larger; font-weight:bold;'>";
                            TableDiv += GetLocalResourceObject("StringResource18") + "：" + GS.GetPayType(setting,rlib.OrderData.PayType);

                            /*歐付寶綁定信用卡付款
                            if (rlib.OrderData.PayType == "Credit" && rlib.OrderData.MemID != "")
                            {
                                bindingcredit.Visible = true;
                                String MerchantID = "2000214";
                                String MerchantMemberID = rlib.OrderData.MemID;
                                String HashKey = "5294y06JbISpM5x9";
                                String HashIV = "v77hoKGq4kWxNNIS";
                                String TradePostUrl = "https://payment-stage.allpay.com.tw/MerchantMember/QueryMemberBinding";

                                GetCarNO(MerchantID, MerchantMemberID, HashKey, HashIV, TradePostUrl);
                            }
                            else
                            {
                                bindingcredit.Visible = false;
                            }
                             * */
                            TableDiv += "   </div>";
                            TableDiv += "</div>";
                        }
                        #endregion
                        jsonStr.Value = JsonConvert.SerializeObject(rlib);
                        #region 快速結帳
                        if (rlib.OrderData.QuickPay == "Y" && rlib.OrderData.MemID != "")
                        {
                            this.name.Text = this.o_name.Text;
                            this.tel.Text = this.o_tel.Text;
                            this.cell.Text = this.o_cell.Text;
                            this.sex.SelectedIndex = this.o_sex.SelectedIndex;
                            if (this.address != null)
                            {
                                this.address.Text = this.o_addr.Value;
                            }
                            LinkButton1_Click(sender, e);
                        }
                        else
                        {
                            shoppingcar.InnerHtml = HttpUtility.HtmlDecode(Server.HtmlDecode(Microsoft.Security.Application.Encoder.HtmlEncode(TableDiv)));
                        }
                        #endregion
                    }
                    else
                    {
                        Response.Write("<script type='text/javascript'>alert('" + GetLocalResourceObject("StringResource19") + "');window.location.href='" + rlib.OrderData.ErrorUrl + "';</script>");
                    }

                }
                else
                {
                    Response.Write("<script type='text/javascript'>alert('" + GetLocalResourceObject("StringResource19") + "');window.location.href='" + rlib.OrderData.ErrorUrl + "';</script>");
                }
            }
        }
        #endregion

        #region 執行優惠券
        private void couponExe(int activeType, int discount, string gift, string prodStoreNo)
        {
            switch (activeType)
            {
                case 1:
                    person = int.Parse(GS.Left(discount.ToString() + "00", 2));
                    break;
                case 2:
                    discont = discount;
                    break;
                case 3:
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(@"
                            select prod.*,prod_Stock.ser_no,prod_Stock.colorID,prod_Stock.sizeID,prod_color.title colorTitle,prod_size.title sizeTitle
                            from prod
                            left join prod_Stock on prod_Stock.prod_id=prod.id
                            left join prod_color on prod_color.id=prod_Stock.colorID
                            left join prod_size on prod_size.id=prod_Stock.sizeID
                            where prod.id=@id and prod_Stock.ser_no=@serNo
                        ", conn);
                        cmd.Parameters.Add(new SqlParameter("@id", gift));
                        cmd.Parameters.Add(new SqlParameter("@serNo", prodStoreNo));
                        SqlDataReader reader = null;
                        try
                        {
                            List<Shoppingcar.OrderItem> item = new List<Shoppingcar.OrderItem>();
                            reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                rlib.OrderData.OrderLists.Add(new Shoppingcar.OrderList
                                {
                                    ID = reader["sub_id"].ToString(),
                                    Title = "優惠券折抵",
                                    Type = -1,
                                    OrderItems = item
                                });
                                List<Shoppingcar.OrderSpec> spc = new List<Shoppingcar.OrderSpec>();
                                spc.Add(new Shoppingcar.OrderSpec
                                {
                                    Size = int.Parse(reader["sizeID"].ToString()),
                                    Color = int.Parse(reader["colorID"].ToString()),
                                    Qty = 1,
                                    Price = double.Parse(reader["value1"].ToString()),
                                    FinalPrice = 0,
                                    Discount = int.Parse(reader["value1"].ToString()),
                                    Bonus = 0
                                });
                                item.Add(new Shoppingcar.OrderItem
                                {
                                    ID = reader["id"].ToString(),
                                    Name = "優惠券贈送" + reader["title"].ToString(),
                                    PosNo = "",
                                    UseTime = "",
                                    UseDate = "",
                                    Virtual = "N",
                                    OrderSpecs = spc,
                                    AdditionalItems = new List<Shoppingcar.AdditionalItem>()
                                });
                            }
                        }
                        catch { }
                        finally
                        {
                            if (reader != null) reader.Close();
                        }
                    }
                    break;
            }
        }
        #endregion

        #region 取得歐付寶綁定卡號
        private void GetCarNO(String MerchantID, String MerchantMemberID, String HashKey, String HashIV, String TradePostUrl)
        {
            String CheckMacValue = HttpUtility.UrlEncode("HashKey=" + HashKey + "&MerchantID=" + MerchantID + "&MerchantMemberID=" + MerchantMemberID + "&HashIV=" + HashIV).ToLower();

            string param = "CheckMacValue=" + GetSHA256String(CheckMacValue) + "&MerchantID=" + MerchantID + "&MerchantMemberID=" + MerchantMemberID;
            DataTable dt = SendForm(TradePostUrl, param);


            String rMerchantID = "";
            String rMerchantMemberID = "";
            String rCount = "";
            String JSonData = "";
            String rCheckMacValue = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                switch (dt.Rows[i][0].ToString())
                {
                    case "MerchantID":
                        rMerchantID = dt.Rows[i][1].ToString();
                        break;
                    case "MerchantMemberID":
                        rMerchantMemberID = dt.Rows[i][1].ToString();
                        break;
                    case "Count":
                        rCount = dt.Rows[i][1].ToString();
                        break;
                    case "JSonData":
                        JSonData = dt.Rows[i][1].ToString();
                        break;
                    case "CheckMacValue":
                        rCheckMacValue = dt.Rows[i][1].ToString();
                        break;
                    default:
                        break;
                }
            }

            CheckMacValue = HttpUtility.UrlEncode("HashKey=" + HashKey + "&Count=" + rCount + "&JSonData=" + JSonData + "&MerchantID=" + MerchantID + "&MerchantMemberID=" + MerchantMemberID + "&HashIV=" + HashIV).ToLower();
            if (SHA256Check(CheckMacValue, rCheckMacValue))
            {

                if (Convert.ToInt16(rCount) >= 1)
                {
                    String StrText = "";
                    try
                    {
                        List<DLData> postf = JsonConvert.DeserializeObject<List<DLData>>(JSonData);
                        foreach (DLData DD in postf)
                        {
                            StrText = DD.Card6No.Substring(0, 4) + "-" + DD.Card6No.Substring(4, 2) + "xx-xxxx-" + DD.Card4No;
                            DropDownList2.Items.Remove(DropDownList1.Items.FindByValue(DD.CardID));
                            DropDownList2.Items.Add(new ListItem(StrText, DD.CardID));
                        }
                    }
                    catch
                    {
                        DLData postf = JsonConvert.DeserializeObject<DLData>(JSonData);
                        StrText = postf.Card6No.Substring(0, 4) + "-" + postf.Card6No.Substring(4, 2) + "xx-xxxx-" + postf.Card4No;
                        DropDownList2.Items.Remove(DropDownList1.Items.FindByValue(postf.CardID));
                        DropDownList2.Items.Add(new ListItem(StrText, postf.CardID));
                    }
                }
                else
                {
                    DropDownList2.Items.Clear();
                    DropDownList2.Items.Add(new ListItem("目前無綁定任何信用卡", ""));
                }

            }
            else
            {
                Response.Write("CheckMacValue Error");
            }
        }
        #endregion

        #region HttpWebRequest送出資料
        private DataTable SendForm(String TradePostUrl, String param)
        {
            byte[] bs = Encoding.ASCII.GetBytes(param);

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(TradePostUrl);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = bs.Length;
            string result = null;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }
            using (WebResponse wr = req.GetResponse())
            {
                StreamReader sr = new StreamReader(wr.GetResponseStream(), System.Text.Encoding.GetEncoding("Big5"));
                result = sr.ReadToEnd();
                sr.Close();
            }

            string[] RequestArray = result.Split('&');
            DataTable dt = new DataTable();
            DataRow workRow;
            DataColumn column1 = new DataColumn("ColumnName");
            DataColumn column2 = new DataColumn("Value");
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);

            for (int i = 0; i < RequestArray.Length; i++)
            {
                workRow = dt.NewRow();

                workRow["ColumnName"] = RequestArray[i].Split('=')[0].ToString();

                if (RequestArray[i].Split('=').Length > 1)
                {
                    workRow["Value"] = RequestArray[i].Split('=')[1].ToString();
                }
                else
                {
                    workRow["Value"] = "";
                }

                dt.Rows.Add(workRow);
            }
            return dt;
        }
        #endregion

        #region Dropdownlist資料
        public class DLData
        {
            public String CardID { get; set; }
            public String Card6No { get; set; }
            public String Card4No { get; set; }
            public String BindingDate { get; set; }
        }
        #endregion

        #region SHA256加密
        private String GetSHA256String(String s)
        {
            SHA256 md5Hasher = SHA256.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(s));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("X2"));
            }
            return sBuilder.ToString();
        }
        #endregion

        #region SHA256驗證
        public bool SHA256Check(String Str, String SHA256Str)
        {
            Str = GetSHA256String(Str);
            if (Str == SHA256Str)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region //與訂購者相同按鈕
        protected void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBox1.Checked)
            {
                this.name.Text = this.o_name.Text;
                this.tel.Text = this.o_tel.Text;
                this.cell.Text = this.o_cell.Text;
                this.sex.SelectedIndex = this.o_sex.SelectedIndex;
                if (this.address != null)
                {
                    this.address.Text = this.o_addr.Value;
                }
                if (CVSdiv.Visible)
                {
                    this.CVSStoreID.Text = this.CVSStoreID.Text;
                    this.CVSStoreName.Text = this.CVSStoreName.Text;
                    this.CVSTelephone.Text = this.CVSTelephone.Text;
                    this.CVSAddress.Text = this.CVSAddress.Text;
                }
            }
        }
        #endregion

        #region //確認庫存
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

        #region //抓會員POS ID

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

        #region //發送email
        public void send_email(string msg, string mysubject, string sender, string mail)
        {
            MailMessage message = new MailMessage();//MailMessage(寄信者, 收信者)
            message.From = new MailAddress(sender, GetLocalResourceObject("StringResource20").ToString());
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

        #region //加入會員
        private int addMember(string setting)
        {
            int check = 0;
            if (rlib.OrderData.MemID == "" && this.CheckBox3.Checked)
            {
                string MemID = "";
                #region //檢查會員是否存在
                if (check == 0)
                {
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd;

                        cmd = new SqlCommand("select mem_id from cust where email=@email", conn);
                        cmd.Parameters.Add(new SqlParameter("@email", this.mail.Text));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                check = 1;
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                }
                #endregion

                #region //新增會員
                if (check == 0)
                {
                    #region //建立memID
                    using (SqlConnection conn2 = new SqlConnection(setting))
                    {
                        conn2.Open();
                        SqlCommand cmd2 = new SqlCommand("select isnull(max(mem_id),'') from cust", conn2);
                        SqlDataReader reader2 = cmd2.ExecuteReader();
                        try
                        {
                            while (reader2.Read())
                            {
                                if (reader2[0].ToString() != "")
                                {
                                    MemID = (Convert.ToInt16(reader2[0].ToString()) + 1).ToString().PadLeft(6, '0');
                                }
                                else
                                {
                                    MemID = "000001";
                                }
                            }
                        }
                        finally
                        {
                            reader2.Close();
                        }
                    }
                    #endregion
                    #region //發送會員通知信
                    if (MemID != "")
                    {
                        using (SqlConnection conn = new SqlConnection(setting))
                        {
                            try
                            {
                                conn.Open();
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "sp_NewMember2";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = conn;

                                cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                                cmd.Parameters.Add(new SqlParameter("@id", this.mail.Text));
                                cmd.Parameters.Add(new SqlParameter("@pwd", this.password.Text));
                                cmd.Parameters.Add(new SqlParameter("@ch_name", this.o_name.Text));
                                cmd.Parameters.Add(new SqlParameter("@sex", this.o_sex.SelectedItem.Value));
                                cmd.Parameters.Add(new SqlParameter("@email", this.mail.Text));
                                cmd.Parameters.Add(new SqlParameter("@birth", ""));
                                cmd.Parameters.Add(new SqlParameter("@tel", this.o_tel.Text));
                                cmd.Parameters.Add(new SqlParameter("@cell_phone", this.o_cell.Text));
                                cmd.Parameters.Add(new SqlParameter("@addr", ""));
                                cmd.Parameters.Add(new SqlParameter("@ident", ""));
                                cmd.Parameters.Add(new SqlParameter("@id2", ""));
                                cmd.Parameters.Add(new SqlParameter("@C_ZIP", ""));
                                cmd.Parameters.Add(new SqlParameter("@SnAndId", ""));
                                cmd.Parameters.Add(new SqlParameter("@chk", "O"));
                                cmd.ExecuteNonQuery();
                                rlib.OrderData.MemID = MemID;
                                sentMemberMail(setting, MemID);
                            }
                            catch
                            {
                                check = 3;
                            }
                        }
                    }
                    #endregion
                }
                #endregion
            }
            return check;
        }
        #endregion

        #region LinkButton1_Click
        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            GetStr GS = new GetStr();
            rlib = JsonConvert.DeserializeObject<Shoppingcar.RootObject>(this.jsonStr.Value);
            if (ChkJson(this.jsonStr.Value))
            {
                if ((Convert.ToInt32(Session["bonus"].ToString()) - Convert.ToInt32(Session["prodBonus"].ToString()) - Convert.ToInt32(rlib.OrderData.BonusDiscount)) >= 0)
                {
                    String OrgName = rlib.OrderData.OrgName;
                    String ErrorUrl = rlib.OrderData.ErrorUrl;
                    String Str_Error = "";
                    String setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
                    int ShopType = rlib.OrderData.ShopType;
                    String VerCode = "";
                    String LogisticsID = rlib.OrderData.LogisticsID;
                    if (!string.IsNullOrEmpty(rlib.OrderData.VCode))
                    {
                        if (!rlib.OrderData.doInsertCoupon)
                        {
                            ChechCouponData chechCouponData = chechCoupon(setting, rlib.OrderData.VCode, rlib.OrderData.GCode, rlib.OrderData.MemID);
                            if (chechCouponData == null)
                            {
                                Response.Write("<script type='text/javascript'>alert('" + "優惠已被領取完畢或優惠券已到期，請重新確認優惠券。" + "');window.location.href='" + rlib.OrderData.ErrorUrl + "';</script>");
                                Response.End();
                            }
                        }
                        else
                        {
                            int cint = 0;
                            ChechCouponData chechCouponData = chechCoupon(setting, null, rlib.OrderData.GCode, rlib.OrderData.MemID);
                            if (chechCouponData != null)
                            {
                                cint = insertCustCoupon(setting, chechCouponData.SerNo, chechCouponData.GCode, chechCouponData.VCode, chechCouponData.ExpireDate, chechCouponData.noType, rlib.OrderData.MemID);
                            }
                            if (cint == 0)
                            {
                                Response.Write("<script type='text/javascript'>alert('" + "優惠已被領取完畢或優惠券已到期，請重新確認優惠券。" + "');window.location.href='" + rlib.OrderData.ErrorUrl + "';</script>");
                                Response.End();
                            }
                        }
                    }
                    if (Str_Error == "")
                    {
                        int check = addMember(setting);
                        bool error = false;
                        switch (check)
                        {
                            case 1:
                                Response.Write("<script type='text/javascript'>alert('帳號已存在，請登入會員?'); window.location.href='" + rlib.OrderData.ReturnUrl + "/" + folder + "/toMember.asp?to=" + rlib.OrderData.ErrorUrl + "';</script> ");
                                error = true;
                                break;
                            case 3:
                                Response.Write("<script type='text/javascript'>alert('帳號開通信發送失敗。');</script> ");
                                break;
                        }
                        if (error) return;
                        if (ShopType != 3)
                        {
                            #region 檢查是否有產品
                            if (rlib.OrderData.OrderLists.Count < 1)    //是否有行銷活動
                            {
                                Response.Write("<script type='text/javascript'>alert('" + Str_Error + GetLocalResourceObject("StringResource21") + "');window.location.href='" + ErrorUrl + "';</script>");
                                Response.End();
                            }
                            else
                            {
                                //檢查所有行銷活動內是否有產品
                                int prod_count = 0;
                                foreach (Shoppingcar.OrderList Orders in rlib.OrderData.OrderLists)
                                {
                                    foreach (Shoppingcar.OrderItem Items in Orders.OrderItems)
                                    {
                                        foreach (Shoppingcar.OrderSpec Spec in Items.OrderSpecs)
                                        {
                                            prod_count = prod_count + Convert.ToInt32(Spec.Qty);

                                            if (Convert.ToInt32(Spec.Qty) <= 0)         //數量有誤
                                            {
                                                Response.Write("<script type='text/javascript'>alert('" + GetLocalResourceObject("StringResource52") + "');window.location.href='" + ErrorUrl + "';</script>");
                                                Response.End();
                                            }
                                        }
                                        foreach (Shoppingcar.AdditionalItem AddItems in Items.AdditionalItems)
                                        {
                                            if (Convert.ToInt32(AddItems.Qty) <= 0)         //數量有誤
                                            {
                                                Response.Write("<script type='text/javascript'>alert('" + GetLocalResourceObject("StringResource52") + "');window.location.href='" + ErrorUrl + "';</script>");
                                                Response.End();
                                            }
                                        }
                                    }
                                }
                                if (prod_count == 0)
                                {
                                    Response.Write("<script type='text/javascript'>alert('" + Str_Error + GetLocalResourceObject("StringResource21") + "');window.location.href='" + ErrorUrl + "';</script>");
                                    Response.End();
                                }
                            }
                            #endregion

                            Str_Error = ChkOrderStock(rlib.OrderData.OrderLists, setting);      //確認庫存
                        }
                        else
                        {
                            VerCode = rlib.OrderData.MenuLists.Vercode;
                        }
                        if (Str_Error != "")        //庫存不足
                        {
                            Response.Write("<script type='text/javascript'>alert('" + Str_Error + GetLocalResourceObject("StringResource22") + "');window.location.href='" + ErrorUrl + "';</script>");
                            Response.End();
                        }
                        else
                        {
                            String ReturnUrl = rlib.OrderData.ReturnUrl;
                            String OEmail = GS.ReplaceStr(this.mail.Text);
                            #region 20140606增加滿額折扣，將此折扣價直接新增至訂單折扣額
                            /*                
                    foreach (Shoppingcar.OrderList Orders in rlib.OrderData.OrderLists)
                    {
                        if (Convert.ToInt32(Orders.Discount) > 0 || Convert.ToInt32(Orders.Discount) < 0)
                        {
                            DiscountAmt += Convert.ToInt32(Orders.Discount) * (-1);
                        }
                    }
                    */
                            #endregion

                            String OrderID = GetOrderID(setting);
                            if (OrderID != "")
                            {
                                FormColumns f = new FormColumns();
                                bool SendMail = SaveOrder(setting, OrderID, rlib, f);      //儲存訂單&&回傳是否要寄信
                                SqlCommand cmd;
                                #region 註記點餐桌號
                                String BookingID = "";

                                using (SqlConnection conn = new SqlConnection(setting))
                                {
                                    conn.Open();
                                    cmd = new SqlCommand("select storetype from head", conn);
                                    SqlDataReader reader = cmd.ExecuteReader();
                                    try
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader[0].ToString() == "1")
                                            {
                                                String StoreName = "";
                                                using (SqlConnection conn2 = new SqlConnection(setting))
                                                {
                                                    conn2.Open();
                                                    SqlCommand cmd3 = new SqlCommand("select title from bookingstore where id=@id", conn2);
                                                    cmd3.Parameters.Add(new SqlParameter("@id", DropDownList1.SelectedValue));
                                                    SqlDataReader reader2 = cmd3.ExecuteReader();
                                                    try
                                                    {
                                                        while (reader2.Read())
                                                        {
                                                            StoreName = reader2[0].ToString();
                                                        }
                                                    }
                                                    finally
                                                    {
                                                        reader2.Close();
                                                    }
                                                }

                                                using (SqlConnection conn2 = new SqlConnection(setting))
                                                {
                                                    conn2.Open();
                                                    SqlCommand cmd2 = new SqlCommand();
                                                    cmd2.CommandText = "sp_order_table";
                                                    cmd2.CommandType = CommandType.StoredProcedure;
                                                    cmd2.Connection = conn2;
                                                    cmd2.Parameters.Add(new SqlParameter("@orderID", OrderID));
                                                    cmd2.Parameters.Add(new SqlParameter("@tableID", ""));
                                                    cmd2.Parameters.Add(new SqlParameter("@vercode", ""));
                                                    cmd2.Parameters.Add(new SqlParameter("@TakeMealType", ""));
                                                    cmd2.Parameters.Add(new SqlParameter("@shopID", DropDownList1.SelectedValue));
                                                    cmd2.Parameters.Add(new SqlParameter("@shopName", StoreName));
                                                    SqlParameter SPOutput = cmd2.Parameters.Add("@Bid", SqlDbType.NVarChar, 12);
                                                    SPOutput.Direction = ParameterDirection.Output;
                                                    try
                                                    {
                                                        cmd2.ExecuteNonQuery();
                                                        BookingID = SPOutput.Value.ToString();
                                                    }
                                                    catch
                                                    {
                                                        BookingID = "";
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


                                #endregion

                                #region 取得寄信相關資料

                                String service_mail = "";
                                String title = "";
                                String MerchantID = string.Empty;
                                String HashKey = string.Empty;
                                String HashIV = string.Empty;
                                String SupplierMail = string.Empty;
                                String FatherMail = string.Empty;
                                String WebID = string.Empty;

                                using (SqlConnection conn = new SqlConnection(setting))
                                {
                                    conn.Open();
                                    cmd = new SqlCommand("select b.service_mail,b.title,b.HashIv,b.HashKey,b.mer_id,b.supplier_mail,b.id from CurrentUseFrame as a left join head as b on a.id=b.hid", conn);
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
                                            title = reader[1].ToString();
                                            MerchantID = reader[4].ToString();
                                            HashKey = reader[3].ToString();
                                            HashIV = reader[2].ToString();
                                            SupplierMail = reader[5].ToString();
                                            WebID = reader[6].ToString();
                                        }
                                    }
                                    finally
                                    {
                                        reader.Close();
                                    }
                                }

                                #region 是否為子站
                                if (SupplierMail != "")
                                {
                                    //select b.id from cocker_cust as a left join cocker_cust as b on a.fid=b.id where a.id=516
                                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
                                    {
                                        conn.Open();
                                        SqlCommand cmd2 = new SqlCommand("select b.id from cocker_cust as a left join cocker_cust as b on a.fid=b.id where a.id=@id", conn);

                                        cmd2.Parameters.Add(new SqlParameter("@id", WebID));
                                        SqlDataReader reader = cmd2.ExecuteReader();
                                        try
                                        {
                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    using (SqlConnection conn3 = new SqlConnection(GS.GetSetting(reader[0].ToString())))
                                                    {
                                                        conn3.Open();
                                                        SqlCommand cmd3 = new SqlCommand("select service_mail from head", conn3);
                                                        SqlDataReader reader2 = cmd3.ExecuteReader();
                                                        try
                                                        {
                                                            if (reader2.HasRows)
                                                            {
                                                                while (reader2.Read())
                                                                {
                                                                    FatherMail = reader2[0].ToString();
                                                                }
                                                            }
                                                        }
                                                        finally
                                                        {
                                                            reader2.Close();
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
                                #endregion
                                #endregion



                                if (SendMail)
                                {
                                    String mail_cont = GetMailCont(setting, OrderID, BookingID,f);
                                    if (OEmail == "") OEmail = service_mail;
                                    if (OEmail != "") send_email(mail_cont, GetLocalResourceObject("StringResource51") + " 【" + title + "】", service_mail, OEmail);//呼叫send_email函式測試      
                                    #region 子站補寄Email給供應商和母站管理者 20161021取消發信給廠商
                                    //if (SupplierMail != "")
                                    //{
                                    //    send_email(mail_cont, GetLocalResourceObject("StringResource51") + " 【" + title + "】", FatherMail, SupplierMail);//呼叫send_email函式測試    
                                    //}
                                    #endregion

                                }
                                #region 歐付寶綁定付款
                                if (rlib.OrderData.PayType == "Credit" && rlib.OrderData.MemID != "")
                                {
                                    if (DropDownList2.SelectedValue != "")
                                    {
                                        AllPayBinding(MerchantID, "網路購物", HashKey, HashIV, "https://payment-stage.allpay.com.tw/MerchantMember/AuthCardID", setting, OrderID, rlib.OrderData.MemID, service_mail, title);
                                    }
                                }
                                #endregion


                                Response.Write("<script type='text/javascript'>window.location.href='" + ReturnUrl + "/" + GS.GetLanString(str_language) + "/shop.asp?id=" + OrderID + "&VerCode=" + VerCode + "&LogisticsID=" + LogisticsID + "';</script>");
                            }
                            else
                            {
                                Response.Write($"<script type='text/javascript'>alert('{GetLocalResourceObject("StringResource19")}');window.location.href='{ErrorUrl}';</script>");
                                Response.End();
                            }
                        }
                    }
                }
                else
                {
                    Response.Write($"<script type='text/javascript'>alert('{GetLocalResourceObject("StringResource19")}');window.location.href='{rlib.OrderData.ErrorUrl}';</script>");
                }
            }
            else
            {
                Response.Write($"<script type='text/javascript'>alert('{GetLocalResourceObject("StringResource19")}');window.location.href='{rlib.OrderData.ErrorUrl}';</script>");
            }

        }
        #endregion

        #region 歐付寶綁定信用卡結帳
        private void AllPayBinding(String MerchantID, String TradeDesc, String HashKey, String HashIV, String TradePostUrl, String setting, String MerchantTradeNo, String MemberID, String service_mail, String title)
        {
            String CardID = DropDownList2.SelectedValue;
            String mail = string.Empty;
            String BonusAmt = "0";
            String BonusDiscount = "0";
            String TotalAmount = "0";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select mail,isnull(bonus_amt,'') as bonus_amt,isnull(bonus_discount,'') as bonus_discount,convert(int,amt)+convert(int,freightamount)-convert(int,bonus_discount)-convert(int,discount_amt) from orders_hd where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@id", MerchantTradeNo));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            mail = reader[0].ToString();
                            if (mail == "")
                            {
                                mail = service_mail;
                            }
                            BonusAmt = reader[1].ToString();
                            if (BonusAmt == "")
                            {
                                BonusAmt = "0";
                            }
                            BonusDiscount = reader[2].ToString();
                            if (BonusDiscount == "")
                            {
                                BonusDiscount = "0";
                            }
                            TotalAmount = reader[3].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            String MerchantTradeDate = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
            String stage = "0";
            String CheckMacValue = HttpUtility.UrlEncode("HashKey=" + HashKey + "&CardID=" + CardID + "&MerchantID=" + MerchantID + "&MerchantTradeDate=" + MerchantTradeDate + "&MerchantTradeNo=" + MerchantTradeNo + "&stage=" + stage + "&TotalAmount=" + TotalAmount + "&TradeDesc=" + TradeDesc + "&HashIV=" + HashIV).ToLower();
            string param = "CheckMacValue=" + GetSHA256String(CheckMacValue) + "&CardID=" + CardID + "&MerchantID=" + MerchantID + "&MerchantTradeDate=" + MerchantTradeDate + "&MerchantTradeNo=" + MerchantTradeNo + "&stage=" + stage + "&TotalAmount=" + TotalAmount + "&TradeDesc=" + HttpUtility.UrlEncode(TradeDesc);

            DataTable dt = SendForm(TradePostUrl, param);

            String rRtnCode = "";
            String rRtnMsg = "";
            String rMerchantID = "";
            String rMerchantTradeNo = "";
            String rAllpayTradeNo = "";
            String rgwsr = "";
            String rprocess_date = "";
            String rauth_code = "";
            String ramount = "";
            String rcard6no = "";
            String rcard4no = "";
            String rstage = "";
            String rstast = "";
            String rstaed = "";
            String reci = "";
            String rCheckMacValue = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                switch (dt.Rows[i][0].ToString())
                {
                    case "AllpayTradeNo":
                        rAllpayTradeNo = dt.Rows[i][1].ToString();
                        break;
                    case "amount":
                        ramount = dt.Rows[i][1].ToString();
                        break;
                    case "auth_code":
                        rauth_code = dt.Rows[i][1].ToString();
                        break;
                    case "card4no":
                        rcard4no = dt.Rows[i][1].ToString();
                        break;
                    case "card6no":
                        rcard6no = dt.Rows[i][1].ToString();
                        break;
                    case "eci":
                        reci = dt.Rows[i][1].ToString();
                        break;
                    case "gwsr":
                        rgwsr = dt.Rows[i][1].ToString();
                        break;
                    case "MerchantID":
                        rMerchantID = dt.Rows[i][1].ToString();
                        break;
                    case "MerchantTradeNo":
                        rMerchantTradeNo = dt.Rows[i][1].ToString();
                        break;
                    case "process_date":
                        rprocess_date = dt.Rows[i][1].ToString();
                        break;
                    case "RtnCode":
                        rRtnCode = dt.Rows[i][1].ToString();
                        break;
                    case "RtnMsg":
                        rRtnMsg = dt.Rows[i][1].ToString();
                        break;
                    case "staed":
                        rstaed = dt.Rows[i][1].ToString();
                        break;
                    case "stage":
                        rstage = dt.Rows[i][1].ToString();
                        break;
                    case "stast":
                        rstast = dt.Rows[i][1].ToString();
                        break;
                    case "CheckMacValue":
                        rCheckMacValue = dt.Rows[i][1].ToString();
                        break;
                    default:
                        break;
                }
            }

            CheckMacValue = HttpUtility.UrlEncode("HashKey=" + HashKey + "&AllpayTradeNo=" + rAllpayTradeNo + "&amount=" + ramount +
                "&auth_code=" + rauth_code + "&card4no=" + rcard4no + "&card6no=" + rcard6no + "&eci=" + reci + "&gwsr=" + rgwsr +
                "&MerchantID=" + rMerchantID + "&MerchantTradeNo=" + rMerchantTradeNo + "&process_date=" + rprocess_date +
                "&RtnCode=" + rRtnCode + "&RtnMsg=" + rRtnMsg + "&staed=" + rstaed + "&stage=" + rstage + "&stast=" + rstast +
                "&HashIV=" + HashIV).ToLower();

            if (SHA256Check(CheckMacValue, rCheckMacValue))
            {
                String rRtn = "";
                if (rRtnCode == "1")
                {
                    rRtn = "授權成功";

                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("update orders_hd set state='2',paid_date=@paid_date,crm_date=replace(replace(CONVERT([nvarchar](256),getdate(),(120)),'-',''),':','') where id=@id", conn);
                        cmd.Parameters.Add(new SqlParameter("@paid_date", MerchantTradeDate.Substring(0, 10)));
                        cmd.Parameters.Add(new SqlParameter("@id", MerchantTradeNo));
                        cmd.ExecuteNonQuery();
                    }

                    #region 修改會員紅利及虛擬商品期限
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();

                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandText = "sp_UpdateOrder";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn;
                        cmd.Parameters.Add(new SqlParameter("@orderID", MerchantTradeNo));
                        cmd.Parameters.Add(new SqlParameter("@bonus_memo", MerchantTradeNo + "付款"));
                        cmd.Parameters.Add(new SqlParameter("@mem_id", MemberID));
                        cmd.Parameters.Add(new SqlParameter("@bonus_add", BonusAmt));
                        cmd.Parameters.Add(new SqlParameter("@user_id", "guest"));
                        cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                        cmd.Parameters.Add(new SqlParameter("@filename", "shopping.aspx"));
                        cmd.Parameters.Add(new SqlParameter("@type", "1"));
                        cmd.Parameters.Add(new SqlParameter("@bonus_spend", "0"));
                        cmd.Parameters.Add(new SqlParameter("@bonus_total_add", BonusAmt));
                        cmd.ExecuteNonQuery();
                    }
                    #endregion


                    String mail_cont = "您好，您於" + MerchantTradeDate + "消費" + TotalAmount + "元，付款成功。";
                    send_email(mail_cont, "交易成功！ 【" + title + "】", service_mail, mail);//呼叫send_email函式測試

                }
                else
                {
                    rRtn = "授權失敗";
                }
            }
            else
            {
                Response.Write("CheckMacValue Error");
            }
        }
        #endregion

        #region 取得IP
        private string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string sIPAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(sIPAddress))
            {
                return context.Request.ServerVariables["REMOTE_ADDR"];
            }
            else
            {
                string[] ipArray = sIPAddress.Split(new Char[] { ',' });
                return ipArray[0];
            }
        }
        #endregion

        #region 驗證json
        private bool ChkJson(String JsonStr)
        {

            GetStr GS = new GetStr();

            Shoppingcar.RootObject rlib = JsonConvert.DeserializeObject<Shoppingcar.RootObject>(JsonStr);
            Int32 ProdBonus = 0;
            String ChkStr = rlib.OrderData.OrgName;
            ChkStr += rlib.OrderData.PayType;
            ChkStr += rlib.OrderData.FreightAmount.ToString();
            ChkStr += rlib.OrderData.BonusDiscount;
            ChkStr += rlib.OrderData.BonusAmt;

            if (rlib.OrderData.ShopType == 3)
            {
                Shoppingcar.MenuLists MenuList = rlib.OrderData.MenuLists;

                foreach (Shoppingcar.Menu Menu in MenuList.Menu)
                {
                    foreach (Shoppingcar.MenuItem MenuItem in Menu.MenuItems)
                    {
                        ChkStr += MenuItem.ID;
                    }
                }
            }
            else
            {
                foreach (Shoppingcar.OrderList Orders in rlib.OrderData.OrderLists)
                {
                    if (Orders.Type != -1)
                    {
                        foreach (Shoppingcar.OrderItem OrderItems in Orders.OrderItems)
                        {
                            foreach (Shoppingcar.OrderSpec OrderSpecs in OrderItems.OrderSpecs)
                            {
                                ChkStr += OrderSpecs.FinalPrice;
                                ProdBonus += Convert.ToInt32(OrderSpecs.Bonus);
                            }
                            foreach (Shoppingcar.AdditionalItem AdditionalItems in OrderItems.AdditionalItems)
                            {
                                ChkStr += AdditionalItems.FinalPrice;
                            }
                        }
                    }
                }
            }
            Session["prodBonus"] = ProdBonus.ToString();
            if (GS.MD5Endode(ChkStr) == rlib.OrderData.Checkm)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 取得點燈控制項內容
        private Shoppingcar.LightRootObject GetLightData()
        {
            Shoppingcar.LightRootObject Lightroot = new Shoppingcar.LightRootObject();
            List<Shoppingcar.LightItem> LightItem = new List<Shoppingcar.LightItem>();
            List<Shoppingcar.LightData> LightData = new List<Shoppingcar.LightData>();
            List<Shoppingcar.ErrorMsg> ErrorMsg = new List<Shoppingcar.ErrorMsg>();

            String ProdID = "";
            String Name = "";
            String Addr = "";
            String Birth = "";
            String Hour = "";
            String Tel = "";
            String CellPhone = "";
            Regex rgx;

            #region 取得Table內所有控制項並產生Json格式
            if (Table1.HasControls())
            {
                Response.Write("tt");
            }
            foreach (TableRow tr in Table1.Rows)
            {
                foreach (TableCell tc in tr.Cells)
                {
                    Response.Write("Controls=" + tc.Controls.Count);
                    foreach (Control myctr in tc.Controls)
                    {

                        #region 取得HiddenField
                        if (myctr is HiddenField)
                        {
                            if (((HiddenField)myctr).ID.IndexOf("ProdID", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                if (((HiddenField)myctr).Value != "")
                                {
                                    ProdID = ((HiddenField)myctr).Value;
                                    LightData = new List<Shoppingcar.LightData>();
                                }
                                else
                                {
                                    Shoppingcar.ErrorMsg ErrorList = new Shoppingcar.ErrorMsg
                                    {
                                        Code = "9001",
                                        Msg = "商品編號不可為空"
                                    };
                                    ErrorMsg.Add(ErrorList);
                                }
                            }

                            if (((HiddenField)myctr).ID.IndexOf("Stop", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                Shoppingcar.LightItem ItemList = new Shoppingcar.LightItem
                                {
                                    prodid = ProdID,
                                    data = LightData
                                };
                                LightItem.Add(ItemList);
                            }
                        }
                        #endregion
                        #region 取得TextBox
                        if (myctr is TextBox)
                        {
                            if (((TextBox)myctr).Text != "")
                            {
                                if (((TextBox)myctr).ID.IndexOf("CHName", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    Name = ((TextBox)myctr).Text;
                                }

                                if (((TextBox)myctr).ID.IndexOf("Address", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    Addr = ((TextBox)myctr).Text;
                                }
                                if (((TextBox)myctr).ID.IndexOf("Birth", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    Birth = ((TextBox)myctr).Text;
                                }
                                if (((TextBox)myctr).ID.IndexOf("CellPhone", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    rgx = new Regex("^09\\d{8}$");
                                    CellPhone = ((TextBox)myctr).Text;
                                    if (!rgx.IsMatch(CellPhone))
                                    {
                                        Shoppingcar.ErrorMsg ErrorList = new Shoppingcar.ErrorMsg
                                        {
                                            Code = "9002",
                                            Msg = "手機電話格式錯誤"
                                        };
                                        ErrorMsg.Add(ErrorList);
                                    }
                                }
                            }
                            else
                            {
                                Shoppingcar.ErrorMsg ErrorList = new Shoppingcar.ErrorMsg
                                {
                                    Code = "9003",
                                    Msg = "前方有*號不可為空"
                                };
                                ErrorMsg.Add(ErrorList);
                            }

                            if (((TextBox)myctr).ID.IndexOf("Tel", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                if (((TextBox)myctr).Text != "")
                                {
                                    rgx = new Regex("^0\\d{7,9}$");
                                    Tel = ((TextBox)myctr).Text;
                                    if (!rgx.IsMatch(Tel))
                                    {
                                        Shoppingcar.ErrorMsg ErrorList = new Shoppingcar.ErrorMsg
                                        {
                                            Code = "9002",
                                            Msg = "住家/公司電話格式錯誤"
                                        };
                                        ErrorMsg.Add(ErrorList);
                                    }
                                }
                            }
                        }
                        #endregion
                        #region 取得DropDownList
                        if (myctr is DropDownList)
                        {
                            if (((DropDownList)myctr).ID.IndexOf("Hour", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                Hour = ((DropDownList)myctr).SelectedValue;

                                Shoppingcar.LightData DataList = new Shoppingcar.LightData
                                {
                                    name = Name,
                                    hour = Hour,
                                    birth = Birth,
                                    addr = Addr,
                                    tel = Tel,
                                    cellphone = CellPhone
                                };
                                LightData.Add(DataList);
                            }
                        }
                        #endregion
                    }
                }
            }
            Lightroot.Items = LightItem;
            Lightroot.Errormsg = ErrorMsg;

            //Response.Write(JsonConvert.SerializeObject(Lightroot));
            #endregion

            return Lightroot;
        }
        #endregion        

        #region 確認庫存
        private String ChkOrderStock(List<Shoppingcar.OrderList> Orderlists, String setting)
        {
            String Str_Error = "";
            foreach (Shoppingcar.OrderList Orders in Orderlists)
            {
                foreach (Shoppingcar.OrderItem Items in Orders.OrderItems)
                {
                    foreach (Shoppingcar.OrderSpec OrderSpecs in Items.OrderSpecs)
                    {
                        if (!CheckStock(setting, Items.ID, OrderSpecs.Size, OrderSpecs.Color, Convert.ToInt32(OrderSpecs.Qty)))
                        {
                            Str_Error += "【" + Items.Name + "】";
                        }
                    }
                }
            }
            return Str_Error;
        }
        #endregion

        #region 取得訂單編號
        private String GetOrderID(String setting)
        {
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
            return OrderID;
        }
        #endregion

        #region 儲存訂單
        private bool SaveOrder(String setting, String OrderID, Shoppingcar.RootObject rlib, FormColumns f)
        {
            bool SendMail = true;
            GetStr GS = new GetStr();
            #region 訂單變數
            int BonusAmt = rlib.OrderData.BonusAmt;
            String BonusDiscount = rlib.OrderData.BonusDiscount;
            int FreightAmount = rlib.OrderData.FreightAmount;
            String MemID = rlib.OrderData.MemID;
            String PayType = rlib.OrderData.PayType;
            String ReturnUrl = rlib.OrderData.ReturnUrl;
            String RID = rlib.OrderData.RID;
            String Click_ID = rlib.OrderData.Click_ID == null ? "" : rlib.OrderData.Click_ID;
            String deliveryDate = rlib.OrderData.deliveryDate;
            String GCode = rlib.OrderData.GCode;
            String affi_id = rlib.OrderData.affi_id == null ? "" : rlib.OrderData.affi_id;
            int CouponID = rlib.OrderData.CouponID;
            int couDiscont = rlib.OrderData.couDiscont;
            if (rlib.OrderData.allVirtualProd) FreightAmount = 0;

            Int32 DiscountAmt = Convert.ToInt32(GS.ReplaceStr(this.discount_amt.Value));
            String OName = "";
            String OTel = "";
            String OCell = "";
            String OSex = "";
            String OEmail = "";

            String SName = "";
            String STel = "";
            String SCell = "";
            String SSex = "";
            String City = "";
            String Country = "";
            String Zip = "";
            String Address = "";
            String Notememo = "";
            String ident = "";
            String invoice = "";
            String invoiceTitle = "";

            #endregion
            SqlCommand cmd;
            if (rlib.OrderData.ShopType == 3)
            {
                if (rlib.OrderData.MenuLists.TakeMealType != "3")
                {
                    OName = GS.ReplaceStr(this.h_o_name.Value);
                    OTel = GS.ReplaceStr(this.h_o_tel.Value);
                    OCell = GS.ReplaceStr(this.h_o_cell.Value);
                    OSex = GS.ReplaceStr(this.h_o_sex.Value);
                    OEmail = GS.ReplaceStr(this.h_mail.Value);

                    SName = OName;
                    STel = OTel;
                    SCell = OCell;
                    SSex = OSex;

                    if (rlib.OrderData.MenuLists.TakeMealType == "1" || rlib.OrderData.MenuLists.TakeMealType == "")
                    {
                        SendMail = false;       //內用不需寄信
                    }
                }
                else
                {
                    OName = GS.ReplaceStr(this.o_name.Text);
                    OTel = GS.ReplaceStr(this.o_tel.Text);
                    OCell = GS.ReplaceStr(this.o_cell.Text);
                    OSex = GS.ReplaceStr(this.o_sex.SelectedItem.Value);
                    OEmail = GS.ReplaceStr(this.mail.Text);
                    ident = GS.ReplaceStr(this.ident.Text.Trim());

                    SName = GS.ReplaceStr(this.name.Text);
                    STel = GS.ReplaceStr(this.tel.Text);
                    SCell = GS.ReplaceStr(this.cell.Text);
                    SSex = GS.ReplaceStr(this.sex.SelectedItem.Value);
                    City = GS.ReplaceStr(this.ddlCity.SelectedItem.Text);
                    Country = GS.ReplaceStr(this.ddlCountry.SelectedItem.Text);
                    Zip = GS.ReplaceStr(this.ddlzip.SelectedItem.Text);
                    Address = City + Country + GS.ReplaceStr(this.address.Text);
                    Notememo = GS.ReplaceStr(this.notememo.Text);
                }
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    cmd = new SqlCommand("select storetype,disablePrice from CurrentUseHead", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            disablePrice = reader[1].ToString();
                            if (reader[0].ToString() == "1")
                            {
                                OName = GS.ReplaceStr(this.o_name.Text);
                                OTel = GS.ReplaceStr(this.o_tel.Text);
                                OCell = GS.ReplaceStr(this.o_cell.Text);
                                OSex = GS.ReplaceStr(this.o_sex.SelectedItem.Value);
                                OEmail = GS.ReplaceStr(this.mail.Text);
                                ident = GS.ReplaceStr(this.ident.Text.Trim());

                                SName = OName;
                                STel = OTel;
                                SCell = OCell;
                                SSex = OSex;
                            }
                            else
                            {
                                OName = GS.ReplaceStr(this.o_name.Text);
                                OTel = GS.ReplaceStr(this.o_tel.Text);
                                OCell = GS.ReplaceStr(this.o_cell.Text);
                                OSex = GS.ReplaceStr(this.o_sex.SelectedItem.Value);
                                OEmail = GS.ReplaceStr(this.mail.Text);
                                ident = GS.ReplaceStr(this.ident.Text.Trim());

                                SName = GS.ReplaceStr(this.name.Text);
                                STel = GS.ReplaceStr(this.tel.Text);
                                SCell = GS.ReplaceStr(this.cell.Text);
                                SSex = GS.ReplaceStr(this.sex.SelectedItem.Value);
                                City = GS.ReplaceStr(this.ddlCity.SelectedItem.Text);
                                Country = GS.ReplaceStr(this.ddlCountry.SelectedItem.Text);
                                Zip = GS.ReplaceStr(this.ddlzip.SelectedItem.Text);
                                Address = Zip + " " + City + Country + GS.ReplaceStr(this.address.Text);
                                invoice = GS.ReplaceStr(this.DropDownList5.SelectedItem.Text + " " + this.invoiceCity.SelectedItem.Text + this.invoiceCountry.SelectedItem.Text + this.oaddr.Text).Trim();
                                invoiceTitle = GS.ReplaceStr(this.invoiceTitle.Text);
                                Notememo = GS.ReplaceStr(this.notememo.Text);
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }



            #region save訂單表頭
            using (SqlConnection conn = new SqlConnection(setting))
            {
                //20140331有更新此預存程序!!!!(新增郵遞區號,縣市,鄉鎮區)
                if (ident.Length > 10)
                {
                    ident = ident.Substring(0, 10);
                }
                conn.Open();

                cmd = new SqlCommand();
                cmd.CommandText = "sp_orderhd";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@id", OrderID));
                cmd.Parameters.Add(new SqlParameter("@name", SName));
                cmd.Parameters.Add(new SqlParameter("@sex", Convert.ToInt32(SSex)));
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
                cmd.Parameters.Add(new SqlParameter("@o_addr", invoice));
                cmd.Parameters.Add(new SqlParameter("@bonus_amt", BonusAmt));
                cmd.Parameters.Add(new SqlParameter("@bonus_discount", BonusDiscount));
                cmd.Parameters.Add(new SqlParameter("@freightamount", FreightAmount));
                cmd.Parameters.Add(new SqlParameter("@c_no", GetPOSID(setting, MemID)));
                cmd.Parameters.Add(new SqlParameter("@ship_city", City));
                cmd.Parameters.Add(new SqlParameter("@ship_zip", Zip));
                cmd.Parameters.Add(new SqlParameter("@ship_countryname", Country));
                cmd.Parameters.Add(new SqlParameter("@discount_amt", DiscountAmt));
                cmd.Parameters.Add(new SqlParameter("@RID", RID));
                cmd.Parameters.Add(new SqlParameter("@Click_ID", Click_ID));
                cmd.Parameters.Add(new SqlParameter("@prod_bonus", Session["prodBonus"].ToString()));
                cmd.Parameters.Add(new SqlParameter("@type", (rlib.OrderData.ShopType == 4 ? "S" : "O")));
                cmd.Parameters.Add(new SqlParameter("@deliveryDate", deliveryDate));
                cmd.Parameters.Add(new SqlParameter("@ident", ident));
                cmd.Parameters.Add(new SqlParameter("@invoice_title", invoiceTitle));
                cmd.Parameters.Add(new SqlParameter("@CouponID", CouponID));
                cmd.Parameters.Add(new SqlParameter("@CouponDiscount", couDiscont));
                cmd.Parameters.Add(new SqlParameter("@GCode", GCode));
                cmd.Parameters.Add(new SqlParameter("@affi_id", affi_id));
                cmd.ExecuteNonQuery();
            }

            #endregion

            #region save訂單課製備註
            if (!string.IsNullOrEmpty(this.formRelationDetail.Text))
            {
                f.columnItems = JsonConvert.DeserializeObject<List<FormColumnItem>>(this.formRelationDetail.Text);
                if (f.columnItems.Count > 0) f.saveAndBindOrder(setting, OrderID);
            }
            #endregion

            int i = 1;
            int order_totalamt = 0;
            String Memo = "";

            if (rlib.OrderData.ShopType == 3)
            {
                #region 點餐
                String VerCode = rlib.OrderData.MenuLists.Vercode;
                String TableID = rlib.OrderData.MenuLists.TableID;
                String ShopID = rlib.OrderData.MenuLists.ShopID;
                String ShopName = rlib.OrderData.MenuLists.ShopName;
                String TakeMealType = "";
                if (rlib.OrderData.MenuLists.TakeMealType == null)
                {
                    TakeMealType = "";
                }
                else
                {
                    TakeMealType = rlib.OrderData.MenuLists.TakeMealType;
                }
                Int32 SpecAmt = 0;
                String Discription = "";
                String MenuItemName = "";

                Int32 MenuPrice = 0;
                Int32 MenuAddPrice = 0;
                Int32 MenuDiscount = 0;


                #region 註記點餐桌號
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();

                    cmd = new SqlCommand();
                    cmd.CommandText = "sp_order_table";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@orderID", OrderID));
                    cmd.Parameters.Add(new SqlParameter("@tableID", TableID));
                    cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                    cmd.Parameters.Add(new SqlParameter("@TakeMealType", TakeMealType));
                    cmd.Parameters.Add(new SqlParameter("@shopID", ShopID));
                    cmd.Parameters.Add(new SqlParameter("@shopName", ShopName));
                    cmd.ExecuteNonQuery();
                }
                #endregion

                foreach (Shoppingcar.Menu Menu in rlib.OrderData.MenuLists.Menu)
                {
                    if (Menu.ID == "Single")
                    {
                        Memo = "";
                    }
                    else
                    {
                        #region 儲存套餐名稱
                        using (SqlConnection conn = new SqlConnection(setting))
                        {
                            conn.Open();
                            #region 新增表身
                            cmd = new SqlCommand();
                            cmd.CommandText = "sp_order";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = conn;
                            cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                            cmd.Parameters.Add(new SqlParameter("@ser_no", i.ToString().PadLeft(3, '0')));
                            cmd.Parameters.Add(new SqlParameter("@prod_name", Menu.Name));
                            cmd.Parameters.Add(new SqlParameter("@price", GetProdPrice(Menu.ID, "", setting, "", "")));
                            cmd.Parameters.Add(new SqlParameter("@qty", Menu.Qty));
                            cmd.Parameters.Add(new SqlParameter("@amt", Convert.ToInt32((Convert.ToInt32(Menu.Qty) * double.Parse(GetProdPrice(Menu.ID, "", setting, "", ""))) + 0.001)));
                            cmd.Parameters.Add(new SqlParameter("@productid", Menu.ID));
                            cmd.Parameters.Add(new SqlParameter("@colorid", ""));
                            cmd.Parameters.Add(new SqlParameter("@sizeid", ""));
                            cmd.Parameters.Add(new SqlParameter("@posno", ""));
                            cmd.Parameters.Add(new SqlParameter("@memo", ""));
                            cmd.Parameters.Add(new SqlParameter("@virtual", "N"));
                            cmd.Parameters.Add(new SqlParameter("@usetime", "0"));
                            cmd.Parameters.Add(new SqlParameter("@usedate", ""));
                            cmd.Parameters.Add(new SqlParameter("@discount", Menu.Discount));
                            cmd.Parameters.Add(new SqlParameter("@discription", ""));
                            cmd.Parameters.Add(new SqlParameter("@bonus", ""));
                            cmd.ExecuteNonQuery();
                            #endregion

                            order_totalamt += Convert.ToInt32((Convert.ToInt32(Menu.Qty) * double.Parse(GetProdPrice(Menu.ID, "", setting, "", ""))) + 0.001) - Convert.ToInt32(Menu.Discount);
                            i = i + 1;
                        }

                        #endregion
                        Memo = Menu.Name;
                    }

                    foreach (Shoppingcar.MenuItem MenuItem in Menu.MenuItems)
                    {
                        MenuPrice = Convert.ToInt32(GetProdPrice(MenuItem.ID, "", setting, "", ""));

                        if (Menu.ID == "Single")
                        {
                            MenuAddPrice = 0;
                        }
                        else
                        {
                            MenuAddPrice = Convert.ToInt32(GetProdPrice(MenuItem.ID, "", setting, "", Menu.ID));
                        }

                        foreach (Shoppingcar.MenuSpec MenuSpec in MenuItem.MenuSpec)
                        {
                            Discription = "";
                            SpecAmt = 0;
                            MenuDiscount = 0;
                            if (MenuSpec.OtherID != null)
                            {
                                for (int j = 0; j < MenuSpec.OtherID.Count; j++)
                                {
                                    Discription += GetMemoName(setting, MenuSpec.OtherID[j]) + "$" + GetProdPrice(MenuItem.ID, "", setting, MenuSpec.OtherID[j], "") + " ";
                                    SpecAmt += Convert.ToInt32(GetProdPrice(MenuItem.ID, "", setting, MenuSpec.OtherID[j], ""));
                                }
                            }

                            if (MenuSpec.Memo != null)
                            {
                                for (int j = 0; j < MenuSpec.Memo.Count; j++)
                                {
                                    Discription += MenuSpec.Memo[j].ToString() + " ";
                                }
                            }

                            if (Menu.ID == "Single")
                            {
                                MenuDiscount = 0 - SpecAmt;
                            }
                            else
                            {
                                MenuDiscount = MenuPrice - SpecAmt - MenuAddPrice;
                            }

                            if (MenuAddPrice > 0)
                            {
                                MenuItemName = MenuItem.Name + "(需加價" + MenuAddPrice + ")";
                            }
                            else
                            {
                                MenuItemName = MenuItem.Name;
                            }

                            #region 儲存餐點項目
                            using (SqlConnection conn = new SqlConnection(setting))
                            {
                                conn.Open();
                                #region 新增表身
                                cmd = new SqlCommand();
                                cmd.CommandText = "sp_order";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = conn;
                                cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                                cmd.Parameters.Add(new SqlParameter("@ser_no", i.ToString().PadLeft(3, '0')));
                                cmd.Parameters.Add(new SqlParameter("@prod_name", MenuItemName));
                                cmd.Parameters.Add(new SqlParameter("@price", MenuPrice));
                                cmd.Parameters.Add(new SqlParameter("@qty", MenuSpec.Qty));
                                cmd.Parameters.Add(new SqlParameter("@amt", Convert.ToInt32(MenuPrice) * Convert.ToInt32(MenuSpec.Qty)));
                                cmd.Parameters.Add(new SqlParameter("@productid", MenuItem.ID));
                                cmd.Parameters.Add(new SqlParameter("@colorid", "0"));
                                cmd.Parameters.Add(new SqlParameter("@sizeid", "0"));
                                cmd.Parameters.Add(new SqlParameter("@posno", ""));
                                cmd.Parameters.Add(new SqlParameter("@memo", Memo));
                                cmd.Parameters.Add(new SqlParameter("@virtual", "N"));
                                cmd.Parameters.Add(new SqlParameter("@usetime", "0"));
                                cmd.Parameters.Add(new SqlParameter("@usedate", ""));
                                cmd.Parameters.Add(new SqlParameter("@discount", MenuDiscount * Convert.ToInt32(MenuSpec.Qty)));
                                cmd.Parameters.Add(new SqlParameter("@discription", Discription));
                                cmd.Parameters.Add(new SqlParameter("@bonus", "0"));
                                cmd.ExecuteNonQuery();
                                #endregion

                                order_totalamt += Convert.ToInt32(MenuSpec.Qty) * (MenuPrice - MenuDiscount);

                                i = i + 1;
                            }
                            #endregion
                        }
                    }
                }

                #endregion
            }
            else
            {
                #region 普通購物車
                foreach (Shoppingcar.OrderList Orders in rlib.OrderData.OrderLists)
                {
                    if (Orders.Type != 0 || disablePrice == "S")
                    {
                        Memo = Orders.Title;
                    }
                    else
                    {
                        Memo = "";
                    }
                    foreach (Shoppingcar.OrderItem Items in Orders.OrderItems)
                    {
                        foreach (Shoppingcar.OrderSpec OrderSpecs in Items.OrderSpecs)
                        {
                            using (SqlConnection conn = new SqlConnection(setting))
                            {
                                conn.Open();
                                #region 新增表身
                                cmd = new SqlCommand();
                                cmd.CommandText = "sp_order";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = conn;
                                cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                                cmd.Parameters.Add(new SqlParameter("@ser_no", i.ToString().PadLeft(3, '0')));
                                cmd.Parameters.Add(new SqlParameter("@prod_name", Items.Name));
                                cmd.Parameters.Add(new SqlParameter("@price",(rlib.OrderData.ShopType == 4?0: OrderSpecs.Price)));
                                cmd.Parameters.Add(new SqlParameter("@qty", Convert.ToInt32(OrderSpecs.Qty)));
                                cmd.Parameters.Add(new SqlParameter("@amt", Convert.ToInt32((OrderSpecs.Qty * OrderSpecs.Price) + 0.001)));
                                cmd.Parameters.Add(new SqlParameter("@productid", Items.ID));
                                cmd.Parameters.Add(new SqlParameter("@colorid", OrderSpecs.Color));
                                cmd.Parameters.Add(new SqlParameter("@sizeid", OrderSpecs.Size));
                                cmd.Parameters.Add(new SqlParameter("@posno", Items.PosNo));
                                cmd.Parameters.Add(new SqlParameter("@memo", Memo));
                                cmd.Parameters.Add(new SqlParameter("@virtual", Items.Virtual));
                                cmd.Parameters.Add(new SqlParameter("@usetime", Items.UseTime));
                                cmd.Parameters.Add(new SqlParameter("@usedate", Items.UseDate));
                                cmd.Parameters.Add(new SqlParameter("@discount", OrderSpecs.Discount));
                                cmd.Parameters.Add(new SqlParameter("@discription", ""));
                                cmd.Parameters.Add(new SqlParameter("@bonus", Convert.ToInt32(OrderSpecs.Bonus)));
                                cmd.ExecuteNonQuery();
                                #endregion

                                order_totalamt += Convert.ToInt32((OrderSpecs.Qty * OrderSpecs.Price) + 0.001) - Convert.ToInt32(OrderSpecs.Discount);
                                #region 庫存更新
                                if (rlib.OrderData.ShopType != 4)
                                {
                                    cmd = new SqlCommand();
                                    cmd.CommandText = "sp_stocks";
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Connection = conn;
                                    cmd.Parameters.Add(new SqlParameter("@prod_id", Convert.ToInt32(Items.ID)));
                                    cmd.Parameters.Add(new SqlParameter("@qty", Convert.ToInt32(OrderSpecs.Qty)));
                                    cmd.Parameters.Add(new SqlParameter("@prod_color", Convert.ToInt32(OrderSpecs.Color)));
                                    cmd.Parameters.Add(new SqlParameter("@prod_size", Convert.ToInt32(OrderSpecs.Size)));
                                    cmd.ExecuteNonQuery();
                                }
                                #endregion
                                i = i + 1;
                            }
                        }
                        #region 加價購部分
                        foreach (Shoppingcar.AdditionalItem AdditionalItems in Items.AdditionalItems)
                        {
                            using (SqlConnection conn = new SqlConnection(setting))
                            {
                                conn.Open();
                                #region 新增表身
                                cmd = new SqlCommand();
                                cmd.CommandText = "sp_order";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = conn;
                                cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                                cmd.Parameters.Add(new SqlParameter("@ser_no", i.ToString().PadLeft(3, '0')));
                                cmd.Parameters.Add(new SqlParameter("@prod_name", AdditionalItems.Name));
                                cmd.Parameters.Add(new SqlParameter("@price", Convert.ToInt32(AdditionalItems.Price)));
                                cmd.Parameters.Add(new SqlParameter("@qty", Convert.ToInt32(AdditionalItems.Qty)));
                                cmd.Parameters.Add(new SqlParameter("@amt", Convert.ToInt32(AdditionalItems.Qty) * Convert.ToInt32(AdditionalItems.Price)));
                                cmd.Parameters.Add(new SqlParameter("@productid", AdditionalItems.ID));
                                cmd.Parameters.Add(new SqlParameter("@colorid", AdditionalItems.Color));
                                cmd.Parameters.Add(new SqlParameter("@sizeid", AdditionalItems.Size));
                                cmd.Parameters.Add(new SqlParameter("@posno", AdditionalItems.PosNo));
                                cmd.Parameters.Add(new SqlParameter("@memo", GetLocalResourceObject("StringResource9")));
                                cmd.Parameters.Add(new SqlParameter("@virtual", Items.Virtual));
                                cmd.Parameters.Add(new SqlParameter("@usetime", Items.UseTime));
                                cmd.Parameters.Add(new SqlParameter("@usedate", Items.UseDate));
                                cmd.Parameters.Add(new SqlParameter("@discount", AdditionalItems.Discount));
                                cmd.Parameters.Add(new SqlParameter("@discription", ""));
                                cmd.Parameters.Add(new SqlParameter("@bonus", "0"));
                                cmd.ExecuteNonQuery();
                                #endregion
                                order_totalamt += Convert.ToInt32(AdditionalItems.Qty) * Convert.ToInt32(AdditionalItems.Price) - Convert.ToInt32(AdditionalItems.Discount);
                                #region 庫存更新
                                cmd = new SqlCommand();
                                cmd.CommandText = "sp_stocks";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = conn;
                                cmd.Parameters.Add(new SqlParameter("@prod_id", Convert.ToInt32(AdditionalItems.ID)));
                                cmd.Parameters.Add(new SqlParameter("@qty", Convert.ToInt32(AdditionalItems.Qty)));
                                if (AdditionalItems.Color == null)
                                {
                                    cmd.Parameters.Add(new SqlParameter("@prod_color", "0"));
                                }
                                else
                                {
                                    cmd.Parameters.Add(new SqlParameter("@prod_color", AdditionalItems.Color));
                                }
                                if (AdditionalItems.Size == null)
                                {
                                    cmd.Parameters.Add(new SqlParameter("@prod_size", "0"));
                                }
                                else
                                {
                                    cmd.Parameters.Add(new SqlParameter("@prod_size", AdditionalItems.Size));
                                }
                                cmd.ExecuteNonQuery();
                                #endregion
                                i = i + 1;
                            }
                        }
                        #endregion
                    }
                }
                #endregion
            }
            #region 儲存訂單總金額
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                cmd = new SqlCommand();
                cmd.CommandText = "sp_order_freight";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@id", OrderID));
                cmd.Parameters.Add(new SqlParameter("@amt", (rlib.OrderData.ShopType != 4? order_totalamt:0)));
                cmd.ExecuteNonQuery();
            }
            #endregion

            #region 插入優惠券
            if (rlib.OrderData.MemID != "")
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmdC1 = new SqlCommand(@"
                        select top 1 * from Coupon
                        where fullGetPrice <= CONVERT(int, @price) and disp_opt='Y' and getType='2' and Stocks>=(GetQty+1) and
                            GETDATE()
                            between 
                                case 
                                    when CHARINDEX('上午',Coupon.[start_date])>0 then REPLACE(Coupon.[start_date],' 上午 ',' ')+' AM'
                                    when CHARINDEX('下午',Coupon.[start_date])>0 then REPLACE(Coupon.[start_date],' 下午 ',' ')+' PM'
                                end
                            and
                                case 
                                    when CHARINDEX('上午',Coupon.[end_date])>0 then REPLACE(Coupon.[end_date],' 上午 ',' ')+' AM'
                                    when CHARINDEX('下午',Coupon.[end_date])>0 then REPLACE(Coupon.[end_date],' 下午 ',' ')+' PM'
                                end
                        order by fullGetPrice desc,id
                    ", conn);
                    cmdC1.Parameters.Add(new SqlParameter("@price", order_totalamt - Convert.ToInt32(rlib.OrderData.BonusDiscount) - DiscountAmt - couDiscont));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmdC1.ExecuteReader();
                        if (reader.Read())
                        {
                            string SerNo = (int.Parse(reader["GetQty"].ToString()) + 1).ToString().PadLeft(10, '0');
                            string GCode2 = reader["GCode"].ToString();
                            string VCode = reader["VCode"].ToString();
                            string ExpireDate = reader["end_date"].ToString();
                            string noType = reader["noType"].ToString();
                            int cid = insertCustCoupon(setting, SerNo, GCode2, VCode, ExpireDate, noType, MemID);
                            using (SqlConnection conn4 = new SqlConnection(setting))
                            {
                                conn4.Open();
                                SqlCommand cmd4 = new SqlCommand();
                                cmd4.CommandText = "sp_updateOrderHdGetCoupon";
                                cmd4.CommandType = CommandType.StoredProcedure;
                                cmd4.Connection = conn4;
                                cmd4.Parameters.Add(new SqlParameter("@OrderID", OrderID));
                                cmd4.Parameters.Add(new SqlParameter("@cid", cid));
                                try
                                {
                                    cmd4.ExecuteReader();
                                }
                                catch (Exception ex)
                                {
                                    Response.Write("0");
                                    Response.Write(ex.StackTrace);
                                    Response.End();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Response.Write("3");
                        Response.Write(ex.StackTrace);
                        Response.End();
                    }
                    finally { if (reader != null) reader.Close(); }
                }
            }
            #endregion    

            #region 儲存物流資料

            String LogisticsTypeID = rlib.OrderData.LogisticstypeID;
            String GoodsAmount = order_totalamt.ToString();
            String CollectionAmount = "";
            String ReceiverStoreID = "";
            String ReceiverStoreName = "";
            String ReceiverStoreAddr = "";
            String ReceiverStoreTel = "";
            String LogisticApi = rlib.OrderData.LogisticApi;
            String Temperature = rlib.OrderData.Temperature;

            if (CVSdiv.Visible)
            {
                ReceiverStoreID = this.CVSStoreID.Text;
                ReceiverStoreName = this.CVSStoreName.Text;
                ReceiverStoreAddr = this.CVSAddress.Text;
                ReceiverStoreTel = this.CVSTelephone.Text;

            }

            if (rlib.OrderData.PayType == "getandpay")
            {
                CollectionAmount = (Convert.ToInt32(GoodsAmount) + Convert.ToInt32(FreightAmount) - Convert.ToInt32(DiscountAmt) - Convert.ToInt32(BonusDiscount) - couDiscont).ToString();
            }
            else
            {
                CollectionAmount = "0";
            }

            SaveOrdersLogistics(setting, OrderID, LogisticsTypeID, LogisticApi, GoodsAmount, CollectionAmount, ReceiverStoreID, ReceiverStoreName, ReceiverStoreAddr, ReceiverStoreTel, Temperature);

            #endregion


            return SendMail;
        }
        #endregion

        #region 插入優惠券
        public int insertCustCoupon(string setting, string SerNo, string GCode, string VCode, string ExpireDate, string noType, string MemID)
        {
            int cind = 0;
            switch (noType)
            {
                case "1":
                    GCode = GS.GetRandomString(9) + GS.Right(SerNo, 3);
                    GCode = GS.GetRandomString(9) + "-" + GS.Right(SerNo, 3) + "-" + GS.checkSunChar(GCode);
                    break;
                case "2":
                    string r = GS.Right(SerNo, 5);
                    GCode = GCode + "-" + GS.Right(SerNo, 5) + "-" + GS.checkSunChar(GCode + r);
                    break;
                case "3": break;
                default:
                    GCode = "";
                    break;
            }
            using (SqlConnection conn2 = new SqlConnection(setting))
            {
                conn2.Open();
                SqlCommand cmd2 = new SqlCommand();
                cmd2.CommandText = "sp_AddCustCoupon";
                cmd2.CommandType = CommandType.StoredProcedure;
                cmd2.Connection = conn2;
                cmd2.Parameters.Add(new SqlParameter("@memid", MemID));
                cmd2.Parameters.Add(new SqlParameter("@VCode", VCode));
                cmd2.Parameters.Add(new SqlParameter("@ExpireDate", ExpireDate));
                cmd2.Parameters.Add(new SqlParameter("@SerNo", SerNo));
                cmd2.Parameters.Add(new SqlParameter("@GCode", GCode));
                cmd2.Parameters.Add(new SqlParameter("@type", "2"));
                try
                {
                    cmd2.ExecuteNonQuery();
                    using (SqlConnection conn3 = new SqlConnection(setting))
                    {
                        conn3.Open();
                        SqlCommand cmd3 = new SqlCommand("select IDENT_CURRENT('Cust_Coupon')", conn3);
                        SqlDataReader reader3 = null;
                        try
                        {
                            reader3 = cmd3.ExecuteReader();
                            if (reader3.Read())
                            {
                                cind = int.Parse(reader3[0].ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            Response.Write(ex.StackTrace);
                            Response.End();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Response.Write("2");
                    Response.Write(ex.StackTrace);
                    Response.End();
                }
            }
            return cind;
        }
        #endregion

        #region 檢查優惠券是否有效
        private class ChechCouponData
        {
            public int id { get; set; }
            public string VCode { get; set; }
            public string GCode { get; set; }
            public bool doInsertCoupon { get; set; }
            public int activeType { get; set; }
            public int discount { get; set; }
            public string gift { get; set; }
            public string prodStoreNo { get; set; }
            public string title { get; set; }
            public string SerNo { get; set; }
            public string ExpireDate { get; set; }
            public string noType { get; set; }
        }
        private ChechCouponData chechCoupon(string setting, string VCode, string GCode, string MemID)
        {
            ChechCouponData chechCouponData = null;
            if (!string.IsNullOrEmpty(VCode))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select Coupon.* from Coupon
                        left join Cust_Coupon on Coupon.VCode=Cust_Coupon.VCode
                        where 
	                        Cust_Coupon.Stat=1 and Coupon.[type]!=4 and Cust_Coupon.memid=@memid and Coupon.VCode=@VCode and Cust_Coupon.GCode=@GCode and
	                        getdate()
                            between 
                                case 
                                    when CHARINDEX('上午',Coupon.[start_date])>0 then REPLACE(Coupon.[start_date],' 上午 ',' ')+' AM'
                                    when CHARINDEX('下午',Coupon.[start_date])>0 then REPLACE(Coupon.[start_date],' 下午 ',' ')+' PM'
                                end
                            and
                                case 
                                    when CHARINDEX('上午',Cust_Coupon.[ExpireDate])>0 then REPLACE(Cust_Coupon.[ExpireDate],' 上午 ',' ')+' AM'
                                    when CHARINDEX('下午',Cust_Coupon.[ExpireDate])>0 then REPLACE(Cust_Coupon.[ExpireDate],' 下午 ',' ')+' PM'
                                end
                            and
                            getdate()
                            between 
                                isnull(Cust_Coupon.canUseDate,'2100/12/31')
                            and
                                case 
                                    when CHARINDEX('上午',Cust_Coupon.[ExpireDate])>0 then REPLACE(Cust_Coupon.[ExpireDate],' 上午 ',' ')+' AM'
                                    when CHARINDEX('下午',Cust_Coupon.[ExpireDate])>0 then REPLACE(Cust_Coupon.[ExpireDate],' 下午 ',' ')+' PM'
                                end
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@VCode", VCode));
                    cmd.Parameters.Add(new SqlParameter("@GCode", GCode));
                    cmd.Parameters.Add(new SqlParameter("@memid", MemID));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            chechCouponData = new ChechCouponData
                            {
                                id = int.Parse(reader["id"].ToString()),
                                VCode = reader["VCode"].ToString(),
                                GCode = GCode,
                                doInsertCoupon = false,
                                activeType = int.Parse(reader["activeType"].ToString()),
                                discount = int.Parse(reader["discount"].ToString()),
                                gift = reader["gift"].ToString(),
                                prodStoreNo = reader["prodStoreNo"].ToString(),
                                title = reader["title"].ToString(),
                                SerNo = (int.Parse(reader["GetQty"].ToString()) + 1).ToString().PadLeft(10, '0'),
                                ExpireDate = reader["end_date"].ToString(),
                                noType = reader["noType"].ToString()
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        Response.Write("K1");
                        Response.Write(ex.StackTrace);
                        Response.End();
                    }
                    finally { if (reader != null) reader.Close(); }
                }
            }
            else if (!string.IsNullOrEmpty(GCode))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select top 1 * from coupon where GCode=@GCode and noType=3 and [type]<>4 and disp_opt='Y' and
                        GetQty+1<=Stocks and
                        getdate()
                        between 
                            case 
                                when CHARINDEX('上午',Coupon.[start_date])>0 then REPLACE(Coupon.[start_date],' 上午 ',' ')+' AM'
                                when CHARINDEX('下午',Coupon.[start_date])>0 then REPLACE(Coupon.[start_date],' 下午 ',' ')+' PM'
                            end
                        and
                            case 
                                when CHARINDEX('上午',Coupon.[end_date])>0 then REPLACE(Coupon.[end_date],' 上午 ',' ')+' AM'
                                when CHARINDEX('下午',Coupon.[end_date])>0 then REPLACE(Coupon.[end_date],' 下午 ',' ')+' PM'
                            end
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@GCode", GCode));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            chechCouponData = new ChechCouponData
                            {
                                id = int.Parse(reader["id"].ToString()),
                                VCode = reader["VCode"].ToString(),
                                GCode = GCode,
                                doInsertCoupon = true,
                                activeType = int.Parse(reader["activeType"].ToString()),
                                discount = int.Parse(reader["discount"].ToString()),
                                gift = reader["gift"].ToString(),
                                prodStoreNo = reader["prodStoreNo"].ToString(),
                                title = reader["title"].ToString(),
                                SerNo = (int.Parse(reader["GetQty"].ToString()) + 1).ToString().PadLeft(10, '0'),
                                ExpireDate = reader["end_date"].ToString(),
                                noType = reader["noType"].ToString()
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        Response.Write("K2");
                        Response.Write(ex.StackTrace);
                        Response.End();
                    }
                    finally { if (reader != null) reader.Close(); }
                }
            }
            return chechCouponData;
        }
        #endregion

        #region 發送會員開通信
        private void sentMemberMail(String setting, String MemID)
        {
            String Mail_Cont = "";
            String Service_mail = "";
            String Mail_title = "";
            String CrmVersion = "";
            String CertificationURL = "";
            String SendMemberMail = "";
            GetStr GS = new GetStr();
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                String Str_sql = "select b.service_mail,b.title,b.crm_version,b.CertificationURL,b.send_member_mail from CurrentUseFrame as a left join head as b on a.id=b.hid";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.FieldCount > 0)
                    {
                        while (reader.Read())
                        {
                            Service_mail = reader[0].ToString();
                            Mail_title = reader[1].ToString();
                            CrmVersion = reader[2].ToString();
                            CertificationURL = reader[3].ToString();
                            SendMemberMail = reader[4].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            Mail_Cont += "<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />";
            Mail_Cont += "<div style='font-family:微軟正黑體, Arial, 新細明體, Helvetica, sans-serif'>";
            Mail_Cont += "    <font size='4' color='#ff0000'><b>" + GetLocalResourceObject("StringResourceM12") + Mail_title + GetLocalResourceObject("StringResourceM13") + "</b></font><br>";
            Mail_Cont += "    <hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px; margin-top:8px; margin-bottom:20px;'>";
            Mail_Cont += GetLocalResourceObject("StringResourceM14").ToString() + "<br>";
            Mail_Cont += "    <br>";
            Mail_Cont += "    <strong>" + GetLocalResourceObject("StringResourceM15").ToString() + "：</strong>" + this.mail.Text + "<br>";
            Mail_Cont += "    <br>";
            if (CertificationURL == "Y")
            {
                Mail_Cont += "    <strong>" + GetLocalResourceObject("StringResourceM17").ToString() + "</strong><br>";
                Mail_Cont += "    <a href='" + rlib.OrderData.ReturnUrl + "/" + folder + "/checkm.asp?mem_id=" + MemID + "'>" + rlib.OrderData.ReturnUrl + "/" + folder + "/checkm.asp?mem_id=" + MemID + "</a><br>";
                Mail_Cont += "    <span style='color: #666;'>(" + GetLocalResourceObject("StringResourceM18") + ")<br>";
            }

            Mail_Cont += GetLocalResourceObject("StringResourceM19") + "~</span><br>";
            Mail_Cont += "    <br>";
            Mail_Cont += "    <span style='color:#f00;'>" + GetLocalResourceObject("StringResourceM20").ToString() + "</span>";
            Mail_Cont += "    <hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px'>";
            Mail_Cont += GetLocalResourceObject("StringResourceM21").ToString() + "    <br>";
            Mail_Cont += GetLocalResourceObject("StringResourceM22").ToString();
            Mail_Cont += "    <hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px;'>";
            Mail_Cont += "</div>";



            String Mail_Cont2 = "";
            Mail_Cont2 += "<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />";
            Mail_Cont2 += "<div style='font-family:微軟正黑體, Arial, 新細明體, Helvetica, sans-serif'>";
            Mail_Cont2 += GetLocalResourceObject("StringResourceM23").ToString() + MemID + GetLocalResourceObject("StringResourceM24").ToString() + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + GetLocalResourceObject("StringResourceM25").ToString() + "<br>";
            Mail_Cont2 += "</div>";

            send_email(Mail_Cont, GetLocalResourceObject("StringResourceM26").ToString() + " 【" + Mail_title + "】", Service_mail, this.mail.Text);//呼叫send_email函式測試，寄給會員

            if (SendMemberMail == "Y")
            {
                send_email(Mail_Cont2, GetLocalResourceObject("StringResourceM26").ToString() + " 【" + Mail_title + "】", Service_mail, Service_mail);//呼叫send_email函式測試，寄給管理者
            }
            return;
        }
        #endregion

        #region 訂單通知信內容製作
        private string GetMailCont(String setting, String OrderID, String BookingID, FormColumns f)
        {
            GetStr GS = new GetStr();

            #region 表單資料
            int BonusAmt = rlib.OrderData.BonusAmt;
            String BonusDiscount = rlib.OrderData.BonusDiscount;
            int FreightAmount = rlib.OrderData.FreightAmount;
            String MemID = rlib.OrderData.MemID;
            String PayType = rlib.OrderData.PayType;
            String ReturnUrl = rlib.OrderData.ReturnUrl;
            String RID = rlib.OrderData.RID;
            String Click_ID = rlib.OrderData.Click_ID;
            string servicePriceType = rlib.OrderData.servicePriceType;
            int servicePrice = rlib.OrderData.servicePrice;
            int servicePriceSum = rlib.OrderData.servicePriceSum;
            int ShopType = rlib.OrderData.ShopType;
            string serId = "";
            if (rlib.OrderData.allVirtualProd) FreightAmount = 0;

            Int32 DiscountAmt = Convert.ToInt32(GS.ReplaceStr(this.discount_amt.Value));
            String OName = "";
            String OTel = "";
            String OCell = "";
            String OSex = "";
            String OEmail = "";

            String SName = "";
            String STel = "";
            String SCell = "";
            String SSex = "";
            String City = "";
            String Country = "";
            String Zip = "";
            String Address = "";
            String Notememo = "";
            string invoice_title="", invoice="", ident="";
            SqlCommand cmd;

            if (rlib.OrderData.ShopType == 3)
            {
                if (rlib.OrderData.MenuLists.TakeMealType != "3")
                {
                    OName = GS.ReplaceStr(this.h_o_name.Value);
                    OTel = GS.ReplaceStr(this.h_o_tel.Value);
                    OCell = GS.ReplaceStr(this.h_o_cell.Value);
                    OSex = GS.ReplaceStr(this.h_o_sex.Value);
                    OEmail = GS.ReplaceStr(this.h_mail.Value);

                    SName = OName;
                    STel = OTel;
                    SCell = OCell;
                    SSex = OSex;
                }
                else
                {
                    OName = GS.ReplaceStr(this.o_name.Text);
                    OTel = GS.ReplaceStr(this.o_tel.Text);
                    OCell = GS.ReplaceStr(this.o_cell.Text);
                    OSex = GS.ReplaceStr(this.o_sex.SelectedItem.Value);
                    OEmail = GS.ReplaceStr(this.mail.Text);

                    SName = GS.ReplaceStr(this.name.Text);
                    STel = GS.ReplaceStr(this.tel.Text);
                    SCell = GS.ReplaceStr(this.cell.Text);
                    SSex = GS.ReplaceStr(this.sex.SelectedItem.Value);
                    City = GS.ReplaceStr(this.ddlCity.SelectedItem.Text);
                    Country = GS.ReplaceStr(this.ddlCountry.SelectedItem.Text);
                    Zip = GS.ReplaceStr(this.ddlzip.SelectedItem.Text);
                    Address = City + Country + GS.ReplaceStr(this.address.Text);
                    Notememo = GS.ReplaceStr(this.notememo.Text);
                }
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    cmd = new SqlCommand("select storetype from head", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            if (reader[0].ToString() == "1")
                            {
                                OName = GS.ReplaceStr(this.o_name.Text);
                                OTel = GS.ReplaceStr(this.o_tel.Text);
                                OCell = GS.ReplaceStr(this.o_cell.Text);
                                OSex = GS.ReplaceStr(this.o_sex.SelectedItem.Value);
                                OEmail = GS.ReplaceStr(this.mail.Text);

                                SName = OName;
                                STel = OTel;
                                SCell = OCell;
                                SSex = OSex;
                            }
                            else
                            {
                                OName = GS.ReplaceStr(this.o_name.Text);
                                OTel = GS.ReplaceStr(this.o_tel.Text);
                                OCell = GS.ReplaceStr(this.o_cell.Text);
                                OSex = GS.ReplaceStr(this.o_sex.SelectedItem.Value);
                                OEmail = GS.ReplaceStr(this.mail.Text);

                                SName = GS.ReplaceStr(this.name.Text);
                                STel = GS.ReplaceStr(this.tel.Text);
                                SCell = GS.ReplaceStr(this.cell.Text);
                                SSex = GS.ReplaceStr(this.sex.SelectedItem.Value);
                                City = GS.ReplaceStr(this.ddlCity.SelectedItem.Text);
                                Country = GS.ReplaceStr(this.ddlCountry.SelectedItem.Text);
                                Zip = GS.ReplaceStr(this.ddlzip.SelectedItem.Text);
                                Address = City + Country + GS.ReplaceStr(this.address.Text);
                                Notememo = GS.ReplaceStr(this.notememo.Text);
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            #endregion

            #region 取得訂單資料
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                cmd = new SqlCommand("select ser_id,invoice_title,o_addr,ident from orders_hd where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@id", OrderID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.Read())
                    {
                        serId = reader[0].ToString();
                        invoice = reader["o_addr"].ToString();
                        invoice_title = reader["invoice_title"].ToString();
                        ident = reader["ident"].ToString();
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            #endregion

            #region 通知信內容製作


            #region 取得網站基本資料
            String service_mail = "";
            String mer_id = "";
            String remit_money1 = "";
            String remit_money2 = "";
            String remit_money3 = "";
            String remit_money4 = "";
            String remit_money5 = "";
            String title = "";
            String freight_range = "";
            String disablePrice = "N";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                cmd = new SqlCommand("select service_mail,mer_id,remit_money1,remit_money2,remit_money3,title,isnull(freight_range,''),remit_money4,remit_money5,disablePrice from head", conn);
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
                        remit_money4 = reader[7].ToString();
                        remit_money5 = reader[8].ToString();
                        if (ShopType == 4) disablePrice = "Y";
                        else disablePrice = reader[9].ToString();
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            #endregion

            #region 表頭
            String mailTableHead = "";
            String mail_cont = $@"
                <html>
                    <head>
                        <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
                        <style>
                            #mainCont{{font-family:微軟正黑體, Arial, 新細明體, Helvetica, sans-serif}}
                            .mail_tb{{border-width: 0 1px;margin:1rem 0; width:600px; font-size:9pt; font-family:'microsoft jhenghei', sans-serif;}}
                            .mail_tb tr>th{{background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;}}
                            .mail_tb tr>th.last,.mail_tb tr>td.last{{border-right-width:0px;}}
                            .mail_tb tr>td{{background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;}}
                            .no-border-bottom{{border-bottom-width:0;}}
                            {getCellMailCssString()}
                        </style>
                    </head>
                    <body>
                        <div id='mainCont'>
            ";
            if (!string.IsNullOrEmpty(f.introduction))
            {
                mail_cont += f.introduction;
            }
            else
            {
                mail_cont += "<font size='4' color='#ff0000'><b>" + GetLocalResourceObject("StringResource23") + "</b></font><br>";
                mail_cont += "<b>" + GetLocalResourceObject("StringResource24") + "</b>";
                mail_cont += "<ul>";
                mail_cont += "	<li>";
                mail_cont += GetLocalResourceObject("StringResource25");
                mail_cont += "    </li>";
                mail_cont += "   <li>";
                mail_cont += GetLocalResourceObject("StringResource26");
                mail_cont += "  </li>";
                mail_cont += "   <li>";
                mail_cont += GetLocalResourceObject("StringResource27");
                mail_cont += "  </li>";
                mail_cont += "</ul>";
            }
            mail_cont += "<hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px; margin-top:8px; margin-bottom:20px;'>";

            mail_cont += "<p style='color:#e30000; font-size:1.1rem; max-width:600px; margin:5px 10px;'>" + GetLocalResourceObject("StringResource28") + "：<span id='order_id' style='font-weight:bold; color:#000;'>" + OrderID + "</span></p>";
            if (serId != "")
            {
                mail_cont += "<p style='color:#e30000; font-size:1.1rem; max-width:600px; margin:5px 10px;'>" + GetLocalResourceObject("StringResource60") + "：<span id='order_id' style='font-weight:bold; color:#000;'>" + serId + "</span></p>";
            }

            if (BookingID != "")
            {
                mail_cont += "<p style='color:#e30000; font-weight:bold; max-width:600px; margin:5px 10px;'>取餐編號：<span id='order_id' style='font-weight:normal; color:#000;'>" + BookingID + "</span></p>";
            }

            mail_cont += "<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0'>";
            mail_cont += "  <tr>";
            mail_cont += "    <th class='last' colspan='4' align='left' valign='middle' scope='col'>" + GetLocalResourceObject("StringResource29") + "：" + GS.Rename(OName, 1);
            if (MemID != null && MemID != "")
            {
                mail_cont += "(" + MemID + ")";
            }
            mail_cont += "</th>";
            mail_cont += "  </tr>";
            mail_cont += "  <tr>";
            mail_cont += "    <td align='right' valign='middle' style='width:15%;'>" + GetLocalResourceObject("StringResource30") + "</td>";
            mail_cont += "    <td align='left' valign='middle' style='width:35%;'><span style='color:#333;'>" + GS.Rename(OCell, 4) + "</span></td>";
            mail_cont += "    <td align='right' valign='middle' style='width:15%;'>" + GetLocalResourceObject("StringResource31") + "</td>";
            mail_cont += "    <td class='last' align='left' valign='middle' style='width:35%;'><span style='color:#333;'>" + GS.Rename(OTel, 4) + "</span></td>";
            mail_cont += "  </tr>";
            mail_cont += "  <tr>";
            mail_cont += "    <td align='right' valign='middle' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GetLocalResourceObject("StringResource32") + "</td>";
            mail_cont += "    <td class='last' colspan='3' align='left' valign='middle'>" + GS.Rename(OEmail, 5) + "</td>";
            mail_cont += "  </tr>";
            if (!string.IsNullOrEmpty(ident)) {
                mail_cont += $@"<tr>
                    <td align='right' valign='middle' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>
                    {GetLocalResourceObject("identResource1.Text")}
                    </td>
                    <td class='last' colspan='3' align='left' valign='middle'>{ident}</td>
                </tr>";
            }
            if (!string.IsNullOrEmpty(invoice_title)) {
                mail_cont += $@"<tr>
                    <td align='right' valign='middle' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>
                    {GetLocalResourceObject("invoice_title")}
                    </td>
                    <td class='last' colspan='3' align='left' valign='middle'>{invoice_title}</td>
                </tr>";
            }
            if (!string.IsNullOrEmpty(invoice))
            {
                mail_cont += $@"<tr>
                    <td align='right' valign='middle' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>
                    {GetLocalResourceObject("invoice_addr")}
                    </td>
                    <td class='last' colspan='3' align='left' valign='middle'>{invoice}</td>
                </tr>";
            }
            mail_cont += "</table>";
            mail_cont += "<br>";

            if (ShopType != 4 && !rlib.OrderData.allVirtualProd && rlib.OrderData.PayType!= "PCHomeIPL7")
            {
                mail_cont += "<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0'>";
                mail_cont += "  <tr>";
                mail_cont += "    <th class='last' colspan='4' align='left' valign='middle' scope='col'>" + GetLocalResourceObject("StringResource33") + "：" + GS.Rename(SName, 1);
                if (SSex == "1")
                {
                    mail_cont = mail_cont + GetLocalResourceObject("StringResource34");
                }
                else
                {
                    mail_cont = mail_cont + GetLocalResourceObject("StringResource35");
                }
                mail_cont += "</th>";
                mail_cont += "  </tr>";
                mail_cont += "  <tr>";
                mail_cont += "    <td align='right' valign='middle'>" + GetLocalResourceObject("StringResource36") + "</td>";
                mail_cont += "    <td class='last' colspan='3' align='left' valign='middle'><span style='color:#333;'>" + GS.Rename(Address, 7) + "</span></td>";
                mail_cont += "  </tr>";
                mail_cont += "  <tr>";
                mail_cont += "    <td align='right' valign='middle' style='width:15%;'>" + GetLocalResourceObject("StringResource30") + "</td>";
                mail_cont += "    <td align='left' valign='middle' style='width:35%;'><span style='color:#333;'>" + GS.Rename(SCell, 3) + "</span></td>";
                mail_cont += "    <td align='right' valign='middle' style='width:15%;'>" + GetLocalResourceObject("StringResource31") + "</td>";
                mail_cont += "    <td class='last' align='left' valign='middle' style='width:35%;'><span style='color:#333;'>" + GS.Rename(STel, 3) + "</span></td>";
                mail_cont += "  </tr>";
                mail_cont += "</table>";
                mail_cont += "<br>";
            }
            #endregion

            #region 備註
            if (f.FID != 0 || Notememo != "")
            {
                mail_cont += $@"
                <table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0'>
                    <tr>
                        <th class='last' colspan='4' align='left' valign='middle' scope='col'>{GetLocalResourceObject("StringResource37")}</th>
                    </tr>";

                if (f.FID == 0)
                {
                    mail_cont += "  <tr>";
                    mail_cont += "    <td class='last' colspan='4' align='left' valign='middle' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'><span style='color:#333;'>" + Notememo + "</span></td>";
                    mail_cont += "  </tr>";
                }
                else
                {
                    f.columnItems.ForEach(e =>
                    {
                        mail_cont += "  <tr>";
                        mail_cont += "    <td align='right' valign='middle'>" + e.title + "</td>";
                        mail_cont += "    <td class='last' colspan='3' align='left' valign='middle><span style='color:#333;'>" + e.value + "</span></td>";
                        mail_cont += "  </tr>";
                    });
                }
                mail_cont += "</table><br />";
            }
            #endregion

            #region 訂購明細
            int i = 0;
            int totalamt = 0;
            bool isProdSize = GS.GetSpec(setting, "sizeid");
            bool isProdColor = GS.GetSpec(setting, "colorid");
            int colNum = 8 - (isProdColor ? 0 : 1) - (isProdSize ? 0 : 1) - (disablePrice != "Y" ? 0 : 3);

            if (ShopType == 3)
            {
                String TakeMealType = rlib.OrderData.MenuLists.TakeMealType;

                mail_cont += "<table class='mail_tb cellDetail' width='600' border='1' cellspacing='0' cellpadding='0'>";
                mail_cont += "  <tr>";
                mail_cont += "  <th class='last no-border-bottom' colspan='6'>" + GS.GetMealType(rlib.OrderData.MenuLists.TakeMealType) + "</th>";
                mail_cont += "  </tr>";
                mail_cont += "  <tr>";
                mail_cont += "    <th class='first' align='left' scope='col' style='width:30%;'>" + GetLocalResourceObject("StringResource1") + "</th>";
                mail_cont += "    <th align='center' scope='col' colspan='3' style='width:10%;'></th>";
                mail_cont += "    <th align='center' scope='col' style='width:10%;'>" + GetLocalResourceObject("StringResource2") + "</th>";
                mail_cont += "    <th align='center' scope='col' style='width:10%;'>" + GetLocalResourceObject("StringResource3") + "</th>";
                mail_cont += "    <th {{discont}} align='center' scope='col' style='width:10%;'>" + GetLocalResourceObject("StringResource4") + "</th>";
                mail_cont += "    <th class='last' align='center' scope='col' style='width:10%;'>" + GetLocalResourceObject("StringResource5") + "</th>";
                mail_cont += "  </tr>";

                String ComboID = string.Empty;
                String ComboName = string.Empty;
                String ComboDiscount = string.Empty;
                String ComboQty = string.Empty;
                String MenuID = string.Empty;
                String MenuName = string.Empty;
                String SpecQty = string.Empty;
                String SpecString = string.Empty;
                Int32 SpecAmt = 0;
                Int32 MenuPrice = 0;
                Int32 MenuAddPrice = 0;
                Int32 MenuDiscount = 0;

                #region 購物車表身
                foreach (Shoppingcar.Menu Menu in rlib.OrderData.MenuLists.Menu)
                {
                    ComboID = Menu.ID;
                    ComboName = Menu.Name;
                    ComboDiscount = Menu.Discount;
                    ComboQty = Menu.Qty;
                    SpecAmt = 0;

                    if (ComboID == "Single")
                    {

                    }
                    else
                    {
                        SpecAmt = Convert.ToInt32((Convert.ToInt32(ComboQty) * double.Parse(GetProdPrice(ComboID, "", setting, "", ""))) + 0.001);

                        mail_cont += "       <tr>";
                        mail_cont += "           <td class='first'>" + ComboName + "</td>";
                        mail_cont += "           <td colspan='3'></td>";
                        mail_cont += "           <td>" + ComboQty + "</td>";
                        mail_cont += "           <td>" + GetProdPrice(ComboID, "", setting, "", "") + "</td>";
                        mail_cont += "           <td {{discont}}>" + ComboDiscount + "</td>";
                        mail_cont += "           <td class='last'>" + SpecAmt + "</td>";
                        mail_cont += "       </tr>";

                        totalamt = totalamt + SpecAmt;
                    }

                    foreach (Shoppingcar.MenuItem MenuItems in Menu.MenuItems)
                    {
                        MenuID = MenuItems.ID;
                        MenuName = MenuItems.Name;
                        MenuPrice = Convert.ToInt32(GetProdPrice(MenuID, "", setting, "", ""));

                        if (ComboID == "Single")
                        {
                            MenuAddPrice = 0;
                        }
                        else
                        {
                            MenuAddPrice = Convert.ToInt32(GetProdPrice(MenuID, "", setting, "", ComboID));
                        }

                        foreach (Shoppingcar.MenuSpec MenuSpec in MenuItems.MenuSpec)
                        {
                            SpecString = "";
                            SpecQty = MenuSpec.Qty;
                            SpecAmt = 0;
                            if (MenuSpec.OtherID != null)
                            {
                                for (int j = 0; j < MenuSpec.OtherID.Count; j++)
                                {
                                    SpecString += GetMemoName(setting, MenuSpec.OtherID[j]) + "$" + GetProdPrice(MenuID, "", setting, MenuSpec.OtherID[j], "") + " ";
                                    SpecAmt += Convert.ToInt32(GetProdPrice(MenuID, "", setting, MenuSpec.OtherID[j], ""));
                                }
                            }
                            if (MenuSpec.Memo != null)
                            {
                                for (int j = 0; j < MenuSpec.Memo.Count; j++)
                                {
                                    SpecString += MenuSpec.Memo[j].ToString() + " ";
                                }
                            }

                            if (ComboID == "Single")
                            {

                                MenuDiscount = 0 - SpecAmt;
                            }
                            else
                            {
                                MenuDiscount = MenuPrice - SpecAmt - MenuAddPrice;
                            }


                            mail_cont += "       <tr>";
                            mail_cont += "           <td>" + MenuName;
                            if (MenuAddPrice > 0)
                            {
                                mail_cont += "(需加價" + MenuAddPrice + ")";
                            }

                            mail_cont += "</td>";
                            mail_cont += "           <td>" + SpecString + "</td>";
                            mail_cont += "           <td>" + SpecQty + "</td>";
                            mail_cont += "           <td>" + MenuPrice + "</td>";
                            mail_cont += "           <td>" + MenuDiscount + "</td>";
                            mail_cont += "           <td>" + (MenuPrice - MenuDiscount) * Convert.ToInt32(SpecQty) + "</td>";
                            mail_cont += "       </tr>";

                            totalamt = totalamt + (MenuPrice - MenuDiscount) * Convert.ToInt32(SpecQty);
                        }
                    }
                }
                #endregion


            }
            else
            {
                int totalDisCount = 0;
                mail_cont += "<table id='cellDetail' class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0'>";
                mailTableHead += "  <tr>";
                mailTableHead += "    <th class='first' align='left' scope='col' style='width:30%;'>" + GetLocalResourceObject("StringResource1") + "</th>";
                mailTableHead += "    <th align='center' scope='col' style='width:10%;'>" + GetLocalResourceObject("StringResource38") + "</th>";
                if (isProdColor)
                    mailTableHead += "    <th align='center' scope='col' style='width:10%;'>" + GS.GetSpecTitle(setting, "1") + "</th>";
                if (isProdSize)
                    mailTableHead += "    <th align='center' scope='col' style='width:10%;'>" + GS.GetSpecTitle(setting, "2") + "</th>";
                mailTableHead += "    <th align='center' scope='col' style='width:10%;'>" + GetLocalResourceObject("StringResource2") + "</th>";
                if (disablePrice != "Y")
                {
                    mailTableHead += "    <th align='center' scope='col' style='width:10%;'>" + GetLocalResourceObject("StringResource3") + "</th>";
                    mailTableHead += "    <th {{discount}} align='center' scope='col' style='width:10%;'>" + GetLocalResourceObject("StringResource4") + "</th>";
                    mailTableHead += "    <th class='last' align='center' scope='col' style='width:10%;'>" + GetLocalResourceObject("StringResource5") + "</th>";
                }
                mailTableHead += "  </tr>";
                if (disablePrice != "S") mail_cont += mailTableHead;
                foreach (Shoppingcar.OrderList Orders in rlib.OrderData.OrderLists)
                {
                    if (disablePrice == "S")
                    {
                        mail_cont += "  <tr>";
                        mail_cont += "    <th class='first last no-border-bottom' colspan='" + colNum + "' align='left'>" + "<b style='font-size: 1.2rem;'>" + Orders.Title + "</b></th>";
                        mail_cont += "  </tr>";
                        mail_cont += mailTableHead;
                    }
                    int localPrice = 0;
                    foreach (Shoppingcar.OrderItem Items in Orders.OrderItems)
                    {

                        #region 購買項目
                        String ProdItemno = "";
                        using (SqlConnection conn = new SqlConnection(setting))
                        {
                            conn.Open();
                            cmd = new SqlCommand("select itemno from prod where id=@id", conn);
                            cmd.Connection = conn;
                            cmd.Parameters.Add(new SqlParameter("@id", Items.ID));
                            SqlDataReader reader = cmd.ExecuteReader();
                            try
                            {
                                while (reader.Read())
                                {
                                    ProdItemno = reader[0].ToString();
                                }
                            }
                            finally
                            {
                                reader.Close();
                            }
                        }
                        foreach (Shoppingcar.OrderSpec OrderSpecs in Items.OrderSpecs)
                        {

                            mail_cont += "  <tr>";
                            mail_cont += "    <td class='first' align='left' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>";
                            if (Orders.Type != 0)
                            {
                                mail_cont += "<span style='color:#ff0000;'>" + Orders.Title + "</span> - ";
                            }
                            mail_cont += Items.Name + "</td>";
                            mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + ProdItemno + "</td>";
                            if (isProdColor)
                                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GS.GetSpec(setting, "prod_color", OrderSpecs.Color) + "</td>";
                            if (isProdSize)
                                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GS.GetSpec(setting, "prod_size", OrderSpecs.Size) + "</td>";
                            mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + OrderSpecs.Qty + "</td>";
                            if (disablePrice != "Y")
                            {
                                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + OrderSpecs.Price + "</td>";
                                mail_cont += "    <td {{discount}} align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + OrderSpecs.Discount + "</td>";

                                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'>";
                                if (Convert.ToInt32(OrderSpecs.Discount) != 0)
                                {
                                    mail_cont += "    <s style='font-size:9pt; color:#bbbbbb; padding-right:3px;'>" + Convert.ToInt32((OrderSpecs.Qty * OrderSpecs.Price) + 0.001) + "</s>";
                                }
                                mail_cont += (Convert.ToInt32((OrderSpecs.Qty * OrderSpecs.Price) + 0.001) - OrderSpecs.Discount);

                                if (Convert.ToInt32(OrderSpecs.Bonus) > 0)
                                {
                                    mail_cont += "<br>(" + GetLocalResourceObject("StringResource53") + OrderSpecs.Bonus + ")";
                                }
                                mail_cont += "</td>";
                            }
                            mail_cont += "  </tr>";
                            int prod_price = Convert.ToInt32((OrderSpecs.Price * OrderSpecs.Qty) + 0.001) - OrderSpecs.Discount;
                            localPrice += prod_price;
                            totalamt += prod_price;
                            totalDisCount += Convert.ToInt32(OrderSpecs.Discount);
                            i = i + 1;
                        }
                        #endregion

                        #region 加購項目
                        foreach (Shoppingcar.AdditionalItem AdditionalItems in Items.AdditionalItems)
                        {
                            using (SqlConnection conn = new SqlConnection(setting))
                            {
                                conn.Open();
                                cmd = new SqlCommand("select itemno from prod where id=@id", conn);
                                cmd.Connection = conn;
                                cmd.Parameters.Add(new SqlParameter("@id", AdditionalItems.ID));
                                SqlDataReader reader = cmd.ExecuteReader();
                                try
                                {
                                    while (reader.Read())
                                    {
                                        ProdItemno = reader[0].ToString();
                                    }
                                }
                                finally
                                {
                                    reader.Close();
                                }
                            }
                            mail_cont += "  <tr>";
                            mail_cont += "    <td class='first' align='left'><span style='color:#ff0000;'>" + GetLocalResourceObject("StringResource9") + "-</span>" + AdditionalItems.Name + "</td>";
                            mail_cont += "    <td align='center'>" + ProdItemno + "</td>";
                            if (isProdColor)
                                mail_cont += "    <td align='center'>" + GS.GetSpec(setting, "prod_color", AdditionalItems.Color) + "</td>";
                            if (isProdSize)
                                mail_cont += "    <td align='center'>" + GS.GetSpec(setting, "prod_size", AdditionalItems.Size) + "</td>";
                            mail_cont += $"    <td{(disablePrice == "Y"?" class='last'":"")} align='center'>{AdditionalItems.Qty}</td>";
                            if (disablePrice != "Y")
                            {
                                mail_cont += "    <td align='center'>" + AdditionalItems.Price + "</td>";
                                mail_cont += "    <td {{discount}} align='center'>" + AdditionalItems.Discount + "</td>";
                                mail_cont += "    <td class='last' align='center'>";
                                if (Convert.ToInt32(AdditionalItems.Discount) != 0)
                                {
                                    mail_cont += "    <s style='font-size:9pt; color:#bbbbbb; padding-right:3px;'>" + (Convert.ToInt32(AdditionalItems.Qty) * Convert.ToInt32(AdditionalItems.Price)) + "</s>";
                                }
                                mail_cont += (Convert.ToInt32(Convert.ToInt32(AdditionalItems.Qty) * Convert.ToInt32(AdditionalItems.Price) + 0.001) - Convert.ToInt32(AdditionalItems.Discount)) + "</td>";
                            }
                            mail_cont += "  </tr>";
                            int prod_price = Convert.ToInt32((Convert.ToInt32(AdditionalItems.Price) * Convert.ToInt32(AdditionalItems.Qty)) + 0.001) - Convert.ToInt32(AdditionalItems.Discount);
                            localPrice += prod_price;
                            totalamt += prod_price;
                            totalDisCount += Convert.ToInt32(AdditionalItems.Discount);
                            i = i + 1;
                        }
                        #endregion

                    }
                    if (rlib.OrderData.ShopType != 4)
                    {
                        if (disablePrice == "S")
                        {
                            mail_cont += "  <tr>";
                            mail_cont += "    <td class='first' colspan='" + colNum + "' align='right'>" + GetLocalResourceObject("StringResource5") + "<b style='font-size: 1.2rem;color: #f00; margin-left: 1rem;'>NT$" + (localPrice) + "</b></td>";
                            mail_cont += "  </tr>";
                        }
                        if (servicePriceType == "L")
                        {
                            mail_cont += "  <tr>";
                            mail_cont += "    <td class='last' colspan='" + colNum + "' align='right'>" + GetLocalResourceObject("servicePrice") + "<b style='font-size: 1.2rem;color: #f00; margin-left: 1rem;'>NT$" + (servicePrice) + "</b></td>";
                            mail_cont += "  </tr>";
                        }
                    }
                }
                if (totalDisCount == 0)
                {
                    mail_cont = mail_cont.Replace("{{discount}}", "class='hide'");
                }
                else
                {
                    mail_cont = mail_cont.Replace(" {{discount}}", "");
                }
            }
            #endregion

            #region 服務費
            if (disablePrice != "Y" && servicePriceSum != 0) {
                mail_cont += $@"<tr>
                    <td class='first last' colspan='{colNum}' align='right'>
                        {(servicePriceType=="L"? GetLocalResourceObject("servicePriceSum"):GetLocalResourceObject("servicePrice"))}
                        <b style='font-size: 1rem;color: #f00; margin-left: 1rem;'>NT${servicePriceSum}</b>
                    </td>
                </tr>";
            }
            #endregion

            #region 折扣金額
            if (disablePrice != "Y" && (Convert.ToInt32(DiscountAmt) > 0 || Convert.ToInt32(DiscountAmt) < 0))
            {
                mail_cont += $@"<tr>
                    <td class='first last' colspan='{colNum}' align='right'>
                        {GetLocalResourceObject("StringResource39")}
                        <b style='font-size: 1.2rem;color: #f00; margin-left: 1rem;'>NT${(DiscountAmt * (-1))}</b>
                    </td>
                </tr>";
            }
            #endregion

            #region 運費
            if (disablePrice != "Y" && !rlib.OrderData.allVirtualProd)
            {
                mail_cont += $@"<tr>
                    <td class='first last' colspan='{colNum}' align='right'>
                        {GetLocalResourceObject("StringResource15")}
                        <b style='font-size: 1rem;color: #f00; margin-left: 1rem;'>NT${FreightAmount}</b>
                    </td>
                </tr>";
            }
            #endregion

            #region 總計
            if (ShopType != 4)
            {
                mail_cont += "  <tr>";
                mail_cont += "    <td class='first last' colspan='" + colNum + "' align='right'>" + GetLocalResourceObject("StringResource16") + "<b style='font-size: 18pt;color: #f00; margin-left: 1rem;'>NT$" + (totalamt + FreightAmount - DiscountAmt + servicePriceSum) + "</b></td>";
                mail_cont += "  </tr>";
            }
            #endregion

            #region 其他折抵
            if (Int32.Parse(BonusDiscount.ToString()) > 0 || rlib.OrderData.couDiscont != 0)
            {
                #region 優惠券折抵
                if (rlib.OrderData.couDiscont != 0)
                {
                    mail_cont += $@"<tr>
                        <td class='first last' colspan='{colNum}' align='right'>
                            {("優惠券(" + rlib.OrderData.couponTitle + ")折抵")}
                            <b style='font-size: 1rem;color: #f00; margin-left: 1rem;'>NT${rlib.OrderData.couDiscont}</b>
                        </td>
                    </tr>";
                }
                #endregion
                #region 紅利扣抵
                if (Int32.Parse(BonusDiscount.ToString()) > 0)
                {
                    mail_cont += $@"<tr>
                        <td class='first last' colspan='{colNum}' align='right'>
                            {GetLocalResourceObject("StringResource40")}
                            <b style='font-size: 1rem;color: #f00; margin-left: 1rem;'>NT${BonusDiscount}</b>
                        </td>
                    </tr>";
                }
                #endregion

                int t = totalamt - DiscountAmt - Convert.ToInt32(BonusDiscount) - rlib.OrderData.couDiscont + servicePriceSum;
                if (t < 0) t = Convert.ToInt32(FreightAmount);
                else t += Convert.ToInt32(FreightAmount);
                mail_cont += $@"<tr>
                    <td class='first last' colspan='{colNum}' align='right'>
                        {GetLocalResourceObject("StringResource41")}
                        <b style='font-size: 1.5rem;color: #f00; margin-left: 1rem;'>NT${t}</b>
                    </td>
                </tr>";
            }
            mail_cont += "</table>";
            mail_cont += "<br>";
            #endregion
            if (ShopType != 4)
            {
                #region 取貨方式
                String ReceiverStoreName = "";
                String ReceiverStoreAddr = "";
                String LogisticsTypeName = "";

                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    cmd = new SqlCommand("select b.title,a.ReceiverStoreName,a.ReceiverStoreAddr from orders_Logistics as a left join Logisticstype as b on a.LogisticstypeID=b.id where order_no=@orderNo", conn);
                    cmd.Parameters.Add(new SqlParameter("@orderNo", OrderID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            LogisticsTypeName = reader[0].ToString();
                            ReceiverStoreName = reader[1].ToString();
                            ReceiverStoreAddr = reader[2].ToString();
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
                if (!rlib.OrderData.allVirtualProd)
                {
                    
                    mail_cont += "<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0'>";
                    mail_cont += "  <tr>";
                    mail_cont += "    <th class='last' align='left' scope='col'>取貨方式：<b style='font-size: 18pt;color: #f00;'>" + LogisticsTypeName;
                    if (ReceiverStoreName != "")
                    {
                        mail_cont += "(" + ReceiverStoreName + " / " + ReceiverStoreAddr + ")";
                    }
                    mail_cont += "</b></th>";
                    mail_cont += "  </tr>";
                    mail_cont += "</table>";   
                }
                #endregion

                #region 取貨日
                if (rlib.OrderData.deliveryDate != "" && !rlib.OrderData.allVirtualProd)
                {
                    mail_cont += "<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0'>";
                    mail_cont += "  <tr>";
                    mail_cont += "    <th class='last' align='left' scope='col'>" + GetLocalResourceObject("StringResource55") + ":<b style='font-size: 18pt;color: #f00;'>";
                    mail_cont += "(" + rlib.OrderData.deliveryDate + ")";
                    mail_cont += "</b></th></tr></table>";
                }
                #endregion

                #region 匯款方式
                String StoreName = "";
                if (BookingID != "")
                {
                    using (SqlConnection conn2 = new SqlConnection(setting))
                    {
                        conn2.Open();
                        SqlCommand cmd3 = new SqlCommand("select shopname from orders_hd where id=@id", conn2);
                        cmd3.Parameters.Add(new SqlParameter("@id", OrderID));
                        SqlDataReader reader2 = cmd3.ExecuteReader();
                        try
                        {
                            while (reader2.Read())
                            {
                                StoreName = reader2[0].ToString();
                            }
                        }
                        finally
                        {
                            reader2.Close();
                        }
                    }
                }
                mail_cont += "<br>";
                mail_cont += "<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0'>";
                mail_cont += "  <tr>";
                mail_cont += "    <th class='last' align='left' scope='col'>" + GetLocalResourceObject("StringResource18") + "：<b style='font-size: 18pt;color: #f00;'>" + GS.GetPayType(setting,rlib.OrderData.PayType);
                if (StoreName != "")
                {
                    mail_cont += "(" + StoreName + ")";
                }

                mail_cont += "</b></th>";
                mail_cont += "  </tr>";
                #endregion

                #region 匯款資訊
                String payment_type = GS.GetPayType(setting,rlib.OrderData.PayType);
                if (payment_type == "ATM")
                {
                    mail_cont += "  <tr>";
                    mail_cont += "    <td class='last' align='left'><span style='color:#f00;'>" + GetLocalResourceObject("StringResource42") + "</span></td>";
                    mail_cont += "  </tr>";
                }
                if (rlib.OrderData.PayType == "POST")
                {
                    mail_cont += "  <tr>";
                    mail_cont += "    <td class='last' align='left'><span style='color:#f00;'>" + GetLocalResourceObject("StringResource56") + "</span></td>";
                    mail_cont += "  </tr>";
                }
                mail_cont += "</table>";
                mail_cont += "<br>";

                if (payment_type == "ATM")
                {

                    mail_cont += "<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0' style='border:none;border-width: 0px;margin-left:10px;margin-right:10px; width:600px; font-size:9pt; font-family:'microsoft jhenghei', sans-serif;'>";
                    mail_cont += "  <tr>";
                    mail_cont += "    <th class='last' colspan='2' align='left' scope='col'>" + GetLocalResourceObject("StringResource43") + "</th>";
                    mail_cont += "  </tr>";
                    mail_cont += "  <tr>";
                    mail_cont += "    <td align='right'><strong>" + GetLocalResourceObject("StringResource44") + "</strong></td>";
                    mail_cont += "    <td class='last' align='left'>" + remit_money1 + "</td>";
                    mail_cont += "  </tr>";
                    mail_cont += "  <tr>";
                    mail_cont += "    <td align='right'><strong>" + GetLocalResourceObject("StringResource45") + "</strong></td>";
                    mail_cont += "    <td class='last' align='left'>" + remit_money2 + "</td>";
                    mail_cont += "  </tr>";
                    mail_cont += "  <tr>";
                    mail_cont += "    <td align='right'><strong>" + GetLocalResourceObject("StringResource46") + "</strong></td>";
                    mail_cont += "    <td class='last'>" + remit_money3 + "</td>";
                    mail_cont += "  </tr>";
                    mail_cont += "  <tr>";
                    mail_cont += "    <td align='right'><strong>" + GetLocalResourceObject("StringResource47") + "</strong></td>";
                    int t = totalamt - DiscountAmt - Convert.ToInt32(BonusDiscount) - rlib.OrderData.couDiscont + servicePriceSum;
                    if (t < 0) t = FreightAmount;
                    else t += FreightAmount;
                    mail_cont += "    <td class='last' align='left'>NT$" + t + "</td>";
                    mail_cont += "  </tr>";
                    mail_cont += "  <tr>";
                    mail_cont += "    <td align='right'><strong style='color:#f00;'>" + GetLocalResourceObject("StringResource48") + "</strong></td>";
                    mail_cont += "    <td class='last' align='left'><span style='color:#f00;'>";
                    if (rlib.OrderData.deliveryDate != "")
                    {
                        mail_cont += GetLocalResourceObject("payout");
                    }
                    else
                    {
                        mail_cont += DateTime.Today.AddDays(1).ToString("yyyy/MM/dd");
                    }
                    mail_cont += "    </span></td>";
                    mail_cont += "  </tr>";
                    mail_cont += "</table>";
                }
                if (rlib.OrderData.PayType == "POST")
                {

                    mail_cont += "<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0' style='border:none;border-width: 0px;margin-left:10px;margin-right:10px; width:600px; font-size:9pt; font-family:'microsoft jhenghei', sans-serif;'>";
                    mail_cont += "  <tr>";
                    mail_cont += "    <th class='last' colspan='2' align='left' scope='col'>" + GetLocalResourceObject("StringResource43") + "</th>";
                    mail_cont += "  </tr>";
                    mail_cont += "  <tr>";
                    mail_cont += "    <td align='right'><strong>" + GetLocalResourceObject("StringResource57") + "</strong></td>";
                    mail_cont += "    <td class='last' align='left'>" + remit_money4 + "</td>";
                    mail_cont += "  </tr>";
                    mail_cont += "  <tr>";
                    mail_cont += "    <td align='right'><strong>" + GetLocalResourceObject("StringResource46") + "</strong></td>";
                    mail_cont += "    <td class='last' align='left'>" + remit_money5 + "</td>";
                    mail_cont += "  </tr>";
                    mail_cont += "  <tr>";
                    mail_cont += "    <td align='right'><strong>" + GetLocalResourceObject("StringResource47") + "</strong></td>";
                    int t = totalamt - DiscountAmt - Convert.ToInt32(BonusDiscount) - rlib.OrderData.couDiscont + servicePriceSum;
                    if (t < 0) t = FreightAmount;
                    else t += FreightAmount;
                    mail_cont += "    <td class='last' align='left'>NT$" + t + "</td>";
                    mail_cont += "  </tr>";
                    mail_cont += "  <tr>";
                    mail_cont += "    <td align='right' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'><strong style='color:#f00;'>" + GetLocalResourceObject("StringResource48") + "</strong></td>";
                    mail_cont += "    <td align='left' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'><span style='color:#f00;'>" + DateTime.Today.AddDays(1).ToString("yyyy/MM/dd") + " 23:59</span></td>";
                    mail_cont += "  </tr>";
                    mail_cont += "</table>";
                }
                #endregion
            }
            #region 表尾注意事項
            if (string.IsNullOrEmpty(f.signature))
            {
                mail_cont += "<hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px;'>";
                mail_cont += "<span style='color:#ff0000;'>" + GetLocalResourceObject("StringResource49") + "</span><br>";
                mail_cont += "<span style='color:#ff0000;'>" + GetLocalResourceObject("StringResource50") + "</span>";
                mail_cont += "<hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px;'>";
            }
            else {
                mail_cont += f.signature;
            }
            #endregion
            mail_cont += $@"
                    </div>
                </body>
            </html>";
            #endregion
            return mail_cont;
        }
        #endregion

        #region 取得生肖
        public String GetAnimal(String Year)
        {
            String[] Animal = { "鼠", "牛", "虎", "兔", "龍", "蛇", "馬", "羊", "猴", "雞", "狗", "豬" };

            int birthpet;
            try
            {
                birthpet = Convert.ToInt32(Year) % 12;           //西元年除12取餘數            
                birthpet = birthpet - 3;        //餘數-3            
                if (birthpet < 0)               //判斷餘數是否大於0，小於0必須+12
                {
                    birthpet = birthpet + 12;
                }

                return Animal[birthpet - 1].ToString();
            }
            catch
            {
                return "";
            }
        }
        #endregion

        #region 轉換農曆日期
        private String GetLDate(DateTime Date)
        {
            TaiwanLunisolarCalendar tlc = new TaiwanLunisolarCalendar();
            //tlc.MaxSupportedDateTime.ToShortDateString();       // 取得目前支援的農曆日曆到幾年幾月幾日( 2051-02-10 );
            // 取得今天的農曆年月日
            String LDate = tlc.GetYear(Date).ToString() + "/" + tlc.GetMonth(Date).ToString() + "/" + tlc.GetDayOfMonth(Date).ToString();
            return LDate;
        }
        #endregion

        #region 製作點燈table
        private void AddTableControls()
        {
            TableCont[] newArray = (TableCont[])ViewState["TableCont"];
            List<TableCont> newList = new List<TableCont>(newArray);

            String ProdNAme = "";
            String ProdID = "";
            Label myLabel1;
            Label myLabel2;
            TextBox myTextBox;
            DropDownList ddl;
            HiddenField HF;

            #region 寫入table內容

            TableRow tr;
            TableCell tc;

            for (int pn = 0; pn < newList.Count; pn++)
            {
                ProdID = newList[pn].ProdID;
                ProdNAme = newList[pn].ProdName;

                tr = new TableRow();
                tc = new TableCell();
                myLabel1 = new Label();
                HF = new HiddenField();

                myLabel1.Text = ProdNAme;
                myLabel1.Attributes.Add("name", "title");
                myLabel1.Attributes.Add("style", "background-color:#E0EDF3; padding:10px 5px;");
                HF.Value = ProdID;
                HF.ID = "ProdID" + pn.ToString();

                tc.Controls.Add(myLabel1);
                tc.Controls.Add(HF);
                tc.ColumnSpan = 2;
                tr.Cells.Add(tc);
                Table1.Rows.Add(tr);


                for (int i = 0; i < Convert.ToInt32(newList[pn].Qty); i++)
                {
                    #region 欄位設定
                    tr = new TableRow();

                    #region 第一列設定
                    myLabel1 = new Label();
                    myLabel1.Text = "*姓名";
                    myLabel1.Attributes.Add("name", "label");

                    myTextBox = new TextBox();
                    myTextBox.ID = "CHName" + pn.ToString() + i.ToString();
                    myTextBox.Attributes.Add("maxlength", "5");
                    myTextBox.Attributes.Add("inputname", "CHName");

                    tc = new TableCell();
                    tc.Controls.Add(myLabel1);
                    tc.Controls.Add(myTextBox);
                    tr.Cells.Add(tc);

                    myLabel1 = new Label();
                    myLabel1.Text = "住家/公司電話";
                    myLabel1.Attributes.Add("name", "label");

                    myTextBox = new TextBox();
                    myTextBox.ID = "Tel" + pn.ToString() + i.ToString();
                    myTextBox.Attributes.Add("maxlength", "10");
                    myTextBox.Attributes.Add("inputname", "ATel");

                    tc = new TableCell();
                    tc.Controls.Add(myLabel1);
                    tc.Controls.Add(myTextBox);
                    tr.Cells.Add(tc);

                    Table1.Rows.Add(tr);
                    #endregion

                    #region 第二列設定
                    tr = new TableRow();

                    myLabel1 = new Label();
                    myLabel1.Text = "*地址";
                    myLabel1.Attributes.Add("name", "label");

                    myTextBox = new TextBox();
                    myTextBox.ID = "Address" + pn.ToString() + i.ToString();
                    myTextBox.Attributes.Add("maxlength", "50");
                    myTextBox.Attributes.Add("inputname", "Address");

                    tc = new TableCell();
                    tc.Controls.Add(myLabel1);
                    tc.Controls.Add(myTextBox);
                    tr.Cells.Add(tc);

                    Table1.Rows.Add(tr);
                    #endregion

                    #region 第四列設定
                    tr = new TableRow();

                    myLabel1 = new Label();
                    myLabel1.Text = "*行動電話";
                    myLabel1.Attributes.Add("name", "label");

                    myTextBox = new TextBox();
                    myTextBox.ID = "CellPhone" + pn.ToString() + i.ToString();
                    myTextBox.Attributes.Add("maxlength", "10");
                    myTextBox.Attributes.Add("inputname", "cellphone");

                    tc = new TableCell();
                    tc.Controls.Add(myLabel1);
                    tc.Controls.Add(myTextBox);
                    tr.Cells.Add(tc);

                    Table1.Rows.Add(tr);
                    #endregion

                    #region 第三列設定
                    tr = new TableRow();

                    myLabel1 = new Label();
                    myLabel1.Text = "*生日";
                    myLabel1.Attributes.Add("name", "label");

                    myTextBox = new TextBox();
                    myTextBox.ID = "Birth" + pn.ToString() + i.ToString();
                    myTextBox.Attributes.Add("readonly", "readonly");
                    myTextBox.Attributes.Add("inputname", "Birth");

                    ddl = new DropDownList();
                    ddl.ID = "Hour" + pn.ToString() + i.ToString();
                    ddl.Attributes.Add("inputname", "Hour");
                    for (int h = 0; h <= 23; h++)
                    {
                        ddl.Items.Add(new ListItem(h.ToString(), h.ToString()));
                    }

                    myLabel2 = new Label();
                    myLabel2.Text = "時";
                    myLabel2.Attributes.Add("name", "label");

                    tc = new TableCell();
                    tc.Controls.Add(myLabel1);
                    tc.Controls.Add(myTextBox);
                    tc.Controls.Add(ddl);
                    tc.Controls.Add(myLabel2);
                    tr.Cells.Add(tc);
                    Table1.Rows.Add(tr);
                    #endregion

                    #endregion
                }

                HF = new HiddenField();
                HF.ID = "Stop" + pn.ToString();
                HF.Value = "Stop";
                tc = new TableCell();
                tc.Controls.Add(HF);
                tr.Cells.Add(tc);
                Table1.Rows.Add(tr);
            }
            #endregion
        }

        [Serializable]
        public class TableCont
        {
            public string ProdName { get; set; }
            public string ProdID { get; set; }
            public string Qty { get; set; }
        }
        #endregion

        #region 取得產品價格
        private String GetProdPrice(String ProdID, String MemberLabel, String setting, String OptionID, String ComboID)
        {
            String Value = "0";
            if (OptionID == "" && ComboID == "")
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    String Str_sql = "select value2,value3 from prod where id=@id";
                    SqlCommand cmd = new SqlCommand(Str_sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@id", ProdID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (MemberLabel == "VIP")
                                {
                                    Value = reader[1].ToString();
                                }
                                else
                                {
                                    Value = reader[0].ToString();
                                }
                            }
                        }
                    }
                    finally { reader.Close(); }
                }
            }
            else if (OptionID == "" && ComboID != "")
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    String Str_sql = "select price from Meal_Detail where fid in (select id from Meal_Sub where fid = @fid) and pid=@pid";
                    SqlCommand cmd = new SqlCommand(Str_sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@fid", ComboID));
                    cmd.Parameters.Add(new SqlParameter("@pid", ProdID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Value = reader[0].ToString();
                            }
                        }
                    }
                    finally { reader.Close(); }
                }
            }
            else if (OptionID != "" && ComboID == "")
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    String Str_sql = "select price from dbo.Meal_Detail_Memo where pid=@pid and Optionid=@Optionid";
                    SqlCommand cmd = new SqlCommand(Str_sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@pid", ProdID));
                    cmd.Parameters.Add(new SqlParameter("@Optionid", OptionID));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Value = reader[0].ToString();
                            }
                        }
                    }
                    finally { reader.Close(); }
                }
            }
            return Value;
        }
        #endregion        

        #region 取得加價客製名稱
        private String GetMemoName(String setting, String OptionID)
        {
            String ReturnStr = "";
            //select * from dbo.Meal_Options
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                String Str_sql = "select title from dbo.Meal_Options where id=@id";
                SqlCommand cmd = new SqlCommand(Str_sql, conn);
                cmd.Parameters.Add(new SqlParameter("@id", OptionID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ReturnStr = reader[0].ToString();
                        }
                    }
                }
                finally { reader.Close(); }
            }
            return ReturnStr;
        }
        #endregion

        #region 儲存物流資料
        private void SaveOrdersLogistics(String Setting, String OrderID, String LogisticstypeID, String LogisticApi, String GoodsAmount, String CollectionAmount, String ReceiverStoreID, String ReceiverStoreName, String ReceiverStoreAddr, String ReceiverStoreTel, String Temperature)
        {
            String LogisticsType = "";
            String LogisticsSubType = "";

            switch (LogisticApi)
            {
                case "ecpay":

                    if (LogisticstypeID == "002")       //宅配
                    {
                        LogisticsType = "Home";
                        LogisticsSubType = "";          //等出貨再讓管理者挑黑貓OR宅配通
                    }
                    else if (LogisticstypeID == "003" || LogisticstypeID == "004")      //超商取貨
                    {
                        LogisticsType = "CVS";

                        using (SqlConnection conn = new SqlConnection(Setting))
                        {
                            conn.Open();

                            SqlCommand cmd = new SqlCommand("select ecpaycode from Logisticstype where id=@id", conn);
                            cmd.Parameters.Add(new SqlParameter("@id", LogisticstypeID));
                            SqlDataReader reader = cmd.ExecuteReader();
                            try
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        LogisticsSubType = reader[0].ToString();

                                    }
                                }
                            }

                            finally { reader.Close(); }
                        }
                    }
                    else            //郵局寄送 OR 自取面交
                    {
                        LogisticsType = "";
                        LogisticsSubType = "";
                    }


                    break;
                default:        //沒接綠界不用給值
                    LogisticsType = "";
                    LogisticsSubType = "";
                    break;
            }


            String IsCollection = "N";
            if (CollectionAmount == "0")
            {
                IsCollection = "N";
            }
            else
            {
                IsCollection = "Y";
            }

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                //20140331有更新此預存程序!!!!(新增郵遞區號,縣市,鄉鎮區)
                conn.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "Insert_orders_Logistics";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                cmd.Parameters.Add(new SqlParameter("@LogisticsType", LogisticsType));
                cmd.Parameters.Add(new SqlParameter("@LogisticsSubType", LogisticsSubType));
                cmd.Parameters.Add(new SqlParameter("@GoodsAmount", GoodsAmount));
                cmd.Parameters.Add(new SqlParameter("@CollectionAmount", CollectionAmount));
                cmd.Parameters.Add(new SqlParameter("@IsCollection", IsCollection));
                cmd.Parameters.Add(new SqlParameter("@ReceiverStoreID", ReceiverStoreID));
                cmd.Parameters.Add(new SqlParameter("@ReceiverStoreName", ReceiverStoreName));
                cmd.Parameters.Add(new SqlParameter("@ReceiverStoreAddr", ReceiverStoreAddr));
                cmd.Parameters.Add(new SqlParameter("@ReceiverStoreTel", ReceiverStoreTel));
                cmd.Parameters.Add(new SqlParameter("@LogisticApi", LogisticApi));
                cmd.Parameters.Add(new SqlParameter("@LogisticstypeID", LogisticstypeID));
                cmd.Parameters.Add(new SqlParameter("@Temperature", Temperature??""));
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        protected void CustomValidator1_ServerValidate(object source, ServerValidateEventArgs args)
        {

        }
    }
}