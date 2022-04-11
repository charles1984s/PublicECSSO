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

namespace ECSSO
{
    public partial class Shopping2 : System.Web.UI.Page
    {
        [Serializable]
        public class TableCont
        {
            public string ProdName { get; set; }
            public string ProdID { get; set; }
            public int Qty { get; set; }
            public double Price { get; set; }
            public int Discount { get; set; }
            public string Spec1 { get; set; }
            public string Spec2 { get; set; }
        }

        [Serializable]
        public class BasicCont
        {
            public string WebTitle { get; set; }
            public string discount_range { get; set; }
            public string discount_price { get; set; }            
            public string BonusDiscount { get; set; }
            public int FreightAmount { get; set; }
            public string MemID { get; set; }
            public int BonusAmt { get; set; }
            public string PayType { get; set; }
            public string OrgName { get; set; }
        }

        #region //語系變換
        private string str_language = string.Empty;
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
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/shopping2.css");
            objLink.Attributes.Add("type", "text/css");
            this.Page.Header.Controls.Add(objLink);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            
            GetStr GS = new GetStr();
            this.language.Value = str_language;
            Shoppingcar.RootObject rlib;
            if (ViewState["TableCont"] == null)
            {
                if (!IsPostBack)
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
                    string output = GS.ReplaceStr(Request.Form["orderData"].ToString());

                    if (ChkJson(output))
                    {
                        jsonStr.Value = output;

                        rlib = JsonConvert.DeserializeObject<Shoppingcar.RootObject>(output);

                        String OrgName = rlib.OrderData.OrgName;
                        String setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
                        String discount_range = "0";
                        String discount_price = "0";

                        Page.Title = rlib.OrderData.WebTitle;

                        List<TableCont> TableList = new List<TableCont>();
                        foreach (Shoppingcar.OrderList Lists in rlib.OrderData.OrderLists)
                        {
                            foreach (Shoppingcar.OrderItem Items in Lists.OrderItems)
                            {
                                foreach (Shoppingcar.OrderSpec Specs in Items.OrderSpecs)
                                {
                                    TableList.Add(
                                        new TableCont()
                                        {
                                            ProdName = Items.Name,
                                            ProdID = Items.ID,
                                            Qty = Specs.Qty,
                                            Price = Specs.Price,
                                            Discount = Specs.Discount,
                                            Spec1 = GS.GetSpec(setting, "prod_color", Specs.Color),
                                            Spec2 = GS.GetSpec(setting, "prod_size", Specs.Size)
                                        });
                                }
                            }
                        }

                        
                        using (SqlConnection conn = new SqlConnection(setting))
                        {

                            conn.Open();
                            SqlCommand cmd = new SqlCommand("select isnull(discount_range,0),isnull(discount_price,0) from head", conn);
                            SqlDataReader reader = cmd.ExecuteReader();
                            try
                            {
                                while (reader.Read())
                                {
                                    discount_range = reader[0].ToString();
                                    discount_price = reader[1].ToString();
                                }
                            }
                            finally
                            {
                                reader.Close();
                            }
                        }
                        
                        List<BasicCont> BC = new List<BasicCont>();
                        BC.Add(new BasicCont
                        {
                            BonusAmt = rlib.OrderData.BonusAmt,
                            BonusDiscount = rlib.OrderData.BonusDiscount,
                            discount_price = discount_price,
                            discount_range = discount_range,
                            FreightAmount = rlib.OrderData.FreightAmount,
                            MemID = rlib.OrderData.MemID,
                            PayType = GS.GetPayType(setting,rlib.OrderData.PayType),
                            WebTitle = rlib.OrderData.WebTitle,
                            OrgName = rlib.OrderData.OrgName
                        });
                        
                        ViewState.Add("BasicCont", BC.ToArray());
                        ViewState.Add("TableCont", TableList.ToArray());
                        AddTableControls();
                        #region 購物車會員資料
                        if (rlib.OrderData.MemID != "")
                        {
                            using (SqlConnection conn = new SqlConnection(setting))
                            {
                                conn.Open();
                                String Str_sql = "select ch_name,email,tel,sex,cell_phone,addr from cust where mem_id=@mem_id";
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
                                    this.o_sex.SelectedIndex = Convert.ToInt32(reader["sex"].ToString()) - 1;
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            else
            {
                AddTableControls();
            }
            
        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            Shoppingcar.RootObject rlib = JsonConvert.DeserializeObject<Shoppingcar.RootObject>(this.jsonStr.Value);

            GetStr GS = new GetStr();
            #region 訂單變數
            int BonusAmt = rlib.OrderData.BonusAmt;
            String BonusDiscount = rlib.OrderData.BonusDiscount;
            int FreightAmount = rlib.OrderData.FreightAmount;
            String MemID = rlib.OrderData.MemID;
            String PayType = rlib.OrderData.PayType;
            String ReturnUrl = rlib.OrderData.ReturnUrl;
            String RID = rlib.OrderData.RID;
            String Click_ID = rlib.OrderData.Click_ID;

            Int32 DiscountAmt = Convert.ToInt32(this.discount_amt.Value);
            String OName = GS.ReplaceStr(this.o_name.Text);
            String OTel = GS.ReplaceStr(this.o_tel.Text);
            String OCell = GS.ReplaceStr(this.o_cell.Text);
            String OSex = GS.ReplaceStr(this.o_sex.SelectedItem.Value);
            String OEmail = GS.ReplaceStr(this.mail.Text);

            String SName = OName;
            String STel = OTel;
            String SCell = OCell;
            String SSex = OSex;
            String City = "";
            String Country = "";
            String Zip = "";
            String Address = City + Country + "";
            String Notememo = GS.ReplaceStr(this.notememo.Text);
            #endregion            
            
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
            String Animal = "";
            String lunarbirth = "";
            String CellPhone = "";
            Regex rgx;

            #region 取得Table內所有控制項並產生Json格式
            foreach (TableRow tr in Table1.Rows)
            {
                foreach (TableCell tc in tr.Cells)
                {
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

            String Str_Error = "";
            if (Lightroot.Errormsg.Count > 0)
            {
                foreach (Shoppingcar.ErrorMsg error in Lightroot.Errormsg)
                {
                    if (error.Msg != "null") Str_Error += error.Msg + "\\n";
                }
                if (Str_Error != "")
                {
                    Str_Error = "請檢查您輸入的資料\\n\\n所有欄位都要填寫\\n\\n住家/公司電話格式(加區碼,只需輸入數字)\\n\\n手機電話格式(只需輸入數字)\\n";// +Str_Error;
                    Response.Write("<script language='javascript'>alert('" + Str_Error + "');</script>");
                }
            }
            else 
            {
                SqlCommand cmd;
                String OrgName = rlib.OrderData.OrgName;
                String setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
                String OrderID = GetOrderID(setting);

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
                    cmd.Parameters.Add(new SqlParameter("@o_addr", ""));
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
                    cmd.Parameters.Add(new SqlParameter("@prod_bonus", "0"));
                    cmd.ExecuteNonQuery();
                }
                #endregion

                int i = 1;
                int order_totalamt = 0;
                String Memo = "";
                foreach (Shoppingcar.OrderList Orders in rlib.OrderData.OrderLists)
                {
                    if (Orders.Type != 0)
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
                                cmd.Parameters.Add(new SqlParameter("@price", Convert.ToInt32(OrderSpecs.Price)));
                                cmd.Parameters.Add(new SqlParameter("@qty", Convert.ToInt32(OrderSpecs.Qty)));
                                cmd.Parameters.Add(new SqlParameter("@amt", Convert.ToInt32(OrderSpecs.Qty) * Convert.ToInt32(OrderSpecs.Price)));
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

                                order_totalamt += Convert.ToInt32(OrderSpecs.Qty) * Convert.ToInt32(OrderSpecs.Price) - Convert.ToInt32(OrderSpecs.Discount);
                                #region 庫存更新
                                cmd = new SqlCommand();
                                cmd.CommandText = "sp_stocks";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = conn;
                                cmd.Parameters.Add(new SqlParameter("@prod_id", Convert.ToInt32(Items.ID)));
                                cmd.Parameters.Add(new SqlParameter("@qty", Convert.ToInt32(OrderSpecs.Qty)));
                                cmd.Parameters.Add(new SqlParameter("@prod_color", Convert.ToInt32(OrderSpecs.Color)));
                                cmd.Parameters.Add(new SqlParameter("@prod_size", Convert.ToInt32(OrderSpecs.Size)));
                                cmd.ExecuteNonQuery();
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

                #region 儲存點燈資料
                foreach (Shoppingcar.LightItem LightItems in Lightroot.Items)
                {
                    foreach (Shoppingcar.LightData LightDatas in LightItems.data)
                    {
                        using (SqlConnection conn = new SqlConnection(setting))
                        {
                            conn.Open();

                            cmd = new SqlCommand();
                            cmd.CommandText = "sp_orders_detail";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = conn;
                            cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                            cmd.Parameters.Add(new SqlParameter("@prodid", LightItems.prodid));
                            cmd.Parameters.Add(new SqlParameter("@name", LightDatas.name));
                            cmd.Parameters.Add(new SqlParameter("@tel", LightDatas.tel));
                            cmd.Parameters.Add(new SqlParameter("@addr", LightDatas.addr));
                            cmd.Parameters.Add(new SqlParameter("@birth", LightDatas.birth));
                            cmd.Parameters.Add(new SqlParameter("@hour", LightDatas.hour));
                            cmd.Parameters.Add(new SqlParameter("@animal", GetAnimal(LightDatas.birth.Substring(0,4))));
                            cmd.Parameters.Add(new SqlParameter("@lunar_birth", GetLDate(Convert.ToDateTime(LightDatas.birth))));
                            cmd.Parameters.Add(new SqlParameter("@cellphone", LightDatas.cellphone));
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                #endregion

                #region 儲存訂單總金額
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
                #endregion

                #region 取得寄信相關資料        
                
                String service_mail = "";
                String title = "";
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    cmd = new SqlCommand("select b.service_mail,b.title from CurrentUseFrame as a left join head as b on a.id=b.hid", conn);
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
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
                #endregion

                String mail_cont = GetMailCont(rlib, setting, OrderID);
                send_email(mail_cont, GetLocalResourceObject("StringResource51") + " 【" + title + "】", service_mail, OEmail);//呼叫send_email函式測試                    
                Response.Write("<script type='text/javascript'>window.location.href='" + ReturnUrl + "/" + GS.GetLanString(str_language) + "/shop.asp?id=" + OrderID + "';</script>");
            }
            #endregion            
        }

        #region 驗證json
        private bool ChkJson(String JsonStr)
        {

            GetStr GS = new GetStr();

            Shoppingcar.RootObject rlib = JsonConvert.DeserializeObject<Shoppingcar.RootObject>(JsonStr);

            String ChkStr = rlib.OrderData.OrgName;
            ChkStr += rlib.OrderData.PayType;
            ChkStr += rlib.OrderData.FreightAmount;
            ChkStr += rlib.OrderData.BonusDiscount;
            ChkStr += rlib.OrderData.BonusAmt;

            foreach (Shoppingcar.OrderList Orders in rlib.OrderData.OrderLists)
            {
                foreach (Shoppingcar.OrderItem OrderItems in Orders.OrderItems)
                {
                    //ChkStr += GS.StringToUnicode(OrderItems.Name);
                    foreach (Shoppingcar.OrderSpec OrderSpecs in OrderItems.OrderSpecs)
                    {
                        ChkStr += OrderSpecs.FinalPrice;
                    }
                    foreach (Shoppingcar.AdditionalItem AdditionalItems in OrderItems.AdditionalItems)
                    {
                        //ChkStr += GS.StringToUnicode(AdditionalItems.Name);
                        ChkStr += AdditionalItems.FinalPrice;
                    }
                }
            }


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

        #region 產生動態欄位
        private void AddTableControls()
        {
            GetStr GS = new GetStr();
            TableCont[] newArray = (TableCont[])ViewState["TableCont"];
            List<TableCont> newList = new List<TableCont>(newArray);

            BasicCont[] newArray2 = (BasicCont[])ViewState["BasicCont"];
            List<BasicCont> BasicList = new List<BasicCont>(newArray2);

            String WebTitle = BasicList[0].WebTitle;
            String discount_range = BasicList[0].discount_range;
            String discount_price = BasicList[0].discount_price;
            double DiscounAmt = 0;
            String BonusDiscount = BasicList[0].BonusDiscount;
            int FreightAmount = BasicList[0].FreightAmount;
            String MemID = BasicList[0].MemID;
            int BonusAmt = BasicList[0].BonusAmt;
            String PayType = BasicList[0].PayType;
            String OrgName = BasicList[0].OrgName;
            String setting = "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
            
            String ProdNAme = "";
            String ProdID = "";
            double Price = 0;
            int DisCount = 0;
            int Qty = 0;
            String Spec1 = "";
            String Spec2 = "";

            int totalamt = 0;
            Label myLabel1;
            Label myLabel2;
            TextBox myTextBox;
            DropDownList ddl;
            HiddenField HF;

            #region 寫入table內容

            TableRow tr;
            TableCell tc;

            #region 表頭
            tr = new TableRow();
            tc = new TableCell();
            myLabel1 = new Label();
            myLabel1.Text = WebTitle;
            tc.Controls.Add(myLabel1);
            tc.ColumnSpan = 7;
            tr.Cells.Add(tc);
            Table1.Rows.Add(tr);

            tr = new TableRow();
            tc = new TableCell();
            myLabel1 = new Label();
            myLabel1.Text = "購買明細";
            tc.Controls.Add(myLabel1);
            tc.ColumnSpan = 7;
            tr.Cells.Add(tc);
            Table1.Rows.Add(tr);

            tr = new TableRow();
            tc = new TableCell();
            myLabel1 = new Label();
            myLabel1.Text = "商品名稱";
            tc.Controls.Add(myLabel1);
            tr.Cells.Add(tc);


            tc = new TableCell();
            myLabel1 = new Label();
            myLabel1.Text = GS.GetSpecTitle(setting, "1");
            tc.Controls.Add(myLabel1);
            tr.Cells.Add(tc);

            tc = new TableCell();
            myLabel1 = new Label();
            myLabel1.Text = GS.GetSpecTitle(setting, "2");
            tc.Controls.Add(myLabel1);
            tr.Cells.Add(tc);

            tc = new TableCell();
            myLabel1 = new Label();
            myLabel1.Text = "數量";
            tc.Controls.Add(myLabel1);
            tr.Cells.Add(tc);

            tc = new TableCell();
            myLabel1 = new Label();
            myLabel1.Text = "單價";
            tc.Controls.Add(myLabel1);
            tr.Cells.Add(tc);

            tc = new TableCell();
            myLabel1 = new Label();
            myLabel1.Text = "折扣";
            tc.Controls.Add(myLabel1);
            tr.Cells.Add(tc);

            tc = new TableCell();
            myLabel1 = new Label();
            myLabel1.Text = "小計";
            tc.Controls.Add(myLabel1);
            tr.Cells.Add(tc);

            Table1.Rows.Add(tr);
            #endregion

            #region 表身
            for (int pn = 0; pn < newList.Count; pn++)
            {
                ProdID = newList[pn].ProdID;
                ProdNAme = newList[pn].ProdName;
                Price = newList[pn].Price;
                DisCount = newList[pn].Discount;
                Qty = newList[pn].Qty;

                totalamt += Convert.ToInt32(Price) * Convert.ToInt32(Qty) - Convert.ToInt32(DisCount);

                #region 購買項目
                tr = new TableRow();
                tc = new TableCell();
                myLabel1 = new Label();
                HF = new HiddenField();

                myLabel1.Text = ProdNAme;                
                HF.Value = ProdID;
                HF.ID = "ProdID" + pn.ToString();

                tc.Controls.Add(myLabel1);
                tc.Controls.Add(HF);
                tr.Cells.Add(tc);

                myLabel1 = new Label();
                myLabel1.Text = Spec1;
                tc = new TableCell();
                tc.Controls.Add(myLabel1);
                tr.Cells.Add(tc);

                myLabel1 = new Label();
                myLabel1.Text = Spec2;
                tc = new TableCell();
                tc.Controls.Add(myLabel1);
                tr.Cells.Add(tc);

                myLabel1 = new Label();
                myLabel1.Text = Qty.ToString();
                tc = new TableCell();
                tc.Controls.Add(myLabel1);
                tr.Cells.Add(tc);

                myLabel1 = new Label();
                myLabel1.Text = Price.ToString();
                
                tc = new TableCell();
                tc.Attributes.Add("class", "money");
                tc.Controls.Add(myLabel1);
                tr.Cells.Add(tc);

                myLabel1 = new Label();
                myLabel1.Text = DisCount.ToString();
                
                tc = new TableCell();
                tc.Attributes.Add("class", "money");
                tc.Controls.Add(myLabel1);
                tr.Cells.Add(tc);

                myLabel1 = new Label();
                myLabel1.Text = ((Convert.ToInt32(Price) * Convert.ToInt32(Qty)) - Convert.ToInt32(DisCount)).ToString();                
                tc = new TableCell();
                tc.Attributes.Add("class", "money");
                tc.Controls.Add(myLabel1);
                tr.Cells.Add(tc);

                Table1.Rows.Add(tr);
                #endregion

                #region 每個項目動態產生填寫欄位
                for (int i = 0; i < Convert.ToInt32(newList[pn].Qty); i++)
                {
                    #region 欄位設定
                    tr = new TableRow();

                    #region 第一列設定
                    myLabel1 = new Label();
                    myLabel1.Text = "姓名";
                    myLabel1.Attributes.Add("name", "label");

                    myTextBox = new TextBox();
                    myTextBox.ID = "CHName" + pn.ToString() + i.ToString();
                    myTextBox.Attributes.Add("maxlength", "5");
                    myTextBox.Attributes.Add("inputname", "CHName");
                    myTextBox.Attributes.Add("placeholder", "必填");


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
                    myTextBox.Attributes.Add("placeholder", "必填，只需填寫數字");

                    tc = new TableCell();
                    tc.Controls.Add(myLabel1);
                    tc.Controls.Add(myTextBox);
                    tc.ColumnSpan = 3;
                    tr.Cells.Add(tc);

                    myLabel1 = new Label();
                    myLabel1.Text = "行動電話";
                    myLabel1.Attributes.Add("name", "label");

                    myTextBox = new TextBox();
                    myTextBox.ID = "CellPhone" + pn.ToString() + i.ToString();
                    myTextBox.Attributes.Add("maxlength", "10");
                    myTextBox.Attributes.Add("inputname", "cellphone");
                    myTextBox.Attributes.Add("placeholder", "必填，只需填寫數字");

                    tc = new TableCell();
                    tc.Controls.Add(myLabel1);
                    tc.Controls.Add(myTextBox);
                    tc.ColumnSpan = 3;
                    tr.Cells.Add(tc);
                    Table1.Rows.Add(tr);
                    #endregion

                    #region 第二列設定
                    tr = new TableRow();

                    myLabel1 = new Label();
                    myLabel1.Text = "地址";
                    myLabel1.Attributes.Add("name", "label");

                    myTextBox = new TextBox();
                    myTextBox.ID = "Address" + pn.ToString() + i.ToString();
                    myTextBox.Attributes.Add("maxlength", "50");
                    myTextBox.Attributes.Add("inputname", "Address");
                    myTextBox.Attributes.Add("placeholder", "必填");

                    tc = new TableCell();
                    tc.Controls.Add(myLabel1);
                    tc.Controls.Add(myTextBox);
                    tc.ColumnSpan = 7;
                    tr.Cells.Add(tc);                    
                    Table1.Rows.Add(tr);
                    #endregion

                    #region 第三列設定
                    tr = new TableRow();

                    myLabel1 = new Label();
                    myLabel1.Text = "生日";
                    myLabel1.Attributes.Add("name", "label");

                    myTextBox = new TextBox();
                    myTextBox.ID = "Birth" + pn.ToString() + i.ToString();
                    myTextBox.Attributes.Add("readonly", "readonly");
                    myTextBox.Attributes.Add("inputname", "Birth");
                    myTextBox.Attributes.Add("value", "1980/01/01");

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
                    tc.ColumnSpan = 7;
                    tr.Cells.Add(tc);
                    Table1.Rows.Add(tr);
                    #endregion

                    #endregion
                }
                #endregion

                HF = new HiddenField();
                HF.ID = "Stop" + pn.ToString();
                HF.Value = "Stop";
                tc = new TableCell();
                tc.Controls.Add(HF);
                tc.ColumnSpan = 7;
                tr = new TableRow();
                tr.Cells.Add(tc);
                tr.Attributes.Add("class", "br");
                Table1.Rows.Add(tr);
            }
            #endregion

            #region 購物車金額

            #region 訂單折扣
            if (Convert.ToInt16(discount_range) > 0)
            {

                tr = new TableRow();
                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = "商品總計";
                tc.Controls.Add(myLabel1);
                tc.ColumnSpan = 6;
                tr.Cells.Add(tc);
                
                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = totalamt.ToString();
                tc.Attributes.Add("class", "money");
                tc.Controls.Add(myLabel1);
                tr.Cells.Add(tc);
                tr.Attributes.Add("class", "result");
                Table1.Rows.Add(tr);

                DiscounAmt = Math.Floor(Convert.ToDouble(totalamt) / Convert.ToDouble(discount_range)) * Convert.ToDouble(discount_price);
                this.discount_amt.Value = DiscounAmt.ToString();
                totalamt = totalamt - Convert.ToInt32(DiscounAmt);

                String DiscountStr = "折扣活動(滿" + discount_range + "元，折價" + discount_price + "元)：";

                tr = new TableRow();
                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = DiscountStr;
                tc.Controls.Add(myLabel1);
                tc.ColumnSpan = 6;
                tr.Cells.Add(tc);

                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = DiscounAmt.ToString();
                tc.Attributes.Add("class", "money");
                tc.Controls.Add(myLabel1);
                tr.Cells.Add(tc);

                tr.Attributes.Add("class", "result");
                Table1.Rows.Add(tr);

            }
            else
            {
                this.discount_amt.Value = "0";

                tr = new TableRow();
                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = "商品總計";
                tc.Controls.Add(myLabel1);
                tc.ColumnSpan = 6;
                tr.Cells.Add(tc);

                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = totalamt.ToString();
                tc.Attributes.Add("class", "money");
                tc.Controls.Add(myLabel1);
                tr.Cells.Add(tc);

                tr.Attributes.Add("class", "result");
                Table1.Rows.Add(tr);
            }
            #endregion
            
            #region 紅利可扣抵金額
            if (Convert.ToInt32(BonusDiscount) > 0)
            {
                tr = new TableRow();
                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = "紅利可扣抵金額";
                tc.Controls.Add(myLabel1);
                tc.ColumnSpan = 6;
                tr.Cells.Add(tc);

                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = BonusDiscount.ToString();
                tc.Attributes.Add("class", "money");
                tc.Controls.Add(myLabel1);
                tr.Cells.Add(tc);

                tr.Attributes.Add("class", "result");
                Table1.Rows.Add(tr);

                tr = new TableRow();
                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = "運費";
                tc.Controls.Add(myLabel1);
                tc.ColumnSpan = 6;
                tr.Cells.Add(tc);

                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = FreightAmount.ToString();
                tc.Attributes.Add("class", "money");
                tc.Controls.Add(myLabel1);
                tr.Cells.Add(tc);

                tr.Attributes.Add("class", "result");
                Table1.Rows.Add(tr);

                tr = new TableRow();
                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = "消費總計";
                tc.Controls.Add(myLabel1);
                tc.ColumnSpan = 6;
                tr.Cells.Add(tc);

                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = (totalamt - Convert.ToInt32(BonusDiscount) + Convert.ToInt32(FreightAmount)).ToString();
                tc.Attributes.Add("class", "money");
                tc.Controls.Add(myLabel1);
                tr.Cells.Add(tc);

                tr.Attributes.Add("class", "result total");
                Table1.Rows.Add(tr);
            }
            else
            {
                tr = new TableRow();
                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = "運費";
                tc.Controls.Add(myLabel1);
                tc.ColumnSpan = 6;
                tr.Cells.Add(tc);

                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = FreightAmount.ToString();
                tc.Attributes.Add("class", "money");
                tc.Controls.Add(myLabel1);
                tr.Cells.Add(tc);

                tr.Attributes.Add("class", "result");
                Table1.Rows.Add(tr);

                tr = new TableRow();
                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = "消費總計";
                tc.Controls.Add(myLabel1);
                tc.ColumnSpan = 6;
                tr.Cells.Add(tc);

                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = (totalamt + Convert.ToInt32(FreightAmount)).ToString();
                tc.Attributes.Add("class", "money");
                tc.Controls.Add(myLabel1);
                tr.Cells.Add(tc);

                tr.Attributes.Add("class", "result total");
                Table1.Rows.Add(tr);
            }
            #endregion

            #region 會員獲得購物紅利
            if (MemID != "")
            {
                tr = new TableRow();
                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = "本次購物可獲得紅利";
                tc.Controls.Add(myLabel1);
                tc.ColumnSpan = 6;
                tr.Cells.Add(tc);

                tc = new TableCell();
                myLabel1 = new Label();
                myLabel1.Text = BonusAmt.ToString();
                tc.Attributes.Add("class", "money");
                tc.Controls.Add(myLabel1);
                tr.Cells.Add(tc);

                tr.Attributes.Add("class", "result");
                Table1.Rows.Add(tr);
            }
            #endregion

            #region 付款方式
            tr = new TableRow();
            tc = new TableCell();
            myLabel1 = new Label();
            myLabel1.Text = "付款方式";
            tc.Controls.Add(myLabel1);
            tc.ColumnSpan = 6;
            tr.Cells.Add(tc);

            tc = new TableCell();
            myLabel1 = new Label();
            myLabel1.Text = PayType;
            tc.Controls.Add(myLabel1);
            tr.Cells.Add(tc);

            tr.Attributes.Add("class", "result");
            Table1.Rows.Add(tr);
            #endregion
            
            
            #endregion
            #endregion
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

        #region 通知信內容製作
        private string GetMailCont(Shoppingcar.RootObject rlib, String setting, String OrderID)
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
            int ShopType = rlib.OrderData.ShopType;

            Int32 DiscountAmt = Convert.ToInt32(GS.ReplaceStr(this.discount_amt.Value));
            String OName = GS.ReplaceStr(this.o_name.Text);
            String OTel = GS.ReplaceStr(this.o_tel.Text);
            String OCell = GS.ReplaceStr(this.o_cell.Text);
            String OSex = GS.ReplaceStr(this.o_sex.SelectedItem.Value);
            String OEmail = GS.ReplaceStr(this.mail.Text);

            String SName = OName;
            String STel = OTel;
            String SCell = OCell;
            String SSex = OSex;
            String City = "";
            String Country = "";
            String Zip = "";
            String Address = City + Country + "";
            String Notememo = GS.ReplaceStr(this.notememo.Text);
            #endregion
            #region 通知信內容製作
            SqlCommand cmd;

            #region 取得網站基本資料
            String service_mail = "";
            String mer_id = "";
            String remit_money1 = "";
            String remit_money2 = "";
            String remit_money3 = "";
            String title = "";
            String freight_range = "";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                cmd = new SqlCommand("select service_mail,mer_id,remit_money1,remit_money2,remit_money3,title,isnull(freight_range,'') from head", conn);
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
            #endregion

            #region 表頭
            String mail_cont = "<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />";
            mail_cont += "<div style='font-family:微軟正黑體, Arial, 新細明體, Helvetica, sans-serif'>";
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
            mail_cont += "   <li>";
            mail_cont += GetLocalResourceObject("StringResource52");
            mail_cont += "  </li>";  
            mail_cont += "</ul>";
            mail_cont += "<hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px; margin-top:8px; margin-bottom:20px;'>";

            mail_cont += "<p style='color:#e30000; font-weight:bold; max-width:600px; margin:5px 10px;'>" + GetLocalResourceObject("StringResource28") + "：<span id='order_id' style='font-weight:normal; color:#000;'>" + OrderID + "</span></p>";

            mail_cont += "<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0' style='border:none;border-width: 0px;margin-left:10px;margin-right:10px; width:600px; font-size:9pt; font-family:'microsoft jhenghei', sans-serif;'>";
            mail_cont += "  <tr>";
            mail_cont += "    <th colspan='4' align='left' valign='middle' scope='col' style='background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;border-right-width:0px;'>" + GetLocalResourceObject("StringResource29") + "：" + GS.Rename(OName, 1) + "</th>";
            mail_cont += "  </tr>";
            mail_cont += "  <tr>";
            mail_cont += "    <td align='right' valign='middle' style='width:15%;background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GetLocalResourceObject("StringResource30") + "</td>";
            mail_cont += "    <td align='left' valign='middle' style='width:35%;background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'><span style='color:#333;'>" + GS.Rename(OCell, 4) + "</span></td>";
            mail_cont += "    <td align='right' valign='middle' style='width:15%;background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GetLocalResourceObject("StringResource31") + "</td>";
            mail_cont += "    <td align='left' valign='middle' style='width:35%;background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'><span style='color:#333;'>" + GS.Rename(OTel, 4) + "</span></td>";
            mail_cont += "  </tr>";
            mail_cont += "  <tr>";
            mail_cont += "    <td align='right' valign='middle' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GetLocalResourceObject("StringResource32") + "</td>";
            mail_cont += "    <td colspan='3' align='left' valign='middle' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'>" + GS.Rename(OEmail, 5) + "</td>";
            mail_cont += "  </tr>";
            mail_cont += "</table>";
            mail_cont += "<br>";


            mail_cont += "<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0' style='border:none;border-width: 0px;margin-left:10px;margin-right:10px; width:600px; font-size:9pt; font-family:'microsoft jhenghei', sans-serif;'>";
            mail_cont += "  <tr>";
            mail_cont += "    <th colspan='4' align='left' valign='middle' scope='col' style='background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;border-right-width:0px;'>" + GetLocalResourceObject("StringResource33") + "：" + GS.Rename(SName, 1);
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
            mail_cont += "    <td align='right' valign='middle' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GetLocalResourceObject("StringResource36") + "</td>";
            mail_cont += "    <td colspan='3' align='left' valign='middle' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'><span style='color:#333;'>" + GS.Rename(Address, 7) + "</span></td>";
            mail_cont += "  </tr>";
            mail_cont += "  <tr>";
            mail_cont += "    <td align='right' valign='middle' style='width:15%;background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GetLocalResourceObject("StringResource30") + "</td>";
            mail_cont += "    <td align='left' valign='middle' style='width:35%;background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'><span style='color:#333;'>" + GS.Rename(SCell, 3) + "</span></td>";
            mail_cont += "    <td align='right' valign='middle' style='width:15%;background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GetLocalResourceObject("StringResource31") + "</td>";
            mail_cont += "    <td align='left' valign='middle' style='width:35%;background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'><span style='color:#333;'>" + GS.Rename(STel, 3) + "</span></td>";
            mail_cont += "  </tr>";
            mail_cont += "  <tr>";
            mail_cont += "    <td align='right' valign='middle' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GetLocalResourceObject("StringResource37") + "</td>";
            mail_cont += "    <td colspan='3' align='left' valign='middle' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'><span style='color:#333;'>" + Notememo + "</span></td>";
            mail_cont += "  </tr>";
            mail_cont += "</table>";
            mail_cont += "<br>";


            mail_cont += "<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0' style='border:none;border-width: 0px;margin-left:10px;margin-right:10px; width:600px; font-size:9pt; font-family:'microsoft jhenghei', sans-serif;'>";
            mail_cont += "  <tr>";
            mail_cont += "    <th align='left' scope='col' style='width:30%;background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;'>" + GetLocalResourceObject("StringResource1") + "</th>";
            mail_cont += "    <th align='center' scope='col' style='width:10%;background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;'>" + GetLocalResourceObject("StringResource38") + "</th>";
            mail_cont += "    <th align='center' scope='col' style='width:10%;background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;'>" + GS.GetSpecTitle(setting, "1") + "</th>";
            mail_cont += "    <th align='center' scope='col' style='width:10%;background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;'>" + GS.GetSpecTitle(setting, "2") + "</th>";
            mail_cont += "    <th align='center' scope='col' style='width:10%;background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;'>" + GetLocalResourceObject("StringResource2") + "</th>";
            mail_cont += "    <th align='center' scope='col' style='width:10%;background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;'>" + GetLocalResourceObject("StringResource3") + "</th>";
            mail_cont += "    <th align='center' scope='col' style='width:10%;background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;'>" + GetLocalResourceObject("StringResource4") + "</th>";
            mail_cont += "    <th align='center' scope='col' style='width:10%;background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;border-right-width:0px;'>" + GetLocalResourceObject("StringResource5") + "</th>";
            mail_cont += "  </tr>";
            #endregion

            int i = 0;
            int totalamt = 0;
            foreach (Shoppingcar.OrderList Orders in rlib.OrderData.OrderLists)
            {
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
                        mail_cont += "    <td align='left' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>";
                        if (Orders.Type != 0)
                        {
                            mail_cont += "<span style='color:#ff0000;'>" + Orders.Title + "</span> - ";
                        }
                        mail_cont += Items.Name + "</td>";
                        mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + ProdItemno + "</td>";
                        mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GS.GetSpec(setting, "prod_color", OrderSpecs.Color) + "</td>";
                        mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GS.GetSpec(setting, "prod_size", OrderSpecs.Size) + "</td>";
                        mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + OrderSpecs.Qty + "</td>";
                        mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + OrderSpecs.Price + "</td>";
                        mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + OrderSpecs.Discount + "</td>";
                        mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'>";
                        if (Convert.ToInt32(OrderSpecs.Discount) != 0)
                        {
                            mail_cont += "    <s style='font-size:9pt; color:#bbbbbb; padding-right:3px;'>" + (Convert.ToInt32(OrderSpecs.Qty) * Convert.ToInt32(OrderSpecs.Price)) + "</s>";
                        }
                        mail_cont += ((Convert.ToInt32(OrderSpecs.Qty) * Convert.ToInt32(OrderSpecs.Price)) - Convert.ToInt32(OrderSpecs.Discount)) + "</td>";
                        mail_cont += "  </tr>";
                        totalamt += Convert.ToInt32(OrderSpecs.Price) * Convert.ToInt32(OrderSpecs.Qty) - Convert.ToInt32(OrderSpecs.Discount);

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
                        mail_cont += "    <td align='left' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'><span style='color:#ff0000;'>" + GetLocalResourceObject("StringResource9") + "-</span>" + AdditionalItems.Name + "</td>";
                        mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + ProdItemno + "</td>";
                        mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GS.GetSpec(setting, "prod_color", AdditionalItems.Color) + "</td>";
                        mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GS.GetSpec(setting, "prod_size", AdditionalItems.Size) + "</td>";
                        mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + AdditionalItems.Qty + "</td>";
                        mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + AdditionalItems.Price + "</td>";
                        mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + AdditionalItems.Discount + "</td>";
                        mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>";
                        if (Convert.ToInt32(AdditionalItems.Discount) != 0)
                        {
                            mail_cont += "    <s style='font-size:9pt; color:#bbbbbb; padding-right:3px;'>" + (Convert.ToInt32(AdditionalItems.Qty) * Convert.ToInt32(AdditionalItems.Price)) + "</s>";
                        }
                        mail_cont += ((Convert.ToInt32(AdditionalItems.Qty) * Convert.ToInt32(AdditionalItems.Price)) - Convert.ToInt32(AdditionalItems.Discount)) + "</td>";
                        mail_cont += "  </tr>";
                        totalamt += Convert.ToInt32(AdditionalItems.Price) * Convert.ToInt32(AdditionalItems.Qty);

                        i = i + 1;
                    }
                    #endregion

                }
            }

            #region 折扣金額
            if (Convert.ToInt32(DiscountAmt) > 0 || Convert.ToInt32(DiscountAmt) < 0)
            {
                mail_cont += "  <tr>";
                mail_cont += "    <td align='right' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GetLocalResourceObject("StringResource39") + "</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width: 0px;'>" + DiscountAmt * (-1) + "</td>";
                mail_cont += "  </tr>";
            }
            #endregion

            #region 運費
            mail_cont += "  <tr>";
            mail_cont += "    <td align='right' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GetLocalResourceObject("StringResource15") + "</td>";
            mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
            mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
            mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
            mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
            mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
            mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
            mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width: 0px;'>" + FreightAmount + "</td>";
            mail_cont += "  </tr>";

            #endregion


            mail_cont += "  <tr>";
            mail_cont += "    <td colspan='8' align='right' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'>" + GetLocalResourceObject("StringResource16") + "<b style='font-size: 18pt;color: #f00;'>NT$" + (totalamt + FreightAmount - DiscountAmt) + "</b></td>";
            mail_cont += "  </tr>";


            #region 紅利扣抵
            if (Int32.Parse(BonusDiscount.ToString()) > 0)
            {
                mail_cont += "  <tr>";
                mail_cont += "    <td align='right' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GetLocalResourceObject("StringResource40") + "</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width: 0px;'>" + BonusDiscount + "</td>";
                mail_cont += "  </tr>";
                mail_cont += "  <tr>";
                mail_cont += "    <td align='right' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>" + GetLocalResourceObject("StringResource41") + "</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'>&nbsp;</td>";
                mail_cont += "    <td align='center' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width: 0px;'>" + (totalamt + Convert.ToInt32(FreightAmount) - DiscountAmt - Convert.ToInt32(BonusDiscount)) + "</td>";
                mail_cont += "  </tr>";
            }
            #endregion


            String payment_type = GS.GetPayType(setting,rlib.OrderData.PayType);
            #region 匯款方式
            mail_cont += "</table>";
            mail_cont += "<br>";
            mail_cont += "<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0' style='border:none;border-width: 0px;margin-left:10px;margin-right:10px; width:600px; font-size:9pt; font-family:'microsoft jhenghei', sans-serif;'>";
            mail_cont += "  <tr>";
            mail_cont += "    <th align='left' scope='col' style='background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;border-right-width:0px;'>" + GetLocalResourceObject("StringResource18") + "：<b style='font-size: 18pt;color: #f00;'>" + GS.GetPayType(setting,rlib.OrderData.PayType) + "</b></th>";
            mail_cont += "  </tr>";
            #endregion

            #region 匯款資訊
            if (payment_type == "ATM")
            {
                mail_cont += "  <tr>";
                mail_cont += "    <td align='left' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'><span style='color:#f00;'>" + GetLocalResourceObject("StringResource42") + "</span></td>";
                mail_cont += "  </tr>";
            }
            mail_cont += "</table>";
            mail_cont += "<br>";

            if (payment_type == "ATM")
            {

                mail_cont += "<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0' style='border:none;border-width: 0px;margin-left:10px;margin-right:10px; width:600px; font-size:9pt; font-family:'microsoft jhenghei', sans-serif;'>";
                mail_cont += "  <tr>";
                mail_cont += "    <th colspan='2' align='left' scope='col' style='background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;border-left-width:0px;border-right-width:0px;'>" + GetLocalResourceObject("StringResource43") + "</th>";
                mail_cont += "  </tr>";
                mail_cont += "  <tr>";
                mail_cont += "    <td align='right' style='width:30%;background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'><strong>" + GetLocalResourceObject("StringResource44") + "</strong></td>";
                mail_cont += "    <td align='left' style='width:70%;background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'>" + remit_money1 + "</td>";
                mail_cont += "  </tr>";
                mail_cont += "  <tr>";
                mail_cont += "    <td align='right' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'><strong>" + GetLocalResourceObject("StringResource45") + "</strong></td>";
                mail_cont += "    <td align='left' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'>" + remit_money2 + "</td>";
                mail_cont += "  </tr>";
                mail_cont += "  <tr>";
                mail_cont += "    <td align='right' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'><strong>" + GetLocalResourceObject("StringResource46") + "</strong></td>";
                mail_cont += "    <td align='left' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'>" + remit_money3 + "</td>";
                mail_cont += "  </tr>";
                mail_cont += "  <tr>";
                mail_cont += "    <td align='right' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'><strong>" + GetLocalResourceObject("StringResource47") + "</strong></td>";
                mail_cont += "    <td align='left' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'>NT$" + (totalamt + Convert.ToInt32(FreightAmount) - Convert.ToInt32(BonusDiscount)) + "</td>";
                mail_cont += "  </tr>";
                mail_cont += "  <tr>";
                mail_cont += "    <td align='right' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;'><strong style='color:#f00;'>" + GetLocalResourceObject("StringResource48") + "</strong></td>";
                mail_cont += "    <td align='left' style='background:#fff; padding:7px; border-top-width:0px; border-color:#808080;border-left-width:0px;border-right-width:0px;'><span style='color:#f00;'>" + DateTime.Today.AddDays(1).ToString("yyyy/MM/dd") + " 23:59</span></td>";
                mail_cont += "  </tr>";
                mail_cont += "</table>";
            }
            #endregion

            #region 表尾注意事項
            mail_cont += "<hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px;'>";
            mail_cont += "<span style='color:#ff0000;'>" + GetLocalResourceObject("StringResource49") + "</span><br>";
            mail_cont += "<span style='color:#ff0000;'>" + GetLocalResourceObject("StringResource50") + "</span>";
            mail_cont += "<hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px;'>";

            #endregion

            #endregion
            return mail_cont;
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
            SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings.Get("smtpServer"),Int32.Parse(ConfigurationManager.AppSettings.Get("smtpPort")));//設定E-mail Server和port
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
    }
}