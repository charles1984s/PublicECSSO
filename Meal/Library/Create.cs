using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Meal.Library
{
    public class Create
    {
        public class Store 
        {
            public String ID { get; set; }
            public String VerCode { get; set; }
            public String Title { get; set; }
            public String Display { get; set; }
            public String Cdate { get; set; }
            public String Edate { get; set; }
            public String[] DelID { get; set; }
        }

        public class StoreTable 
        {
            public String ID { get; set; }
            public String StoreID { get; set; }
            public String Title { get; set; }
            public String Num { get; set; }
            public String Stat { get; set; }
            public String SeatingTime { get; set; }
            public String Cdate { get; set; }
            public String Edate { get; set; }
            public String Calltime { get; set; }
            public String CancelMeal { get; set; }
            public String[] DelID { get; set; }
            public String amt { get; set; }
        }

        public class TableLog 
        {
            public String ID { get; set; }
            public String TableID { get; set; }
            public String VerCode { get; set; }
            public String Stat { get; set; }
            public String Fnum { get; set; }
            public String Mnum { get; set; }
            public String amt { get; set; }
            public String country { get; set; }
            public String StartTime { get; set; }
            public String EndTime { get; set; }
        }

        public class ProdAuthors
        {
            public String ID { get; set; }
            public String Title { get; set; }
            public String Display { get; set; }
            public String SerNo { get; set; }
            public String[] DelID { get; set; }
        }

        public class ProdSub
        {
            public String ID { get; set; }
            public String AuID { get; set; }
            public String Title { get; set; }
            public String Display { get; set; }
            public String SerNo { get; set; }
            public String BtnImg { get; set; }
            public String Cdate { get; set; }
            public String Edate { get; set; }
            public String[] ProdID { get; set; }
            public String[] DelID { get; set; }
        }

        public class Prod
        {
            public String ID { get; set; }
            public String SubID { get; set; }
            public String Title { get; set; }
            public String Display { get; set; }
            public String SerNo { get; set; }
            public String StartDate { get; set; }
            public String EndDate { get; set; }
            public String Img1 { get; set; }
            public String Item1 { get; set; }
            public String Item2 { get; set; }
            public String Item3 { get; set; }
            public String Item4 { get; set; }
            public String Value1 { get; set; }
            public String Value2 { get; set; }
            public String Value3 { get; set; }
            public String PrinterID { get; set; }
            public String Cdate { get; set; }
            public String Edate { get; set; }
            public String GS1Code { get; set; }
            public String[] DelID { get; set; }
            public List<Meal_Options_Sub> Options { get; set; }
        }

        public class Meal_Options_Sub
        {
            public String ID { get; set; }
            public String Title { get; set; }
            public String Type { get; set; }                //1=加價,2=免費客製
            public List<Meal_Options> Meal_Options { get; set; }
        }

        public class Meal_Options
        {
            public String ID { get; set; }
            public String Title { get; set; }
            public String Price { get; set; }
            public String Stat { get; set; }
        }

        public class SearchItem 
        {
            public List<Filter> Filter { get; set; }
            public List<OrderBy> OrderBy { get; set; }
            public Range Range { get; set; }
        }

        public class Filter 
        {
            public String ColumnName { get; set; }
            public String Logic { get; set; }
            public String Value { get; set; }
            public String Value2 { get; set; }
            public String[] Value3 { get; set; }
        }

        public class OrderBy
        {
            public String ColumnName { get; set; }
            public String Mode { get; set; }
        }

        public class Range
        {
            public String From { get; set; }
            public String GetCount { get; set; }
        }

        public class Meal
        {
            public string ID { get; set; }
            public string Title { get; set; }
            public string SerNo { get; set; }
            public string Value1 { get; set; }
            public string Value2 { get; set; }
            public string Value3 { get; set; }
            public string Img { get; set; }
            public string Cont { get; set; }
            public string DispOpt { get; set; }
            public string VerCode { get; set; }
            public string GS1Code { get; set; }
            public string Cdate { get; set; }
            public string Edate { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public List<Library.Meal.Menu> Menus { get; set; }
        }

        public class CancelMeal {
            public String OrderNo { get; set; }
            public String SerNo { get; set; }
            public String ProdName { get; set; }
            public String Amt { get; set; }
            public String Discription { get; set; }
            public String AlreadyCancel { get; set; }
            public String MealReady { get; set; }
        }

        public class Printer
        {
            public String ID { get; set; }
            public String Title { get; set; }
            public String PrinterName { get; set; }
        }
    }
}