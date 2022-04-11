using System;
using System.Collections.Generic;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Meal.Library;

namespace Meal.api
{
    /// <summary>
    /// Account 的摘要描述
    /// </summary>
    public class Account : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Params["Type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Type必填", ""));
            if (context.Request.Params["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Params["OrgName"] == null) ResponseWriteEnd(context, ErrorMsg("error", "OrgName必填", ""));
            if (context.Request.Params["Items"] == null) ResponseWriteEnd(context, ErrorMsg("error", "Items必填", ""));

            if (context.Request.Params["Type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Type必填", ""));
            if (context.Request.Params["OrgName"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "OrgName必填", ""));
            if (context.Request.Params["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填", ""));
            if (context.Request.Params["Items"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "Items必填", ""));

            String ChkM = context.Request.Params["CheckM"].ToString();
            String OrgName = context.Request.Params["OrgName"].ToString();
            String Type = context.Request.Params["Type"].ToString();

            GetMealStr GS = new GetMealStr();
            String Setting = GS.GetSetting(OrgName);
            if (!GS.MD5Check(Type + OrgName, ChkM)) ResponseWriteEnd(context, ErrorMsg("error", "error:0", Setting));     //驗證碼錯誤
            
            Library.Account.InputData postf = JsonConvert.DeserializeObject<Library.Account.InputData>(context.Request.Params["Items"]);
            String ID = "";
            String Pwd = "";
            String StoreID = "";
            String CID = "";
            if (postf.ID == null || postf.ID == "") ResponseWriteEnd(context, ErrorMsg("error", "ID必填", ""));
            ID = postf.ID;

            switch (Type)
            {
                case "1":   //驗證帳密

                    if (postf.Pwd == null || postf.Pwd == "") ResponseWriteEnd(context, ErrorMsg("error", "Pwd必填", ""));
                    Pwd = postf.Pwd;
                    ResponseWriteEnd(context, Register(OrgName, ID, Pwd).ToString());
                    break;

                case "2":   //帳號所屬商店

                    ResponseWriteEnd(context, GetStore(Setting, ID));
                    
                    break;
                case "3":   //帳號對應角色

                    if (postf.StoreID == null || postf.StoreID == "") ResponseWriteEnd(context, ErrorMsg("error", "StoreID必填", ""));
                    StoreID = postf.StoreID;
                    ResponseWriteEnd(context, GetCharacter(Setting, ID, StoreID));

                    break;
                case "4":   //角色對應的webjob群組及權限

                    if (postf.StoreID == null || postf.StoreID == "") ResponseWriteEnd(context, ErrorMsg("error", "StoreID必填", ""));
                    if (postf.CID == null || postf.CID == "") ResponseWriteEnd(context, ErrorMsg("error", "CID必填", ""));
                    StoreID = postf.StoreID;
                    CID = postf.CID;
                    ResponseWriteEnd(context, GetWebjobs(Setting, ID, StoreID, CID));

                    break;
                default:

                    break;
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
            
            Library.Account.ErrorObject root = new Library.Account.ErrorObject();
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

        #region 驗證帳密
        private bool Register(String OrgName,String ID,String Pwd) 
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB2"].ToString()))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("select * from IDManagement where orgName=@orgname and Manager_ID=@id and Manager_PWD=sys.fn_VarBinToHexStr(hashbytes('MD5', convert(nvarchar,@pwd))) and End_Date >= (CONVERT([nvarchar](10),getdate(),(120)))", conn);
                cmd.Parameters.Add(new SqlParameter("@orgname", OrgName));
                cmd.Parameters.Add(new SqlParameter("@id", ID));
                cmd.Parameters.Add(new SqlParameter("@pwd", Pwd));
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

        #region 帳號對應角色
        private String GetCharacter(String Setting, String EmplID, String StoreID)
        {
            List<Library.Account.Character> root = new List<Library.Account.Character>();

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("select isnull(a.cid,'') as id,isnull(b.title,'') as title from authors as a left join Character as b on a.cid=b.id where a.empl_id=@empl_id and a.storeid=@storeid and a.cid<>'' group by a.cid,b.title  ", conn);
                cmd.Parameters.Add(new SqlParameter("@empl_id", EmplID));
                cmd.Parameters.Add(new SqlParameter("@storeid", StoreID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Library.Account.Character CList = new Library.Account.Character
                            {
                                ID = reader[0].ToString(),
                                Title = reader[1].ToString()
                            };
                            root.Add(CList);
                        }
                    }
                    else
                    {
                        root = null;
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
        #region 帳號對應商店
        private String GetStore(String Setting, String EmplID)
        {
            List<Library.Account.Store> root = new List<Library.Account.Store>();

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("select isnull(a.storeid,'') as id,isnull(b.title,'') as title from authors as a left join bookingStore as b on a.storeid=b.id where a.empl_id=@empl_id and a.storeid<>'' group by a.storeid,b.title ", conn);
                cmd.Parameters.Add(new SqlParameter("@empl_id", EmplID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Library.Account.Store SList = new Library.Account.Store
                            {
                                ID = reader[0].ToString(),
                                Title = reader[1].ToString()
                            };
                            root.Add(SList);
                        }
                    }
                    else
                    {
                        root = null;
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
        #region 帳號對應權限
        private String GetWebjobs(String Setting, String EmplID, String StoreID, String CID)
        {
            List<Library.Account.Webjobs> Webjobs = new List<Library.Account.Webjobs>();
            List<Library.Account.WebjobsGroup> WebJobsGroup = new List<Library.Account.WebjobsGroup>();

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                cmd = new SqlCommand("select b.id,b.title from dbo.WebjobsGroup_Character as a left join dbo.WebjobsGroup_Head as b on a.Gid=b.id where a.Cid=@Cid", conn);
                cmd.Parameters.Add(new SqlParameter("@Cid", CID));
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
                                cmd2 = new SqlCommand("select a.job_id,c.job_name,isnull(b.canadd,'N') as canadd,isnull(b.canedit,'N') as canedit,isnull(b.candel,'N') as candel,isnull(b.canexe,'N') as canexe,isnull(b.canqry,'N') as canqry,c.job_url from WebjobsGroup as a left join authors as b on a.job_id=b.job_id and storeid=@storeid and cid=@cid and empl_id=@empl_id left join webjobs as c on a.job_id=c.job_id where Gid=@Gid", conn2);
                                cmd2.Parameters.Add(new SqlParameter("@storeid", StoreID));
                                cmd2.Parameters.Add(new SqlParameter("@cid", CID));
                                cmd2.Parameters.Add(new SqlParameter("@empl_id", EmplID));
                                cmd2.Parameters.Add(new SqlParameter("@Gid", reader[0].ToString()));
                                SqlDataReader reader2 = cmd2.ExecuteReader();
                                try
                                {
                                    if (reader2.HasRows)
                                    {
                                        while (reader2.Read())
                                        {
                                            Library.Account.Webjobs WList = new Library.Account.Webjobs
                                            {
                                                JobID = reader2[0].ToString(),
                                                Title = reader2[1].ToString(),
                                                CanAdd = reader2[2].ToString(),
                                                CanEdit = reader2[3].ToString(),
                                                CanDel = reader2[4].ToString(),
                                                CanExe = reader2[5].ToString(),
                                                CanQry = reader2[6].ToString(),
                                                JobUrl = reader2[7].ToString()
                                            };
                                            Webjobs.Add(WList);
                                        }
                                    }
                                    else {
                                        Webjobs = null;
                                    }
                                }
                                finally
                                {
                                    reader2.Close();
                                }
                            }
                            Library.Account.WebjobsGroup WGList = new Library.Account.WebjobsGroup
                            {
                                ID = reader[0].ToString(),
                                Title = reader[1].ToString(),
                                Webjobs = Webjobs
                            };
                            WebJobsGroup.Add(WGList);
                            Webjobs = new List<Library.Account.Webjobs>();
                        }
                    }
                    else 
                    {
                        WebJobsGroup = null;
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            return JsonConvert.SerializeObject(WebJobsGroup);
        }
        #endregion

    }
}