using ECSSO.Library;
using ECSSO.Library.file;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace ECSSO.api.Order
{
    /// <summary>
    /// OrderFileHandler 的摘要描述
    /// </summary>
    public class OrderFileHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        private GetStr GS { get; set; }
        private FileResponseJson response { get; set; }
        private string setting { get; set; }
        private string ManagerID { get; set; }
        private string OrgName { get; set; }
        private string orderID { get; set; }
        private string token { get; set; }
        private string StorageRoot { get; set; }
        private string HandlerPath { get { return "/admin/UploadFiles/"; } }
        private string ext { get; set; }
        private string DirectoryPath { get; set; }
        private HttpContext context { get; set; }
        public string FilePath { get { return ConfigurationManager.AppSettings.Get("FilePath").ToString(); } }
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            GS = new GetStr();
            response = new FileResponseJson
            {
                code = "500",
                error = "驗證未通過",
                success = false
            };
            try
            {
                if (string.IsNullOrEmpty(context.Request.Form["siteid"])) throw new Exception("siteid必填");
                else if (string.IsNullOrEmpty(context.Request.Form["orderID"])) throw new Exception("orderID必填");
                else if (string.IsNullOrEmpty(context.Request.Form["memID"])) throw new Exception("MemberID必填");
                else if (string.IsNullOrEmpty(context.Request.Form["token"])) throw new Exception("token必填");
                else if (string.IsNullOrEmpty(context.Request.Form["key"])) throw new Exception("key必填");
                else
                {
                    setting = GS.GetSetting2(context.Request.Form["siteid"]);
                    ManagerID = context.Request.Form["memID"];
                    token = context.Request.Form["token"];
                    if (checkToken(token, ManagerID))
                    {
                        orderID = context.Request.Form["orderID"];
                        OrgName = GS.GetOrgName(setting);
                        StorageRoot = $"{FilePath}cocker-{OrgName}/upload/Cust/{ManagerID}/";
                        DirectoryPath = $"Cust/{ManagerID}";
                        Process();
                        response.code = "200";
                        response.error = null;
                        response.success = true;
                    }
                    else
                    {
                        response.code = "401";
                        throw new Exception("token錯誤");
                    }
                }
            }
            catch (Exception e)
            {
                response.error = e.Message;
            }
            finally
            {
                GS.ResponseWriteEnd(context, JsonConvert.SerializeObject(response));
            }
        }
        private void Process()
        {
            switch (context.Request.HttpMethod)
            {
                case "TRACE":
                    //ListCurrentFiles(context);
                    break;
                case "GET":
                    //if (GivenFileName(context)) DeliverFile(context);
                    //else ListCurrentFiles(context);
                    break;

                case "POST":
                case "PUT":
                    UploadFile();
                    break;
                case "DELETE":
                    DeleteFile(getOrderFile());
                    removerOrderFile();
                    break;
                case "OPTIONS":
                    //ReturnOptions(context);
                    break;

                default:
                    response.code = "405";
                    throw new Exception("操作不存在");
            }
        }
        private void removerOrderFile()
        {
            int type = 0;
            switch (context.Request.Form["key"])
            {
                case "store2File1":
                    type = 1;
                    break;
                case "store2File2":
                    type = 2;
                    break;
                case "store2img1":
                    type = 3;
                    break;
                default:
                    throw new Exception("不明確的操作");
            }
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    update orders_hd set store2File1=
	                    case @type 
		                    when 1 then '' else store2File1 end,
	                    store2File2=
	                    case @type 
		                    when 2 then '' else store2File2 end,
	                    store2img1=
	                    case @type 
		                    when 3 then '' else store2img1 end,
                        edate=getdate()
                    where id=@orderID and mem_id=@MemID
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@orderID", orderID));
                cmd.Parameters.Add(new SqlParameter("@MemID", ManagerID));
                cmd.Parameters.Add(new SqlParameter("@type", type));
                try
                {
                    cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        private string getOrderFile()
        {
            string path = "";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select store2File1,store2File2,store2img1 from orders_hd as head
                    left join Cust on Cust.mem_id=head.mem_id
                    left join token on Cust.id=token.ManagerID
                    where head.id=@orderID and token.id=@token
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@orderID", orderID));
                cmd.Parameters.Add(new SqlParameter("@token", token));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        switch (context.Request.Form["key"])
                        {
                            case "store2File1":
                                path = reader["store2File1"].ToString().Replace($"/upload/Cust/{ManagerID}/", "");
                                break;
                            case "store2File2":
                                path = reader["store2File2"].ToString().Replace($"/upload/Cust/{ManagerID}/", "");
                                break;
                            case "store2img1":
                                path = reader["store2img1"].ToString().Replace($"/upload/Cust/{ManagerID}/", "");
                                break;
                            default:
                                throw new Exception("不明確的操作");
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return path;
        }
        private bool checkToken(string token, string MemID)
        {
            bool result = false;
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    select token.* from token 
                    left join Cust on cust.id=token.ManagerID
                    where Cust.mem_id=@MemID and token.id=@token
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@MemID", MemID));
                cmd.Parameters.Add(new SqlParameter("@token", token));
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read()) result = true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return result;
        }
        private void DeleteFile(string fileName)
        {
            var filePath = StorageRoot + fileName;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else throw new Exception("檔案不存在");
        }
        private void UploadFile()
        {
            var statuses = new List<FileStatus>();
            var headers = context.Request.Headers;
            if (!string.IsNullOrEmpty(context.Request["qquuid"]))
            {
                string fileName = context.Request["qqfilename"];
                bool check;
                string[] file = { "pdf" };
                string[] image = { "jpeg", "jpg", "gif", "png", "svg", "pdf" };
                switch (context.Request.Form["key"])
                {
                    case "store2File1":
                    case "store2File2":
                        check = checkAndSetExt(fileName, file);
                        break;
                    case "store2img1":
                        check = checkAndSetExt(fileName, image);
                        break;
                    default:
                        throw new Exception("不明確的操作");
                }
                if (check)
                {
                    switch (context.Request.Form["key"])
                    {
                        case "store2File1":
                            fileName = updateCustImg(1);
                            break;
                        case "store2File2":
                            fileName = updateCustImg(2);
                            break;
                        case "store2img1":
                            fileName = updateCustImg(3);
                            break;
                    }
                    UploadSingleFile(fileName);
                }
                else
                {
                    throw new HttpRequestValidationException("資料格式錯誤");
                }
            }
        }
        private void UploadSingleFile(string fileName)
        {
            if (context.Request.Files.Count != 1) throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");
            var inputStream = context.Request.Files[0].InputStream;
            string fullName = StorageRoot + Path.GetFileName(fileName);
            Directory.CreateDirectory(StorageRoot);
            using (var fs = new FileStream(fullName, FileMode.Append, FileAccess.Write))
            {
                var buffer = new byte[1024];

                var l = inputStream.Read(buffer, 0, 1024);
                while (l > 0)
                {
                    fs.Write(buffer, 0, l);
                    l = inputStream.Read(buffer, 0, 1024);
                }
                fs.Flush();
                fs.Close();
            }
            FileStatus file = new FileStatus(new FileInfo(fullName), this.HandlerPath, this.DirectoryPath);
            response.path = file.url + "&org=" + OrgName;
        }
        private bool checkAndSetExt(string fileName, string[] Restrict)
        {
            bool check = false;
            string[] attr = fileName.Split('.');
            ext = attr[attr.Length - 1];
            for (int i = 0; i < Restrict.Length; i++)
            {
                if (string.Compare(Restrict[i], ext, true) == 0)
                {
                    check = true;
                }
            }
            return check;
        }
        private string updateCustImg(int type)
        {
            string path = "";
            using (SqlConnection conn = new SqlConnection(setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "sp_updateOrderFile";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.Parameters.Add(new SqlParameter("@MemID", ManagerID));
                cmd.Parameters.Add(new SqlParameter("@orderID", orderID));
                cmd.Parameters.Add(new SqlParameter("@type", type));
                cmd.Parameters.Add(new SqlParameter("@ext", ext));
                SqlParameter SPOutput = cmd.Parameters.Add("@ReturnCode", SqlDbType.NVarChar, 50);
                SPOutput.Direction = ParameterDirection.Output;
                try
                {
                    cmd.ExecuteNonQuery();
                    path = SPOutput.Value.ToString();
                    if (string.IsNullOrEmpty(path)) throw new Exception("用戶驗證失敗");
                }
                catch (Exception e)
                {
                    throw new HttpRequestValidationException(e.Message);
                }
            }
            return path;
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}