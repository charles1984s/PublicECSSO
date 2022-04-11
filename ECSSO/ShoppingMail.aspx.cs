using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ECSSO
{
    public partial class ShoppingMail : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

                    String mail_cont = "<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />";
                    mail_cont +="<font size='4' color='#ff0000'><b>親愛的會員，您好！</b></font><br>";
                    mail_cont += "<b>非常感謝您的訂購，以下為您的訂購清單，而非付款收據，為保障您的資訊安全，部份訊息將以'*'標記。</b>";
                    mail_cont +="<ul>";
                    mail_cont +="	<li>";
                    mail_cont +="    	若您使用ATM轉帳付款，請於訂購日起兩日內轉帳，繳費完成後請主動與公司客服聯絡。";
                    mail_cont +="    </li>";
                    mail_cont +="   <li>";
                    mail_cont +="    	若您使用其它付款方式或 貨到付款，您可以至訂單查詢了解訂單詳情與處理進度。";
                    mail_cont += "  </li>";
                    mail_cont +="</ul>";
                    mail_cont +="<hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px; margin-top:8px; margin-bottom:20px;'>";
                    mail_cont +="<style>";
                    mail_cont +="	.mail_tb{border:none;margin-left:10px;margin-right:10px; width:600px; font-size:9pt; font-family:'microsoft jhenghei', sans-serif;}";
                    mail_cont +="	.mail_tb th{ background:#f2f2f2;padding:7px;font-size:11pt; font-weight:bold;}";
                    mail_cont +="	.mail_tb td{ background:#fff; padding:7px; border-top-width:0px; border-color:#808080;}";
                    mail_cont +="	.mail_tb td, .mail_tb th{ border-left-width:0px;}";
                    mail_cont +="	.mail_tb td:last-child, .mail_tb th:last-child{ border-right-width:0px;}";
                    mail_cont +="</style>";

                    mail_cont += "<p style='color:#e30000; font-weight:bold; max-width:600px; margin:5px 10px;'>" + GetLocalResourceObject("訂單編號") + "：<span id='order_id' style='font-weight:normal; color:#000;'>" + OrderID + "</span></p>";

                    mail_cont +="<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0'>";
                    mail_cont +="  <tr>";
                    mail_cont += "    <th colspan='4' align='left' valign='middle' scope='col'>" + GetLocalResourceObject("訂購人") + "：" + OName + "</th>";
                    mail_cont +="  </tr>";
                    mail_cont +="  <tr>";
                    mail_cont += "    <td align='right' valign='middle' style='width:15%;'>" + GetLocalResourceObject("手機") + "</td>";
                    mail_cont += "    <td align='left' valign='middle' style='width:35%;'><span style='color:#333;'>" + OCell + "</span></td>";
                    mail_cont += "    <td align='right' valign='middle' style='width:15%;'>" + GetLocalResourceObject("電話") + "</td>";
                    mail_cont += "    <td align='left' valign='middle' style='width:35%;'><span style='color:#333;'>" + OTel + "</span></td>";
                    mail_cont +="  </tr>";
                    mail_cont +="  <tr>";
                    mail_cont += "    <td align='right' valign='middle'>" + GetLocalResourceObject("電子信箱") + "</td>";
                    mail_cont += "    <td colspan='3' align='left' valign='middle'>" + OEmail + "</td>";
                    mail_cont +="  </tr>";
                    mail_cont +="</table>";
                    mail_cont +="<br>";


                    mail_cont +="<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0'>";
                    mail_cont +="  <tr>";
                    mail_cont += "    <th colspan='4' align='left' valign='middle' scope='col'>" + GetLocalResourceObject("收件人") + "：" + SName + "<b style=' margin-left:400px'>";
                    if (SSex == "1")
                    {
                        mail_cont = mail_cont + "先生";
                    }
                    else {
                        mail_cont = mail_cont + "小姐";
                    }
                    mail_cont = mail_cont + "</b></th>";
                    mail_cont +="  </tr>";
                    mail_cont +="  <tr>";
                    mail_cont += "    <td align='right' valign='middle'>" + GetLocalResourceObject("地址") + "</td>";
                    mail_cont += "    <td colspan='3' align='left' valign='middle'><span style='color:#333;'>" + Address + "</span></td>";
                    mail_cont +="  </tr>";
                    mail_cont +="  <tr>";
                    mail_cont += "    <td align='right' valign='middle' style='width:15%;'>" + GetLocalResourceObject("手機") + "</td>";
                    mail_cont += "    <td align='left' valign='middle' style='width:35%;'><span style='color:#333;'>" + SCell + "</span></td>";
                    mail_cont += "    <td align='right' valign='middle' style='width:15%;'>" + GetLocalResourceObject("電話") + "</td>";
                    mail_cont += "    <td align='left' valign='middle' style='width:35%;'><span style='color:#333;'>" + STel + "</span></td>";
                    mail_cont +="  </tr>";
                    mail_cont +="  <tr>";
                    mail_cont += "    <td align='right' valign='middle'>" + GetLocalResourceObject("備註") + "</td>";
                    mail_cont += "    <td colspan='3' align='left' valign='middle'><span style='color:#333;'>" + Notememo + "</span></td>";
                    mail_cont +="  </tr>";
                    mail_cont +="</table>";
                    mail_cont +="<br>";


                    mail_cont +="<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0'>";
                    mail_cont +="  <tr>";
                    mail_cont += "    <th align='left' scope='col' style='width:40%;'>" + GetLocalResourceObject("訂購項目") + "</th>";
                    mail_cont += "    <th align='center' scope='col' style='width:10%;'>" + GetLocalResourceObject("料號") + "</th>";
                    mail_cont += "    <th align='center' scope='col' style='width:10%;'>" + GS.GetSpecTitle(setting, "1") + "</th>";
                    mail_cont += "    <th align='center' scope='col' style='width:10%;'>" + GS.GetSpecTitle(setting, "2") + "</th>";
                    mail_cont += "    <th align='center' scope='col' style='width:10%;'>" + GetLocalResourceObject("數量") + "</th>";
                    mail_cont += "    <th align='center' scope='col' style='width:15%;'>" + GetLocalResourceObject("單價") + "</th>";
                    mail_cont += "    <th align='center' scope='col' style='width:15%;'>" + GetLocalResourceObject("合計") + "</th>";
                    mail_cont +="  </tr>";

                    i = 0;
                    int totalamt = 0;
                    foreach (Shoppingcar.OrderList Orders in rlib.OrderData.OrderLists)
                    {
                        foreach (Shoppingcar.OrderItem Items in Orders.OrderItems)
                        {
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
                                
                                mail_cont +="  <tr>";
                                mail_cont += "    <td align='left'>";
                                if (Orders.Type != "0")
                                {
                                    mail_cont += "<span style='color:#ff0000;'>" + Orders.Title + "</span> - ";
                                }
                                mail_cont += Items.Name + "</td>";
                                mail_cont += "    <td align='center'>" + ProdItemno + "</td>";
                                mail_cont += "    <td align='center'>" + GS.GetSpec(setting, "prod_color", OrderSpecs.Color) + "</td>";
                                mail_cont += "    <td align='center'>" + GS.GetSpec(setting, "prod_size", OrderSpecs.Size) + "</td>";
                                mail_cont += "    <td align='center'>" + OrderSpecs.Qty + "</td>";
                                mail_cont += "    <td align='center'>" + OrderSpecs.FinalPrice + "</td>";
                                mail_cont += "    <td align='center'>" + Convert.ToInt32(OrderSpecs.Qty) * Convert.ToInt32(OrderSpecs.FinalPrice) + "</td>";
                                mail_cont +="  </tr>";
                                totalamt += Convert.ToInt32(OrderSpecs.FinalPrice) * Convert.ToInt32(OrderSpecs.Qty);

                                i = i + 1;
                            }

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
                                mail_cont += "    <td align='left'><span style='color:#ff0000;'>加價購-</span>" + AdditionalItems.Name + "</td>";
                                mail_cont += "    <td align='center'>" + ProdItemno + "</td>";
                                mail_cont += "    <td align='center'>" + GS.GetSpec(setting, "prod_color", AdditionalItems.Color) + "</td>";
                                mail_cont += "    <td align='center'>" + GS.GetSpec(setting, "prod_size", AdditionalItems.Size) + "</td>";
                                mail_cont += "    <td align='center'>" + AdditionalItems.Qty + "</td>";
                                mail_cont += "    <td align='center'>" + AdditionalItems.FinalPrice + "</td>";
                                mail_cont += "    <td align='center'>" + Convert.ToInt32(AdditionalItems.Qty) * Convert.ToInt32(AdditionalItems.FinalPrice) + "</td>";
                                mail_cont += "  </tr>";
                                totalamt += Convert.ToInt32(AdditionalItems.FinalPrice) * Convert.ToInt32(AdditionalItems.Qty);

                                i = i + 1;
                            }
                        }
                    }

                    if (Convert.ToInt32(DiscountAmt) > 0 || Convert.ToInt32(DiscountAmt) < 0) {
                            mail_cont += "  <tr>";
                            mail_cont += "    <td align='right'>" + GetLocalResourceObject("訂單總折扣") + "</td>";
                            mail_cont += "    <td align='center'>&nbsp;</td>";
                            mail_cont += "    <td align='center'>&nbsp;</td>";
                            mail_cont += "    <td align='center'>&nbsp;</td>";
                            mail_cont += "    <td align='center'>&nbsp;</td>";
                            mail_cont += "    <td align='center'>&nbsp;</td>";
                            mail_cont += "    <td align='center'>" + DiscountAmt * (-1) + "</td>";
                            mail_cont += "  </tr>";
                    }
            
                    mail_cont +="  <tr>";
                    mail_cont += "    <td align='right'>" + GetLocalResourceObject("運費") + "</td>";
                    mail_cont +="    <td align='center'>&nbsp;</td>";
                    mail_cont +="    <td align='center'>&nbsp;</td>";
                    mail_cont +="    <td align='center'>&nbsp;</td>";
                    mail_cont +="    <td align='center'>&nbsp;</td>";
                    mail_cont +="    <td align='center'>&nbsp;</td>";
                    mail_cont += "    <td align='center'>" + FreightAmount + "</td>";
                    mail_cont +="  </tr>";


                    mail_cont +="  <tr>";
                    mail_cont += "    <td colspan='7' align='right'>合計新台幣<b style='font-size: 18pt;color: #f00;'>NT$" + (totalamt + Int32.Parse(FreightAmount) - DiscountAmt) + "</b></td>";
                    mail_cont +="  </tr>";


                    //紅利扣抵
                    if (Int32.Parse(BonusDiscount.ToString()) > 0)
                    {
                        mail_cont += "  <tr>";
                        mail_cont += "    <td align='right'>" + GetLocalResourceObject("紅利扣抵金額") + "</td>";
                        mail_cont += "    <td align='center'>&nbsp;</td>";
                        mail_cont += "    <td align='center'>&nbsp;</td>";
                        mail_cont += "    <td align='center'>&nbsp;</td>";
                        mail_cont += "    <td align='center'>&nbsp;</td>";
                        mail_cont += "    <td align='center'>&nbsp;</td>";
                        mail_cont += "    <td align='center'>" + BonusDiscount + "</td>";
                        mail_cont += "  </tr>";
                        mail_cont += "  <tr>";
                        mail_cont += "    <td align='right'>" + GetLocalResourceObject("扣抵後金額(含運費)") + "</td>";
                        mail_cont += "    <td align='center'>&nbsp;</td>";
                        mail_cont += "    <td align='center'>&nbsp;</td>";
                        mail_cont += "    <td align='center'>&nbsp;</td>";
                        mail_cont += "    <td align='center'>&nbsp;</td>";
                        mail_cont += "    <td align='center'>&nbsp;</td>";
                        mail_cont += "    <td align='center'>" + (totalamt + Convert.ToInt32(FreightAmount) - DiscountAmt - Convert.ToInt32(BonusDiscount)) + "</td>";
                        mail_cont += "  </tr>";

                    }

                    mail_cont +="</table>";
                    mail_cont +="<br>";
                    mail_cont +="<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0'>";
                    mail_cont +="  <tr>";
                    mail_cont += "    <th align='left' scope='col'>" + GetLocalResourceObject("付款方式") + "：<b style='font-size: 18pt;color: #f00;'>" + GS.GetPayType(rlib.OrderData.PayType) + "</b></th>";
                    mail_cont +="  </tr>";
                    mail_cont +="  <tr>";
                    mail_cont +="    <td align='left'><span style='color:#f00;'>提醒您選擇的付款式為ATM轉帳方式，目前尚未付款完成，請您於繳費期限內完成，繳費完成後請主動與公司客服聯絡。若逾期未付清款項將自動取消本訂單，謝謝。</span></td>";
                    mail_cont +="  </tr>";
                    mail_cont +="</table>";
                    mail_cont += "<br>";

                    if (payment_type == "ATM")
                    {
    
                        mail_cont += "<table class='mail_tb' width='600' border='1' cellspacing='0' cellpadding='0'>";
                        mail_cont += "  <tr>";
                        mail_cont += "    <th colspan='2' align='left' scope='col'>" + GetLocalResourceObject("繳費資訊") + "</th>";
                        mail_cont += "  </tr>";
                        mail_cont += "  <tr>";
                        mail_cont += "    <td align='right' style='width:30%;'><strong>" + GetLocalResourceObject("轉帳銀行代號") + "</strong></td>";
                        mail_cont += "    <td align='left' style='width:70%;'>" + remit_money1 + "</td>";
                        mail_cont += "  </tr>";
                        mail_cont += "  <tr>";
                        mail_cont += "    <td align='right'><strong>" + GetLocalResourceObject("轉帳帳號") + "</strong></td>";
                        mail_cont += "    <td align='left'>" + remit_money2 + "</td>";
                        mail_cont += "  </tr>";
                        mail_cont += "  <tr>";
                        mail_cont += "    <td align='right'><strong>" + GetLocalResourceObject("戶名") + "</strong></td>";
                        mail_cont += "    <td align='left'>" + remit_money3 + "</td>";
                        mail_cont += "  </tr>";
                        mail_cont += "  <tr>";
                        mail_cont += "    <td align='right'><strong>" + GetLocalResourceObject("應繳金額") + "</strong></td>";
                        mail_cont += "    <td align='left'>新台幣NT$" + (totalamt + Convert.ToInt32(FreightAmount) - Convert.ToInt32(BonusDiscount)) + "</td>";
                        mail_cont += "  </tr>";
                        mail_cont += "  <tr>";
                        mail_cont += "    <td align='right'><strong style='color:#f00;'>" + GetLocalResourceObject("繳費期限") + "</strong></td>";
                        mail_cont += "    <td align='left'><span style='color:#f00;'>" + DateTime.Today.AddDays(1).ToString("yyyy/MM/dd") + " 23:59</span></td>";
                        mail_cont += "  </tr>";
                        mail_cont += "</table>";
                    }

                    mail_cont +="<hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px;'>";
                    mail_cont +="提醒您，客服人員均不會要求消費者更改帳號或要求以ATM重新轉帳匯款<br>";
                    mail_cont +="若有上述情形，請立即撥打165防詐騙專線查詢";
                    mail_cont +="<hr style='border:0px; border-top-color: #999;border-top-style: dashed;border-top-width: 1px;border-bottom-color: #999;border-bottom-style: dashed;border-bottom-width: 1px; margin-top:10px; margin-bottom:10px; padding-bottom:3px;'>";
        }
        private void oldmail() {
            String mail_cont = "<center><span style='color:red;'>" + GetLocalResourceObject("提醒您；此封『訂購通知』為系統發出，請勿直接回覆。") + "</span></center><br><table width='688' border='0' cellpadding='0' cellspacing='0' align='center'>";
            mail_cont = mail_cont + "  <tr>";
            mail_cont = mail_cont + "    <td><img src='http://www.cocker.com.tw/images/bg_01.gif'></td>";
            mail_cont = mail_cont + "  </tr>";
            mail_cont = mail_cont + "  <tr>";
            mail_cont = mail_cont + "    <td background='http://www.cocker.com.tw/images/bg_04.gif'><table width='600' border='0' align='center' cellpadding='0' cellspacing='0'>";
            mail_cont = mail_cont + "      <tr>";
            mail_cont = mail_cont + "        <td width='54'><img src='http://www.cocker.com.tw/images/icon_03.gif'></td>";
            mail_cont = mail_cont + "        <td width='546'><span style='color:#e30000; font-weight:bold; font-size:11pt;'>" + GetLocalResourceObject("訂單編號") + "：</span>";
            mail_cont = mail_cont + OrderID;
            mail_cont = mail_cont + "		</td>";
            mail_cont = mail_cont + "      </tr>";
            mail_cont = mail_cont + "    </table>";
            mail_cont = mail_cont + "        <br />";
            mail_cont = mail_cont + "        <table width='580' border='0' align='center' cellpadding='3' cellspacing='3' style='border-bottom:#CCCCCC 1px solid; border-left:#CCCCCC 1px solid;border-right:#CCCCCC 1px solid;border-top:#CCCCCC 1px solid;'>";
            mail_cont = mail_cont + "          <tr style='background:#f7f7f7;'>";
            mail_cont = mail_cont + "            <td colspan='2'>【" + GetLocalResourceObject("訂購人") + "】";
            mail_cont = mail_cont + OName;
            mail_cont = mail_cont + "			</td>";
            mail_cont = mail_cont + "          </tr>";
            mail_cont = mail_cont + "          <tr style='background:#f7f7f7;'>";
            mail_cont = mail_cont + "            <td>【" + GetLocalResourceObject("手機") + "】";
            mail_cont = mail_cont + OCell;
            mail_cont = mail_cont + "			</td>";
            mail_cont = mail_cont + "            <td>【" + GetLocalResourceObject("電話") + "】";
            mail_cont = mail_cont + OTel;
            mail_cont = mail_cont + "			</td>";
            mail_cont = mail_cont + "          </tr>";
            mail_cont = mail_cont + "          <tr style='background:#f4f2f7;'>";
            mail_cont = mail_cont + "            <td colspan='2'>【" + GetLocalResourceObject("電子信箱") + "】";
            mail_cont = mail_cont + OEmail;
            mail_cont = mail_cont + "			</td>";
            mail_cont = mail_cont + "          </tr>";
            mail_cont = mail_cont + "          <tr>";
            mail_cont = mail_cont + "            <td colspan='2' height='30'></td>";
            mail_cont = mail_cont + "          </tr>";
            mail_cont = mail_cont + "          <tr style='background:#f7f7f7;'>";
            mail_cont = mail_cont + "            <td>【" + GetLocalResourceObject("收件人") + "】";
            mail_cont = mail_cont + SName;
            mail_cont = mail_cont + "			</td>";
            mail_cont = mail_cont + "            <td>【" + GetLocalResourceObject("性別") + "】";
            if (SSex == "1")
            {
                mail_cont = mail_cont + "先生";
            }
            else
            {
                mail_cont = mail_cont + "小姐";
            }
            mail_cont = mail_cont + "			</td>";
            mail_cont = mail_cont + "          </tr>";
            mail_cont = mail_cont + "          <tr style='background:#f4f2f7;'>";
            mail_cont = mail_cont + "            <td colspan='2'>【" + GetLocalResourceObject("地址") + "】";
            mail_cont = mail_cont + Address;
            mail_cont = mail_cont + "			</td>";
            mail_cont = mail_cont + "          </tr>";
            mail_cont = mail_cont + "          <tr style='background:#f7f7f7;'>";
            mail_cont = mail_cont + "            <td>【" + GetLocalResourceObject("手機") + "】";
            mail_cont = mail_cont + SCell;
            mail_cont = mail_cont + "			</td>";
            mail_cont = mail_cont + "            <td>【" + GetLocalResourceObject("電話") + "】";
            mail_cont = mail_cont + STel;
            mail_cont = mail_cont + "			</td>";
            mail_cont = mail_cont + "          </tr>";
            mail_cont = mail_cont + "          <tr>";
            mail_cont = mail_cont + "            <td colspan='2' height='30'></td>";
            mail_cont = mail_cont + "          </tr>";
            mail_cont = mail_cont + "          <tr class='shop01'>";
            mail_cont = mail_cont + "            <td colspan='2'>【" + GetLocalResourceObject("付款方式") + "】";

            String payment_type = GS.GetPayType(rlib.OrderData.PayType);

            mail_cont = mail_cont + payment_type;
            mail_cont = mail_cont + "			</td>";
            mail_cont = mail_cont + "          </tr>";
            mail_cont = mail_cont + "          <tr style='background:#f4f2f7;'>";
            mail_cont = mail_cont + "            <td colspan='2'>【" + GetLocalResourceObject("備註") + "】";
            mail_cont = mail_cont + Notememo;
            mail_cont = mail_cont + "			</td>";
            mail_cont = mail_cont + "          </tr>";
            mail_cont = mail_cont + "        </table>";
            mail_cont = mail_cont + "      <br />";
            mail_cont = mail_cont + "        <table width='600' border='0' align='center' cellpadding='0' cellspacing='0'>";
            mail_cont = mail_cont + "          <tr>";
            mail_cont = mail_cont + "            <td width='54'><img src='http://www.cocker.com.tw/images/icon_03.gif'></td>";
            mail_cont = mail_cont + "            <td width='546'><span style='color:#e30000; font-weight:bold; font-size:11pt;'>" + GetLocalResourceObject("訂購項目") + "</span></td>";
            mail_cont = mail_cont + "          </tr>";
            mail_cont = mail_cont + "        </table>";
            mail_cont = mail_cont + "      <br />";
            mail_cont = mail_cont + "        <table width='580' border='0' align='center' cellpadding='3' cellspacing='3' style='border-bottom:#CCCCCC 1px solid; border-left:#CCCCCC 1px solid;border-right:#CCCCCC 1px solid;border-top:#CCCCCC 1px solid;'>";
            mail_cont = mail_cont + "          <tr style='background-color:#f0f0f0; color:0d087f;' align='center'>";
            mail_cont = mail_cont + "            <td width='280'>" + GetLocalResourceObject("訂購項目") + "</td>";
            mail_cont = mail_cont + "            <td width='50'>" + GetLocalResourceObject("料號") + "</td>";
            mail_cont = mail_cont + "            <td width='50'>" + GS.GetSpecTitle(setting, "1") + "</td>";
            mail_cont = mail_cont + "            <td width='50'>" + GS.GetSpecTitle(setting, "2") + "</td>";
            mail_cont = mail_cont + "            <td width='50'>" + GetLocalResourceObject("數量") + "</td>";
            mail_cont = mail_cont + "            <td width='50'>" + GetLocalResourceObject("單價") + "</td>";
            mail_cont = mail_cont + "            <td width='50'>" + GetLocalResourceObject("合計") + "</td>";
            mail_cont = mail_cont + "          </tr>";
            i = 0;
            int totalamt = 0;
            foreach (Shoppingcar.OrderList Orders in rlib.OrderData.OrderLists)
            {
                foreach (Shoppingcar.OrderItem Items in Orders.OrderItems)
                {
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
                        if (i % 2 == 0)
                        {
                            mail_cont = mail_cont + "	<tr style='background:#f4f2f7;'>";
                        }
                        else
                        {
                            mail_cont = mail_cont + "	<tr style='background:#f7f7f7;'>";
                        }
                        mail_cont = mail_cont + "				<td>";
                        if (Orders.Type != "0")
                        {
                            mail_cont = mail_cont + "<span style='color:#ff0000;'>" + Orders.Title + "</span> - ";
                        }
                        mail_cont = mail_cont + Items.Name;
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "				<td align='left'>";
                        mail_cont = mail_cont + ProdItemno;
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "				<td align='center'>";
                        mail_cont = mail_cont + GS.GetSpec(setting, "prod_color", OrderSpecs.Color);
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "				<td align='center'>";
                        mail_cont = mail_cont + GS.GetSpec(setting, "prod_size", OrderSpecs.Size);
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "				<td align='center'>";
                        mail_cont = mail_cont + OrderSpecs.Qty;
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "				<td align='right'>";
                        mail_cont = mail_cont + OrderSpecs.FinalPrice;
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "				<td align='right'>";
                        mail_cont = mail_cont + Convert.ToInt32(OrderSpecs.Qty) * Convert.ToInt32(OrderSpecs.FinalPrice);
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "			  </tr>";
                        totalamt += Convert.ToInt32(OrderSpecs.FinalPrice) * Convert.ToInt32(OrderSpecs.Qty);

                        i = i + 1;
                    }


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
                        if (i % 2 == 0)
                        {
                            mail_cont = mail_cont + "	<tr style='background:#f4f2f7;'>";
                        }
                        else
                        {
                            mail_cont = mail_cont + "	<tr style='background:#f7f7f7;'>";
                        }
                        mail_cont = mail_cont + "				<td><span style='color:#ff0000;'>加價購-</span>";
                        mail_cont = mail_cont + AdditionalItems.Name;
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "				<td align='left'>";
                        mail_cont = mail_cont + ProdItemno;
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "				<td align='center'>";
                        mail_cont = mail_cont + GS.GetSpec(setting, "prod_color", AdditionalItems.Color);
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "				<td align='center'>";
                        mail_cont = mail_cont + GS.GetSpec(setting, "prod_size", AdditionalItems.Size);
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "				<td align='center'>";
                        mail_cont = mail_cont + AdditionalItems.Qty;
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "				<td align='right'>";
                        mail_cont = mail_cont + AdditionalItems.FinalPrice;
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "				<td align='right'>";
                        mail_cont = mail_cont + Convert.ToInt32(AdditionalItems.Qty) * Convert.ToInt32(AdditionalItems.FinalPrice);
                        mail_cont = mail_cont + "				</td>";
                        mail_cont = mail_cont + "			  </tr>";
                        totalamt += Convert.ToInt32(AdditionalItems.FinalPrice) * Convert.ToInt32(AdditionalItems.Qty);

                        i = i + 1;
                    }
                }
            }
            mail_cont = mail_cont + "			  <tr style='background:#f4f2f7;'>";
            if (Convert.ToInt32(DiscountAmt) > 0 || Convert.ToInt32(DiscountAmt) < 0)
            {
                mail_cont = mail_cont + "			  <td>" + GetLocalResourceObject("訂單總折扣") + "</td><td></td><td></td><td></td><td></td><td></td><td align='right'>";
                mail_cont = mail_cont + DiscountAmt * (-1);
                mail_cont = mail_cont + "</td></tr>";
                mail_cont = mail_cont + "			  <tr style='background:#f4f2f7;'>";
            }

            mail_cont = mail_cont + "			  <td>" + GetLocalResourceObject("運費") + "</td><td></td><td></td><td></td><td></td><td></td><td align='right'>";
            mail_cont = mail_cont + FreightAmount;
            mail_cont = mail_cont + "</td></tr>";

            mail_cont = mail_cont + "<tr>";
            mail_cont = mail_cont + "            <td colspan='7' align='right'><span style='font-size:14pt; font-weight:bold;'>合計&nbsp;新台幣</span><span style='color:#FF0000; font-weight:bold;'>NT$";
            mail_cont = mail_cont + (totalamt + Int32.Parse(FreightAmount) - DiscountAmt);
            //紅利扣抵
            if (Int32.Parse(BonusDiscount.ToString()) > 0)
            {
                mail_cont = mail_cont + "			  <tr style='background:#f4f2f7;'>";
                mail_cont = mail_cont + "            <td colspan='7' align='right'><span style='font-size:14pt; font-weight:bold;'>" + GetLocalResourceObject("紅利扣抵金額") + "</span><span style='color:#FF0000; font-weight:bold;'>NT$";
                mail_cont = mail_cont + BonusDiscount;
                mail_cont = mail_cont + "			</span></td>";
                mail_cont = mail_cont + "          </tr>";
                mail_cont = mail_cont + "			  <tr style='background:#f4f2f7;'>";
                mail_cont = mail_cont + "            <td colspan='7' align='right'><span style='font-size:14pt; font-weight:bold;'>" + GetLocalResourceObject("扣抵後金額(含運費)") + "</span><span style='color:#FF0000; font-weight:bold;'>NT$";
                mail_cont = mail_cont + (totalamt + Convert.ToInt32(FreightAmount) - DiscountAmt - Convert.ToInt32(BonusDiscount));
                mail_cont = mail_cont + "			</span></td>";
                mail_cont = mail_cont + "          </tr>";
            }

            mail_cont = mail_cont + "			</span></td>";
            mail_cont = mail_cont + "          </tr>";
            mail_cont = mail_cont + "        </table>";
            if (Convert.ToInt32(FreightAmount) > 0)
            {
                /*
                mail_cont = mail_cont + "        <table width='580' border='0' align='center' cellpadding='0' cellspacing='0'>";
                mail_cont = mail_cont + "          <tr>";
                mail_cont = mail_cont + "            <td align='right' style='color:#FF0000; font-weight:bold;' valign='top'>" + GetLocalResourceObject("(未滿");
                mail_cont = mail_cont + freight_range;
                mail_cont = mail_cont + GetLocalResourceObject("元，酌收運費");
                mail_cont = mail_cont + FreightAmount;
                mail_cont = mail_cont + "			元)</td>";
                mail_cont = mail_cont + "          </tr>";
                mail_cont = mail_cont + "        </table>";
                 * */
            }
            mail_cont = mail_cont + "      <br />";

            if (payment_type == "ATM")
            {
                mail_cont = mail_cont + "		<table width='600' border='0' align='center' cellpadding='0' cellspacing='0'>";
                mail_cont = mail_cont + "          <tr>";
                mail_cont = mail_cont + "            <td width='54'><img src='http://www.cocker.com.tw/images/icon_03.gif'></td>";
                mail_cont = mail_cont + "            <td width='546' style='color:#e30000; font-weight:bold; font-size:11pt;'>" + GetLocalResourceObject("繳費資訊") + "</td>";
                mail_cont = mail_cont + "          </tr>";
                mail_cont = mail_cont + "        </table>";
                mail_cont = mail_cont + "      <br />";
                mail_cont = mail_cont + "        <table width='580' border='0' align='center' cellpadding='3' cellspacing='3' style='border-bottom:#CCCCCC 1px solid; border-left:#CCCCCC 1px solid;border-right:#CCCCCC 1px solid;border-top:#CCCCCC 1px solid;'>";
                mail_cont = mail_cont + "          <tr style='background:#f7f7f7;'>";
                mail_cont = mail_cont + "            <td width='120' align='center' style='color:0d087f;'>" + GetLocalResourceObject("轉帳銀行代號") + "</td>";
                mail_cont = mail_cont + "            <td width='437'>";
                mail_cont = mail_cont + remit_money1;
                mail_cont = mail_cont + "</td>";
                mail_cont = mail_cont + "          </tr>";
                mail_cont = mail_cont + "          <tr style='background:#f4f2f7;'>";
                mail_cont = mail_cont + "            <td align='center' style='color:0d087f;'>" + GetLocalResourceObject("轉帳帳號") + "</td>";
                mail_cont = mail_cont + "            <td>";
                mail_cont = mail_cont + remit_money2;
                mail_cont = mail_cont + "			</td>";
                mail_cont = mail_cont + "          </tr>";
                mail_cont = mail_cont + "          <tr style='background:#f7f7f7;'>";
                mail_cont = mail_cont + "            <td align='center' style='color:0d087f;'>" + GetLocalResourceObject("戶名") + "</td>";
                mail_cont = mail_cont + "            <td>";
                mail_cont = mail_cont + remit_money3;
                mail_cont = mail_cont + "			</td>";
                mail_cont = mail_cont + "          </tr>";
                mail_cont = mail_cont + "          <tr style='background:#f4f2f7;'>";
                mail_cont = mail_cont + "            <td align='center' style='color:0d087f;'>" + GetLocalResourceObject("應繳金額") + "</td>";
                mail_cont = mail_cont + "            <td>新台幣&nbsp;&nbsp;&nbsp;NT$";
                mail_cont = mail_cont + (totalamt + Convert.ToInt32(FreightAmount) - Convert.ToInt32(BonusDiscount));
                mail_cont = mail_cont + "			</td>";
                mail_cont = mail_cont + "          </tr>";
                mail_cont = mail_cont + "          <tr style='background:#f7f7f7;'>";
                mail_cont = mail_cont + "            <td align='center' style='color:0d087f;'>" + GetLocalResourceObject("繳費期限") + "</td>";
                mail_cont = mail_cont + "            <td>";
                mail_cont = mail_cont + DateTime.Today.AddDays(1).ToString("yyyy/MM/dd") + " 23:59</td>";
                mail_cont = mail_cont + "          </tr>";
                mail_cont = mail_cont + "      </table><br>";
            }


            /*mail_cont = mail_cont + "        <center>‧" + GetLocalResourceObject("隨後我們也會將轉帳的資料mail一封到您指定的電子信箱") + "：<span style='color:#FF0000; font-weight:bold;'>";
            mail_cont = mail_cont + OEmail;
            mail_cont = mail_cont + "			</span></center>";
             * */
            mail_cont = mail_cont + "       </td>";
            mail_cont = mail_cont + "  </tr>";
            mail_cont = mail_cont + "  <tr>";
            mail_cont = mail_cont + "    <td><img src='http://www.cocker.com.tw/images/bg_06.jpg'></td>";
            mail_cont = mail_cont + "  </tr>";
            mail_cont = mail_cont + "</table>";

        }
    }
}