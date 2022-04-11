using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ECSSO
{
    public partial class EIPLogin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string sessionId = Request["sessionId"];
            string siteid = Request["sideID"];
            Ldap.ILdapServicePortType service = new Ldap.LdapServicePortTypeClient();
            string account = service.verifySessionId(sessionId);
            string state = "";
            string linkUrl = "/admin/system_login1.asp?talken=";
            string ReturnCode = null;
            string orgname = null;
            if (account != null && !account.Equals(""))
            {
                GetStr gs = new GetStr();
                String setting = gs.GetSetting(siteid);
                orgname = gs.GetOrgName(setting);
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "sp_EIPlogin";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlParameter("@orgname", orgname));
                    cmd.Parameters.Add(new SqlParameter("@userid", account));
                    cmd.Parameters.Add(new SqlParameter("@ip", gs.GetIPAddress()));
                    SqlParameter SPOutput = cmd.Parameters.Add("@ReturnCode", SqlDbType.NVarChar, 50);
                    SPOutput.Direction = ParameterDirection.Output;
                    try
                    {
                        cmd.ExecuteNonQuery();
                        ReturnCode = SPOutput.Value.ToString();
                        if (ReturnCode.IndexOf("error")<0) state = "success";
                        else state = "error";
                    }
                    catch (Exception ex)
                    {
                        state = ex.Message;
                    }
                }
            }else state = "error";
            if (state == "success") Response.Redirect(linkUrl + ReturnCode + "&username=" + account + "&orgName=" + orgname);
            else
            {
                if (!Page.ClientScript.IsStartupScriptRegistered("alert"))
                {
                    Page.ClientScript.RegisterStartupScript
                        (this.GetType(), "alert", "invokeMeMaster('帳號沒有權限，請向相關單位申請後再做嘗試。');", true);
                }
            }
        }
    }
}