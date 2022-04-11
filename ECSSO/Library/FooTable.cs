using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSSO.Library
{
    public class FooTable : responseJson
    {
        public FooTableDetail table { get; set; }
    }
    public class FooTableDetail
    {
        public FooTableSort sorting { get; set; }
        public FooTablePaging paging { get; set; }
        public FooTableFiltering filtering { get; set; } = new FooTableFiltering();
    public string empty { get; set; }
        public List<FooTableColumn> columns { get; set; }
        public List<FooTabkeRow> rows { get; set; }
        public int total { get; set; }
    }
    public class FooTableSort
    {
        public bool enabled { get; set; } = false;
    }
    public class FooTablePaging
    {
        public bool enabled { get; set; }
        public int size { get; set; }
        public int limit { get; set; }
    }
    public class FooTableFiltering {
        public bool enabled { get; set; }
    }
    public class FooTableColumn
    {
        public string name { get; set; }
        public string title { get; set; }
        public string breakpoints { get; set; }
        public string type { get; set; }
        public bool sortable { get; set; }
        public bool filterable { get; set; } = true;
        public FooTableStyle style { get; set; }
    }
    public class FooTableStyle
    {
        public int width { get; set; }
        public int maxWidth { get; set; }
    }
    public class FooTabkeRow
    {
        public RowOptions options { get; set; }
        public RowValue value { get; set; }
    }
    public class RowOptions
    {
        public String classes { get; set; }
        public bool expanded { get; set; }
    }
    public class RowValue
    {
        public int id { get; set; }
    }
}