using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.IO;
using System.Data;
using Newtonsoft.Json;

namespace ECSSO
{
    public partial class testallpay : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            

            if (Request.Form["RtnMsg"] != null) {
                Response.Write("<script>alert('" + Request.Form["RtnMsg"].ToString() + "')</script>");
            }

        }
        
        protected void Button3_Click(object sender, EventArgs e)
        {
            String MerchantID = "2000214";
            String MerchantMemberID = "000001";
            String HashKey = "5294y06JbISpM5x9";
            String HashIV = "v77hoKGq4kWxNNIS";
            String TradePostUrl = "https://payment-stage.allpay.com.tw/MerchantMember/QueryMemberBinding";

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
                    catch {
                        DLData postf = JsonConvert.DeserializeObject<DLData>(JSonData);

                        StrText = postf.Card6No.Substring(0, 4) + "-" + postf.Card6No.Substring(4, 2) + "xx-xxxx-" + postf.Card4No;
                        DropDownList1.Items.Remove(DropDownList1.Items.FindByValue(postf.CardID));
                        DropDownList1.Items.Add(new ListItem(StrText, postf.CardID));
                        
                    }
                }
                
            }
            else 
            {
                Response.Write("CheckMacValue Error");
            }
        }

        
        protected void Button4_Click(object sender, EventArgs e)
        {
            String MerchantID = "2000214";
            String MerchantMemberID = "000001";
            String HashKey = "5294y06JbISpM5x9";
            String HashIV = "v77hoKGq4kWxNNIS";
            String ServerReplyURL = "http://sso.ezsale.tw/testallpayreturn.aspx";
            String ClientRedirectURL = "http://sso.ezsale.tw/testallpay.aspx";
            String TradePostUrl = "https://payment-stage.allpay.com.tw/MerchantMember/BindingCardID";

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

        protected void Button5_Click(object sender, EventArgs e)
        {
            if (DropDownList1.SelectedValue == "")
            {
                Label1.Text = "請選擇卡號";
            }
            else {
                String CardID = DropDownList1.SelectedValue;
                String MerchantID = "2000214";
                String MerchantTradeDate = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
                String MerchantTradeNo = DateTime.Now.ToString("yyyyMMddhhmmss");
                String stage = "0";
                String TotalAmount = "150";
                String TradeDesc = "網路購物";
                String HashKey = "5294y06JbISpM5x9";
                String HashIV = "v77hoKGq4kWxNNIS";
                String TradePostUrl = "https://payment-stage.allpay.com.tw/MerchantMember/AuthCardID";
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
                    }
                    else
                    {
                        rRtn = "授權失敗";
                    }
                    Label1.Text = rRtn + ",歐付寶交易序號=" + rAllpayTradeNo;
                }
                else
                {
                    Response.Write("CheckMacValue Error");
                }
            }

            
        }
        
        protected void Button6_Click(object sender, EventArgs e)
        {
            if (DropDownList1.SelectedValue == "")
            {
                Label1.Text = "請選擇卡號";
            }
            else
            {
                String CardID = DropDownList1.SelectedValue;
                String MerchantID = "2000214";
                String MerchantMemberID = "000001";
                String HashKey = "5294y06JbISpM5x9";
                String HashIV = "v77hoKGq4kWxNNIS";
                String TradePostUrl = "https://payment-stage.allpay.com.tw/MerchantMember/DeleteCardID";

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
                Response.Write(GetSHA256String(CheckMacValue)+"<br>");
                Response.Write(rCheckMacValue);
                if (SHA256Check(rCheckMacValue, CheckMacValue))
                {
                    DropDownList1.Items.Remove(DropDownList1.Items.FindByValue(rCardID));
                    Label1.Text = "信用卡已取消";
                }
                else
                {
                    Label1.Text = "CheckMacValue錯誤";
                }
            }            
        }

        protected void Button7_Click(object sender, EventArgs e)
        {
            if (DropDownList1.SelectedValue == "")
            {
                Label1.Text = "請選擇卡號";
            }
            else
            {
                String CardID = DropDownList1.SelectedValue;
                String MerchantID = "2000214";
                String MerchantMemberID = "131313";
                String HashKey = "5294y06JbISpM5x9";
                String HashIV = "v77hoKGq4kWxNNIS";
                String ServerReplyURL = "http://sso.ezsale.tw/testallpayreturn.aspx";
                String ClientRedirectURL = "http://sso.ezsale.tw/testallpay.aspx";
                String TradePostUrl = "https://payment-stage.allpay.com.tw/MerchantMember/EditCardID";

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
    }
}