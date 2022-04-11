using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;

namespace ECSSO.Library
{
    public class Member
    {
        public class Data		//同舊版說明，沒更新
        {
            public String ID { get; set; }   //會員帳號
            public String Pwd { get; set; }      //密碼
            public String ChName { get; set; }    //姓名
            public String Sex { get; set; }  //性別
            public String Email { get; set; }    //Email
            public String Birth { get; set; }    //生日           
            public String Tel { get; set; }    //電話           
            public String CellPhone { get; set; }    //手機           
            public String Addr { get; set; }    //地址
            public String C_ZIP { get; set; }    //郵遞區號     
            public String Language { get; set; }    //語系
            public String token { get; set; }    //登入token
            public int totalBouns { get; set; } //剩餘紅利
            public int Vip { get; set; } //會員等級
            public void setSData(string setting,string token) {
                using (SqlConnection conn = new SqlConnection(setting)) {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select c.* from token as t
                        left join Cust as c on c.id=t.ManagerID
                        where t.id = @token and DateDiff(MINUTE,GETDATE(),CONVERT(datetime,end_time))>=0
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@token", token));
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read()) {
                        this.token = token;
                        if (!reader.IsDBNull(reader.GetOrdinal("id")))
                        {
                            this.ID = reader["id"].ToString();
                            this.totalBouns = int.Parse(reader["bonus_total"].ToString());
                            this.Vip = int.Parse(reader["vip"].ToString());
                        }
                    }
                }
            }
        }

        #region 紅利歷史紀錄
        public class RootObject
        {
            public String MemberID { get; set; }
            public String VIP { get; set; }
            public String StartDate { get; set; }
            public String EndDate { get; set; }
            public List<Bonus> Bonus { get; set; }
        }
        public class Bonus		//同舊版說明，沒更新
        {
            public String MemID { get; set; }   //會員編號
            public String Date { get; set; }      //紅利日期
            public String Add { get; set; }    //新增紅利
            public String Spend { get; set; }  //減少紅利
            public String Memo { get; set; }    //備註                        
        }
        #endregion

        #region 取得會員資料
        public string GetMemberData(String MemID, String setting)
        {
            RootObject root = new RootObject();            
            String StartDate = "";
            String EndDate = "";
            String VIP = "";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select starttime,endtime,vip from cust where mem_id=@MemID and Chk='Y'", conn);
                cmd.Parameters.Add(new SqlParameter("@MemID", MemID));
                
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            StartDate=reader[0].ToString();
                            EndDate=reader[1].ToString();
                            VIP = reader[2].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            root.MemberID = MemID;
            root.StartDate = StartDate;
            root.EndDate = EndDate;
            root.VIP = VIP;
            root.Bonus = null;

            String returnStr = JsonConvert.SerializeObject(root);
            return returnStr;

        }
        #endregion

        #region 取得紅利json字串
        public string GetBonusListJson(String MemID, String SiteID)
        {
            GetStr GS = new GetStr();
            String setting = GS.GetSetting(SiteID);
            RootObject root = new RootObject();
            List<Bonus> Bonus = new List<Bonus>();           

            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select bonus_memo,mem_id,date,bonus_add,bonus_spend from bonus where mem_id=@MemID and (bonus_add > 0 or bonus_spend > 0) and date >=@date order by cdate desc", conn);

                int u = 0;
                String MemoStr = "";
                if (int.TryParse(MemID, out u))
                {
                    cmd.Parameters.Add(new SqlParameter("@MemID", MemID));
                    cmd.Parameters.Add(new SqlParameter("@date", DateTime.Now.AddMonths(-6).ToString("yyyy/MM/dd")));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                switch (reader[0].ToString().Trim()) 
                                { 
                                    case "new":
                                        MemoStr = "加入會員";
                                        break;
                                    default:
                                        MemoStr = reader[0].ToString().Replace("edit","");
                                        break;
                                }
                                Bonus BList = new Bonus
                                {
                                    Memo = MemoStr,
                                    MemID=reader[1].ToString(),
                                    Date=reader[2].ToString(),
                                    Add=reader[3].ToString(),
                                    Spend=reader[4].ToString()                                    
                                };
                                Bonus.Add(BList);
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            root.Bonus = Bonus;
            String returnStr = JsonConvert.SerializeObject(root);
            return returnStr;
        }
        #endregion
    }
}