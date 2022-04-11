using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO
{
    public class Shoppingcar
    {
        #region 購物車架構
        public class RootObject
        {
            public OrderData OrderData { get; set; }
        }
        public class OrderData		//同舊版說明，沒更新
        {
            public String WebTitle;                                         //網站名稱
            public String OrgName;                                          //Orgname
            public String ReturnUrl;                                        //結帳後回到原站網址
            public String ErrorUrl;                                         //訂購失敗原站網址
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
            public String LogisticApi;                                      //物流介接註記
            public String LogisticstypeID;                                  //物流方式
            public String LogisticsID;                                      //結帳的運費
            public int FreightAmount;                                    //運費
            public String Temperature;                                      //溫層
            public String deliveryDate;                                     //出貨日選擇
            public String BonusDiscount;                                    //紅利折扣
            public int BonusAmt;                                         //本次訂單獲得紅利
            public String MemID;                                            //會員編號
            public String QuickPay;                                         //快速付款  Y:快速付款,N:普通付款
            public String RID;                                              //美安訂購註記
            public String Click_ID;                                         //美安、聯盟網訂購註記
            public String affi_id;                                          //聯盟網訂購註記
            public String Checkm;                                           //驗證json字串(MD5):OrgName + PayType + FreightAmount + BonusDiscount + BonusAmt + {OrderItem.Name1 + OrderSpec.FinalPrice1 + OrderItem.Name2 + OrderSpec.FinalPrice2 + .... + OrderItem.NameN + OrderSpec.FinalPriceN} + {AdditionalItem.Name1 + AdditionalItem.FinalPrice1 + AdditionalItem.Name2 + AdditionalItem.FinalPrice2 + .... + AdditionalItem.NameN + AdditionalItem.FinalPriceN}
            public int ShopType;                                            //1:普通購物,2:點燈系統,3.點餐系統,4.詢價
            public String VCode;                                            //優惠券代碼
            public String GCode;                                            //優惠券驗證碼
            public int CouponID;                                            //優惠券ID
            public int couDiscont;                                          //優惠券折扣金額
            public string couponTitle;                                      //優惠券標題
            public bool doInsertCoupon;                                     //插入優惠券
            public int servicePrice;                                        //服務費
            public int servicePriceSum;                                     //服務費總計
            public string servicePriceType;                                 //服務費計算方式
            public bool allVirtualProd;                                     //是否全部都是虛擬商品
            public string version;                                             //購物車版本
            public List<OrderList> OrderLists { get; set; }                 //普通購物系統架構
            public MenuLists MenuLists { get; set; }                        //點餐系統架構
        }

        #region 普通購物系統架構
        public class OrderList		//行銷活動
        {
            public string ID { get; set; }		            //prod_list.id (有沒有行銷都給)
            public int Type { get; set; }		        //prod_list.sales_type(行銷活動種類)
            public int saleQty { get; set; }                //prod_list.sales_qty(行銷活動件數限制)
            public double salePrice { get; set; }           //prod_list.sales_value(行銷活動銷售金額)
            public string Title { get; set; }		        //prod_list.title行銷活動標題(有沒有行銷都給)
            //public string Discount { get; set; }
            public List<OrderItem> OrderItems { get; set; }
        }
        public class OrderItem		//購買產品
        {
            public string ID { get; set; }		    // ID            
            public string Name { get; set; }		//名稱            
            public string PosNo { get; set; }		//POS代碼
            public String UseTime { get; set; }     //有效時間
            public String UseDate { get; set; }     //使用期限
            public String Virtual { get; set; }     //是否為虛體商品 N:實體商品,Y:虛擬商品
            public string isDel { get; set; }         //是否已下架、刪除

            public string URL { get; set; }         //商品連結
            public string Img { get; set; }         //商品圖片
            public List<OrderSpec> OrderSpecs { get; set; }      //購買規格數量
            public List<AdditionalItem> AdditionalItems { get; set; }   //加價購
        }
        public class OrderSpec      //購買產品規格及數量
        {
            public int Size { get; set; }		//尺寸
            public int Color { get; set; }		//顏色
            public string ColorTitle { get; set; }  //規格一名稱
            public string SizeTitle { get; set; }   //規格二名稱
            public int Qty { get; set; }		    //數量
            public double Price { get; set; }		//原始價格
            public double FinalPrice { get; set; }	//結帳價格            
            public int Discount { get; set; }    //本商品總折扣
            public int Bonus { get; set; }       //商品總紅利折抵
            public int PriceType { get; set; }      //商品選擇金額
        }
        public class AdditionalItem		//加價購產品
        {
            public int ID { get; set; }		    // ID
            public int Size { get; set; }		//尺寸
            public int Color { get; set; }		//顏色
            public string ColorTitle { get; set; }  //規格一名稱
            public string SizeTitle { get; set; }   //規格二名稱
            public string Name { get; set; }		//名稱
            public int Qty { get; set; }		    //數量
            public double Price { get; set; }		//原始價格
            public double FinalPrice { get; set; }	//結帳價格
            public string PosNo { get; set; }		//POS代碼
            public string Discount { get; set; }    //本商品總折扣
        }
        #endregion
        
        #endregion

        #region 點燈系統架構
        public class LightRootObject
        {
            public List<LightItem> Items { get; set; }
            public List<ErrorMsg> Errormsg { get; set; }
        }
        public class LightItem
        {
            public string prodid { get; set; }
            public List<LightData> data { get; set; }
        }
        public class LightData
        {
            public string name { get; set; }
            public string tel { get; set; }
            public string addr { get; set; }
            public string birth { get; set; }
            public string hour { get; set; }
            public string cellphone { get; set; }
            public string animal { get; set; }
            public string lunarbirth { get; set; }

        }
        public class ErrorMsg
        {
            public string Code { get; set; }
            public string Msg { get; set; }
        }
        #endregion

        #region 點餐系統架構
        public class MenuSpec
        {
            public List<string> OtherID { get; set; }
            public List<string> Memo { get; set; }
            public string Qty { get; set; }
        }

        public class MenuItem
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public List<MenuSpec> MenuSpec { get; set; }
        }

        public class Menu
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Qty { get; set; }
            public string Discount { get; set; }
            public List<MenuItem> MenuItems { get; set; }
        }

        public class MenuLists
        {
            public string Vercode { get; set; }
            public string ShopName { get; set; }
            public string ShopID { get; set; }
            public string TableID { get; set; }
            public string TakeMealType { get; set; }
            public List<Menu> Menu { get; set; }
        }
        #endregion
    }
}