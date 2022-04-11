using System;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Web.Administration;
using Newtonsoft.Json;

namespace ECSSO
{
    /// <summary>
    /// Flow 的摘要描述
    /// </summary>
    public class Flow : IHttpHandler
    {
        private HttpContext myContext;
        string dateStr,orgName;
        public void ProcessRequest(HttpContext context)
        {
            myContext = context;
            context.Response.ContentType = "text/plain";
            using (ServerManager mgr = new ServerManager())
            {
                orgName = "develop";
                Site site = mgr.Sites[orgName];
                Site sso = mgr.Sites["SSO"];
                if (site != null)
                {
                    SiteLogFile logFile = site.LogFile;
                    dateStr = DateTime.Now.AddDays(-1).ToString("yyMMdd");
                    string dirPath = logFile.Directory.Replace("%SystemDrive%","c:") + "\\W3SVC" + site.Id+"\\";
                    string ssoPath = logFile.Directory.Replace("%SystemDrive%","c:") + "\\W3SVC" + sso.Id+"\\";
                    string fileName = dirPath+"u_ex" + dateStr + ".log";
                    string ssoFileName = ssoPath+"u_ex" + dateStr + ".log";
                    if (File.Exists(fileName)) {
                        try
                        {
                            myContext.Response.Write("前台\n");
                            readLoad(new StreamReader(fileName), orgName,false);
                            myContext.Response.Write("後台\n");
                            readLoad(new StreamReader(ssoFileName), orgName,true);
                        }catch { }
                        
                        
                    }
                }
            }
        }
        private void readLoad(StreamReader file, string orgName,bool checkOrgName)
        {
            try
            {
                string line = "";
                int counter = 0, upload = 0, donwload = 0;
                bool start = false;
                while ((line = file.ReadLine()) != null)
                {
                    if (start && (!checkOrgName||(checkOrgName && line.IndexOf(orgName) > 0)))
                    {
                        string[] data = line.Split(' ');
                        int number = 0;
                        if (data.Length == 16)
                        {
                            if (int.TryParse(data[14], out number)) upload += Convert.ToInt32(data[14]);
                            if (int.TryParse(data[13], out number)) donwload += Convert.ToInt32(data[13]);
                            //context.Response.Write(line + "\n");
                            //context.Response.Write(data.Length + "\n");
                            counter++;
                        }
                    }
                    if (line.IndexOf("sc-bytes cs-bytes") > 0) start = true;
                }
                myContext.Response.Write("網站代碼 " + orgName + " 在" + dateStr + "時\n");
                myContext.Response.Write("共有" + counter + "筆記錄\n");
                myContext.Response.Write("下載流量為:" + converByte(donwload) + "\n");
                myContext.Response.Write("上傳流量為:" + converByte(upload) + "\n");
            }
            catch
            {

            }
            finally
            {
                if (file != null) file.Close();
            }
        }
        private string converByte(double size) { return converByte(size, 0); }
        private string converByte(double size, int times)
        {
            string unite = "";
            if (size > 1024) return converByte(size / 1024, ++times);
            else
            {
                switch (times)
                {
                    case 1:
                        unite = "KB";
                        break;
                    case 2:
                        unite = "MB";
                        break;
                    case 3:
                        unite = "GB";
                        break;
                    case 4:
                        unite = "TB";
                        break;
                    default:
                        unite = "Bytes";
                        break;
                }
                return Math.Round(size, 2).ToString() + unite;
            }
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