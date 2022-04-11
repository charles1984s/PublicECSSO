using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using ECSSO.Library;
using System.Net;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Facebook;

namespace ECSSO.api
{
    /// <summary>
    /// GameData 的摘要描述
    /// </summary>
    public class GameData : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            //String ChkM = context.Request.Form["CheckM"];
            #region 檢查post值
            if (context.Request.Form["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Form["VerCode"] == null) ResponseWriteEnd(context, ErrorMsg("error", "VerCode必填"));
            if (context.Request.Form["type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "type必填"));

            if (context.Request.Form["CheckM"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Form["VerCode"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "VerCode必填"));
            if (context.Request.Form["type"].ToString() == "") ResponseWriteEnd(context, ErrorMsg("error", "type必填"));
            #endregion

            String type = context.Request.Form["type"].ToString();
            String ChkM = context.Request.Form["CheckM"].ToString();
            String VerCode = context.Request.Form["VerCode"].ToString();
            String Orgname = GetOrgName(VerCode);
            if (Orgname == "") ResponseWriteEnd(context, ErrorMsg("error", "查無Orgname"));            

            if (context.Request.Form["ItemData"] != null)
            {
                PostForm.GamePost postf = JsonConvert.DeserializeObject<PostForm.GamePost>(context.Request.Form["ItemData"]);                
                
                String GameID = "";
                String QID = "";
                String Answer = "";
                String name = "";
                String email = "";
                String UID = "";
                String time = "";
                String CID = "";
                String photo = "";
                String Answertime = "";
                String Gender = "";
                String Stime = "";
                String QuestionNum = "";

                GetStr GS = new GetStr();
                if (GS.MD5Check(VerCode + type, ChkM))
                {
                    String Setting = GetSetting(Orgname);

                    switch (type)
                    {
                        case "Login":   //API - 玩家FB登入
                            
                            name = postf.name;                            
                            email = postf.email;
                            UID = postf.UID;
                            photo = postf.photo;
                            Gender = postf.Gender;

                            ResponseWriteEnd(context, Login(VerCode, name, email, UID, photo, Gender, Setting, GetSetting("ezsaleo2o")));
                            break;
                        case "Wait":    //API -詢問玩家等待數

                            time = postf.time;
                            Stime = postf.stime;

                            ResponseWriteEnd(context, Wait(VerCode, time, Stime, Setting));
                            break;
                        case "Wait2":    //API -詢問玩家等待數(2)

                            time = postf.time;
                            UID = postf.UID;

                            ResponseWriteEnd(context, Wait2(VerCode, time, UID, Setting));
                            break;
                        case "Stop":   //API -遊戲中斷
                            ResponseWriteEnd(context, Stop(VerCode, Setting));
                            break;
                        case "Start":   //API -遊戲開始
                            Stime = postf.stime;

                            ResponseWriteEnd(context, Start(VerCode, Stime, Setting));
                            break;
                        case "GetQuestion":  //API-取得題目

                            CID = postf.CID;

                            ResponseWriteEnd(context, GetQuestion(VerCode, CID, Setting));
                            break;
                        case "Signup":

                            UID = postf.UID;

                            ResponseWriteEnd(context, Signup(VerCode, UID, Setting));
                            break;
                        case "Question":   //API -出題目

                            GameID = postf.GameID;
                            QID = postf.QID;
                            Answer = postf.Answer;
                            Answertime = postf.Answertime;


                            ResponseWriteEnd(context, Question(VerCode, GameID, QID, Answer, Answertime, Setting));
                            break;
                        case "Answertime":

                            UID = postf.UID;

                            ResponseWriteEnd(context, AnswerTime(VerCode, UID, Setting));
                            break;
                        case "Answer":   //API -傳答案

                            GameID = postf.GameID;
                            Answertime = postf.Answertime;
                            UID = postf.UID;
                            Answer = postf.Answer;

                            ResponseWriteEnd(context, SendAnswer(VerCode, GameID, Answertime, UID, Answer, Setting));
                            break;
                        case "Check":   //API -公布答案

                            GameID = postf.GameID;
                            UID = postf.UID;
                            Answertime = postf.Answertime;

                            ResponseWriteEnd(context, Check(VerCode, GameID, UID, Answertime, Setting));
                            break;
                        case "Win":   //API -查詢闖關成功玩家

                            GameID = postf.GameID;
                            QuestionNum = postf.QuestionNum;

                            ResponseWriteEnd(context, Win(VerCode, GameID, Setting, QuestionNum));
                            break;
                        case "Gift":   //API –完全答對獲得紅利點數

                            UID = postf.UID;
                            GameID = postf.GameID;

                            ResponseWriteEnd(context, Gift(VerCode, UID, GameID, Setting, GetSetting("ezsaleo2o")));
                            break;
                        case "Share":   //API -分享FB訊息獲得抽獎機會

                            UID = postf.UID;
                            GameID = postf.GameID;

                            ResponseWriteEnd(context, Share(VerCode, UID, GameID, Setting, GetSetting("ezsaleo2o")));
                            break;
                        case "GetQuestionClass":    //API-取得題目分類

                            ResponseWriteEnd(context, GetQuestionClass(Setting));
                            break;
                        default:
                            ResponseWriteEnd(context, ErrorMsg("error", "type值有誤，查無此作業"));
                            break;
                    }
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

        private void ResponseWriteEnd(HttpContext context, string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }

        #region 取得題目分類
        private String GetQuestionClass(String Setting)
        {
            Game.GetQuestionClass root = new Game.GetQuestionClass();
            Game.QuestionClass QClass = new Game.QuestionClass();
            List<Game.QuestionClass> QClassItem = new List<Game.QuestionClass>();
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select id,title from Game_QA_List", conn);                
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            QClass = new Game.QuestionClass { 
                                CID = reader[0].ToString(), 
                                Title = reader[1].ToString() 
                            };
                            QClassItem.Add(QClass);
                        }
                    }
                }
                finally { reader.Close(); }
            }
            
            root = new Game.GetQuestionClass
            {
                RspnCode = "0",
                RspnMsg = "",
                QuestionClass = QClassItem
            };

            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 玩家FB登入
        private String Login(String VerCode, String Name, String Email, String UID, String Photo,String Gender, String Setting, String EZSetting)
        {
            SaveLog("Login", "Name=" + Name + " / Email=" + Email + " / UID=" + UID + " / Photo=" + Photo + " / Gender=" + Gender, Setting);

            SaveCust(Setting, UID, Name, Photo, Email, Gender);
            SaveCust(EZSetting, UID, Name, Photo, Email, Gender);
            GameSave(Setting, UID, Name, Photo, VerCode);            
            
            Game.FBData fbdata = new Game.FBData()
            {
                UID = UID,
                photo = Photo,
                name = Name
            };

            String RspnCode = "0";
            String RspnMsg = "";

            Game.Login root = new Game.Login()
            {
                RspnCode = RspnCode,
                RspnMsg = RspnMsg,
                Item = fbdata
            };

            SaveLog("Login", JsonConvert.SerializeObject(root), Setting);
            
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 詢問玩家等待數
        private String Wait(String VerCode, String time, String stime, String Setting)
        {
            SaveLog("Wait", "time=" + time + " / stime=" + stime, Setting);

            Game.GamerNum root = new Game.GamerNum();
            Game.Num gNum = new Game.Num();
            DateTime dt;

            String Num = "0";

            if (stime == "")
            {
                dt = new DateTime(Convert.ToInt16(time.Substring(0, 4)), Convert.ToInt16(time.Substring(4, 2)), Convert.ToInt16(time.Substring(6, 2)), Convert.ToInt16(time.Substring(8, 2)), Convert.ToInt16(time.Substring(10, 2)), Convert.ToInt16(time.Substring(12, 2)));
            }
            else 
            {
                dt = new DateTime(Convert.ToInt16(stime.Substring(0, 4)), Convert.ToInt16(stime.Substring(4, 2)), Convert.ToInt16(stime.Substring(6, 2)), Convert.ToInt16(stime.Substring(8, 2)), Convert.ToInt16(stime.Substring(10, 2)), Convert.ToInt16(stime.Substring(12, 2)));
            }
            
            
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;
                
                if (stime == "")
                {
                    cmd = new SqlCommand("select COUNT(*) from gamer where GameID='' and vercode=@vercode and createdate between @d1 and @d2", conn);
                    cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                    cmd.Parameters.Add(new SqlParameter("@d1", dt.AddSeconds(-1).ToString("yyyyMMddHHmmss")));
                    cmd.Parameters.Add(new SqlParameter("@d2", dt.AddSeconds(0).ToString("yyyyMMddHHmmss")));

                    SaveLog("Wait", "select COUNT(*) from gamer where GameID='' and vercode='" + VerCode + "' and createdate between '" + dt.AddSeconds(-1).ToString("yyyyMMddHHmmss") + "' and '" + dt.AddSeconds(0).ToString("yyyyMMddHHmmss") + "'", Setting);

                }
                else
                {
                    cmd = new SqlCommand("select COUNT(*) from gamer where GameID='' and vercode=@vercode and (starttime=@starttime or createdate between @d1 and @d2)", conn);
                    cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                    cmd.Parameters.Add(new SqlParameter("@starttime", stime));
                    cmd.Parameters.Add(new SqlParameter("@d1", stime));
                    cmd.Parameters.Add(new SqlParameter("@d2", dt.AddSeconds(30).ToString("yyyyMMddHHmmss")));

                    SaveLog("Wait", "select COUNT(*) from gamer where GameID='' and vercode='" + VerCode + "' and (starttime='" + stime + "' or createdate between '" + stime + "' and '" + dt.AddSeconds(30).ToString("yyyyMMddHHmmss") + "')", Setting);
                }
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Num = reader[0].ToString();
                        }
                    }
                }
                finally { reader.Close(); }
            }

            gNum = new Game.Num()
            {
                gamer = Num
            };

            root.RspnCode = "0";
            root.Item = gNum;
            root.RspnMsg = "";

            SaveLog("Wait", JsonConvert.SerializeObject(root), Setting);

            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 詢問玩家等待數(2)
        private String Wait2(String VerCode, String time, String UID, String Setting)
        {
            SaveLog("Wait2", "time=" + time + " / UID=" + UID, Setting);

            String LogStr = "";

            if (time != "")
            {
                if (UID == "")
                {
                    using (SqlConnection conn = new SqlConnection(Setting))
                    {
                        DateTime dt = new DateTime(Convert.ToInt16(time.Substring(0, 4)), Convert.ToInt16(time.Substring(4, 2)), Convert.ToInt16(time.Substring(6, 2)), Convert.ToInt16(time.Substring(8, 2)), Convert.ToInt16(time.Substring(10, 2)), Convert.ToInt16(time.Substring(12, 2)));

                        conn.Open();
                        SqlCommand cmd;

                        cmd = new SqlCommand("update gamer set StartTime=@StartTime where VerCode=@VerCode and checkin='S' and GameID='' and round='0' and createdate between @d1 and @d2", conn);
                        cmd.Parameters.Add(new SqlParameter("@StartTime", time));
                        cmd.Parameters.Add(new SqlParameter("@VerCode", VerCode));
                        cmd.Parameters.Add(new SqlParameter("@d1", dt.AddSeconds(-1).ToString("yyyyMMddHHmmss")));
                        cmd.Parameters.Add(new SqlParameter("@d2", dt.AddSeconds(0).ToString("yyyyMMddHHmmss")));
                        cmd.ExecuteNonQuery();
                    }

                    LogStr = "update gamer set StartTime='" + time + "' where VerCode='" + VerCode + "' and checkin='S' and GameID='' and round='0'";
                    
                    SaveLog("Wait2", LogStr, Setting);
                }
                else
                {
                    using (SqlConnection conn = new SqlConnection(Setting))
                    {
                        conn.Open();
                        SqlCommand cmd;

                        cmd = new SqlCommand("select StartTime from gamer where GameID='' and vercode=@vercode and StartTime<>''", conn);
                        cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                        cmd.Parameters.Add(new SqlParameter("@UID", UID));
                        SqlDataReader reader = cmd.ExecuteReader();
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    time = reader[0].ToString();
                                }
                            }
                            else {
                                time = "";
                            }
                        }
                        finally { reader.Close(); }
                    }

                    LogStr = "select StartTime from gamer where GameID='' and vercode='" + VerCode + "' and StartTime<>''";
                    
                    SaveLog("Wait2", LogStr, Setting);
                }
            }
            else {
                time = DateTime.Now.ToString("yyyyMMddHHmmss");                
            }

