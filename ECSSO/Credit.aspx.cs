using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Threading;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.IO;

namespace ECSSO
{
    public partial class Credit : System.Web.UI.Page
    {
        private string str_language = string.Empty;
        

        #region  語系變換
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
        #endregion
        protected void Page_Init(object sender, EventArgs e)
        {
            HtmlGenericControl objLink = new HtmlGenericControl("link");
            objLink.Attributes.Add("rel", "stylesheet");
            objLink.Attributes.Add("href", "SSOcss/" + str_language + "/Credit.css");
            objLink.Attributes.Add("type", "text/css");
            this.Page.Header.Controls.Add(objLink);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            this.language.Value = str_language;
            
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
                    this.MemberID.Value = Request.Form["MemID"].ToString();
                }
                else
                {
                    if (Request.QueryString["MemID"] != null)
                    {
                        this.MemberID.Value = Request.QueryString["MemID"].ToString();
                    }
                }

                String CheckM = string.Empty;
                if (Request.Form["CheckM"] != null)
                {
                    CheckM = Microsoft.Security.Application.Encoder.HtmlEncode(Request.Form["CheckM"].ToString().Split('?')[0]);
                }
                else
                {
                    if (Request.QueryString["CheckM"] != null)
                    {
                        CheckM = Microsoft.Security.Application.Encoder.HtmlEncode(Request.QueryString["CheckM"].ToString().Split('?')[0]);
                    }
                }
                #endregion
                GetStr GS = new GetStr();

                String MerchantID = string.Empty;
                String MerchantMemberID = this.MemberID.Value;
                String HashKey = string.Empty;
                String HashIV = string.Empty;
                String TradePostUrl = "https://payment-stage.allpay.com.tw/MerchantMember/QueryMemberBinding";


