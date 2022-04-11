using System;
using System.Collections.Generic;
using System.Web;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.IO;

namespace ECSSO.api
{
    /// <summary>
    /// QRCode 的摘要描述
    /// </summary>
    public class QRCode : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Params["code"] != null)
            {
                if (context.Request.Params["code"].ToString() != null) 
                {
                    String code = context.Request.Params["code"].ToString();
                    BarcodeWriter bw = new BarcodeWriter();
                    bw.Format = BarcodeFormat.QR_CODE;
                    bw.Options.Width = 300;
                    bw.Options.Height = 300;

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