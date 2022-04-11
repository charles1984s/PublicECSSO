using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Security.Application;

namespace ECSSO
{
    /// <summary>
    /// GetVirtualProd 的摘要描述
    /// </summary>
    public class GetVirtualProd : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            if (context.Request.Params["siteid"] == null) ResponseWriteEnd(context, ErrorMsg("error", "siteid必填"));
            if (context.Request.Params["prodid"] == null) ResponseWriteEnd(context, ErrorMsg("error", "prodid必填"));
            if (context.Request.Params["MemberID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "MemberID必填"));
            if (context.Request.Params["Type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Type必填"));

            if (context.Request.Params["siteid"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "siteid必填"));
            if (context.Request.Params["prodid"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "prodid必填"));
            if (context.Request.Params["MemberID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "MemberID必填"));
            if (context.Request.Params["Type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Type必填"));

            String SiteID = Encoder.HtmlEncode(context.Request.Params["siteid"].ToString());
            String ProdID = Encoder.HtmlEncode(context.Request.Params["prodid"].ToString());
            String MemberID = Encoder.HtmlEncode(context.Request.Params["MemberID"].ToString());
            String Type = Encoder.HtmlEncode(context.Request.Params["Type"].ToString());
            
            String ReturnStr = "";
            GetStr GS = new GetStr();
            String setting = GS.GetSetting(SiteID);
            
            List<String> MySearchID = new List<String>();
            MySearchID.Add(ProdID);
            
            #region 判斷目前是否為瀏覽時間內

            using (SqlConnection conn = new SqlConnection(setting))
            {                
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from orders as a left join orders_hd as b on a.order_no=b.id where b.mem_id=@MemberID and a.virtual='Y' and @datetime between a.start_date and a.end_date and a.productid=@prodid", conn);
                if (MemberID != "" && ProdID != "") {
                    cmd.Parameters.Add(new SqlParameter("@MemberID", MemberID));
                    cmd.Parameters.Add(new SqlParameter("@datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    cmd.Parameters.Add(new SqlParameter("@prodid", ProdID));
                    
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            if (Type == "1") 
                            {
                                Products Prods = new Products();
                                Library.Products.RootObject rlib = JsonConvert.DeserializeObject<Library.Products.RootObject>(Prods.GetProdJson(MySearchID, MemberID, SiteID));
                                foreach (Library.Products.ProductData Product in rlib.ProductDatas)
                                {
                                    ReturnStr += "<div class='col-md-12 menu_title'>" + Product.Title + "</div>";
                                    String CodeURL = System.Web.Configuration.WebConfigurationManager.AppSettings["Protocol"] + "://" + System.Web.Configuration.WebConfigurationManager.AppSettings["Server_Host"] + "/GetVirtualProd.ashx?siteid=" + SiteID + "&prodid=" + ProdID + "&MemberID=" + MemberID + "&Type=2";
                                    ReturnStr += @"<div class='col-md-12'><img src='/api/QRCode.ashx?code=" + HttpContext.Current.Server.UrlEncode(CodeURL) + "'></div>";
                                    
                                    foreach (Library.Products.MenuCont menucont in Product.MenuConts)
                                    {
                                        ReturnStr += "<div class='col-md-12'>";
                                        ReturnStr += "<img src='" + menucont.Img.Replace("/upload/", "/upload/" + GS.GetOrgName(setting) + "/") + "' title='" + menucont.Title + "' style='float: " + menucont.ImgAlign + ";' />" + HttpUtility.HtmlDecode(HttpUtility.HtmlDecode(menucont.Cont));
                                        ReturnStr += "</div>";
                                    }
                                }
                                ResponseWriteEnd(context, HttpUtility.HtmlDecode(Encoder.HtmlEncode(ReturnStr)));
                            }
                            if (Type == "2") 
                            {
                                while (reader.Read()) 
                                {
                                    ResponseWriteEnd(context, reader["prod_name"].ToString() + "，卡號：" + reader["vcode"].ToString() + "，有效期限：" + reader["start_date"].ToString() + " ~ " + reader["end_date"].ToString());
                                }                                
                            }
                        }
                        else
                        {
                            ResponseWriteEnd(context, "<div class='col-md-12'><span class='expired'>已過期</span></div>");
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }                
            }
            #endregion                        
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg)
        {

            Library.Products.RootObject root = new Library.Products.RootObject();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);

        }
        #endregion

        private void ResponseWriteEnd(HttpContext context, string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }
    }
}