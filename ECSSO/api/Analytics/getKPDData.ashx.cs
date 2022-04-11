using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using ECSSO.Library;
using ECSSO.Library.Analytics;
using Newtonsoft.Json;

namespace ECSSO.api.Analytics
{
    /// <summary>
    /// getKPDData 的摘要描述
    /// </summary>
    public class getKPDData : IHttpHandler
    {
        GetStr GS;
        TokenItem token;
        KPDData responseJson;
        string setting, settingHR;
        public void ProcessRequest(HttpContext context)
        {
            string code, message, type;
            code = "404";
            message = "not fount";
            token = null;
            GS = new GetStr();
            responseJson = new KPDData();
            try
            {
                if (context.Request.Form["token"] != null)
                {
                    token = new TokenItem
                    {
                        token = context.Request.Form["token"]
                    };
                    this.setting = GS.checkToken(token);
                    this.settingHR = ConfigurationManager.ConnectionStrings["WebHRDB"].ToString();
                    type = context.Request.Form["type"];
                    if (this.setting.IndexOf("error") < 0)
                    {
                        string CODE, text;
                        CODE = "";
                        text = "%" + context.Request.Form["text"].Trim() + "%";
                        switch (type)
                        {
                            case "4":
                                searchOrgName(text);
                                break;
                            case "5":
                                CODE = "CHI";
                                break;
                            case "6":
                                CODE = "EPT";
                                break;
                            case "7":
                                CODE = "PKD";
                                break;
                            case "9":
                                CODE = "EXK";
                                break;
                            case "10":
                                CODE = "EXS";
                                break;
                            case "12":
                                CODE = "EDL";
                                break;
                            case "13":
                                searchMenuSub(text);
                                break;
                            case "14":
                                searchMenu(text);
                                break;
                            case "15":
                                searchTag(text);
                                break;
                            case "16":
                                searchName(text);
                                break;
                            case "19":
                                searchZip(text);
                                break;
                            default:
                                throw new Exception("操作不存在");
                        }
                        if (CODE != "") searchBasic(CODE, text);
                        code = "200";
                        message = "success";
                    }
                    else
                    {
                        code = "401";
                        message = "Token已過期";
                    }
                }
                else
                {
                    code = "401";
                    message = "Token不可為空";
                }
            }
            catch (Exception ex)
            {
                code = "500";
                message = ex.Message;
            }
            finally
            {
                context.Response.Write(printMsg(code, message));
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        private void searchName(string like)
        {
            if (GS.hasPwoer(setting, "G002", "canadd", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select * from Cust where ch_name like @like or ident like @like
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@like", like));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        responseJson.list = new List<KPDDataItem>();
                        while (reader.Read())
                        {
                            responseJson.list.Add(new KPDDataItem
                            {
                                title = reader["ch_name"].ToString(),
                                code = reader["mem_id"].ToString()
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("資料錯誤:" + e.Message);
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
            else throw new Exception("沒有權限");
        }
        private void searchOrgName(string like)
        {
            if (GS.hasPwoer(setting, "G002", "canadd", token.id))
            {
                using (SqlConnection conn = new SqlConnection(settingHR))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select top 100 ORG_ID,ORG_NAME 
                        from [dbo].[V_ORG_CODE] 
                        where (ORG_NAME like @like or ORG_ID like @like) and ORG_ID like '397%'
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@like", like));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        responseJson.list = new List<KPDDataItem>();
                        while (reader.Read())
                        {
                            responseJson.list.Add(new KPDDataItem {
                                title = reader["ORG_NAME"].ToString(),
                                code = reader["ORG_ID"].ToString()
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("資料錯誤:" + e.Message);
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
            else throw new Exception("沒有權限");
        }
        private void searchBasic(string code,string like) {
            if (GS.hasPwoer(setting, "G002", "canadd", token.id))
            {
                using (SqlConnection conn = new SqlConnection(settingHR))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select top 100 CODE_NAME,CODE_CODE
                        from [dbo].[V_BASIC_CODE] 
                        where ITEM_CODE=@code and (CODE_NAME like @like or CODE_CODE like @like)
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@code", code));
                    cmd.Parameters.Add(new SqlParameter("@like", like));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        responseJson.list = new List<KPDDataItem>();
                        while (reader.Read())
                        {
                            responseJson.list.Add(new KPDDataItem
                            {
                                title = reader["CODE_NAME"].ToString(),
                                code = reader["CODE_CODE"].ToString()
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("資料錯誤:" + e.Message);
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
            else throw new Exception("沒有權限");
        }
        private void searchMenuSub(string like) {
            if (GS.hasPwoer(setting, "G002", "canadd", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select top 100 title,id
                        from menu_sub
                        where title like @like
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@like", like));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        responseJson.list = new List<KPDDataItem>();
                        while (reader.Read())
                        {
                            responseJson.list.Add(new KPDDataItem
                            {
                                title = reader["title"].ToString(),
                                code = reader["id"].ToString()
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("資料錯誤:" + e.Message);
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
            else throw new Exception("沒有權限");
        }
        private void searchMenu(string like)
        {
            if (GS.hasPwoer(setting, "G002", "canadd", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select top 100 title,id
                        from menu
                        where title like @like
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@like", like));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        responseJson.list = new List<KPDDataItem>();
                        while (reader.Read())
                        {
                            responseJson.list.Add(new KPDDataItem
                            {
                                title = reader["title"].ToString(),
                                code = reader["id"].ToString()
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("資料錯誤:" + e.Message);
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
            else throw new Exception("沒有權限");
        }
        private void searchTag(string like)
        {
            if (GS.hasPwoer(setting, "G002", "canadd", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select top 100 title,id
                        from tag
                        where title like @like
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@like", like));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        responseJson.list = new List<KPDDataItem>();
                        while (reader.Read())
                        {
                            responseJson.list.Add(new KPDDataItem {
                                title=reader["title"].ToString(),
                                code= reader["id"].ToString()
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("資料錯誤:" + e.Message);
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
            else throw new Exception("沒有權限");
        }
        private void searchZip(string like)
        {
            if (GS.hasPwoer(setting, "G002", "canadd", token.id))
            {
                using (SqlConnection conn = new SqlConnection(setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        select * from area 
                        where cityid=16 and (name like @like or zip like @like)
                    ", conn);
                    cmd.Parameters.Add(new SqlParameter("@like", like));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        responseJson.list = new List<KPDDataItem>();
                        while (reader.Read())
                        {
                            responseJson.list.Add(new KPDDataItem
                            {
                                title = reader["name"].ToString(),
                                code = reader["zip"].ToString()
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("資料錯誤:" + e.Message);
                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
            else throw new Exception("沒有權限");
        }
        
        private String printMsg(String RspnCode, String RspnMsg)
        {
            responseJson.RspnCode = RspnCode;
            responseJson.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(responseJson);
        }
    }
}