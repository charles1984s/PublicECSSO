using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ECSSO
{
    public partial class map : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (Request.Form["CVSStoreID"] != null)
            {
                this.cMerchantID.Text = Request.Form["MerchantID"].ToString();
                this.cMerchantTradeNo.Text = Request.Form["MerchantTradeNo"].ToString();
                this.cLogisticsSubType.Text = Request.Form["LogisticsSubType"].ToString();
                this.cCVSStoreID.Text = Request.Form["CVSStoreID"].ToString();
                this.cCVSStoreName.Text = Request.Form["CVSStoreName"].ToString();
                this.cCVSAddress.Text = Request.Form["CVSAddress"].ToString();
                this.cCVSTelephone.Text = Request.Form["CVSTelephone"].ToString();

                this.Page.Controls.Add(
                    new LiteralControl(
                        string.Format("<script>opener.form1.CVSStoreID.value ='{0}';" +
                                        "opener.form1.CVSStoreName.value ='{1}';opener.form1.CVSAddress.value ='{2}';" +
                                        "opener.form1.CVSTelephone.value ='{3}'; window.close();</script>"
                    , Request.Form["CVSStoreID"].ToString()
                    , Request.Form["CVSStoreName"].ToString()
                    , Request.Form["CVSAddress"].ToString()
                    , Request.Form["CVSTelephone"].ToString())));

            }
            else
            {
                if (!IsPostBack)
                {
                    if (Request.QueryString["CheckM"] == null) Response.Write("CheckM必填");
                    if (Request.QueryString["SiteID"] == null) Response.Write("SiteID必填");
                    if (Request.QueryString["LogisticsSubType"] == null) Response.Write("LogisticsSubType必填");
                    if (Request.QueryString["IsCollection"] == null) Response.Write("IsCollection必填");

                    if (Request.QueryString["CheckM"] == "") Response.Write("CheckM必填");
                    if (Request.QueryString["SiteID"] == "") Response.Write("SiteID必填");
                    if (Request.QueryString["LogisticsSubType"] == "") Response.Write("LogisticsSubType必填");
                    if (Request.QueryString["IsCollection"] == "") Response.Write("IsCollection必填");

                    String CheckM = Request.QueryString["CheckM"];
                    String SiteID = Request.QueryString["SiteID"];

                    GetStr GS = new GetStr();
                    String Setting = GS.GetSetting(SiteID);
                    String OrgName = GS.GetOrgName(Setting);

                    if (GS.MD5Check(SiteID + OrgName, CheckM))
                    {

                        String TradePostUrl = "https://newlogistics-stage.allpay.com.tw/Express/map";
                        String MerchantID = "";
                        String MerchantTradeNo = DateTime.Now.ToString("yyyyMMddhhmmss");
                        String LogisticsType = "CVS";  //CVS:超商取貨
                        String LogisticsSubType = Request.QueryString["LogisticsSubType"].ToString();   //---B2C---//FAMI：全家,UNIMART：統一超商,HILIFE：萊爾富//---C2C---//FAMIC2C：全家店到店,UNIMARTC2C：統一超商交貨便,HILIFEC2C:萊爾富店到店
                        String IsCollection = Request.QueryString["IsCollection"].ToString();   //N：不代收貨款。Y：代收貨款。
                        String ServerReplyURL = WebConfigurationManager.AppSettings["Protocol"] + "://" + WebConfigurationManager.AppSettings["Server_Host"] + "/map.aspx"; //取得超商店鋪代號等資訊後，會回傳到此網址。
                        String ExtraData = "";  //供廠商傳遞保留的資訊，在回傳參數中，會原值回傳。
                        String Device = "0";     //0：PC（預設值）1：Mobile

                        using (SqlConnection conn = new SqlConnection(Setting))
                        {
                            conn.Open();

                            SqlCommand cmd = new SqlCommand("select b.mer_id,b.ecpay_logistics_url from CurrentUseFrame as a left join head as b on a.id=b.hid", conn);
                            SqlDataReader reader = cmd.ExecuteReader();
                            try
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        MerchantID = reader[0].ToString();
                                        TradePostUrl = "https://" + reader[1].ToString() + "/Express/map";
                                    }
                                }
                            }
                            finally
                            {
                                reader.Close();
                            }



                            String param = "MerchantID=" + MerchantID + "&MerchantTradeNo=" + MerchantTradeNo + "&LogisticsType=" + LogisticsType + "&LogisticsSubType=" + LogisticsSubType + "&IsCollection=" + IsCollection + "&ServerReplyURL=" + ServerReplyURL + "&ExtraData=" + ExtraData + "&Device=" + Device;
                            //SendForm(TradePostUrl, param);

                            Response.Redirect(TradePostUrl + "?" + param);

                            //choiceshop.InnerHtml = "<iframe src='" + TradePostUrl + "?" + param + "' height='700'></iframe>";
                        }

                    }
                    else 
                    {

                        Response.Write("驗證錯誤");
                    }

                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (Request.Form["CVSStoreID"] != null)
            {
                string script = "window.opener.form1.CVSStoreID.value = " + Request.Form["CVSStoreID"].ToString() + ";";
                ClientScript.RegisterStartupScript(GetType(), "script", script, true);
            }
        }
    }
}