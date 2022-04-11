using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    public class PostForm
    {
        public class Condition
        {
            public string ID { get; set; }
            public string Parent { get; set; }
            public string Visible { get; set; }
            public string Stocks { get; set; }
        }

        public class Range
        {
            public string from { get; set; }
            public string GetCount { get; set; }
        }

        public class Form
        {
            public string OrgName { get; set; }
            public string Type { get; set; }
            public Condition Condition { get; set; }
            public Range Range { get; set; }
            public List<string> OrderBy { get; set; }
        }

        public class GamePost
        {
            public string GameID { get; set; }
            public string QID { get; set; }
            public string Answer { get; set; }
            public string name { get; set; }
            public string photo { get; set; }
            public string birth { get; set; }
            public string email { get; set; }
            public string UID { get; set; }
            public string time { get; set; }
            public string CID { get; set; }
            public string Gender { get; set; }
            public string Answertime { get; set; }
            public string stime { get; set; }
            public string QuestionNum { get; set; }
        }

        public class BookingPost
        {
            public string VerCode { get; set; }
            public string StoreID { get; set; }
            public string SeatID { get; set; }
            public string Date { get; set; }
            public string Hour { get; set; }
            public string Tel { get; set; }
            public string Time { get; set; }
            //public string Num { get; set; }   20151118 用不到先拿掉
            public string RegisterType { get; set; }
            public string ArrivalTime { get; set; }
            public string Pno { get; set; }     //20160407增加現場點餐序號
        }

        public class Account
        {
            public string CID { get; set; }
            public string StoreID { get; set; }
            public string ID { get; set; }
            public string Pwd { get; set; }
        }
        public class HelpPage
        {
            public string AreaID { get; set; }
            public string funcID { get; set; }
            public string PageID { get; set; }
            public string StepID { get; set; }
        }
    }
}