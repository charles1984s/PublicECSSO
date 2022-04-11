using System;
using System.Collections.Generic;
using ECSSO.Library.CustFormLibary;

namespace ECSSO.Library.Analytics
{
    public class Flow : responseJson
    {
        public FlowData table { get; set; }
    }
    public class FlowData
    {
        public FormSort sorting { get; set; }
        public string empty { get; set; }
        public List<FormColumn> columns { get; set; }
        public List<FlowItem> rows { get; set; }
    }
    public class FlowItem
    {
        public FormOptions options { get; set; }
        public FlowItemValue value { get; set; }
    }
    public class Flow2 : responseJson
    {
        public FlowData2 table { get; set; }
    }
    public class FlowData2
    {
        public FormSort sorting { get; set; }
        public string empty { get; set; }
        public List<FormColumn> columns { get; set; }
        public List<FlowItem2> rows { get; set; }
    }
    public class FlowItem2
    {
        public FormOptions options { get; set; }
        public ReportTableItem value { get; set; }
    }
    public class FlowItemValue {
        public int id { get; set; }
    }
    public class AlltableItem : FlowItemValue
    {
        public int count { get; set; }
        public int countOfperson { get; set; }
        public string date { get; set; }
    }
    public class ReportTableItem : FlowItemValue
    {
        public int count { get; set; }
        public int countOfperson { get; set; }
        public int age { get; set; }
        public string sex { get; set; }
        public string marry { get; set; }
        public string org { get; set; }
        public string chf { get; set; }
        public string crk { get; set; }
        public string POPESN { get; set; }
        public int TSER_day { get; set; }
        public string ETP { get; set; }
        public string ESU { get; set; }
        public string ELV { get; set; }
        public string menuSub { get; set; }
        public string menu { get; set; }
        public string tag { get; set; }
        public string orgLevel { get; set; }
        public string name { get; set; }
        public int peopleTh { get; set; }
        public string zip { get; set; }

    }
    public class CustTableListItem : FlowItemValue {
        public string title { get; set; }
        public string cuser { get; set; }
        public string cdate { get; set; }
        public string euser { get; set; }
        public string edate { get; set; }
    }
}