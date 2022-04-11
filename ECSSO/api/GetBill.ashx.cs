using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using ECSSO.Library;
using System.Net;
using System.IO;
using System.Text;

namespace ECSSO.api
{
    /// <summary>
    /// GetBill 的摘要描述
    /// </summary>
    public class GetBill : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Params["Type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Type必填", ""));
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Params["VerCode"] == null) ResponseWriteEnd(context, ErrorMsg("error", "VerCode必填", ""));

            if (context.Request.Params["Type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Type必填", ""));
            if (context.Request.Params["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Params["VerCode"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "VerCode必填", ""));

            String ChkM = context.Request.Params["CheckM"].ToString();
            String VerCode = context.Request.Params["VerCode"].ToString();
            String Type = context.Request.Params["Type"].ToString();

            GetStr GS = new GetStr();
            if (GS.MD5Check(Type + VerCode, ChkM))
            {
                String Orgname = GetOrgName("{" + VerCode + "}");

                if (Orgname == "") ResponseWriteEnd(context, ErrorMsg("error", "查無Orgname", ""));

                String Setting = GetSetting(Orgname);

                if (context.Request.Params["Items"] == null || context.Request.Params["Items"] == "") ResponseWriteEnd(context, ErrorMsg("error", "Items必填", Setting));

                Bill.Items bill = JsonConvert.DeserializeObject<Bill.Items>(context.Request.Params["Items"]); ;
                                
                switch (Type)
                {
                    case "1":   //取得訂單資料(桌號未結帳)
                        if (bill.ShopID == null || bill.ShopID == "") ResponseWriteEnd(context, ErrorMsg("error", "ShopID必填", Setting));
                        if (bill.TableID == null || bill.TableID == "") ResponseWriteEnd(context, ErrorMsg("error", "TableID必填", Setting));

                        ResponseWriteEnd(context, GetBillData(bill.ShopID, bill.TableID, Setting, ""));
                        break;
                    case "2":   //修改訂單狀態=已付款
                        if (bill.ShopID == null || bill.ShopID == "") ResponseWriteEnd(context, ErrorMsg("error", "ShopID必填", Setting));
                        if (bill.TableID == null || bill.TableID == "") ResponseWriteEnd(context, ErrorMsg("error", "TableID必填", Setting));

                        UpdateOrderStat(bill.ShopID, bill.TableID, Setting);
                        ResponseWriteEnd(context, "success");
                        break;
                    case "3":   //修改訂單
                        
                        if (bill.OrderID == null || bill.OrderID == "") ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填", Setting));
                        if (bill.SerNo == null) ResponseWriteEnd(context, ErrorMsg("error", "SerNo必填", Setting));

                        CancelOrderDetail(Setting, bill.OrderID, bill.SerNo);
                        ResponseWriteEnd(context, "success");

                        break;
                    case "4":   //出餐確認

                        if (bill.OrderID == null || bill.OrderID == "") ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填", Setting));
                        if (bill.SerNo == null) ResponseWriteEnd(context, ErrorMsg("error", "SerNo必填", Setting));

                        CheckMeal(Setting, bill.OrderID, bill.SerNo);
                        ResponseWriteEnd(context, "success");

                        break;
                    case "5":   //取得訂單資料(訂單號碼查詢)
                        if (bill.OrderID == null || bill.OrderID == "") ResponseWriteEnd(context, ErrorMsg("error", "OrderID必填", Setting));
                        ResponseWriteEnd(context, GetBillData("", "", Setting, bill.OrderID));
                        break;
                }
            }
            else
            {
                ResponseWriteEnd(context, ErrorMsg("error", "檢查碼錯誤", ""));
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg, String Setting)
        {
            if (Setting != "")
            {
                InsertLog(Setting, "booking error", "", RspnMsg);
            }

            Library.booking.RootObject root = new Library.booking.RootObject();
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

        private void CancelOrderDetail(String Setting,String OrderID,List<String> SerNo) 
        {
            int statcount = 0;
            for (int i = 0; i < SerNo.Count; i++) 
            { 
                //更新orders
                //update orders set stat='Y' where order_no=@order_no and ser_no=@ser_no



                //計算orders已註銷的金額
                //$$ = amt-discount
            }
            //更新orders_hd.amt
            //update orders_hd set amt=amt-$$ where id=@order_no
        }

        #region Get IP
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

        #region insert log
        private void InsertLog(String Setting, String JobName, String JobTitle, String Detail)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_userlogAdd";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@id", "guest"));
                cmd.Parameters.Add(new SqlParameter("@prog_name", "候位前台"));
                cmd.Parameters.Add(new SqlParameter("@job_name", JobName));
                cmd.Parameters.Add(new SqlParameter("@title", JobTitle));
                cmd.Parameters.Add(new SqlParameter("@table_id", ""));
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@ip", GetIPAddress()));
                cmd.Parameters.Add(new SqlParameter("@filename", " booking.ashx"));

                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region 由Vercode取得Orgname
        private String GetOrgName(String VerCode)
        {
            String OrgName = "";
            String Str_Sql = "select orgname from Device where stat='Y' and getdate() between start_date and end_date and VerCode=@VerCode";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand(Str_Sql, conn);
                cmd.Parameters.Add(new SqlParameter("@VerCode", VerCode));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            OrgName = reader[0].ToString();
                        }
                    }
                }
                finally { reader.Close(); }
            }
            return OrgName;
        }
        #endregion

        #region 取得Orgname連結字串
        private String GetSetting(String OrgName)
        {
            return "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
        }
        #endregion   

        #region 取得訂單資料
        private String GetBillData(String ShopID, String TableID, String Setting, String OrderID)
        {
            Bill.RootObject root = new Bill.RootObject();
            Bill.Bills bill = new Bill.Bills();
            Bill.Item item = new Bill.Item();
            Bill.Detail detail = new Bill.Detail();

            List<Bill.Bills> BillList = new List<Bill.Bills>();
            List<Bill.Item> ItemList = new List<Bill.Item>();
            List<Bill.Detail> DetailList = new List<Bill.Detail>();

            GetStr GS = new GetStr();
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                if (OrderID == "")
                {
                    cmd = new SqlCommand("select id,TakeMealType,amt from orders_hd where shopid=@shopid and tableid=@tableid and state='1'", conn);
                    cmd.Parameters.Add(new SqlParameter("@shopid", ShopID));
                    cmd.Parameters.Add(new SqlParameter("@tableid", TableID));
                }
                else {
                    cmd = new SqlCommand("select id,TakeMealType,amt from orders_hd where id=@orderid", conn);
                    cmd.Parameters.Add(new SqlParameter("@orderid", OrderID));        
                }
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            bill = new Bill.Bills
                            {
                                Amt = reader["amt"].ToString(),
                                Items = GetBillItem(Setting, reader["id"].ToString(), ""),
                                CancelItems = GetBillItem(Setting, reader["id"].ToString(), "Y"),
                                OrderID = reader["id"].ToString(),
                                TakeMealType = GS.GetMealType(reader["TakeMealType"].ToString())
                            };
                            BillList.Add(bill);
                            ItemList = new List<Bill.Item>();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            root.Bills = BillList;
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 取得訂單內容
        private List<Bill.Item> GetBillItem(String Setting, String OrderID, String Stat)
        {

            Bill.Item item = new Bill.Item();
            Bill.Detail detail = new Bill.Detail();

            List<Bill.Item> ItemList = new List<Bill.Item>();
            List<Bill.Detail> DetailList = new List<Bill.Detail>();

            String SerNo = "";
            #region 取得此訂單點菜數量

            using (SqlConnection conn2 = new SqlConnection(Setting))
            {
                conn2.Open();
                SqlCommand cmd2;

                cmd2 = new SqlCommand("select max(ser_no) from orders where order_no=@order_no and stat=@stat", conn2);
                cmd2.Parameters.Add(new SqlParameter("@order_no", OrderID));
                cmd2.Parameters.Add(new SqlParameter("@stat", Stat));

                SqlDataReader reader2 = cmd2.ExecuteReader();
                try
                {
                    if (reader2.HasRows)
                    {
                        while (reader2.Read())
                        {
                            SerNo = reader2[0].ToString();
                        }
                    }
                }
                finally
                {
                    reader2.Close();
                }
            }
            #endregion

            #region 點餐細項
            using (SqlConnection conn2 = new SqlConnection(Setting))
            {
                conn2.Open();
                SqlCommand cmd2;

                cmd2 = new SqlCommand("select b.sub_id,a.prod_name,a.qty,(a.qty*a.price)-a.discount as amt,a.discription,a.memo,a.ser_no from orders as a left join prod as b on a.productid=b.id where order_no=@order_no and a.stat=@stat order by a.ser_no", conn2);
                cmd2.Parameters.Add(new SqlParameter("@order_no", OrderID));
                cmd2.Parameters.Add(new SqlParameter("@stat", Stat));
                SqlDataReader reader2 = cmd2.ExecuteReader();
                try
                {
                    if (reader2.HasRows)
                    {

                        String MealSetName = "";
                        String MealQty = "";
                        String MealAmt = "";
                        String MealSerNo = "";

                        while (reader2.Read())
                        {
                            if (reader2["sub_id"].ToString() == "999999999")
                            {
                                MealSetName = reader2["prod_name"].ToString();
                                MealQty = reader2["qty"].ToString();
                                MealAmt = reader2["amt"].ToString();
                                MealSerNo = reader2["ser_no"].ToString();
                            }
                            else
                            {


                                if (reader2["memo"].ToString() != MealSetName && MealSetName != "")
                                {
                                    if (MealQty != "")
                                    {
                                        item = new Bill.Item
                                        {
                                            Detail = DetailList,
                                            Amt = MealAmt,
                                            title = MealSetName,
                                            Qty = MealQty,
                                            SerNo = MealSerNo
                                        };
                                        ItemList.Add(item);
                                        DetailList = new List<Bill.Detail>();

                                        MealSetName = "";
                                        MealAmt = "";
                                        MealQty = "";
                                        MealSerNo = "";
                                    }
                                    else
                                    {
                                        item = new Bill.Item
                                        {
                                            Detail = null,
                                            Amt = reader2["amt"].ToString(),
                                            title = reader2["prod_name"].ToString(),
                                            Qty = reader2["qty"].ToString(),
                                            SerNo = reader2["ser_no"].ToString()
                                        };
                                        ItemList.Add(item);
                                    }
                                }

                                if (reader2["memo"].ToString() == MealSetName && MealSetName != "")
                                {
                                    detail = new Bill.Detail
                                    {
                                        Amt = reader2["amt"].ToString(),
                                        Discription = reader2["discription"].ToString(),
                                        Qty = reader2["qty"].ToString(),
                                        Title = reader2["prod_name"].ToString(),
                                        SerNo = reader2["ser_no"].ToString()
                                    };
                                    DetailList.Add(detail);

                                    if (reader2["ser_no"].ToString() == SerNo)
                                    {
                                        item = new Bill.Item
                                        {
                                            Detail = DetailList,
                                            Amt = MealAmt,
                                            title = MealSetName,
                                            Qty = MealQty,
                                            SerNo = MealSerNo
                                        };
                                        ItemList.Add(item);
                                        DetailList = new List<Bill.Detail>();
                                    }
                                }

                                if (reader2["memo"].ToString() == MealSetName && MealSetName == "")
                                {
                                    item = new Bill.Item
                                    {
                                        Detail = DetailList,
                                        Amt = reader2["amt"].ToString(),
                                        title = reader2["prod_name"].ToString(),
                                        Qty = reader2["qty"].ToString(),
                                        SerNo = reader2["ser_no"].ToString()
                                    };
                                    ItemList.Add(item);
                                    DetailList = new List<Bill.Detail>();
                                }
                            }
                        }
                    }
                }
                finally
                {
                    reader2.Close();
                }
            }
            #endregion

            return ItemList;

        }
        #endregion
        
        #region 修改訂單狀態
        private void UpdateOrderStat(String ShopID, String TableID, String Setting) 
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("update orders_hd set state='2',edate=getdate() where shopid=@shopid and tableid=@tableid and state='1'", conn);
                cmd.Parameters.Add(new SqlParameter("@shopid", ShopID));
                cmd.Parameters.Add(new SqlParameter("@tableid", TableID));
                cmd.ExecuteNonQuery();
            }
            
        }
        #endregion

        #region 出餐確認
        private void CheckMeal(String Setting, String OrderID, List<String> SerNo)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                for (int i = 0; i < SerNo.Count; i++)
                {
                    cmd = new SqlCommand("update orders set state='R',edate=getdate() where order_no=@order_no and ser_no=@ser_no and state<>'Y'", conn);
                    cmd.Parameters.Add(new SqlParameter("@order_no", OrderID));
                    cmd.Parameters.Add(new SqlParameter("@ser_no", SerNo[i].ToString()));
                    cmd.ExecuteNonQuery();
                }
                
            }

        }
        #endregion

        #region 儲存訂單
        private bool SaveOrder(String setting, String OrderID, Bill.RootObject2 rlib)
        {
            bool SendMail = true;
            GetStr GS = new GetStr();
            #region 訂單變數
            String BonusAmt = rlib.OrderData.BonusAmt;
            String BonusDiscount = rlib.OrderData.BonusDiscount;
            String FreightAmount = rlib.OrderData.FreightAmount;
            String MemID = rlib.OrderData.MemID;
            String PayType = rlib.OrderData.PayType;
            String ReturnUrl = rlib.OrderData.ReturnUrl;
            String RID = rlib.OrderData.RID;

            Int32 DiscountAmt = 0;  //Convert.ToInt32(GS.ReplaceStr(this.discount_amt.Value));
            String Name = GS.ReplaceStr(rlib.OrderData.Name);
            String Tel = GS.ReplaceStr(rlib.OrderData.Tel);            
            String Sex = GS.ReplaceStr(rlib.OrderData.Sex);
            String Email = GS.ReplaceStr(rlib.OrderData.Mail);
            String City = GS.ReplaceStr(rlib.OrderData.City);
            String Country = GS.ReplaceStr(rlib.OrderData.Country);
            String Zip =GS.ReplaceStr(rlib.OrderData.Zip);
            String Address = City + Country + GS.ReplaceStr(rlib.OrderData.Address);
            String Notememo = GS.ReplaceStr(rlib.OrderData.Memo);
            #endregion

            SqlCommand cmd;

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
                cmd.Parameters.Add(new SqlParameter("@name", Name));
                cmd.Parameters.Add(new SqlParameter("@sex", Convert.ToInt32(Sex)));
                cmd.Parameters.Add(new SqlParameter("@tel", Tel));
                cmd.Parameters.Add(new SqlParameter("@cell", Tel));
                cmd.Parameters.Add(new SqlParameter("@addr", Address));
                cmd.Parameters.Add(new SqlParameter("@mail", Email));
                cmd.Parameters.Add(new SqlParameter("@notememo", Notememo));
                cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                cmd.Parameters.Add(new SqlParameter("@item2", ""));
                cmd.Parameters.Add(new SqlParameter("@item3", ""));
                cmd.Parameters.Add(new SqlParameter("@item4", ""));
                cmd.Parameters.Add(new SqlParameter("@payment_type", PayType));
                cmd.Parameters.Add(new SqlParameter("@o_name", Name));
                cmd.Parameters.Add(new SqlParameter("@o_tel", Tel));
                cmd.Parameters.Add(new SqlParameter("@o_cell", Tel));
                cmd.Parameters.Add(new SqlParameter("@o_addr", ""));
                cmd.Parameters.Add(new SqlParameter("@bonus_amt", BonusAmt));
                cmd.Parameters.Add(new SqlParameter("@bonus_discount", BonusDiscount));
                cmd.Parameters.Add(new SqlParameter("@freightamount", FreightAmount));
                cmd.Parameters.Add(new SqlParameter("@c_no", ""));
                cmd.Parameters.Add(new SqlParameter("@ship_city", City));
                cmd.Parameters.Add(new SqlParameter("@ship_zip", Zip));
                cmd.Parameters.Add(new SqlParameter("@ship_countryname", Country));
                cmd.Parameters.Add(new SqlParameter("@discount_amt", DiscountAmt));
                cmd.Parameters.Add(new SqlParameter("@RID", RID));
                cmd.Parameters.Add(new SqlParameter("@prod_bonus", ""));
                cmd.ExecuteNonQuery();
            }

            #endregion

            int i = 1;
            int order_totalamt = 0;
            String Memo = "";

            
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

            foreach (Bill.Menu Menu in rlib.OrderData.MenuLists.Menu)
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
                        cmd.Parameters.Add(new SqlParameter("@amt", Convert.ToInt32(Menu.Qty) * Convert.ToInt32(GetProdPrice(Menu.ID, "", setting, "", ""))));
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

                        order_totalamt += Convert.ToInt32(Menu.Qty) * Convert.ToInt32(GetProdPrice(Menu.ID, "", setting, "", "")) - Convert.ToInt32(Menu.Discount);
                        i = i + 1;
                    }

                    #endregion
                    Memo = Menu.Name;
                }

                foreach (Bill.MenuItem MenuItem in Menu.MenuItems)
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

                    foreach (Bill.MenuSpec MenuSpec in MenuItem.MenuSpec)
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


            return SendMail;
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
    }
}