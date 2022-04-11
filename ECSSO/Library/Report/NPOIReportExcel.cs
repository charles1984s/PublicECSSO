using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ECSSO.Library.Report
{
    public class NPOIReportExcel
    {
        public byte[] EntityListToExcel2003(Dictionary<string, string> cellHeard, IList enList, string sheetName, byte[] odd, byte[] even)
        {
            byte[] response;
            try
            {
                MemoryStream ms = new MemoryStream();

                // 2.建議，設置表頭的中文名稱
                HSSFWorkbook workbook = new HSSFWorkbook(); //建立活頁簿
                ISheet sheet = workbook.CreateSheet(sheetName); // 工作表
                IRow row = sheet.CreateRow(0);
                List<string> keys = cellHeard.Keys.ToList();
                for (int i = 0; i < keys.Count; i++)
                {
                    row.CreateCell(i).SetCellValue(cellHeard[keys[i]]); // 列名为Key的值
                }
                // 建立儲存格樣式。
                ICellStyle style1 = workbook.CreateCellStyle();
                HSSFPalette palette = workbook.GetCustomPalette();
                palette.SetColorAtIndex(HSSFColor.Pink.Index, odd[0], odd[1], odd[2]);
                style1.FillForegroundColor = HSSFColor.Pink.Index;
                style1.FillPattern = FillPattern.SolidForeground;

                ICellStyle style2 = workbook.CreateCellStyle();
                HSSFPalette palette2 = workbook.GetCustomPalette();
                palette2.SetColorAtIndex(HSSFColor.Blue.Index, even[0], even[1], even[2]);
                style2.FillForegroundColor = HSSFColor.Blue.Index;
                style2.FillPattern = FillPattern.SolidForeground;

                // 3.List对象的值赋值到Excel的单元格里
                int rowIndex = 1; // 从第二行开始赋值(第一行已设置为单元头)
                foreach (var en in enList)
                {
                    ICellStyle style = style1;
                    IRow rowTmp = sheet.CreateRow(rowIndex);
                    for (int i = 0; i < keys.Count; i++) // 根据指定的属性名称，获取对象指定属性的值
                    {
                        string cellValue = ""; // 单元格的值
                        object properotyValue = null; // 属性的值
                        System.Reflection.PropertyInfo properotyInfo = null; // 属性的信息

                        // 3.1 若属性头的名称包含'.',就表示是子类里的属性，那么就要遍历子类，eg：UserEn.UserName
                        if (keys[i].IndexOf(".") >= 0)
                        {
                            // 3.1.1 解析子类属性(这里只解析1层子类，多层子类未处理)
                            string[] properotyArray = keys[i].Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                            string subClassName = properotyArray[0]; // '.'前面的为子类的名称
                            string subClassProperotyName = properotyArray[1]; // '.'后面的为子类的属性名称
                            System.Reflection.PropertyInfo subClassInfo = en.GetType().GetProperty(subClassName); // 获取子类的类型
                            if (subClassInfo != null)
                            {
                                // 3.1.2 获取子类的实例
                                var subClassEn = en.GetType().GetProperty(subClassName).GetValue(en, null);
                                // 3.1.3 根据属性名称获取子类里的属性类型
                                properotyInfo = subClassInfo.PropertyType.GetProperty(subClassProperotyName);
                                if (properotyInfo != null)
                                {
                                    properotyValue = properotyInfo.GetValue(subClassEn, null); // 获取子类属性的值
                                }
                            }
                        }
                        else
                        {
                            // 3.2 若不是子类的属性，直接根据属性名称获取对象对应的属性
                            properotyInfo = en.GetType().GetProperty(keys[i]);
                            if (properotyInfo != null)
                            {
                                properotyValue = properotyInfo.GetValue(en, null);
                            }
                        }

                        // 3.3 属性值经过转换赋值给单元格值
                        if (properotyValue != null)
                        {
                            cellValue = properotyValue.ToString();
                            // 3.3.1 对时间初始值赋值为空
                            if (cellValue.Trim() == "0001/1/1 0:00:00" || cellValue.Trim() == "0001/1/1 23:59:59")
                            {
                                cellValue = "";
                            }
                        }
                        if (rowIndex % 2 == 0) style = style2;
                        // 3.4 填充到Excel的单元格里
                        ICell cell = rowTmp.CreateCell(i);
                        cell.CellStyle = style;
                        cell.SetCellValue(cellValue);
                    }
                    rowIndex++;
                }

                // 4.生成文件
                workbook.Write(ms);
                response = ms.ToArray();
                ms.Close(); 
                ms.Dispose();

                // 5.返回檔案
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}