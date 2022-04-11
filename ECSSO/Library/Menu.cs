using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    public class Menu
    {
        public class ErrorObject
        {
            public string RspnCode { get; set; }
            public string RspnMsg { get; set; }
        }

        public class MenuAu 
        {
            public string AuID { get; set; }
            public string Title { get; set; }
            public bool HasMenuSub { get; set; }
        }

        public class MenuSub
        {
            public string SubID { get; set; }
            public string Title { get; set; }
            public string Cont { get; set; }
            public string BannerImg { get; set; }
            public bool HasMenuSub { get; set; }
            public bool HasMenu { get; set; }
        }

        public class MenuList
        {
            public string ID { get; set; }
            public string Title { get; set; }
            public string NoteData { get; set; }
            public string Img1 { get; set; }
        }

        public class MenuDetail
        {
            public string ID { get; set; }
            public string Title { get; set; }
            public string NoteDate { get; set; }
            public string MediaLink { get; set; }
            public string Img1 { get; set; }
            public string Img2 { get; set; }
            public string Img3 { get; set; }
            public string Cont { get; set; }
            public List<MenuCont> MenuCont { get; set; }
        }

        public class MenuCont
        {
            public string Img { get; set; }
            public string Cont { get; set; }
        }
    }
}