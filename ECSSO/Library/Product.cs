using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    public class Product
    {
        public class ErrorObject
        {
            public string RspnCode { get; set; }
            public string RspnMsg { get; set; }
        }

        public class InputData
        {
            public string AuID { get; set; }
            public string SubID { get; set; }
            public string ID { get; set; }
            public String StartDate { get; set; }
            public String EndDate { get; set; }
            public Range Range { get; set; }
        }


        public class Range
        {
            public String From { get; set; }
            public String GetCount { get; set; }

        }

        public class ProductAu 
        {
            public String AuID { get; set; }
            public String Title { get; set; }
        }

        public class ProductSub 
        {
            public String SubID { get; set; }
            public String Title { get; set; }
            public String BannerImg { get; set; }
        }

        public class ProductList
        {
            public String ID { get; set; }
            public String Title { get; set; }
            public String StockNo { get; set; }
            public String ColorTitle { get; set; }
            public String SizeTitle { get; set; }
            public String Img1 { get; set; }
            public String Value1 { get; set; }
            public String Value2 { get; set; }
            public String Value3 { get; set; }
            public int ColorID { get; set; }
            public int SizeID { get; set; }
        }

        public class ProductDetail
        {
            public String SubID { get; set; }
            public String ID { get; set; }
            public String Title { get; set; }
            public String Img1 { get; set; }
            public String Img2 { get; set; }
            public String Img3 { get; set; }
            public String Value1 { get; set; }
            public String Value2 { get; set; }
            public String Value3 { get; set; }
            public String item1 { get; set; }
            public String item2 { get; set; }
            public String item3 { get; set; }
            public String item4 { get; set; }
            public String SalesQty { get; set; }
            public List<ProductStock> Stock { get; set; }
        }

        public class ProductStock 
        {
            public String SizeID { get; set; }
            public String SizeName { get; set; }
            public String ColorID { get; set; }
            public String ColorName { get; set; }
            public String Num { get; set; }
        }
    }
}