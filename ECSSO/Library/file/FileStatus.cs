using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ECSSO.Library.file
{
    public class FileStatus
    {
        public string name { get; set; }
        public int size { get; set; }
        public string url { get; set; }
        public string thumbnailUrl { get; set; }
        public string deleteUrl { get; set; }
        public string deleteType { get; set; }
        public string error { get; set; }
        #region Constructor
        public FileStatus() { }
        public FileStatus(string fileName, int fileLength, string handlerPath, string directoryPath) { SetValues(fileName, fileLength, handlerPath, directoryPath); }
        public FileStatus(FileInfo fileInfo, string handlerPath, string directoryPath)
        {
            SetValues(fileInfo.Name, (int)fileInfo.Length, handlerPath, directoryPath);
            try
            {
                System.Drawing.Bitmap image = new System.Drawing.Bitmap(fileInfo.FullName);
                image.Dispose();
            }
            catch (Exception ex) { this.thumbnailUrl = ""; }
        }

        #endregion
        private void SetValues(string fileName, int fileLength, string handlerPath, string directoryPath)
        {

            name = fileName;
            size = fileLength;
            url = string.Format("{0}FileTransferHandler.ashx?f={1}", handlerPath, fileName);
            thumbnailUrl = string.Format("{0}Thumbnail.ashx?f={1}", handlerPath, fileName);
            deleteUrl = string.Format("{0}FileTransferHandler.ashx?f={1}", handlerPath, fileName);
            deleteType = "DELETE";
            if (!string.IsNullOrEmpty(directoryPath))
            {
                url += string.Format("&d={0}", directoryPath);
                thumbnailUrl += string.Format("&d={0}", directoryPath);
                deleteUrl += string.Format("&d={0}", directoryPath);
            }

        }
    }
}