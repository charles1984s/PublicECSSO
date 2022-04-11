using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using ECSSO.Library.Component;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace ECSSO.api.Component
{
    /// <summary>
    /// Components 的摘要描述
    /// </summary>
    public class Components : IHttpHandler
    {
        CokerComponentList obj;
        GetStr GS;
        private string setting;
        private string code, message;
        public void ProcessRequest(HttpContext context)
        {
            GS = new GetStr();
            obj = new CokerComponentList();
            code = "404";
            message = "not fount";
            try
            {
                if (context.Request.Form["token"] != null)
                {
                    setting = GS.checkToken(context.Request.Form["token"]);
                    if (setting.IndexOf("error") < 0) {
                        Regex NumberPattern = new Regex("^[0-9]*[1-9][0-9]*$");
                        if (NumberPattern.IsMatch(context.Request.Form["fid"]) &&
                            NumberPattern.IsMatch(context.Request.Form["type"])
                        )
                        {
                            getComponents(int.Parse(context.Request.Form["fid"]), int.Parse(context.Request.Form["type"]));
                            code = "200";
                            message = "success";
                        }
                        else {
                            code = "403";
                            message = "data error";
                        }
                    }
                    else
                    {
                        code = "401";
                        message = "Token已過期" + this.setting;
                    }
                }
            }
            catch (Exception ex)
            {
                code = "500";
                message = ex.StackTrace;
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
        private void getComponents(int fid,int type) {
            using (SqlConnection conn = new SqlConnection(setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select m.*,f.colNum from menu_cont as m
                    left join fence as f on f.id=m.bid and m.objectType=8
                    where m.menu_id=@fid and m.[type]=@type
                    order by CONVERT(int,m.ser_no)
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@fid", fid));
                cmd.Parameters.Add(new SqlParameter("@type", type));
                SqlDataReader reader = null;
                try {
                    reader = cmd.ExecuteReader();
                    obj.list = new List<Library.Component.Component>();
                    while (reader.Read()) {
                        int objectType = GS.StringToInt(reader["objectType"].ToString(), 0);
                        Library.Component.Component c = null;
                        switch (objectType) {
                            case 1:
                                CokerText t = new CokerText();
                                t.link = reader["col1"].ToString();
                                t.target = reader["col9"].ToString();
                                t.cont = HttpUtility.HtmlDecode(reader["cont"].ToString());
                                c = t;
                                break;
                            case 2:
                                CokerFile f = new CokerFile();
                                f.file = reader["img"].ToString();
                                c = f;
                                break;
                            case 3:
                                CokerImg i = new CokerImg();
                                i.img = reader["img"].ToString();
                                i.link = reader["col1"].ToString();
                                i.target = reader["col9"].ToString();
                                i.direct = reader["img_align"].ToString();
                                i.scale = GS.StringToInt(reader["col2"].ToString(),12);
                                i.cont = HttpUtility.HtmlDecode(reader["cont"].ToString());
                                c = i;
                                break;
                            case 4:
                                CokerGoogleMap m = new CokerGoogleMap();
                                m.map = reader["col1"].ToString();
                                m.direct = reader["img_align"].ToString();
                                m.scale = GS.StringToInt(reader["col2"].ToString(), 12);
                                m.cont = HttpUtility.HtmlDecode(reader["cont"].ToString());
                                c = m;
                                break;
                            case 6:
                                CokerYoutude y = new CokerYoutude();
                                y.img = reader["img"].ToString();
                                y.link = reader["col1"].ToString();
                                y.target = reader["col9"].ToString();
                                y.direct = reader["img_align"].ToString();
                                y.scale = GS.StringToInt(reader["col2"].ToString(), 12);
                                y.cont = HttpUtility.HtmlDecode(reader["cont"].ToString());
                                c = y;
                                break;
                            case 7:
                                CokerCoupon u = new CokerCoupon();
                                u.bid = GS.StringToInt(reader["bid"].ToString(), 0);
                                u.bTitle = getCouponTitle(u.bid);
                                u.img = reader["img"].ToString();
                                c = u;
                                break;
                            case 8:
                                CokerFence fence = new CokerFence();
                                fence.setFence(setting, GS.StringToInt(reader["bid"].ToString(), 0));
                                c = fence;
                                break;
                        }
                        if (c != null)
                        {
                            c.id = GS.StringToInt(reader["id"].ToString(), 0);
                            c.fid = GS.StringToInt(reader["menu_id"].ToString(), 0);
                            c.type = GS.StringToInt(reader["type"].ToString(), 0);
                            c.dispOpt = reader["disp_opt"].ToString();
                            c.objectType = objectType;
                            c.serNo = GS.StringToInt(reader["ser_no"].ToString(), 500);
                            c.title = HttpUtility.HtmlDecode(reader["title"].ToString());
                            c.startDate = reader["start_date"].ToString();
                            c.endDate = reader["end_date"].ToString();
                            obj.list.Add(c);
                        }else obj.list.Add(new CokerCoupon());
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
        }
        private string getCouponTitle(int id) {
            string t = "";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select title from coupon where id=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                SqlDataReader reader = null;
                try {
                    reader = cmd.ExecuteReader();
                    if (reader.Read()) {
                        t = reader["title"].ToString();
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return t;
        }
        private String printMsg(String RspnCode, String RspnMsg) {
            string rsp = "";
            obj.RspnCode = RspnCode;
            obj.RspnMsg = RspnMsg;
            rsp = JsonConvert.SerializeObject(obj);
            return rsp;
        }
    }
}