using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using ECSSO.Library;

namespace ECSSO.api
{
    /// <summary>
    /// Coupon 的摘要描述
    /// </summary>
    public class CouponOption : IHttpHandler
    {
        GetStr GS;
        HttpContext context;
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Params["VCode"] == null) ResponseWriteEnd(context, ErrorMsg("error", "VCode必填"));
            if (context.Request.Params["MemberID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "MemberID必填"));
            if (context.Request.Params["SiteID"] == null) ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填"));
            if (context.Request.Params["Type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Type必填"));

            if (context.Request.Params["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Params["VCode"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "VCode必填"));
            if (context.Request.Params["MemberID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "MemberID必填"));
            if (context.Request.Params["SiteID"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "SiteID必填"));
            if (context.Request.Params["Type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Type必填"));

            String GCode = "";
            String ChkM = context.Request.Params["CheckM"].ToString();
            String SiteID = context.Request.Params["SiteID"].ToString();
            String VCode = context.Request.Params["VCode"].ToString();
            String MemberID = context.Request.Params["MemberID"].ToString();
            String Type = context.Request.Params["Type"].ToString();
            if (context.Request.Params["GCode"] != null)
                GCode = context.Request.Params["GCode"].ToString();

            GS = new GetStr();
            String Setting = GS.GetSetting(SiteID);
            String OrgName = GS.GetOrgName(Setting);
            
            if (GS.MD5Check(Type + SiteID + OrgName + MemberID + VCode, ChkM))
            {
                if (!ChkMemberID(Setting, MemberID)) ResponseWriteEnd(context, ErrorMsg("error", "error:0"));   //查無此會員
                switch (Type) 
                {
                    case "1":       //領取優惠券
                        String[] GetCouponStr = ChkCoupon(Setting, VCode);

                        if (GetCouponStr == null)
                        {
                            ResponseWriteEnd(context, ErrorMsg("error", "error:6"));   //優惠券已過期
                        }
                        else
                        {
                            if (ChkCanGetCoupon(Setting, VCode, MemberID))
                            {
                                if (InsertCustCoupon(Setting, MemberID, VCode, GetCouponStr[0].ToString(), GetCouponStr[1].ToString(), GetCouponStr[2].ToString(), GetCouponStr[3].ToString()))
                                {
                                    if (GCode != "")
                                    {
                                        if (ChkUseCoupon(Setting, MemberID, VCode))
                                        {
                                            ResponseWriteEnd(context, ErrorMsg("error", "success"));
                                        }
                                        else
                                        {
                                            ResponseWriteEnd(context, ErrorMsg("error", "error:7"));        //優惠券已使用
                                        }
                                    }
                                    else ResponseWriteEnd(context, ErrorMsg("error", "success"));        //成功
                                }
                                else
                                {
                                    ResponseWriteEnd(context, ErrorMsg("error", "error:2"));        //儲存CustCoupon有誤
                                }
                            }
                            else
                            {
                                ResponseWriteEnd(context, ErrorMsg("error", "error:3"));        //重複領取
                            }
                        }
                        break;
                    case "2":       //使用(註銷)優惠券
                        if (ChkUseDDateCoupon(Setting, MemberID, VCode, GCode))
                        {
                            if (UseCoupon(Setting, MemberID, VCode, GCode))
                            {
                                ResponseWriteEnd(context, "success"); 
                            }
                            else 
                            {
                                ResponseWriteEnd(context, "error:1");        //儲存失敗
                            }
                        }
                        else
                        {
                            ResponseWriteEnd(context, "error:0");        //優惠券已過期
                        }

                        break;
                }
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

        #region 確認是否有此會員
        private bool ChkMemberID(String Setting, String MemberID) 
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from cust where mem_id=@MemID and Chk='Y'", conn);
                cmd.Parameters.Add(new SqlParameter("@MemID", MemberID));

                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                    else {
                        return false;
                    }

                }
                finally
                {
                    reader.Close();
                }
            }
        }
        #endregion

        #region 確認是否開放領取此優惠券
        private String[] ChkCoupon(String Setting, String VCode)
        {
            String[] RetunrnStr = null;
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select end_date,getqty+1,noType,GCode
                    from coupon 
                    where disp_opt='Y' and
                        getdate()
	                    between 
		                    case 
			                    when CHARINDEX('上午',[start_date])>0 then REPLACE([start_date],' 上午 ',' ')+' AM'
			                    when CHARINDEX('下午',[start_date])>0 then REPLACE([start_date],' 下午 ',' ')+' PM'
		                    end
	                    and
		                    case 
			                    when CHARINDEX('上午',[end_date])>0 then REPLACE([end_date],' 上午 ',' ')+' AM'
			                    when CHARINDEX('下午',[end_date])>0 then REPLACE([end_date],' 下午 ',' ')+' PM'
		                    end
	                    and
                        VCode=@Vcode and 
                        stocks>=getqty+1 
                order by id desc", conn);
                cmd.Parameters.Add(new SqlParameter("@Vcode", VCode));

                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            RetunrnStr = new String[] {
                                reader[0].ToString(),
                                reader[1].ToString().PadLeft(10,'0'),
                                reader[2].ToString(),
                                reader[3].ToString()
                            };
                        }
                        
                        return RetunrnStr;
                    }
                    else
                    {
                        return RetunrnStr;
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        #endregion

        #region 檢查會員是否曾領取優惠券
        private bool ChkCanGetCoupon(String Setting, String VCode, String MemberID)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from Cust_Coupon where VCode=@VCode and memid=@MemID", conn);
                cmd.Parameters.Add(new SqlParameter("@VCode", VCode));
                cmd.Parameters.Add(new SqlParameter("@MemID", MemberID));

                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (!reader.HasRows)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                finally
                {
                    reader.Close();
                }
            }            
        }
        #endregion

        #region 會員登錄優惠券
        private bool InsertCustCoupon(String Setting, String MemberID, String VCode, String ExpireDate, String SerNo,String noType,String code)
        {
            string GCode = code;
            switch (noType){
                case "1":
                    GCode = GS.GetRandomString(9) + GS.Right(SerNo, 3);
                    GCode = GS.GetRandomString(9) + "-" + GS.Right(SerNo, 3) + "-" + GS.checkSunChar(GCode);
                    break;
                case "2":
                    string r = GS.Right(SerNo, 5);
                    GCode = GCode + "-"+GS.Right(SerNo,5) + "-" + GS.checkSunChar(GCode+r);
                    break;
                case "3": break;
                default:
                    GCode = "";
                    break;
            }
            //context.Response.Write("sp_AddCustCoupon '"+ MemberID+"','"+ VCode+"','"+ ExpireDate+"','"+ SerNo+"','"+ GCode+"',"+"1");
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_AddCustCoupon";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@memid", MemberID));
                cmd.Parameters.Add(new SqlParameter("@VCode", VCode));
                cmd.Parameters.Add(new SqlParameter("@ExpireDate", ExpireDate));
                cmd.Parameters.Add(new SqlParameter("@SerNo", SerNo));
                cmd.Parameters.Add(new SqlParameter("@GCode", GCode));
                cmd.Parameters.Add(new SqlParameter("@type", "1"));
                try
                {
                    cmd.ExecuteNonQuery();
                    return true;                
                }
                catch {
                    return false;
                }
            }
        }
        #endregion

        #region 使用優惠券
        private bool UseCoupon(String Setting,String MemberID,String VCode, String GCode) {

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_EditCustCoupon";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@mem_id", MemberID));
                cmd.Parameters.Add(new SqlParameter("@VCode", VCode));
                cmd.Parameters.Add(new SqlParameter("@GCode", GCode));
                try
                {
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

        }
        #endregion

        #region 優惠券紅利兌換
        private bool addBonus(HttpContext context,String Setting, String MemberID, String VCode)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from Coupon where VCode=@VCode", conn);
                cmd.Parameters.Add(new SqlParameter("@VCode", VCode));

                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        DataRow row = dt.Rows[0];
                        using (SqlConnection conn2 = new SqlConnection(Setting))
                        {
                            conn2.Open();

                            SqlCommand cmd2 = new SqlCommand();
                            cmd2.CommandText = "sp_cust_bonus";
                            cmd2.CommandType = CommandType.StoredProcedure;
                            cmd2.Connection = conn;
                            cmd2.Parameters.Add(new SqlParameter("@bonus_memo", "優惠券[" + row["title"].ToString() + "] 兌換"));
                            cmd2.Parameters.Add(new SqlParameter("@mem_id", MemberID));
                            cmd2.Parameters.Add(new SqlParameter("@bonus_add", row["Price"].ToString()));
                            cmd2.Parameters.Add(new SqlParameter("@bonus_spend", "0"));
                            try
                            {
                                cmd2.ExecuteNonQuery();
                                return true;
                            }
                            catch
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }

                }
                finally
                {
                    reader.Close();
                }
            }
        }
        #endregion

        #region 檢查已領取優惠券使用期限
        private bool ChkUseDDateCoupon(String Setting, String MemberID, String VCode, String GCode)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select * from Cust_Coupon where VCode=@VCode and GCode=@GCode and memid=@MemID and 
                    datediff(SECOND,
                        getdate(),
	                    case 
                            when CHARINDEX('上午',[ExpireDate]) > 0 then REPLACE([ExpireDate], ' 上午 ', ' ') + ' AM'
                            when CHARINDEX('下午',[ExpireDate]) > 0 then REPLACE([ExpireDate], ' 下午 ', ' ') + ' PM'
                        end
                    )>= 0", conn);
                cmd.Parameters.Add(new SqlParameter("@VCode", VCode));
                cmd.Parameters.Add(new SqlParameter("@GCode", GCode));
                cmd.Parameters.Add(new SqlParameter("@MemID", MemberID));

                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                finally
                {
                    reader.Close();
                }
            }        
        }
        #endregion

        #region 檢查優惠券是否已使用
        private bool ChkUseCoupon(String Setting, String MemberID, String VCode)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from Cust_Coupon where VCode=@VCode and memid=@MemID and stat='1'", conn);
                cmd.Parameters.Add(new SqlParameter("@VCode", VCode));
                cmd.Parameters.Add(new SqlParameter("@MemID", MemberID));

                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
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
}