using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Meal.Library
{
    public class Bill
    {
        public class OrderDate 
        {
            public string Date { get; set; }
            public string num { get; set; }
        }

        public class DateObject 
        {
            public List<OrderDate> OrderDate { get; set; }
        }

        public class Cash 
        {
            public string Income { get; set; }
            public string Change { get; set; }
            public string Redeem { get; set; }
            public string type { get; set; }
        }

        public class EasyCard
        {
            public string ShopName { get; set; }        //特店名稱
            public string ShopName2 { get; set; }       //分店名稱
            public string ShopID { get; set; }          //特店代號
            public string MachineID { get; set; }       //設備編號
            public string Type { get; set; }            //交易類別
            public string CardID { get; set; }          //悠遊卡晶片號
            public string BeforeAmt { get; set; }       //交易前餘額
            public string AddAmt { get; set; }          //加值金額
            public string PayAmt { get; set; }          //交易金額
            public string AfterAmt { get; set; }        //交易後餘額
            public string TransTime { get; set; }       //交易時間
        }

        public class Items
        {
            public string TableID { get; set; }
            public string MemberID { get; set; }
            public string ShopID { get; set; }
            public string BookingID { get; set; }
            public string OrderID { get; set; }
            public string CancelChk { get; set; }
            public string Stat { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string num { get; set; }
            public string[] Track { get; set; }
            public string[] SerNo { get; set; }
            public List<Cash> Cash { get; set; }
            public List<EasyCard> EasyCard { get; set; }
            public OrderData OrderData { get; set; }
            public string TakeMealType { get; set; }
        }

        public class CancelItem 
        {
            public string OrderID { get; set; }
            public string Title { get; set; }
            public string SerNo { get; set; }
            public string Amt { get; set; }
            public string Ready { get; set; }
        }

        public class MealReady
        {
            public string OrderID { get; set; }
            public string Title { get; set; }
            public string SerNo { get; set; }
            public string Track { get; set; }
        }

        public class Detail
        {
            public string Title { get; set; }
            public string SerNo { get; set; }
            public string Amt { get; set; }
            public string Qty { get; set; }
            public string Ready { get; set; }
            public string Discription { get; set; }
            public string Printer { get; set; }
            public string GS1Code { get; set; }
        }

        public class Item
        {
            public string title { get; set; }
            public string Img { get; set; }
            public string SerNo { get; set; }
            public string Amt { get; set; }
            public string Qty { get; set; }
            public string Ready { get; set; }
            public string Discription { get; set; }
            public string Printer { get; set; }
            public string GS1Code { get; set; }
            public List<Detail> Detail { get; set; }
        }

        public class Bills
        {
            public string OrderID { get; set; }
            public string TakeMealType { get; set; }
            public string Amt { get; set; }
            public string PringTime { get; set; }
            public string State { get; set; }
            public string TableID { get; set; }
            public List<Item> Items { get; set; }
            public List<Item> CancelItems { get; set; }
            public List<Cash> Cash { get; set; }
            public List<EasyCard> EasyCard { get; set; }
        }

        public class RootObject
        {
            public List<Bills> Bills { get; set; }
        }

        
        public class OrderData		//同舊版說明，沒更新
        {
            public String Name;
            public String Tel;
            public String Mail;
            public String Sex;
            public String Memo;
            public String City;                                             //高雄市
            public String Country;                                          //三民區
            public String Address;                                          //鼎昌街258號
            public String Zip;                                              //807

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

            public String FreightAmount;                                    //運費
            public String BonusDiscount;                                    //紅利折扣
            public String BonusAmt;                                         //本次訂單獲得紅利
            public String MemID;                                            //會員編號
            public String QuickPay;                                         //快速付款  Y:快速付款,N:普通付款
            public String RID;                                              //美安訂購註記
            public String Checkm;                                           //驗證json字串(MD5):OrgName + PayType + FreightAmount + BonusDiscount + BonusAmt + {OrderItem.Name1 + OrderSpec.FinalPrice1 + OrderItem.Name2 + OrderSpec.FinalPrice2 + .... + OrderItem.NameN + OrderSpec.FinalPriceN} + {AdditionalItem.Name1 + AdditionalItem.FinalPrice1 + AdditionalItem.Name2 + AdditionalItem.FinalPrice2 + .... + AdditionalItem.NameN + AdditionalItem.FinalPriceN}
            public String ShopType;                                         //1:普通購物,2:點燈系統,3.點餐系統            
            public MenuLists MenuLists { get; set; }              //點餐系統架構
        }

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

        public class ErrorObject
        {
            public string RspnCode { get; set; }
            public string RspnMsg { get; set; }
            public string Pno { get; set; }
        }
    }
}