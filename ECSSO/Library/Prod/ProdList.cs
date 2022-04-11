using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ECSSO.Library;

namespace ECSSO.Library.Prod
{
    public class ProdList : responseJson
    {
        public List<Product.ProductList> list { get; set; }
        public int TotalPage { get; set; }
        public int CurrentPage { get; set; }
    }
    public class ProdAuList : responseJson
    {
        public List<Product.ProductAu> list { get; set; }
    }
    public class ProdSubList : responseJson
    {
        public List<Product.ProductSub> list { get; set; }
    }
}