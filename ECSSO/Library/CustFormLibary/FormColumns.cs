using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ECSSO.Library.CustFormLibary
{
    public class FormColumns
    {
        public List<FormColumnItem> columnItems { get; set; }
        public string RspnCode { get; set; }
        public CustForm CustDatas { get; set; }
        public string RspnMsg { get; set; }
        public string Token { get; set; }
        public int FID { get; set; }
        public string introduction { get; set; }
        public string signature { get; set; }
        public string getBootstrap3Html() {
            string r = "<div class='row'>";

            columnItems.ForEach( e => {
                r += $"<div class='col-md-{e.span} col-sm-12 contactItem'>";
                r += $@"<label for='Contact{e.id}' class='{(e.must ? "must" : "")}' data-key='{e.id}' data-displayType='{e.displayType}'>
                            {e.title}
                            <span class='remind'>{(string.IsNullOrEmpty(e.memo)?"":"("+ e.memo + ")")}</span>
                    </label>
                    <div>";
                switch (e.displayType) {
                    case 1: //簡答
                        r += $"<input id='Contact{e.id}' name='Contact{e.id}' type='text' value='{e.initText}' />";
                        break;
                    case 2: //段落
                        r += $"<textarea id='Contact{e.id}' name='Contact{e.id}' type='text'>{e.initText}</textarea>";
                        break;
                    case 6: //日曆
                        r += $"<input id='Contact{e.id}' name='Contact{e.id}' type='date' value='{e.initText}' />";
                        break;
                    case 3: //選擇題
                        e.detail.ForEach(d => {
                            r += 
                                $@"<div class='items'>
                                    <input id='myItem{d.id}' name='Contact{e.id}' value='{d.title}' type = 'radio' class='checkSquare'{(e.initText==d.title?" checked":"")}>
						            <label for='myItem{d.id}'>
							            <i class='fa fa-check-square-o' aria-hidden='true'></i>
							            <i class='fa fa-square-o' aria-hidden='true'></i>
							            <span >{d.title}</span>
						            </label>
					            </div>";
                        });
                        if (e.dispOther) {
                            r +=
                                $@"<div class='items'>
                                    <input id='myItemOther{e.id}' name='Contact{e.id}' value='' type='radio' class='checkSquare other'>
						            <label for='myItemOther{e.id}'>
							            <i class='fa fa-check-square-o' aria-hidden='true'></i>
							            <i class='fa fa-square-o' aria-hidden='true'></i>
							            <span >其他</span>
                                        <input type='text' name='myItemOther{e.id}' />
                                        <span class='memo hidden'>請備註{e.title}的其他資料</span>
						            </label>
					            </div>";
                        }
                        break;
                    case 4: //核選方塊
                        e.detail.ForEach(d => {
                            r +=
                                $@"<div class='items'>
                                    <input id='myItem{d.id}' name='Contact{e.id}' value='{d.title}' type='checkbox' class='checkSquare'{(e.initText==d.title?" checked":"")}>
						            <label for='myItem{d.id}'>
							            <i class='fa fa-check-square-o' aria-hidden='true'></i>
							            <i class='fa fa-square-o' aria-hidden='true'></i>
							            <span >{d.title}</span>
						            </label>
					            </div>";
                        });
                        if (e.dispOther)
                        {
                            r +=
                                $@"<div class='items'>
                                    <input id='myItemOther{e.id}' name='Contact{e.id}' value='' type='checkbox' class='checkSquare other'>
						            <label for='myItemOther{e.id}'>
							            <i class='fa fa-check-square-o' aria-hidden='true'></i>
							            <i class='fa fa-square-o' aria-hidden='true'></i>
							            <span >其他</span>
                                        <input type='text' name='myItemOther{e.id}' />
                                        <span class='memo hidden'>請備註{e.title}的其他資料</span>
						            </label>
					            </div>";
                        }
                        break;
                    case 5: //下拉式選單
                        r += $"<select id='Contact{e.id}' name='Contact{e.id}'>";
                        e.detail.ForEach(d => {
                            r +=
                                $@"<option value='{d.title}'{(e.initText==d.title?" seleced":"")}>{d.title}</option>";
                        });
                        r += "</select>";
                        break;
                }
                r += $@"
                        <div class='memo hidden'>{e.title}不可為空</div>
                    </div >
                </div >
                ";
            });
            r += "</div>";
            return r;
        }
        private void insertColumn(SqlDataReader reader) {
            FormColumnItem item = columnItems.Find(e => e.id == int.Parse(reader["cid"].ToString()));
            if (item == null)
            {
                item = new FormColumnItem
                {
                    id = int.Parse(reader["cid"].ToString()),
                    f_id = int.Parse(reader["f_id"].ToString()),
                    title = reader["columnTitle"].ToString(),
                    span = int.Parse(reader["colspan"].ToString()),
                    ser = columnItems.Count + 1,
                    must = reader["must"].ToString() == "Y",
                    initText = reader["initText"].ToString(),
                    displayType = int.Parse(reader["dispType"].ToString()),
                    memo = reader["memo"].ToString(),
                    dispOther = reader["disp_other"].ToString() == "Y",
                    detail = new List<ColumnDetail>()
                };
                columnItems.Add(item);
            }
            if (!reader.IsDBNull(reader.GetOrdinal("id"))) {
                ColumnDetail detail = item.detail.Find(e => e.id == int.Parse(reader["id"].ToString()));
                if (detail == null) {
                    detail = new ColumnDetail {
                        id = int.Parse(reader["id"].ToString()),
                        title = reader["title"].ToString()
                    };
                    item.detail.Add(detail);
                }
            }
        }
        public FormColumns() { }
        public FormColumns(string setting, string orderID) {
            using (SqlConnection conn = new SqlConnection(setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select 
	                    c.id ,col.title,	v.value
                    from orders_hd as hd
                    left join Contact as c on c.[type] =hd.id
                    left join contactValue as v on c.id=v.contactID
                    left join contactUsColumn as col on v.columnID=col.id
                    where hd.id=@orderID
                    order by col.ser_num
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@orderID", orderID));
                try {
                    SqlDataReader reader = cmd.ExecuteReader();
                    columnItems = new List<FormColumnItem>();
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal("id")))
                        {
                            do
                            {
                                columnItems.Add(new FormColumnItem
                                {
                                    title = reader["title"].ToString(),
                                    value = reader["value"].ToString()
                                });
                            } while (reader.Read());
                        }
                        else { 
                            RspnCode = "404";
                        }
                        if (string.IsNullOrEmpty(RspnCode)) RspnCode = "200";
                    }
                    else RspnCode = "404";
                }
                catch (Exception e)
                {
                    RspnMsg = e.Message;
                }
            }
        }
        public FormColumns(string setting, int sub_id)
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select 
	                    r.f_id,
	                    c.id cid,c.title columnTitle,c.colspan,c.must,c.initText,c.dispType,c.disp_other,c.memo,
	                    d.id,d.title
                    from formRelation as r
                    left join contactUsColumn as c on r.f_id=c.f_id
                    left join columnDetail as d on c.id=d.col_id
                    where r.sub_id=@subID and c.disp_opt='Y'
                    order by c.ser_num,d.ser_no
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@subID", sub_id));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    columnItems = new List<FormColumnItem>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("cid")))
                            {
                                insertColumn(reader);
                            }
                            else RspnCode = "404";
                        }
                        if(string.IsNullOrEmpty(RspnCode)) RspnCode = "200";
                    }
                    else RspnCode="404";
                }
                catch (Exception e)
                {
                    RspnMsg = e.Message;
                }

            }
        }
        private void setColumnTitle(string setting) {
            using (SqlConnection conn = new SqlConnection(setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select 
	                    c.id cid,c.title
                    from contactUsColumn as c
                    where c.f_id=@fid and c.disp_opt='Y'
                    order by c.ser_num
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@fid", FID));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        columnItems.ForEach(e => { 
                            if(e.id == int.Parse(reader["cid"].ToString()))
                                e.title = reader["title"].ToString();
                        });
                    }
                }
                catch (Exception e)
                {

                }
            }
        }
        private void setOrderFid(string setting,int sub_id) {
            using (SqlConnection conn = new SqlConnection(setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select 
                        f.id,f.introduction,f.[signature]
                    from formRelation as r
                    left join form as f on f.id=r.f_id
                    where r.sub_id=@subID
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@subID", sub_id));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read()) {
                        FID = int.Parse(reader["id"].ToString());
                        introduction = reader["introduction"].ToString();
                        signature = reader["signature"].ToString();
                        setColumnTitle(setting);
                    }
                }
                catch(Exception e) {
                    RspnCode = e.Message;
                }
            }
        }
        public string getColumnStr() {
            string custColume = "";
            columnItems.ForEach(delegate (FormColumnItem item)
            {
                custColume = custColume + item.id + ":" + item.value.Replace(",", "%2C").Replace(":", "%3A") + ",";
            });
            if (custColume.Length > 0)
                custColume = custColume.Substring(0, custColume.Length - 1);
            return custColume;
        }
        public void saveAndBindOrder(string setting,string OrderID) {
            string custColume = getColumnStr();
            setOrderFid(setting, 0);
            using (SqlConnection conn = new SqlConnection(setting)) {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_AddContact";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@f_id", FID));
                cmd.Parameters.Add(new SqlParameter("@type", OrderID));
                cmd.Parameters.Add(new SqlParameter("@name", ""));
                cmd.Parameters.Add(new SqlParameter("@email", ""));
                cmd.Parameters.Add(new SqlParameter("@tel", ""));
                cmd.Parameters.Add(new SqlParameter("@subject", ""));
                cmd.Parameters.Add(new SqlParameter("@orderdatetime", ""));
                cmd.Parameters.Add(new SqlParameter("@notememo", ""));
                cmd.Parameters.Add(new SqlParameter("@custColume", custColume));
                cmd.Parameters.Add(new SqlParameter("@sex", "0"));
                cmd.Parameters.Add(new SqlParameter("@typeID", "0"));
                cmd.ExecuteReader();
            }
        }
    }
    public class FormColumnItem
    {
        public int id { get; set; }
        public int f_id { get; set; }
        public string title { get; set; }
        public bool display { get; set; }
        public bool dispOther { get; set; }
        public bool must { get; set; }
        public int span { get; set; }
        public int ser { get; set; }
        public string initText { get; set; }
        public string memo { get; set; }
        public int displayType { get; set; }
        public string value { get; set; }
        public List<ColumnDetail> detail { get; set; }
    }
    public class ColumnDetail
    {
        public int id { get; set; }
        public string title { get; set; }
        public int ser { get; set; }
    }
}