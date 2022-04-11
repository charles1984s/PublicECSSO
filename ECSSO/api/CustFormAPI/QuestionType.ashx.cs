using ECSSO.Library.CustFormLibary;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace ECSSO.api.CustFormAPI
{
    /// <summary>
    /// QuestionType 的摘要描述
    /// </summary>
    public class QuestionType : IHttpHandler
    {
        HttpContext context;
        FromQuestionType custForm;
        string setting, orderby, column, seq;
        int PAGECOUNT = 20, page = 1, id;
        public void ProcessRequest(HttpContext context)
        {
            GetStr GS = new GetStr();
            string code, message;
            code = "404";
            message = "not fount";
            custForm = new FromQuestionType();
            try
            {
                if (context.Request.Form["token"] != null)
                {

                    this.setting = GS.checkToken(context.Request.Form["token"]);
                    if (this.setting.IndexOf("error") < 0)
                    {
                        this.context = context;
                        if (context.Request.Form["c"] != null) column = context.Request.Form["c"].ToString();
                        else column = "id";
                        if (context.Request.Form["s"] != null) seq = context.Request.Form["s"].ToString();
                        else seq = "ASC";
                        if (context.Request.Form["p"] != null) page = Int32.Parse(context.Request.Form["p"].ToString());
                        orderby = column + " " + seq;
                        custForm.from = new List<QuestionTypeItem>();
                        switch (context.Request.Form["t"]) {
                            case "list":
                                getQuestionList();
                                code = "200";
                                message = "success";
                                break;
                            case "rela":
                                if (context.Request.Form["id"] != null)
                                {
                                    id = Int32.Parse(context.Request.Form["id"].ToString());
                                    getQuestionRelation();
                                    code = "200";
                                    message = "success";
                                }
                                else {
                                    code = "404";
                                    message = "操作不存在";
                                }
                                break;
                            case "relaBind":
                                if (context.Request.Form["id"] != null)
                                {
                                    id = Int32.Parse(context.Request.Form["id"].ToString());
                                    getQuestionRelationBind();
                                    code = "200";
                                    message = "success";
                                }
                                else
                                {
                                    code = "404";
                                    message = "操作不存在";
                                }
                                break;
                            default:
                                code = "404";
                                message = "操作不存在";
                                break;
                        }
                    }
                    else
                    {
                        code = "401";
                        message = "Token已過期";
                    }
                }
            }
            catch (Exception ex)
            {
                code = "500";
                message = ex.Message + ex.StackTrace;
            }
            finally
            {
                context.Response.Write(printMsg(code, message));
            }
        }
        private void getQuestionRelationBind() {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select ContactUsType.id,ContactUsType.title,ContactUsType.service_mail,(
	                    select case when COUNT(*)>0 then 'true' else 'false' end
	                    from ContactTypeRelation 
	                    where ContactTypeRelation.f_id=@id and ContactTypeRelation.typeID=ContactUsType.id
                    ) [check] from ContactUsType
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                setCustForm(cmd);
            }
        }
        private void getQuestionRelation()
        {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select ContactUsType.id,ContactUsType.title,ContactUsType.service_mail,'false' [check]
                    from ContactTypeRelation
                    left join ContactUsType on ContactTypeRelation.typeID=ContactUsType.id
                    where f_id=@id and not ContactUsType.id is null
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                setCustForm(cmd);
            }
        }
        private void getQuestionList() {
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT * FROM(
                        SELECT ROW_NUMBER() OVER(ORDER BY id desc) rowNumber,
                        (select CEILING(COUNT(*)/@PAGECOUNT) from ContactUsType) t,'false' [check],* 
                        FROM ContactUsType
                    ) myTable 
                    WHERE rowNumber>@from AND rowNumber<=@to
                    ORDER BY    
                    case WHEN @orderby = 'id ASC' Then id ELSE null END ASC,
                    case WHEN @orderby = 'id DESC' Then id ELSE null END DESC,
                    CASE WHEN @orderby = 'title ASC' then title ELSE null END ASC,
                    CASE WHEN @orderby = 'title DESC' then title ELSE null END DESC
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@PAGECOUNT", PAGECOUNT));
                cmd.Parameters.Add(new SqlParameter("@from", (page - 1) * PAGECOUNT));
                cmd.Parameters.Add(new SqlParameter("@to", page * PAGECOUNT));
                cmd.Parameters.Add(new SqlParameter("@orderby", orderby));
                setCustForm(cmd);
            }
        }
        private void setCustForm(SqlCommand cmd) {
            SqlDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    QuestionTypeItem form = new QuestionTypeItem();
                    form.options = new FormOptions();
                    form.options.classes = "formItem" + reader["id"].ToString();
                    form.value = new QuestionTypeValue();
                    form.value.id = int.Parse(reader["id"].ToString());
                    form.value.title = reader["title"].ToString();
                    form.value.mail = reader["service_mail"].ToString();
                    form.value.check = bool.Parse(reader["check"].ToString());
                    form.value.del = custForm.setCustForm2(
                        "cell-btn cell-del",
                        "<div class='delbtn btn btn-xs btn-default btn-del'><i class='glyphicon glyphicon-trash'></i></div>"
                    );
                    form.value.edit = custForm.setCustForm2(
                        "cell-btn cell-edit",
                        "<div class='delbtn btn btn-xs btn-default btn-edit'><i class='glyphicon glyphicon-pencil'></i></div>"
                    );
                    custForm.from.Add(form);
                }
            }
            finally
            {
                reader.Close();
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        #region 回傳error字串
        private String printMsg(String RspnCode, String RspnMsg)
        {
            custForm.RspnCode = RspnCode;
            custForm.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(custForm);
        }
        #endregion
    }
}