using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using WebAPI.Models;
using System.Web.Http.Cors;
using System.Runtime.Serialization;

namespace WebAPI.Controllers
{
    public class ProductsController : ApiController
    {
        public class RecipeInformation
        {
            public int id { get; set; }
            public string name { get; set; }
        }
        public Product Get(int id)
        {
            Product p = new Product();
            p.ID = 1;
            p.Name = "測試";
            return p;
        }
        [System.Web.Http.HttpPost]
        public Product AjaxMethod(RecipeInformation recipeInformation)
        {
            Product p = new Product();
            p.ID = recipeInformation.id;
            p.Name = "測試" + recipeInformation.id;
            p.Category = recipeInformation.name;
            return p;
        }
    }
}
