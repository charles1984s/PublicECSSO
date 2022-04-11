using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    public class Products
    {
        
        public class RootObject
        {
            public String RspnCode { get; set; }
            public List<ProductData> ProductDatas { get; set; }
            public String RspnMsg { get; set; }
            public String Token { get; set; }
        }

        public class ProductData
        {
            public String AuID { get; set; }
            public String AuTitle { get; set; }
            public String SubID { get; set; }
            public String SubTitle { get; set; }
            public String ID { get; set; }
            public String Title { get; set; }
            public String Value1 { get; set; }
            public String Value2 { get; set; }
            public String Value3 { get; set; }
            public String Item1 { get; set; }
            public String Item2 { get; set; }
            public String Item3 { get; set; }
            public String Item4 { get; set; }
            public String Img1 { get; set; }
            public String Img2 { get; set; }
            public String Img3 { get; set; }
            public String Virtual { get; set; }
            public String URL { get; set; }
            public List<MenuCont> MenuConts { get; set; }
            public List<Stocks> Stock { get; set; }
        }

        public class MenuCont
        {
            public String Img { get; set; }
            public String Cont { get; set; }
            public String Title { get; set; }
            public String ImgAlign { get; set; }
        }

        public class Stocks {
            public String SpecTitle1 { get; set; }
            public String SpecTitle2 { get; set; }
            public String Num { get; set; }            
        }
    }
}