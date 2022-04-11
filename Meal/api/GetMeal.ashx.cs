using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Meal.Library;

namespace Meal.api
{
    /// <summary>
    /// GetMeal 的摘要描述
    /// </summary>
    public class GetMeal : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            #region 檢查post值
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Params["VerCode"] == null) ResponseWriteEnd(context, ErrorMsg("error", "VerCode必填"));
            if (context.Request.Params["type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "type必填"));

            if (context.Request.Params["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Params["VerCode"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "VerCode必填"));
            if (context.Request.Params["type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "type必填"));
            #endregion

            String type = context.Request.Params["type"].ToString();
            String ChkM = context.Request.Params["CheckM"].ToString();
            String VerCode = context.Request.Params["VerCode"].ToString();
            GetMealStr GS = new GetMealStr();

            if (GS.MD5Check(VerCode, ChkM))
            {
                VerCode = "{" + VerCode + "}";
                String Orgname = GetOrgName(VerCode);

                if (Orgname == "") ResponseWriteEnd(context, ErrorMsg("error", "查無Orgname"));

                String Setting = GetSetting(Orgname);

                switch (type)
                {
                    case "1":   //取得菜單資料
                        String SubID = "";
                        if (context.Request.Params["Items"] != null && context.Request.Params["Items"] != "")
                        {
                            Library.Meal.FormData postf = JsonConvert.DeserializeObject<Library.Meal.FormData>(context.Request.Params["Items"]);
                            SubID = postf.SubID;
                        }
                        ResponseWriteEnd(context, GetAllMeal(Setting, VerCode, SubID));
                        break;

                    case "2":   //取得套餐大類
                        ResponseWriteEnd(context, GetMenuList(Setting));
                        break;
                }
            }
            else
            {
                ResponseWriteEnd(context, ErrorMsg("error", "檢查碼錯誤"));
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
        private String ErrorMsg(String RspnCode, String RspnMsg)
        {
            Library.Meal.VoidReturn root = new Library.Meal.VoidReturn();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 由Vercode取得Orgname
        private String GetOrgName(String VerCode)
        {
            String OrgName = "";
            String Str_Sql = "select orgname from Device where stat='Y' and getdate() between start_date and end_date and VerCode=@VerCode";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB2"].ToString()))
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

        #region 取得所有菜單(套餐+單點)
        private String GetAllMeal(String Setting, String VerCode, String SubId)
        {
            Library.Meal.Items root = new Library.Meal.Items();
            Library.Meal.Item Item = new Library.Meal.Item();
            Library.Meal.Menu Menu = new Library.Meal.Menu();
            Library.Meal.Item2 Item2 = new Library.Meal.Item2();
            Library.Meal.Memo Memo = new Library.Meal.Memo();
            Library.Meal.Other Other = new Library.Meal.Other();
            Library.Meal.Single Single = new Library.Meal.Single();
            Library.Meal.SingleMenu SingleMenu = new Library.Meal.SingleMenu();

            List<Library.Meal.Other> OtherList = new List<Library.Meal.Other>();
            List<Library.Meal.Memo> MemoList = new List<Library.Meal.Memo>();
            List<Library.Meal.Item2> Item2List = new List<Library.Meal.Item2>();
            List<Library.Meal.Menu> MenuList = new List<Library.Meal.Menu>();
            List<Library.Meal.Item> ItemList = new List<Library.Meal.Item>();
            List<Library.Meal.Single> SingleList = new List<Library.Meal.Single>();
            List<Library.Meal.SingleMenu> SingleMenuList = new List<Library.Meal.SingleMenu>();

            #region 取得套餐名稱
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                if (SubId != "")
                {
                    cmd = new SqlCommand("select id,title,value1,value2,img,cont from Meal where disp_opt='Y' and (VerCode=@VerCode or VerCode='') and id in (select id from meallist where sub_id=@sub_id) order by ser_no", conn);
                    cmd.Parameters.Add(new SqlParameter("@VerCode", VerCode));
                    cmd.Parameters.Add(new SqlParameter("@sub_id", SubId));
                }
                else
                {
                    cmd = new SqlCommand("select id,title,value1,value2,img,cont from Meal where disp_opt='Y' and (VerCode=@VerCode or VerCode='') order by ser_no", conn);
                    cmd.Parameters.Add(new SqlParameter("@VerCode", VerCode));
                }
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        //套餐
                        while (reader.Read())
                        {
                            //ItemList = new List<Meal.Item>();
                            #region 取得套餐選擇項目
                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2;

                                cmd2 = new SqlCommand("select id,title,ChoiceNum,Discount from dbo.Meal_Sub where fid=@fid order by ser_no", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@fid", reader["id"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            #region 取得菜名

                                            using (SqlConnection conn3 = new SqlConnection(Setting))
                                            {
                                                conn3.Open();
                                                SqlCommand cmd3;

                                                cmd3 = new SqlCommand("select b.title,a.id,a.pid,a.price,b.img1,b.item1 from dbo.Meal_Detail as a left join prod as b on a.pid=b.id where a.fid=@fid order by a.ser_no", conn3);
                                                cmd3.Parameters.Add(new SqlParameter("@fid", reader2["id"].ToString()));
                                                SqlDataReader reader3 = cmd3.ExecuteReader();
                                                try
                                                {
                                                    if (reader3.HasRows)
                                                    {
                                                        while (reader3.Read())
                                                        {
                                                            #region 取得可加價項目
                                                            using (SqlConnection conn4 = new SqlConnection(Setting))
                                                            {
                                                                conn4.Open();
                                                                SqlCommand cmd4;

                                                                cmd4 = new SqlCommand("select c.title as title,b.price,b.Optionid from prod as a left join Meal_Detail_Memo as b on a.id=b.pid left join Meal_Options as c on b.Optionid=c.id left join Meal_Options_Sub as d on c.fid=d.id where a.id=@pid and d.type='1' order by c.title", conn4);
                                                                cmd4.Parameters.Add(new SqlParameter("@pid", reader3["pid"].ToString()));
                                                                SqlDataReader reader4 = cmd4.ExecuteReader();
                                                                try
                                                                {
                                                                    if (reader4.HasRows)
                                                                    {
                                                                        while (reader4.Read())
                                                                        {
                                                                            Library.Meal.Other Otheritem = new Library.Meal.Other
                                                                            {
                                                                                id = reader4["Optionid"].ToString(),
                                                                                title = reader4["title"].ToString(),
                                                                                price = reader4["price"].ToString()
                                                                            };
                                                                            OtherList.Add(Otheritem);
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        OtherList = null;
                                                                    }
                                                                }
                                                                finally { reader4.Close(); }
                                                            }
                                                            #endregion

                                                            #region 取得非加價項目

                                                            using (SqlConnection conn4 = new SqlConnection(Setting))
                                                            {
                                                                conn4.Open();
                                                                SqlCommand cmd4;

                                                                cmd4 = new SqlCommand("select d.title as title1,c.title as title2 from prod as a left join Meal_Detail_Memo as b on a.id=b.pid left join Meal_Options as c on b.Optionid=c.id left join Meal_Options_Sub as d on c.fid=d.id where a.id=@pid and d.type='2' order by d.title", conn4);
                                                                cmd4.Parameters.Add(new SqlParameter("@pid", reader3["pid"].ToString()));
                                                                SqlDataReader reader4 = cmd4.ExecuteReader();
                                                                try
                                                                {
                                                                    if (reader4.HasRows)
                                                                    {
                                                                        String MemoTitle = "";
                                                                        List<String> MemoItemList = new List<String>();
                                                                        while (reader4.Read())
                                                                        {
                                                                            if (MemoTitle != reader4["title1"].ToString())
                                                                            {
                                                                                if (MemoTitle != "")
                                                                                {
                                                                                    Memo = new Library.Meal.Memo
                                                                                    {
                                                                                        title = MemoTitle,
                                                                                        item = MemoItemList
                                                                                    };
                                                                                    MemoList.Add(Memo);
                                                                                }

                                                                                MemoTitle = reader4["title1"].ToString();
                                                                                MemoItemList = new List<string>();

                                                                            }
                                                                            MemoItemList.Add(reader4["title2"].ToString());
                                                                        }
                                                                        Memo = new Library.Meal.Memo
                                                                        {
                                                                            title = MemoTitle,
                                                                            item = MemoItemList
                                                                        };
                                                                        MemoList.Add(Memo);
                                                                    }
                                                                    else
                                                                    {
                                                                        MemoList = null;
                                                                    }
                                                                }
                                                                finally { reader4.Close(); }
                                                            }
                                                            #endregion

                                                            Item2 = new Library.Meal.Item2
                                                            {
                                                                id = reader3["pid"].ToString(),
                                                                title = reader3["title"].ToString(),
                                                                price = reader3["price"].ToString(),
                                                                img = reader3["img1"].ToString(),
                                                                cont = reader3["item1"].ToString(),
                                                                Memo = MemoList,
                                                                other = OtherList
                                                            };
                                                            Item2List.Add(Item2);
                                                            MemoList = new List<Library.Meal.Memo>();
                                                            OtherList = new List<Library.Meal.Other>();
                                                        }
                                                    }
                                                }
                                                finally { reader3.Close(); }
                                            }
                                            #endregion

                                            Menu = new Library.Meal.Menu
                                            {
                                                Title = reader2["title"].ToString(),
                                                ChoiceNum = reader2["ChoiceNum"].ToString(),
                                                Discount = reader2["Discount"].ToString(),
                                                Item = Item2List
                                            };
                                            MenuList.Add(Menu);
                                            Item2List = new List<Library.Meal.Item2>();
                                        }
                                    }
                                }
                                finally { reader2.Close(); }
                            }
                            #endregion

                            Item = new Library.Meal.Item
                            {
                                id = reader["id"].ToString(),
                                title = reader["title"].ToString(),
                                value1 = reader["value1"].ToString(),
                                value2 = reader["value2"].ToString(),
                                img = reader["img"].ToString(),
                                cont = reader["cont"].ToString(),
                                Menus = MenuList
                            };
                            ItemList.Add(Item);
                            MenuList = new List<Library.Meal.Menu>();
                        }

                    }
                    else
                    {
                        //單點
                        using (SqlConnection conn2 = new SqlConnection(Setting))
                        {
                            conn2.Open();
                            SqlCommand cmd2;

                            cmd2 = new SqlCommand("select id,title,value1,value2,img1,item1,sub_id from prod where disp_opt='Y' and id in (select id from meallist where sub_id=@sub_id) order by ser_no", conn2);
                            cmd2.Parameters.Add(new SqlParameter("@sub_id", SubId));
                            SqlDataReader reader2 = cmd2.ExecuteReader();
                            try
                            {
                                if (reader2.HasRows)
                                {
                                    while (reader2.Read())
                                    {

                                        #region 取得可加價項目
                                        using (SqlConnection conn4 = new SqlConnection(Setting))
                                        {
                                            conn4.Open();
                                            SqlCommand cmd4;

                                            cmd4 = new SqlCommand("select c.title as title,b.price,b.Optionid from prod as a left join Meal_Detail_Memo as b on a.id=b.pid left join Meal_Options as c on b.Optionid=c.id left join Meal_Options_Sub as d on c.fid=d.id where a.id=@pid and d.type='1' order by c.title", conn4);
                                            cmd4.Parameters.Add(new SqlParameter("@pid", reader2["id"].ToString()));
                                            SqlDataReader reader4 = cmd4.ExecuteReader();
                                            try
                                            {
                                                if (reader4.HasRows)
                                                {
                                                    while (reader4.Read())
                                                    {
                                                        Library.Meal.Other Otheritem = new Library.Meal.Other
                                                        {
                                                            id = reader4["Optionid"].ToString(),
                                                            title = reader4["title"].ToString(),
                                                            price = reader4["price"].ToString()
                                                        };
                                                        OtherList.Add(Otheritem);
                                                    }
                                                }
                                                else
                                                {
                                                    OtherList = null;
                                                }
                                            }
                                            finally { reader4.Close(); }
                                        }
                                        #endregion

                                        #region 取得非加價項目

                                        using (SqlConnection conn4 = new SqlConnection(Setting))
                                        {
                                            conn4.Open();
                                            SqlCommand cmd4;

                                            cmd4 = new SqlCommand("select d.title as title1,c.title as title2 from prod as a left join Meal_Detail_Memo as b on a.id=b.pid left join Meal_Options as c on b.Optionid=c.id left join Meal_Options_Sub as d on c.fid=d.id where a.id=@pid and d.type='2' order by d.title", conn4);
                                            cmd4.Parameters.Add(new SqlParameter("@pid", reader2["id"].ToString()));
                                            SqlDataReader reader4 = cmd4.ExecuteReader();
                                            try
                                            {
                                                if (reader4.HasRows)
                                                {
                                                    String MemoTitle = "";
                                                    List<String> MemoItemList = new List<String>();
                                                    while (reader4.Read())
                                                    {
                                                        if (MemoTitle != reader4["title1"].ToString())
                                                        {
                                                            if (MemoTitle != "")
                                                            {
                                                                Memo = new Library.Meal.Memo
                                                                {
                                                                    title = MemoTitle,
                                                                    item = MemoItemList
                                                                };
                                                                MemoList.Add(Memo);
                                                            }

                                                            MemoTitle = reader4["title1"].ToString();
                                                            MemoItemList = new List<string>();

                                                        }
                                                        MemoItemList.Add(reader4["title2"].ToString());
                                                    }
                                                    Memo = new Library.Meal.Memo
                                                    {
                                                        title = MemoTitle,
                                                        item = MemoItemList
                                                    };
                                                    MemoList.Add(Memo);
                                                }
                                                else
                                                {
                                                    MemoList = null;
                                                }
                                            }
                                            finally { reader4.Close(); }
                                        }
                                        #endregion

                                        SingleMenu = new Library.Meal.SingleMenu
                                        {
                                            Subid = reader2["sub_id"].ToString(),
                                            id = reader2["id"].ToString(),
                                            title = reader2["title"].ToString(),
                                            value1 = reader2["value1"].ToString(),
                                            value2 = reader2["value2"].ToString(),
                                            img = reader2["img1"].ToString(),
                                            cont = reader2["item1"].ToString(),
                                            other = OtherList,
                                            Memo = MemoList
                                        };
                                        SingleMenuList.Add(SingleMenu);
                                        MemoList = new List<Library.Meal.Memo>();
                                        OtherList = new List<Library.Meal.Other>();
                                    }


                                    String SingleBannerImg = "";
                                    String SingleTitle = "";

                                    using (SqlConnection conn3 = new SqlConnection(Setting))
                                    {
                                        conn3.Open();
                                        SqlCommand cmd3;

                                        cmd3 = new SqlCommand("select title,banner_img from prod_list where id=@sub_id and disp_opt='Y' order by ser_no", conn3);
                                        cmd3.Parameters.Add(new SqlParameter("@sub_id", SubId));
                                        SqlDataReader reader3 = cmd3.ExecuteReader();
                                        try
                                        {
                                            if (reader3.HasRows)
                                            {
                                                while (reader3.Read())
                                                {
                                                    SingleBannerImg = reader3[1].ToString();
                                                    SingleTitle = reader3[0].ToString();
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            reader3.Close();
                                        }
                                    }

                                    Single = new Library.Meal.Single
                                    {
                                        title = SingleTitle,
                                        img = SingleBannerImg,
                                        SingleMenu = SingleMenuList
                                    };
                                    SingleList.Add(Single);
                                    SingleMenuList = new List<Library.Meal.SingleMenu>();

                                }
                            }
                            finally
                            {
                                reader2.Close();
                            }
                        }
                        

                        
                    }
                }
                finally { reader.Close(); }
            }
            #endregion

            if (SubId == "")
            {
                #region 取得單點分類(不搜尋套餐才出現)
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand("select a.id,a.title,a.banner_img from prod_list as a left join prod_authors as b on a.au_id=b.id where a.disp_opt='Y' and b.type='1' order by a.ser_no", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {

                                using (SqlConnection conn2 = new SqlConnection(Setting))
                                {
                                    conn2.Open();
                                    SqlCommand cmd2;

                                    cmd2 = new SqlCommand("select id,title,value1,value2,img1,item1,sub_id from prod where sub_id=@sub_id and disp_opt='Y' and CONVERT([nvarchar](10),getdate(),(120)) between start_date and end_date order by ser_no", conn2);
                                    cmd2.Parameters.Add(new SqlParameter("@sub_id", reader["id"].ToString()));
                                    SqlDataReader reader2 = cmd2.ExecuteReader();
                                    try
                                    {
                                        if (reader2.HasRows)
                                        {
                                            while (reader2.Read())
                                            {
                                                #region 取得可加價項目
                                                using (SqlConnection conn4 = new SqlConnection(Setting))
                                                {
                                                    conn4.Open();
                                                    SqlCommand cmd4;

                                                    cmd4 = new SqlCommand("select c.title as title,b.price,b.Optionid from prod as a left join Meal_Detail_Memo as b on a.id=b.pid left join Meal_Options as c on b.Optionid=c.id left join Meal_Options_Sub as d on c.fid=d.id where a.id=@pid and d.type='1' order by c.title", conn4);
                                                    cmd4.Parameters.Add(new SqlParameter("@pid", reader2["id"].ToString()));
                                                    SqlDataReader reader4 = cmd4.ExecuteReader();
                                                    try
                                                    {
                                                        if (reader4.HasRows)
                                                        {
                                                            while (reader4.Read())
                                                            {
                                                                Library.Meal.Other Otheritem = new Library.Meal.Other
                                                                {
                                                                    id = reader4["Optionid"].ToString(),
                                                                    title = reader4["title"].ToString(),
                                                                    price = reader4["price"].ToString()
                                                                };
                                                                OtherList.Add(Otheritem);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            OtherList = null;
                                                        }
                                                    }
                                                    finally { reader4.Close(); }
                                                }
                                                #endregion

                                                #region 取得非加價項目

                                                using (SqlConnection conn4 = new SqlConnection(Setting))
                                                {
                                                    conn4.Open();
                                                    SqlCommand cmd4;

                                                    cmd4 = new SqlCommand("select d.title as title1,c.title as title2 from prod as a left join Meal_Detail_Memo as b on a.id=b.pid left join Meal_Options as c on b.Optionid=c.id left join Meal_Options_Sub as d on c.fid=d.id where a.id=@pid and d.type='2' order by d.title", conn4);
                                                    cmd4.Parameters.Add(new SqlParameter("@pid", reader2["id"].ToString()));
                                                    SqlDataReader reader4 = cmd4.ExecuteReader();
                                                    try
                                                    {
                                                        if (reader4.HasRows)
                                                        {
                                                            String MemoTitle = "";
                                                            List<String> MemoItemList = new List<String>();
                                                            while (reader4.Read())
                                                            {
                                                                if (MemoTitle != reader4["title1"].ToString())
                                                                {
                                                                    if (MemoTitle != "")
                                                                    {
                                                                        Memo = new Library.Meal.Memo
                                                                        {
                                                                            title = MemoTitle,
                                                                            item = MemoItemList
                                                                        };
                                                                        MemoList.Add(Memo);
                                                                    }

                                                                    MemoTitle = reader4["title1"].ToString();
                                                                    MemoItemList = new List<string>();

                                                                }
                                                                MemoItemList.Add(reader4["title2"].ToString());
                                                            }
                                                            Memo = new Library.Meal.Memo
                                                            {
                                                                title = MemoTitle,
                                                                item = MemoItemList
                                                            };
                                                            MemoList.Add(Memo);
                                                        }
                                                        else
                                                        {
                                                            MemoList = null;
                                                        }
                                                    }
                                                    finally { reader4.Close(); }
                                                }
                                                #endregion

                                                SingleMenu = new Library.Meal.SingleMenu
                                                {
                                                    Subid = reader2["sub_id"].ToString(),
                                                    id = reader2["id"].ToString(),
                                                    title = reader2["title"].ToString(),
                                                    value1 = reader2["value1"].ToString(),
                                                    value2 = reader2["value2"].ToString(),
                                                    img = reader2["img1"].ToString(),
                                                    cont = reader2["item1"].ToString(),
                                                    other = OtherList,
                                                    Memo = MemoList
                                                };
                                                SingleMenuList.Add(SingleMenu);
                                                MemoList = new List<Library.Meal.Memo>();
                                                OtherList = new List<Library.Meal.Other>();
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        reader2.Close();
                                    }
                                }

                                Single = new Library.Meal.Single
                                {
                                    title = reader["title"].ToString(),
                                    img = reader["banner_img"].ToString(),
                                    SingleMenu = SingleMenuList
                                };
                                SingleList.Add(Single);
                                SingleMenuList = new List<Library.Meal.SingleMenu>();
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
                #endregion
            }

            root.combo = ItemList;
            root.Single = SingleList;

            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 取得套單大類
        private String GetMenuList(String Setting)
        {
            Library.Meal.MealList ML = new Library.Meal.MealList();
            List<Library.Meal.MealList> root = new List<Library.Meal.MealList>();
            String Type = "";

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select id,title,banner_img from prod_list where disp_opt='Y' and type='meal' order by ser_no", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            using (SqlConnection conn2 = new SqlConnection(Setting))
                            {
                                conn2.Open();
                                SqlCommand cmd2;

                                cmd2 = new SqlCommand("select distinct(MealType) from dbo.MealList where sub_id=@subID", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@subID", reader["id"].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            if (reader2[0].ToString() == "999999999")
                                            {
                                                Type = "M";
                                            }
                                            else 
                                            {
                                                Type = "S";
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    reader2.Close();
                                }
                            }
                            //select distinct(MealType) from dbo.MealList where sub_id='133'

                            ML = new Library.Meal.MealList
                            {
                                id = reader["id"].ToString(),
                                title = reader["title"].ToString(),
                                img = reader["banner_img"].ToString(),
                                type = Type
                            };
                            root.Add(ML);
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
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