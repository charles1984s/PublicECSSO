using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.CustFormLibary
{
    public class CustForm : responseJson
    {
        public List<FormItem> from { get; set; }
        public CustForm CustDatas { get; set; }
        public FormItem2 setCustForm2(string classe, string value)
        {
            FormItem2 myFrom = new FormItem2();
            myFrom.options = new FormOptions();
            myFrom.options.classes = classe;
            myFrom.value = value;
            return myFrom;
        }
    }
    public class FormItem
    {
        public FormOptions options { get; set; }
        public FormTableValue value { get; set; }
    }
    public class FormItem2
    {
        public FormOptions options { get; set; }
        public String value { get; set; }
    }
    public class FormOptions
    {
        public String classes { get; set; }
        public bool expanded { get; set; }

    }
    public class FormValue
    {
        public int id { get; set; }
        public String title { get; set; }
        public FormItem2 edit { get; set; }
    }
    public class FormTableValue : FormValue
    {
        public FormItem2 log { get; set; }
    }
    public class FormSort
    {
        public bool enabled { get; set; }
    }
    public class FormColumn
    {
        public string name { get; set; }
        public string title { get; set; }
        public string breakpoints { get; set; }
        public string type { get; set; }
        public TableStyle style { get; set; }
    }
    public class TableStyle {
        public int width { get; set; }
        public int maxWidth { get; set; }
    }
}