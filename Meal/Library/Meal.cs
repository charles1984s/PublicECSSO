using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Meal.Library
{
    public class Meal
    {
        public class FormData
        {
            public string SubID { get; set; }
        }

        public class Other
        {
            public string id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string GS1Code { get; set; }
        }

        public class Memo
        {
            public string title { get; set; }
            public List<string> item { get; set; }
        }

        public class Item2
        {
            public string id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string img { get; set; }
            public string cont { get; set; }
            public string GS1Code { get; set; }
            public List<Other> other { get; set; }
            public List<Memo> Memo { get; set; }
        }

        public class Menu
        {
            public string id { get; set; }
            public string Title { get; set; }
            public string Discount { get; set; }
            public string ChoiceNum { get; set; }
            public string SerNo { get; set; }
            public string GS1Code { get; set; }
            public List<Item2> Item { get; set; }
        }

        public class Item
        {
            public string id { get; set; }
            public string title { get; set; }
            public string value1 { get; set; }
            public string value2 { get; set; }
            public string img { get; set; }
            public string cont { get; set; }
            public List<Menu> Menus { get; set; }
        }

        public class SingleMenu
        {
            public string Subid { get; set; }
            public string id { get; set; }
            public string title { get; set; }
            public string value1 { get; set; }
            public string value2 { get; set; }
            public string img { get; set; }
            public string cont { get; set; }
            public List<Other> other { get; set; }
            public List<Memo> Memo { get; set; }
        }

        public class Single
        {
            public string title { get; set; }
            public string img { get; set; }
            public List<SingleMenu> SingleMenu { get; set; }
        }

        public class Items
        {
            public List<Item> combo { get; set; }
            public List<Single> Single { get; set; }
        }

        public class MealList
        {
            public string id { get; set; }
            public string title { get; set; }
            public string img { get; set; }
            public string type { get; set; }
        }
        #region ErrorMsg
        public class VoidReturn
        {
            public string RspnCode { get; set; }
            public string RspnMsg { get; set; }
        }
        #endregion

        public class Table
        {
            public string id { get; set; }
            public string title { get; set; }
            public string state { get; set; }
            public string seatingtime { get; set; }
            public string number { get; set; }
            public string CancelMeal { get; set; }
            public string Calltime { get; set; }
        }

        public class Tables
        {
            public List<Table> Table { get; set; }
        }
    }
}