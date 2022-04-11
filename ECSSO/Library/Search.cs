using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    #region input
    public class Search
    {
        public string AreaID { get; set; }  //
        public string ClassID { get; set; }  //搜尋類別，0: 吃喝、玩樂、1: 住宿、購物
        public string Keyword { get; set; }     //關鍵字
        public string Distance { get; set; }     //距離
        public string TagID { get; set; }     //搜尋TagId
        public Point Point { get; set; }     //距離
        public Range Range { get; set; }
    }
    #endregion
    #region output
    public class Point
    {
        public string Px { get; set; }     //Px
        public string Py { get; set; }     //Py
    }
    public class SearchItem
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Name_ch { get; set; }
        public string Brief { get; set; }
        public string PicURL { get; set; }
        public float Px { get; set; }
        public float Py { get; set; }
        public string DetailURL { get; set; }
        public string Type { get; set; }
        public int Duration { get; set; }
        public int Popular { get; set; }
        public List<Tags> Tag { get; set; }
    }
    public class SearchItemByTagId : SearchItem
    {
        public string Ser_No { get; set; }
    }
    #endregion
}