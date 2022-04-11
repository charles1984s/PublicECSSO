using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.ShoppingCar
{
    public class ShoppingCarItem: ShoppingCarInput
    {
        public string title { get; set; }
        public double price { get; set; }
        public int bonus { get; set; }
        public int store { get; set; }
    }
}