            Game.GamerSTime root = new Game.GamerSTime();
            Game.STime gST = new Game.STime();

            gST = new Game.STime()
            {
                time = time
            };

            root.RspnCode = "0";
            root.Item = gST;
            root.RspnMsg = "";

            SaveLog("Wait2", JsonConvert.SerializeObject(root), Setting);
            
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 遊戲中斷
        private String Stop(String VerCode, String Setting)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("delete from gamer where vercode=@vercode", conn);                
                cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));                
                cmd.ExecuteNonQuery();

                SaveLog("Stop", "delete from gamer where vercode='" + VerCode + "'", Setting);
            }

            String RespCode = "0";
            String RespMsg = "";

            return ErrorMsg(RespCode, RespMsg);
        }
        #endregion

        #region 遊戲開始
        private String Start(String VerCode, String stime, String Setting)
        {
            SaveLog("Start", "stime=" + stime, Setting);

            Game.Start root = new Game.Start();
            List<Game.GameClass> GameclassList = new List<Game.GameClass>();
            Game.GameClassData CData = new Game.GameClassData();
            DateTime dt = new DateTime(Convert.ToInt16(stime.Substring(0, 4)), Convert.ToInt16(stime.Substring(4, 2)), Convert.ToInt16(stime.Substring(6, 2)), Convert.ToInt16(stime.Substring(8, 2)), Convert.ToInt16(stime.Substring(10, 2)), Convert.ToInt16(stime.Substring(12, 2)));            
            String GameID = stime;
            String Num = "0";

            #region 列出問題分類
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select id,title from game_QA_List", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Game.GameClass GCList = new Game.GameClass()
                            {
                                CID = reader[0].ToString(),
                                Title = reader[1].ToString()
                            };
                            GameclassList.Add(GCList);
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            #endregion

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("update gamer set GameID = @gameid,checkin='Y' where GameID='' and vercode=@vercode and UID in (select top 10 UID from gamer where GameID='' and vercode=@vercode2 and starttime=@starttime or createdate between @d1 and @d2 order by createdate)", conn);
                cmd.Parameters.Add(new SqlParameter("@gameid", GameID));
                cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                cmd.Parameters.Add(new SqlParameter("@vercode2", VerCode));
                cmd.Parameters.Add(new SqlParameter("@starttime", stime));
                cmd.Parameters.Add(new SqlParameter("@d1", stime));
                cmd.Parameters.Add(new SqlParameter("@d2", dt.AddSeconds(30).ToString("yyyyMMddHHmmss")));
                cmd.ExecuteNonQuery();

                SaveLog("Start", "update gamer set GameID = '" + GameID + "',checkin='Y' where GameID='' and vercode='" + VerCode + "' and UID in (select top 10 UID from gamer where GameID='' and vercode='" + VerCode + "' and starttime='" + stime + "' or createdate between '" + stime + "' and '" + dt.AddSeconds(30).ToString("yyyyMMddHHmmss") + "' order by createdate)", Setting);

                cmd = new SqlCommand("delete gamer where GameID<>@GameID and vercode = @vercode", conn);
                cmd.Parameters.Add(new SqlParameter("@gameid", GameID));
                cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                cmd.ExecuteNonQuery();

                SaveLog("Start", "delete gamer where GameID<>'" + GameID + "' and vercode = '" + VerCode + "'", Setting);
            }

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select count(*) from gamer where vercode=@vercode and gameid=@gameid and checkin='Y'", conn);                
                cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                cmd.Parameters.Add(new SqlParameter("@gameid", GameID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Num = reader[0].ToString();
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }

                SaveLog("Start", "select count(*) from gamer where vercode='" + VerCode + "' and gameid='" + GameID + "' and checkin='Y'", Setting);

            }

            CData = new Game.GameClassData()
            {
                GameID = GameID,
                Gamer = Num,
                Question = GameclassList
            };

            root = new Game.Start()
            {
                RspnCode = "0",
                Item = CData,
                RspnMsg = ""
            };
            SaveLog("Start", JsonConvert.SerializeObject(root), Setting);
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 取得題目
        private String GetQuestion(String VerCode, String CID, String setting)
        {
            
            Game.GetQuestion root = new Game.GetQuestion();
            Game.GameData GData = new Game.GameData();
            List<Game.Question> QItem = new List<Game.Question>();

            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select top 5 id,question,answer,item1,item2,qtype,qlink,qtime from game_QA where class_id=@CID order by newid()", conn);
                cmd.Parameters.Add(new SqlParameter("@CID", CID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            String[] AList = new String[]{
                                reader[2].ToString(),reader[3].ToString(),reader[4].ToString()
                            };
                            
                            Game.Question QList = new Game.Question()
                            {
                                QID = reader[0].ToString(),
                                Q = reader[1].ToString(),
                                Answer = AList,
                                Type = reader[5].ToString(),
                                Link = reader[6].ToString(),
                                Time = reader[7].ToString()
                            };
                            QItem.Add(QList);
                        }
                    }
                }
                finally
                {
                    reader.Close();
                }

                GData = new Game.GameData()
                {
                    Question = QItem
                };

                root = new Game.GetQuestion()
                {
                    RspnCode = "0",
                    Item = GData,
                    RspnMsg = ""
                };
            }
            SaveLog("GetQuestion", JsonConvert.SerializeObject(root), setting);
            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 遊戲報名結果
        private String Signup(String VerCode, String UID, String Setting)
        {
            String Checkin = "N";

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select checkin from gamer where UID=@UID and vercode=@vercode", conn);
                cmd.Parameters.Add(new SqlParameter("@UID", UID));
                cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read()) 
                        {
                            Checkin = reader[0].ToString();
                        }                        
                    }
                }
                finally { reader.Close(); }
            }

            SaveLog("Signup", "select checkin from gamer where UID='" + UID + "' and vercode='" + VerCode + "'", Setting);            

            Game.result rs = new Game.result()
            {
                Checkin = Checkin
            };

            String RespCode = "0";
            String RespMsg = "";

            Game.Check root = new Game.Check()
            {
                RspnCode = RespCode,
                Item = rs,
                RspnMsg = RespMsg
            };

            SaveLog("Signup", JsonConvert.SerializeObject(root), Setting);

            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 出題目
        private String Question(String VerCode, String GameID, String QID, String Answer, String Answertime, String Setting)
        {
            DateTime dt = new DateTime(Convert.ToInt16(Answertime.Substring(0, 4)), Convert.ToInt16(Answertime.Substring(4, 2)), Convert.ToInt16(Answertime.Substring(6, 2)), Convert.ToInt16(Answertime.Substring(8, 2)), Convert.ToInt16(Answertime.Substring(10, 2)), Convert.ToInt16(Answertime.Substring(12, 2)));

            

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("update gamer set QuestionID=@QuestionID,QAnswer=@QAnswer,Answertime=@Answertime,UserAnswer='' where checkin='Y' and gameid=@gameid and vercode=@vercode", conn);
                cmd.Parameters.Add(new SqlParameter("@QuestionID", QID));
                cmd.Parameters.Add(new SqlParameter("@QAnswer", Answer));
                cmd.Parameters.Add(new SqlParameter("@Answertime", dt.AddSeconds(10).ToString("yyyyMMddHHmmss")));
                cmd.Parameters.Add(new SqlParameter("@gameid", GameID));
                cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                cmd.ExecuteNonQuery();
            
                SaveLog("Question", "update gamer set QuestionID='" + QID + "',QAnswer='" + Answer + "',Answertime='" + dt.AddSeconds(5).ToString("yyyyMMddHHmmss") + "',UserAnswer='' where checkin='Y' and gameid='" + GameID + "' and vercode='" + VerCode + "'", Setting);
            }

            String RespCode = "0";
            String RespMsg = "";

            return ErrorMsg(RespCode, RespMsg);
        }
        #endregion

        #region 詢問答題開始時間
        private String AnswerTime(String VerCode, String UID, String Setting)
        {
            String StartTime = "";
            String EndTime = "";
            String GID = "";

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select AnswerTime,GameID from gamer where UID=@UID and vercode=@vercode", conn);
                cmd.Parameters.Add(new SqlParameter("@UID", UID));
                cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read()) {
                            EndTime = reader[0].ToString();
                            GID = reader[1].ToString();
                        }                        
                    }
                }
                finally { reader.Close(); }
            }

            SaveLog("AnswerTime", "select Answertime,GameID from gamer where UID='" + UID + "' and vercode='" + VerCode + "'", Setting);
            
            if (EndTime != "") {
                DateTime dt = new DateTime(Convert.ToInt16(EndTime.Substring(0, 4)), Convert.ToInt16(EndTime.Substring(4, 2)), Convert.ToInt16(EndTime.Substring(6, 2)), Convert.ToInt16(EndTime.Substring(8, 2)), Convert.ToInt16(EndTime.Substring(10, 2)), Convert.ToInt16(EndTime.Substring(12, 2)));
                StartTime = dt.AddSeconds(-10).ToString("yyyyMMddHHmmss");
            }            

            Game.TimeData td = new Game.TimeData() { 
                starttime = StartTime,
                endtime = EndTime,
                GameID = GID
            };

            String RespCode = "0";
            String RespMsg = "";

            Game.Answertime root = new Game.Answertime()
            {
                RspnCode = RespCode,
                Item = td,
                RspnMsg = RespMsg
            };

            SaveLog("AnswerTime", JsonConvert.SerializeObject(root), Setting);

            return JsonConvert.SerializeObject(root);
        }
        #endregion

        #region 傳答案
        private String SendAnswer(String VerCode, String GameID, String Answertime, String UID, String Answer, String Setting)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("update gamer set UserAnswer=@UserAnswer where checkin='Y' and gameid=@gameid and UID=@UID and Answertime=@Answertime", conn);
                cmd.Parameters.Add(new SqlParameter("@UserAnswer", Answer));
                cmd.Parameters.Add(new SqlParameter("@gameid", GameID));
                cmd.Parameters.Add(new SqlParameter("@UID", UID));
                cmd.Parameters.Add(new SqlParameter("@Answertime", Answertime));
                cmd.ExecuteNonQuery();
            }

            SaveLog("AnswerTime", "update gamer set UserAnswer='" + Answer + "' where checkin='Y' and gameid='" + GameID + "' and UID='" + UID + "' and Answertime='" + Answertime + "'", Setting);

            String RespCode = "0";
            String RespMsg = "";

            return ErrorMsg(RespCode, RespMsg);
        }
        #endregion

        #region 公布答案
        private String Check(String VerCode, String GameID, String UID, String Answertime, String Setting)
        {
            String Checkin = "N";

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select * from gamer where QAnswer=UserAnswer and checkin='Y' and UID=@UID and Answertime=@Answertime and GameID=@GameID and vercode=@vercode", conn);
                cmd.Parameters.Add(new SqlParameter("@UID", UID));
                cmd.Parameters.Add(new SqlParameter("@Answertime", Answertime));
                cmd.Parameters.Add(new SqlParameter("@GameID", GameID));
                cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    SaveLog("Check", "select * from gamer where QAnswer=UserAnswer and checkin='Y' and UID='" + UID + "' and Answertime='" + Answertime + "' and GameID='" + GameID + "' and vercode='" + VerCode + "'", Setting);

                    if (!reader.HasRows)
                    {
                        using (SqlConnection conn2 = new SqlConnection(Setting))
                        {
                            conn2.Open();
                            SqlCommand cmd2;

                            cmd2 = new SqlCommand("update gamer set checkin='N' where UID=@UID and GameID=@GameID and vercode=@vercode", conn2);
                            cmd2.Parameters.Add(new SqlParameter("@UID", UID));                            
                            cmd2.Parameters.Add(new SqlParameter("@GameID", GameID));
                            cmd2.Parameters.Add(new SqlParameter("@vercode", VerCode));
                            cmd2.ExecuteNonQuery();

                            SaveLog("Check", "update gamer set checkin='N' where UID='" + UID + "' and GameID='" + GameID + "' and vercode='" + VerCode + "'", Setting);

                        }
                        Checkin = "N";
                    }
                    else
                    {
                        using (SqlConnection conn2 = new SqlConnection(Setting))
                        {
                            conn2.Open();
                            SqlCommand cmd2;

                            cmd2 = new SqlCommand("update gamer set round=round+1 where UID=@UID and GameID=@GameID and vercode=@vercode", conn2);
                            cmd2.Parameters.Add(new SqlParameter("@UID", UID));
                            cmd2.Parameters.Add(new SqlParameter("@GameID", GameID));
                            cmd2.Parameters.Add(new SqlParameter("@vercode", VerCode));
                            cmd2.ExecuteNonQuery();

                            SaveLog("Check", "update gamer set round=round+1 where UID='" + UID + "' and GameID='" + GameID + "' and vercode='" + VerCode + "'", Setting);

                        }
                        Checkin = "Y";
                    }
                }
                finally { reader.Close(); }
            }

            Game.result rs = new Game.result()
            {
                Checkin = Checkin
            };

            String RespCode = "0";
            String RespMsg = "";

            Game.Check root = new Game.Check()
            {
                RspnCode = RespCode,
                Item = rs,
                RspnMsg = RespMsg
            };

            SaveLog("Check", JsonConvert.SerializeObject(root), Setting);

            return JsonConvert.SerializeObject(root);
        }
        #endregion        

        #region 查詢闖關成功玩家
        private String Win(String VerCode, String GameID, String Setting, String QuestionNum)
        {

            if (Convert.ToInt16(QuestionNum) > 0) 
            {
                using (SqlConnection conn2 = new SqlConnection(Setting))
                {
                    conn2.Open();
                    SqlCommand cmd2;

                    cmd2 = new SqlCommand("update gamer set checkin='N' where round<@round and GameID=@GameID and vercode=@vercode", conn2);
                    cmd2.Parameters.Add(new SqlParameter("@round", QuestionNum));
                    cmd2.Parameters.Add(new SqlParameter("@GameID", GameID));
                    cmd2.Parameters.Add(new SqlParameter("@vercode", VerCode));
                    cmd2.ExecuteNonQuery();

                    SaveLog("Win", "update gamer set checkin='N' where round<'" + QuestionNum + "' and GameID='" + GameID + "' and vercode='" + VerCode + "'", Setting);

                }
            }


            Game.WinGamer root = new Game.WinGamer();
            List<Game.GamerData> GData = new List<Game.GamerData>();

            String UID = "";
            String Name = "";
            String Photo = "";

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select UID,name,photo from gamer where GameID=@gameid and vercode=@vercode and checkin='Y'", conn);
                cmd.Parameters.Add(new SqlParameter("@gameid", GameID));
                cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));  
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    SaveLog("Win", "select UID,name,photo from gamer where GameID='" + GameID + "' and vercode='" + VerCode + "' and checkin='Y'", Setting);

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            UID = reader[0].ToString();
                            Name = reader[1].ToString();
                            Photo = reader[2].ToString();

                            Game.GamerData GDList = new Game.GamerData()
                            {
                                UID = UID,
                                name = Name,
                                photo = Photo
                            };

                            GData.Add(GDList);
                        }
                    }
                }
                finally { reader.Close(); }
            }

            root.RspnCode = "0";
            root.Item = GData;
            root.RspnMsg = "";

            SaveLog("Win", JsonConvert.SerializeObject(root), Setting);
            
            return JsonConvert.SerializeObject(root);
        }
        #endregion
        
        #region 完全答對獲得紅利點數
        private String Gift(String VerCode, String UID, String GameID, String Setting, String EZSetting)
        {
            String Bonus = "";

            #region search mem_id
            String MemID = "";
            using (SqlConnection conn = new SqlConnection(EZSetting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select mem_id from cust where uid=@uid", conn);
                cmd.Parameters.Add(new SqlParameter("@uid", UID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            MemID = reader[0].ToString();
                        }
                    }
                }
                finally { reader.Close(); }
            }
            #endregion

            if (MemID != "") 
            {
                using (SqlConnection conn2 = new SqlConnection(EZSetting))
                {
                    conn2.Open();
                    SqlCommand cmd2;

                    cmd2 = new SqlCommand("select * from [bonus] where [bonus_memo] like @bonus_memo and [mem_id]=@mem_id", conn2);
                    cmd2.Parameters.Add(new SqlParameter("@bonus_memo", "%闖關成功紅利獎勵"));
                    cmd2.Parameters.Add(new SqlParameter("@mem_id", MemID));
                    SqlDataReader reader2 = cmd2.ExecuteReader();
                    try
                    {
                        if (!reader2.HasRows)
                        {
                            #region 一個會員只能獲得一次遊戲闖關成功的紅利
                            using (SqlConnection conn = new SqlConnection(EZSetting))
                            {
                                conn.Open();
                                SqlCommand cmd;

                                cmd = new SqlCommand("select GameBonus from head", conn);
                                SqlDataReader reader = cmd.ExecuteReader();
                                try
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            Bonus = reader[0].ToString();
                                        }
                                    }
                                }
                                finally { reader.Close(); }
                            }

                            #region update cust.bonus_total
                            using (SqlConnection conn = new SqlConnection(EZSetting))
                            {
                                conn.Open();
                                SqlCommand cmd;

                                cmd = new SqlCommand("update cust set bonus_total = bonus_total + @bonus where uid = @uid", conn);
                                cmd.Parameters.Add(new SqlParameter("@bonus", Bonus));
                                cmd.Parameters.Add(new SqlParameter("@uid", UID));
                                cmd.ExecuteNonQuery();

                                SaveLog("Gift", "update cust set bonus_total = bonus_total + " + Bonus + " where uid = '" + UID + "'", Setting);
                            }
                            #endregion

                            #region insert bonus log
                            using (SqlConnection conn = new SqlConnection(EZSetting))
                            {
                                conn.Open();
                                SqlCommand cmd;

                                cmd = new SqlCommand("INSERT INTO [bonus] ([bonus_memo],[mem_id],[date],[bonus_add],[bonus_spend]) VALUES (@bonus_memo,@mem_id,replace(CONVERT([varchar](10),getdate(),(120)),'-','/'),@bonus_add,'0')", conn);
                                cmd.Parameters.Add(new SqlParameter("@bonus_memo", "遊戲(ID：" + GameID + "/機碼：" + VerCode + ")闖關成功紅利獎勵"));
                                cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                                cmd.Parameters.Add(new SqlParameter("@bonus_add", Bonus));
                                cmd.ExecuteNonQuery();
                            }
                            #endregion

                            #endregion
                        }
                    }
                    finally { reader2.Close(); }
                }
            }
            

            String RespCode = "0";
            String RespMsg = "";

            Game.bonus GB = new Game.bonus()
            {
                Giftbonus = Bonus
            };

            Game.Gift root = new Game.Gift()
            {
                RspnCode = RespCode,
                Item = GB,
                RspnMsg = RespMsg
            };


            SaveLog("Gift", JsonConvert.SerializeObject(root), Setting);

            return JsonConvert.SerializeObject(root);
        }
        #endregion        

        #region 分享FB訊息獲得紅利
        private string Share(String VerCode, String UID, String GameID, String Setting, String EZSetting)
        {

            String ClientID = "";
            String client_secret = "";
            

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select top 1 Game_AppID,Game_AppSecret from head", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ClientID = reader[0].ToString();
                            client_secret = reader[1].ToString();
                        }
                    }
                }
                finally { reader.Close(); }
            }
            /*
            WebClient wc = new WebClient();
            //因為access_token會有過期失效問題，所以每次都重新取得access_token
            string wc_result = wc.DownloadString("https://graph.facebook.com/oauth/access_token?client_id=" + ClientID + "&client_secret=" + client_secret + "&grant_type=client_credentials");
            string access_token = wc_result.Split('=')[1];

            FacebookClient client = new FacebookClient(access_token);

            try {
                client.Post(UID + "/feed",
                    new
                    {
                        message = DateTime.Now.ToString(),
                        caption = "標題",
                        description = "描述",
                        name = "名稱",
                        picture = "http://develop.ezsale.tw/upload/contImg/0403260012.png",
                        link = "http://develop.ezsale.tw"
                    }
                );

                SaveLog("Share", "分享FB", Setting);
            }
            catch (FacebookOAuthException ex)
            {
                SaveLog("Share", ex.Message, Setting);
            }
            */
            #region 取得紅利點數
            String Bonus = "";
            using (SqlConnection conn = new SqlConnection(EZSetting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select ShareBonus from head", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Bonus = reader[0].ToString();
                        }
                    }
                }
                finally { reader.Close(); }
            }
            #endregion            
            
            #region update cust.bonus_total
            using (SqlConnection conn = new SqlConnection(EZSetting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("update cust set bonus_total = bonus_total + @bonus where uid = @uid", conn);
                cmd.Parameters.Add(new SqlParameter("@bonus", Bonus));
                cmd.Parameters.Add(new SqlParameter("@uid", UID));
                cmd.ExecuteNonQuery();

                SaveLog("Share", "update cust set bonus_total = bonus_total + " + Bonus + " where uid = '" + UID + "'", Setting);
            }
            #endregion

            #region search mem_id
            String MemID = "";
            using (SqlConnection conn = new SqlConnection(EZSetting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select mem_id from cust where uid=@uid", conn);
                cmd.Parameters.Add(new SqlParameter("@uid", UID));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            MemID = reader[0].ToString();
                        }
                    }
                }
                finally { reader.Close(); }
            }
            #endregion

            #region insert bonus log
            using (SqlConnection conn = new SqlConnection(EZSetting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("INSERT INTO [bonus] ([bonus_memo],[mem_id],[date],[bonus_add],[bonus_spend]) VALUES (@bonus_memo,@mem_id,replace(CONVERT([varchar](10),getdate(),(120)),'-','/'),@bonus_add,'0')", conn);
                cmd.Parameters.Add(new SqlParameter("@bonus_memo", "遊戲(ID：" + GameID + ")分享獎勵"));
                cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                cmd.Parameters.Add(new SqlParameter("@bonus_add", Bonus));
                cmd.ExecuteNonQuery();
            }
            #endregion

            String RespCode = "0";
            String RespMsg = "";

            Game.bonus GB = new Game.bonus()
            {
                Giftbonus = Bonus
            };

            Game.Gift root = new Game.Gift()
            {
                RspnCode = RespCode,
                Item = GB,
                RspnMsg = RespMsg
            };

            SaveLog("Share", JsonConvert.SerializeObject(root), Setting);

            return JsonConvert.SerializeObject(root);
        }
        #endregion
        
        #region 取得Orgname連結字串
        private String GetSetting(String OrgName)
        {
            return "data source=" + ConfigurationManager.AppSettings.Get("MemberApiUrl") + ";user id=i_template_" + OrgName + "; password=i_template_" + OrgName + "1234; database=template_" + OrgName;
        }
        #endregion    

        #region 回傳error字串
        private String ErrorMsg(String RspnCode, String RspnMsg)
        {

            Game.VoidReturn root = new Game.VoidReturn();
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

        #region 儲存會員資料
        private void SaveCust(String Setting, String UID, String Name, String Photo, String Email, String Gender)
        {

            String MemID = "";
            String Pwd = DateTime.Now.ToString("yyyyMMddHHmmss");
            String Birth = "";
            String Bonus = "0";

            switch (Gender)
            {
                case "female":
                    Gender = "2";
                    break;
                case "male":
                    Gender = "1";
                    break;
                default:
                    Gender = "1";
                    break;
            }
            String Str_Sql = "";

            #region 判斷是否已是會員
            bool IsMem = false;
            
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select mem_id from cust where id=@id", conn);
                cmd.Parameters.Add(new SqlParameter("@id", Email));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        IsMem = true;
                    }
                }
                finally { reader.Close(); }
            }
            #endregion

            if (!IsMem)
            {
                #region 取得MemID
                Str_Sql = "select REPLICATE('0',6-LEN(isnull(MAX(mem_id),'0')+1)) + RTRIM(CAST(isnull(MAX(mem_id),'0')+1 AS CHAR)) from cust";
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand(Str_Sql, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                MemID = reader[0].ToString();
                            }
                        }
                    }
                    finally { reader.Close(); }
                }
                #endregion

                #region insert into cust
                Str_Sql = "insert into Cust (mem_id,id,pwd,ch_name,sex,birth,email,vip,bonus_total,chk,UID,photo,crm_date,logintime)";
                Str_Sql += " values (@mem_id,@id,sys.fn_VarBinToHexStr(hashbytes('MD5', convert(nvarchar,@pwd))),@ch_name,@sex,@birth,@email,'1',@bonus_total,'Y',@UID,@photo,replace(replace(CONVERT([varchar](256),getdate(),(120)),'-',''),':',''),getdate())";
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand(Str_Sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                    cmd.Parameters.Add(new SqlParameter("@id", Email));
                    cmd.Parameters.Add(new SqlParameter("@pwd", Pwd));
                    cmd.Parameters.Add(new SqlParameter("@ch_name", Name));
                    cmd.Parameters.Add(new SqlParameter("@sex", Gender));
                    cmd.Parameters.Add(new SqlParameter("@birth", Birth));
                    cmd.Parameters.Add(new SqlParameter("@email", Email));
                    cmd.Parameters.Add(new SqlParameter("@bonus_total", "0"));
                    cmd.Parameters.Add(new SqlParameter("@UID", UID));
                    cmd.Parameters.Add(new SqlParameter("@photo", Photo));
                    cmd.ExecuteNonQuery();
                }
                #endregion

                #region 取得加入會員紅利
                Str_Sql = "select bonus_first from head";
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand(Str_Sql, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Bonus = reader[0].ToString();
                            }
                        }
                    }
                    finally { reader.Close(); }
                }
                #endregion

                #region 紀錄紅利LOG
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "sp_CheckMail";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;

                    cmd.Parameters.Add(new SqlParameter("@mem_id", MemID));
                    cmd.Parameters.Add(new SqlParameter("@bonus", Bonus));
                    cmd.ExecuteNonQuery();
                }
                #endregion
            }
        }
        #endregion

        #region 儲存資料到玩家暫存區
        private void GameSave(String Setting, String UID, String Name, String Photo, String VerCode)
        {
            
            bool isGamer = false;

            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("select * from gamer where UID = @UID and VerCode = @vercode and GameID='' and round=0 and checkin='S'", conn);
                cmd.Parameters.Add(new SqlParameter("@UID", UID));
                cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    if (reader.HasRows)
                    {
                        isGamer = true;
                    }
                }
                finally { reader.Close(); }
            }


            if (!isGamer)
            {
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand("insert into gamer(UID,name,photo,vercode) values (@UID,@name,@photo,@vercode)", conn);
                    cmd.Parameters.Add(new SqlParameter("@UID", UID));
                    cmd.Parameters.Add(new SqlParameter("@name", Name));
                    cmd.Parameters.Add(new SqlParameter("@photo", Photo));
                    cmd.Parameters.Add(new SqlParameter("@vercode", VerCode));
                    cmd.ExecuteNonQuery();
                }
            }
            else {
                using (SqlConnection conn = new SqlConnection(Setting))
                {
                    conn.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand("update gamer set createdate =(replace(replace(replace(CONVERT([varchar](256),getdate(),(120)),'-',''),':',''),' ','')) where UID=@UID and VerCode=@VerCode and GameID='' and round=0 and checkin='N'", conn);
                    cmd.Parameters.Add(new SqlParameter("@UID", UID));
                    cmd.Parameters.Add(new SqlParameter("@VerCode", VerCode));
                    cmd.ExecuteNonQuery();
                }
            }
            
        }
        #endregion

        #region 儲存USERLOG
        private void SaveLog(String APIName, String Detail, String Setting)
        {
            using (SqlConnection conn = new SqlConnection(Setting))
            {
                conn.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("insert into userlog (user_id,detail,job_name) values ('gamer',@detail,@jobname)", conn);
                cmd.Parameters.Add(new SqlParameter("@detail", Detail));
                cmd.Parameters.Add(new SqlParameter("@jobname", APIName));
                cmd.ExecuteNonQuery();
            }
        }
        #endregion
    }
}