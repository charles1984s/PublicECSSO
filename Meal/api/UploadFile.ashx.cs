using System;
using System.Collections.Generic;
using System.Web;
using Meal.Library;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Net;
using System.IO;

namespace Meal.api
{
    /// <summary>
    /// UploadFile 的摘要描述
    /// </summary>
    public class UploadFile : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Form["CheckM"] == null) ResponseWriteEnd(context, ErrorMsg("error", "CheckM必填"));
            if (context.Request.Form["OrgName"] == null) ResponseWriteEnd(context, ErrorMsg("error", "OrgName必填"));
            //if (context.Request.Form["type"] == null) ResponseWriteEnd(context, ErrorMsg("error", "type必填"));

            String CheckM = context.Request.Form["CheckM"].ToString();
            String OrgName = context.Request.Form["OrgName"].ToString();
            //String type = context.Request.Form["Type"].ToString();
            String type = "";
            HttpPostedFile fileNamePath = context.Request.Files["fileNamePath"];
            if (fileNamePath == null) 
            {
                ResponseWriteEnd(context, "請提供檔案");
            }

            GetMealStr GS = new GetMealStr();
            if (GS.MD5Check(type + OrgName, CheckM))
            {
                String Setting = GS.GetSetting(OrgName);
                
                if (fileNamePath != null)
                {
                    string fileName = fileNamePath.FileName;
                    string suffix = fileName.Substring(fileName.LastIndexOf(".") + 1).ToLower();
                    String NewFileName = DateTime.Now.ToString("yyyyMMddhhmmss") + "." + suffix;
                    String UploadFileName = @"~/upload/" + OrgName + @"/" + NewFileName;

                    try
                    {
                        fileNamePath.SaveAs(HttpContext.Current.Server.MapPath(UploadFileName));     //儲存圖片
                        ResponseWriteEnd(context, NewFileName); //上傳成功
                    }
                    catch (Exception ex)
                    {
                        //ResponseWriteEnd(context, ex.Message.ToString()); //上傳失敗
                    }
                }
                

            }
            else {
                ResponseWriteEnd(context, "CheckM error");
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
        private String ErrorMsg(String RspnCode, String RspnMsg)
        {

            Library.Meal.VoidReturn root = new Library.Meal.VoidReturn();
            root.RspnCode = RspnCode;
            root.RspnMsg = RspnMsg;
            return JsonConvert.SerializeObject(root);

        }
        #endregion

        private void ResponseWriteEnd(HttpContext context, string msg)
        {
            context.Response.Write(msg);
            context.Response.End();
        }

        public String UpLoadFile(string fileNamePath, string uriString, bool IsAutoRename)
        {

            int indexOf = 0;

            if (fileNamePath.Contains(@"\"))
            {
                indexOf = fileNamePath.LastIndexOf(@"\");
            }
            else if (fileNamePath.Contains("/"))
            {
                indexOf = fileNamePath.LastIndexOf("/");
            }

            string fileName = fileNamePath.Substring(indexOf + 1);
            string NewFileName = fileName;

            if (IsAutoRename)
            {
                NewFileName = DateTime.Now.ToString("yyMMddhhmmss") + DateTime.Now.Millisecond.ToString() + fileNamePath.Substring(fileNamePath.LastIndexOf("."));
            }

            string fileNameExt = fileName.Substring(fileName.LastIndexOf(".") + 1);

            if (uriString.EndsWith("/") == false) uriString = uriString + "/";

            uriString = uriString + NewFileName;

            /// 創建WebClient實例
            WebClient myWebClient = new WebClient();
            myWebClient.Credentials = CredentialCache.DefaultCredentials;
            // 要上傳的檔
            FileStream fs = new FileStream(fileNamePath, FileMode.Open, FileAccess.Read);
            //FileStream fs = OpenFile();
            BinaryReader r = new BinaryReader(fs);
            byte[] postArray = r.ReadBytes((int)fs.Length);
            Stream postStream = myWebClient.OpenWrite(uriString, "PUT");
            try
            {
                //使用UploadFile方法可以用下面的格式
                //myWebClient.UploadFile(uriString,"PUT",fileNamePath);
                if (postStream.CanWrite)
                {
                    postStream.Write(postArray, 0, postArray.Length);
                    postStream.Close();
                    fs.Dispose();
                }
                else
                {
                    postStream.Close();
                    fs.Dispose();
                }

                return uriString;
            }
            catch (Exception err)
            {
                postStream.Close();
                fs.Dispose();
                throw err;
            }
            finally
            {
                postStream.Close();
                fs.Dispose();
            }
        }
    }
}