using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library.CustFormLibary
{
    public class FromQuestionType : CustForm
    {
        public new List<QuestionTypeItem> from { get; set; }
    }
    public class QuestionTypeItem : FormItem
    {
        public new QuestionTypeValue value { get; set; }
    }
    public class QuestionTypeValue : FormValue
    {
        public string mail { get; set; }
        public bool check { get; set; }
        public FormItem2 del { get; set; }
    }
}