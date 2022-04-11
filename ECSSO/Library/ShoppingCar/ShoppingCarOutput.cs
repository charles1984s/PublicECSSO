using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.ShoppingCar
{
    public class ShoppingCarOutput : responseJson
    {
        public Shoppingcar.OrderData Order { get; set; }
    }
}