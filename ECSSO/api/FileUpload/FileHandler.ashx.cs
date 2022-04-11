using ECSSO.Library;
using ECSSO.Library.file;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace ECSSO.api.FileUpload
{
    /// <summary>
    /// FileHandler 的摘要描述
    /// </summary>
    public class FileHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        private readonly JavaScriptSerializer js = new JavaScriptSerializer();
        public string StorageRoot { get; set; }
        public string HandlerPath { get { return "/admin/UploadFiles/"; } }
        public string FilePath { get { return ConfigurationManager.AppSettings.Get("FilePath").ToString(); } }
        public string DirectoryPath { get; set; }
        public string orgName { get; set; }
        public string[] Restrict = { "jpg", "jpeg", "png", "gif", "mp4", "wmv", "wav", "wma", "mpeg", "mp3", "xls", "avi", "pdf", "doc", "docx", "xlsx", "ogv", "rar", "zip", "csv", "tiff", "ppt", "pptx", "swf", "bmp", "odf", "xml", "css", "htm", "html", "dwg", "psd", "odt", "ods", "odp", "odg", "odb" };
        private CheckToken checkToken { get; set; }
        private responseJson responseJson { get; set; }
        private Dictionary<string, int> saveTo { get; set; }
        public void ProcessRequest(HttpContext context)
        {
            checkToken = new CheckToken();
            responseJson = new responseJson();
            try
            {
                checkToken.check(context);
                responseJson = new responseJson
                {
                    RspnCode = checkToken.response.RspnCode,
                    RspnMsg = checkToken.response.RspnMsg
                };
                if (checkToken.response.RspnCode == "200")
                {
                    setStorageRoot(context);
                    StorageRoot = StorageRoot + DirectoryPath + "/";
                    context.Response.AddHeader("Pragma", "no-cache");
                    context.Response.AddHeader("Cache-Control", "private, no-cache");
                    Process(context);
                }
                else throw new Exception("Token已過期");
            }
            catch (Exception ex)
            {
                responseJson.RspnCode = "500";
                responseJson.RspnMsg = ex.Message;
                context.Response.Write(responseJson.printMsg());
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        #region 設定目錄
        private void setStorageRoot(HttpContext context)
        {
            orgName = checkToken.token.orgName;
            if (string.IsNullOrEmpty(orgName))
            {
                throw new Exception("Token已過期");
            }
            StorageRoot = FilePath + "cocker-" + orgName + "/upload/";
            saveTo = new Dictionary<string, int>();
            switch (context.Request.Form["key"])
            {
                case "360Prod":
                    DirectoryPath = "menu/image360/" + context.Request.Form["prodID"];
                    saveTo.Add("360Prod", int.Parse(context.Request.Form["prodID"]));
                    if (string.IsNullOrEmpty(context.Request.Form["prodID"])) throw new Exception("查無資料");
                    break;
                default:
                    switch (context.Request["key"])
                    {
                        case "360ProdList":
                            DirectoryPath = "menu/image360/" + context.Request["prodID"];
                            if (string.IsNullOrEmpty(context.Request["prodID"])) throw new Exception("查無資料");
                            break;
                        default:
                            responseJson.RspnCode = "404";
                            throw new Exception("查無操作");
                    }
                    break;
            }
        }
        #endregion
        #region 作業判斷
        private void Process(HttpContext context)
        {
            switch (context.Request.HttpMethod)
            {
                case "TRACE":
                    ListCurrentFiles(context);
                    break;
                case "GET":
                    if (GivenFileName(context)) DeliverFile(context);
                    else ListCurrentFiles(context);
                    break;

                case "POST":
                case "PUT":
                    UploadFile(context);
                    break;

                case "DELETE":
                    if (context.Request.Headers["type"] == "folder") DeleteFloder(context);
                    else DeleteFile(context);
                    break;

                case "OPTIONS":
                    ReturnOptions(context);
                    break;

                default:
                    context.Response.ClearHeaders();
                    context.Response.StatusCode = 405;
                    break;
            }
        }
        #endregion

        #region GET
        #region 下載檔案
        private void DeliverFile(HttpContext context)
        {
            var filename = context.Request["f"];
            var filePath = StorageRoot + filename;

            if (File.Exists(filePath))
            {
                context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + filename + "\"");
                context.Response.ContentType = "application/octet-stream";
                context.Response.ClearContent();
                context.Response.WriteFile(filePath);
            }
            else
                context.Response.StatusCode = 404;
        }
        #endregion
        #region 目錄圖片列表
        private void ListCurrentFiles(HttpContext context)
        {
            var files =
                new DirectoryInfo(StorageRoot)
                    .GetFiles("*", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden))
                    .Select(f => new FilesStatus(f, this.HandlerPath, this.DirectoryPath, orgName))
                    .ToArray();
            var jsonObj = js.Serialize(new { files = files.ToArray() });
            context.Response.AddHeader("Content-Disposition", "inline; filename=\"files.json\"");
            context.Response.Write(jsonObj);
            context.Response.ContentType = "application/json";
        }
        #endregion
        #endregion

        #region PUT 檔案上傳
        private void UploadFile(HttpContext context)
        {
            var statuses = new List<FilesStatus>();
            var headers = context.Request.Headers;
            if (context.Request.Files.Count == 1)
            {
                UploadSingleFile(context.Request.Form["qqfilename"], context, statuses);
            }
            else
            {
                UploadMultiFiles(context, statuses);
            }

            responseJson.RspnCode = "200";
            responseJson.RspnMsg = "上傳成功";
            context.Response.Write(responseJson.printMsg());
            //WriteJson(context, statuses);
        }
        #endregion
        #region Delete
        private void DeleteFloder(HttpContext context)
        {
            if (!Directory.Exists(StorageRoot))
                return;
            // get the directory with the specific name
            DirectoryInfo dir = new DirectoryInfo(StorageRoot);
            try
            {
                foreach (FileInfo fi in dir.GetFiles())
                    fi.Delete();
                Directory.Delete(StorageRoot);
                responseJson.RspnCode = "200";
                responseJson.RspnMsg = "刪除成功";
                context.Response.Write(responseJson.printMsg());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void DeleteFile(HttpContext context)
        {
            var statuses = new List<FilesStatus>();
            var filePath = StorageRoot + context.Request["f"];
            statuses.Add(new FilesStatus(new FileInfo(filePath), this.HandlerPath, this.DirectoryPath, orgName));
            WriteJson(context, statuses);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }


        }
        #endregion

        #region Options
        #region 操作查詢
        private void ReturnOptions(HttpContext context)
        {
            context.Response.AddHeader("Allow", "DELETE,GET,HEAD,POST,PUT,OPTIONS");
            context.Response.StatusCode = 200;
        }
        #endregion
        #endregion

        #region Help Method
        private bool GivenFileName(HttpContext context)
        {
            return !string.IsNullOrEmpty(context.Request["f"]);
        }
        private bool GivenDirectoryName(HttpContext context)
        {
            return !string.IsNullOrEmpty(context.Request["d"]);
        }
        // 單檔案上傳
        private void UploadSingleFile(string fileName, HttpContext context, List<FilesStatus> statuses)
        {
            if (context.Request.Files.Count != 1) throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");
            var inputStream = context.Request.Files[0].InputStream;
            var fullName = StorageRoot + Path.GetFileName(fileName);
            bool check = false;
            string ext = fileName.Split('.')[1];
            for (int i = 0; i < Restrict.Length; i++)
            {
                if (string.Compare(Restrict[i], ext, true) == 0)
                {
                    check = true;
                }
            }
            if (check)
            {
                if (!Directory.Exists(StorageRoot)) Directory.CreateDirectory(StorageRoot);
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
                statuses.Add(new FilesStatus(new FileInfo(fullName), this.HandlerPath, this.DirectoryPath, orgName));
                foreach (KeyValuePair<string, int> e in saveTo)
                {
                    switch (e.Key)
                    {
                        case "360Prod":
                            saveProd360(e.Value, ext);
                            break;
                    }
                }
            }
            else
            {
                context.Response.Write("資料格式錯誤");
            }
        }
        // 多檔案上傳
        private void UploadMultiFiles(HttpContext context, List<FilesStatus> statuses)
        {
            for (int i = 0; i < context.Request.Files.Count; i++)
            {
                var file = context.Request.Files[i];
                bool check = false;
                string ext = Path.GetFileName(file.FileName).Split('.')[1];
                for (int j = 0; j < Restrict.Length; j++)
                {
                    if (string.Compare(Restrict[j], ext, true) == 0)
                    {
                        check = true;
                    }
                }
                if (check)
                {
                    file.SaveAs(StorageRoot + Path.GetFileName(file.FileName));

                    string fullName = Path.GetFileName(file.FileName);
                    statuses.Add(new FilesStatus(fullName, file.ContentLength, this.HandlerPath, this.DirectoryPath, orgName));
                }
                else
                {
                    context.Response.Write("資料格式錯誤");
                }
            }
        }
        //輸出Json
        private void WriteJson(HttpContext context, List<FilesStatus> statuses)
        {
            context.Response.AddHeader("Vary", "Accept");
            try
            {
                if (context.Request["HTTP_ACCEPT"].Contains("application/json"))
                    context.Response.ContentType = "application/json";
                else
                    context.Response.ContentType = "text/plain";
            }
            catch
            {
                context.Response.ContentType = "text/plain";
            }

            var jsonObj = js.Serialize(new { files = statuses.ToArray() });
            context.Response.Write(jsonObj);
        }

        #endregion
        #region updateDB
        public void saveProd360(int id, string ext)
        {
            using (SqlConnection conn = new SqlConnection(checkToken.setting))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    update menu set fileExt=@ext,img1=@img1 where id=@id
                ", conn);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                cmd.Parameters.Add(new SqlParameter("@ext", ext));
                cmd.Parameters.Add(new SqlParameter("@img1", "/upload/"+this.DirectoryPath+"/1."+ ext));
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
        #endregion
    }
}