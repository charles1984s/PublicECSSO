using System;
using System.Collections.Generic;
using System.Web;

namespace ECSSO.Library
{
    public class Game
    {
        #region API - 玩家FB登入
        public class FBData 
        {
            public string UID { get; set; }
            public string name { get; set; }
            public string photo { get; set; }
        }
        public class Login
        {
            public string RspnCode { get; set; }
            public FBData Item { get; set; }
            public string RspnMsg { get; set; }
        }
        #endregion

        #region API - 出題目 / 傳答案 / 分享FB訊息獲得抽獎機會 / ErrorMsg
        public class VoidReturn
        {
            public string RspnCode { get; set; }
            public string RspnMsg { get; set; }
        }
        #endregion

        #region API -詢問玩家等待數
        public class Num
        {
            public string gamer { get; set; }
        }

        public class GamerNum
        {
            public string RspnCode { get; set; }
            public Num Item { get; set; }
            public string RspnMsg { get; set; }
        }
        #endregion

        #region API -詢問玩家等待數(2)
        public class STime
        {
            public string time { get; set; }
        }

        public class GamerSTime
        {
            public string RspnCode { get; set; }
            public STime Item { get; set; }
            public string RspnMsg { get; set; }
        }
        #endregion

        #region API -查詢闖關成功玩家
        public class GamerData
        {
            public string UID { get; set; }
            public string name { get; set; }
            public string photo { get; set; }
        }

        public class WinGamer
        {
            public string RspnCode { get; set; }
            public List<GamerData> Item { get; set; }
            public string RspnMsg { get; set; }
        }
        #endregion

        #region API -遊戲開始
        public class GameClass
        {
            public string CID { get; set; }
            public string Title { get; set; }
        }

        public class GameClassData
        {
            public string GameID { get; set; }
            public string Gamer { get; set; }
            public List<GameClass> Question { get; set; }
        }

        public class Start
        {
            public string RspnCode { get; set; }
            public GameClassData Item { get; set; }
            public string RspnMsg { get; set; }
        }
        #endregion

        #region API-取得題目分類
        public class GetQuestionClass
        {
            public string RspnCode { get; set; }
            public List<QuestionClass> QuestionClass { get; set; }
            public string RspnMsg { get; set; }
        }
        public class QuestionClass
        {
            public string CID { get; set; }
            public string Title { get; set; }
        }
        #endregion

        #region API –取得題目

        public class Question
        {
            public string QID { get; set; }
            public string Q { get; set; }
            public string[] Answer { get; set; }
            public string Type { get; set; }
            public string Link { get; set; }
            public string Time { get; set; }
        }

        public class GameData
        {            
            public List<Question> Question { get; set; }
        }

        public class GetQuestion
        {
            public string RspnCode { get; set; }
            public GameData Item { get; set; }
            public string RspnMsg { get; set; }
        }
        #endregion

        #region API -公布答案 / 遊戲報名結果
        public class result
        {
            public string Checkin { get; set; }
        }

        public class Check
        {
            public string RspnCode { get; set; }
            public result Item { get; set; }
            public string RspnMsg { get; set; }
        }
        #endregion

        #region API –完全答對獲得紅利點數
        public class bonus
        {
            public string Giftbonus { get; set; }
        }

        public class Gift
        {
            public string RspnCode { get; set; }
            public bonus Item { get; set; }
            public string RspnMsg { get; set; }
        }
        #endregion

        #region 詢問答題開始時間
        public class TimeData
        {
            public string starttime { get; set; }
            public string endtime { get; set; }
            public string GameID { get; set; }
        }

        public class Answertime
        {
            public string RspnCode { get; set; }
            public TimeData Item { get; set; }
            public string RspnMsg { get; set; }
        }
        #endregion
    }
}