                if (GS.MD5Check(this.siteid.Value + this.MemberID.Value, CheckM))
                {
                    setting = GS.GetSetting(this.siteid.Value);
                    using (SqlConnection conn = new SqlConnection(setting))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("select HashIv,HashKey,mer_id from head", conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    MerchantID = reader[2].ToString();
                                    HashKey = reader[1].ToString();
                                    HashIV = reader[0].ToString();
                                }
                            }
                        }
                        finally {
                            reader.Close();
                        }
                    }

                    GetCarNO(MerchantID, MerchantMemberID, HashKey, HashIV, TradePostUrl);

                }
            
        }

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
                            DropDownList1.Items.Remove(DropDownList1.Items.FindByValue(DD.CardID));
                            DropDownList1.Items.Add(new ListItem(StrText, DD.CardID));
                        }
                    }
                    catch
                    {
                        DLData postf = JsonConvert.DeserializeObject<DLData>(JSonData);
                        StrText = postf.Card6No.Substring(0, 4) + "-" + postf.Card6No.Substring(4, 2) + "xx-xxxx-" + postf.Card4No;
                        DropDownList1.Items.Remove(DropDownList1.Items.FindByValue(postf.CardID));
                        DropDownList1.Items.Add(new ListItem(StrText, postf.CardID));
                    }
                }
                else {
                    DropDownList1.Items.Clear();
                    DropDownList1.Items.Add(new ListItem("目前無綁定任何信用卡", ""));
                }

            }
            else
            {
                Response.Write("CheckMacValue Error");
            }
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            GetStr GS = new GetStr();
            
            String MerchantID = string.Empty;
            String MerchantMemberID = this.MemberID.Value;
            String HashKey = string.Empty;
            String HashIV = string.Empty;

            String ServerReplyURL = "http://sso.ezsale.tw/testallpayreturn.aspx";
            String ClientRedirectURL = "http://sso.ezsale.tw/Credit.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + Server.UrlEncode(this.returnurl.Value) + "&MemID=" + MerchantMemberID + "&language=" + this.language.Value + "&CheckM=" + GS.MD5Endode(this.siteid.Value + MerchantMemberID);
            String TradePostUrl = "https://payment-stage.allpay.com.tw/MerchantMember/BindingCardID";

            String setting = GS.GetSetting(this.siteid.Value);
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select HashIv,HashKey,mer_id from head", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            MerchantID = reader[2].ToString();
                            HashKey = reader[1].ToString();
                            HashIV = reader[0].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            String CheckMacValue = HttpUtility.UrlEncode("HashKey=" + HashKey + "&ClientRedirectURL=" + ClientRedirectURL + "&MerchantID=" + MerchantID + "&MerchantMemberID=" + MerchantMemberID + "&ServerReplyURL=" + ServerReplyURL + "&HashIV=" + HashIV).ToLower();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<html><body>").AppendLine();
            sb.Append("<form name='allpaywebatm'  id='allpaywebatm' action='" + TradePostUrl + "' method='POST'>").AppendLine();
            sb.Append("<input type='hidden' name='ClientRedirectURL' value='" + ClientRedirectURL + "'>").AppendLine();
            sb.Append("<input type='hidden' name='CheckMacValue' value='" + GetSHA256String(CheckMacValue) + "'>").AppendLine();
            sb.Append("<input type='hidden' name='MerchantID' value='" + MerchantID + "'>").AppendLine();
            sb.Append("<input type='hidden' name='MerchantMemberID' value='" + MerchantMemberID + "'>").AppendLine();
            sb.Append("<input type='hidden' name='ServerReplyURL' value='" + ServerReplyURL + "'>").AppendLine();
            sb.Append("</form>").AppendLine();
            sb.Append("<script> var theForm = document.forms['allpaywebatm'];  if (!theForm) { theForm = document.allpaywebatm; } theForm.submit(); </script>").AppendLine();
            sb.Append("<html><body>").AppendLine();
            Response.Write(sb.ToString());
            Response.End();
        }

        protected void Button6_Click(object sender, EventArgs e)
        {
            if (DropDownList1.SelectedValue == "")
            {
                
            }
            else
            {
                GetStr GS = new GetStr();

                String MerchantID = string.Empty;
                String MerchantMemberID = this.MemberID.Value;
                String HashKey = string.Empty;
                String HashIV = string.Empty;

                String CardID = DropDownList1.SelectedValue;
                String TradePostUrl = "https://payment-stage.allpay.com.tw/MerchantMember/DeleteCardID";
                String UpdateCarNoUrl = "https://payment-stage.allpay.com.tw/MerchantMember/QueryMemberBinding";

                String setting = GS.GetSetting(this.siteid.Value);
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select HashIv,HashKey,mer_id from head", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                MerchantID = reader[2].ToString();
                                HashKey = reader[1].ToString();
                                HashIV = reader[0].ToString();
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }

                String CheckMacValue = HttpUtility.UrlEncode("HashKey=" + HashKey + "&CardID=" + CardID + "&MerchantID=" + MerchantID + "&MerchantMemberID=" + MerchantMemberID + "&HashIV=" + HashIV).ToLower();

                string param = "CheckMacValue=" + GetSHA256String(CheckMacValue) + "&CardID=" + CardID + "&MerchantID=" + MerchantID + "&MerchantMemberID=" + MerchantMemberID;

                DataTable dt = SendForm(TradePostUrl, param);

                String rRtnCode = "";
                String rRtnMsg = "";
                String rMerchantID = "";
                String rMerchantMemberID = "";
                String rCardID = "";
                String rCheckMacValue = "";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    switch (dt.Rows[i][0].ToString())
                    {
                        case "RtnCode":
                            rRtnCode = dt.Rows[i][1].ToString();
                            break;
                        case "RtnMsg":
                            rRtnMsg = dt.Rows[i][1].ToString();
                            break;
                        case "MerchantID":
                            rMerchantID = dt.Rows[i][1].ToString();
                            break;
                        case "MerchantMemberID":
                            rMerchantMemberID = dt.Rows[i][1].ToString();
                            break;
                        case "CardID":
                            rCardID = dt.Rows[i][1].ToString();
                            break;
                        case "CheckMacValue":
                            rCheckMacValue = dt.Rows[i][1].ToString();
                            break;
                        default:
                            break;
                    }
                }

                CheckMacValue = HttpUtility.UrlEncode("HashKey=" + HashKey + "&CardID=" + rCardID + "&MerchantID=" + rMerchantID + "&MerchantMemberID=" + rMerchantMemberID + "&RtnCode=" + rRtnCode + "&RtnMsg=" + rRtnMsg + "&HashIV=" + HashIV).ToLower();

                    GetCarNO(MerchantID, MerchantMemberID, HashKey, HashIV, UpdateCarNoUrl);
                    Response.Write("<script>alert('success')</script>");   
                
            }
        }

        protected void Button7_Click(object sender, EventArgs e)
        {           
            if (DropDownList1.SelectedValue == "")
            {
                //Label1.Text = "請選擇卡號";
            }
            else
            {
                GetStr GS = new GetStr();

                String MerchantID = string.Empty;
                String MerchantMemberID = this.MemberID.Value;
                String HashKey = string.Empty;
                String HashIV = string.Empty;

                String CardID = DropDownList1.SelectedValue;               
                String ServerReplyURL = "http://sso.ezsale.tw/testallpayreturn.aspx";
                String ClientRedirectURL = "http://sso.ezsale.tw/Credit.aspx?SiteID=" + this.siteid.Value + "&ReturnUrl=" + Server.UrlEncode(this.returnurl.Value) + "&MemID=" + MerchantMemberID + "&language=" + this.language.Value + "&CheckM=" + GS.MD5Endode(this.siteid.Value + this.MemberID.Value);
                String TradePostUrl = "https://payment-stage.allpay.com.tw/MerchantMember/EditCardID";

                String setting = GS.GetSetting(this.siteid.Value);
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select HashIv,HashKey,mer_id from head", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                MerchantID = reader[2].ToString();
                                HashKey = reader[1].ToString();
                                HashIV = reader[0].ToString();
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }

                String CheckMacValue = HttpUtility.UrlEncode("HashKey=" + HashKey + "&CardID=" + CardID + "&ClientRedirectURL=" + ClientRedirectURL + "&MerchantID=" + MerchantID + "&MerchantMemberID=" + MerchantMemberID + "&ServerReplyURL=" + ServerReplyURL + "&HashIV=" + HashIV).ToLower();

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("<html><body>").AppendLine();
                sb.Append("<form name='allpaywebatm'  id='allpaywebatm' action='" + TradePostUrl + "' method='POST'>").AppendLine();
                sb.Append("<input type='hidden' name='ClientRedirectURL' value='" + ClientRedirectURL + "'>").AppendLine();
                sb.Append("<input type='hidden' name='CheckMacValue' value='" + GetSHA256String(CheckMacValue) + "'>").AppendLine();
                sb.Append("<input type='hidden' name='CardID' value='" + CardID + "'>").AppendLine();
                sb.Append("<input type='hidden' name='MerchantID' value='" + MerchantID + "'>").AppendLine();
                sb.Append("<input type='hidden' name='MerchantMemberID' value='" + MerchantMemberID + "'>").AppendLine();
                sb.Append("<input type='hidden' name='ServerReplyURL' value='" + ServerReplyURL + "'>").AppendLine();
                sb.Append("</form>").AppendLine();
                sb.Append("<script> var theForm = document.forms['allpaywebatm'];  if (!theForm) { theForm = document.allpaywebatm; } theForm.submit(); </script>").AppendLine();
                sb.Append("<html><body>").AppendLine();
                Response.Write(sb.ToString());
                Response.End();
            }

        }

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
    }
}