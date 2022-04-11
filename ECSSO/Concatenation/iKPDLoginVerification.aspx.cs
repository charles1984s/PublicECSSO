using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ECSSO.Concatenation
{
    public partial class iKPDLoginVerification : System.Web.UI.Page
    {
        string token = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            token = Request["token"];
            creatLinks(checkToken());
        }
        private void creatLinks(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Response.Write("登入逾時，請重新登入");
            }
            else {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select b.comp_name,a.* from IDManagement as a left join cocker_cust as b on a.orgName=b.crm_org where Manager_ID=@id", conn);
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = null;
                    try
                    {
                        reader = cmd.ExecuteReader();
                        int count = 0;
                        while (reader.Read())
                        {
                            count++;

                            Button btn = new Button();

                            btn.Text = reader["comp_name"].ToString();
                            btn.ID = reader["orgName"].ToString();
                            btn.Font.Size = FontUnit.Point(16);
                            btn.ControlStyle.CssClass = "button";
                            btn.Click += new EventHandler(delegate(object sender, EventArgs e) {
                                Button b = (Button)sender;
                                if (updateToken(b.ID))
                                {
                                    Response.Redirect("/admin/system_login1.asp?talken=" + token);
                                }
                                else {
                                    Response.Write("登入逾時"+ b.ID);
                                }
                            });
                            form1.Controls.Add(btn);
                        }
                    }
                    catch
                    {

                    }
                    finally
                    {
                        if (reader != null) reader.Close();
                    }
                }
            }
        }
        private string checkToken()
        {
            string id = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from talken where talken=@token", conn);
                cmd.Parameters.Add(new SqlParameter("@token", token));
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        id = reader["ManagerID"].ToString();
                    }
                }
                catch
                {

                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            return id;
        }
        private bool updateToken(string orgName)
        {
            string id = null;
            bool result = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlDB"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "iKPD_updateToken";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@token", token));
                cmd.Parameters.Add(new SqlParameter("@orgName", orgName));
                SqlParameter SPOutput = cmd.Parameters.Add("@ReturnCode", SqlDbType.NVarChar, 50);
                SPOutput.Direction = ParameterDirection.Output;
                SqlDataReader reader = null;
                try
                {
                    reader = cmd.ExecuteReader();
                    if (SPOutput.Value.ToString() != "error") {
                        token = SPOutput.Value.ToString();
                        result = true;
                    };
                }
                catch
                {

                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            return result;
        }
    }
}