using System;
using System.Collections.Generic;
using System.Web;
using ZXing;
using ZXing.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.IO;

namespace ECSSO.api
{
    /// <summary>
    /// Code39 的摘要描述
    /// </summary>
    public class Code39 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Params["code"] != null)
            {
                if (context.Request.Params["code"].ToString() != null)
                {
                    String code = context.Request.Params["code"].ToString();
                    BarcodeWriter bw = new BarcodeWriter();
                    bw.Format = BarcodeFormat.CODE_39;
                    bw.Options.Width = 300;
                    bw.Options.Height = 100;

                    //Bitmap image = bw.Write(" " + code + " ");
                    Bitmap image = bw.Write(code);
                    MemoryStream ms = new MemoryStream();
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);

                    context.Response.ContentType = "image/gif";
                    /*
                     這行是讓瀏覽器download用
                     context.Response.AddHeader("content-disposition", "attachment;filename=abc.gif"); 
                     */
                    context.Response.BinaryWrite(ms.GetBuffer());


                }
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