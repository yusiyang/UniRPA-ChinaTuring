using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Uni.Common
{
    /// <summary>
    /// NPOI操作类
    /// </summary>
    public class NPOIUtility
    {
        /// <summary>读取excel 到datatable    
        /// 默认第一行为表头，导入第一个工作表   
        /// </summary>      
        /// <param name="fileName">excel文档路径</param>      
        /// <returns></returns>      
        public static DataTable ExcelToDataTable(string fileName)
        {
            if (!Directory.Exists(fileName))
            {
                return null;
            }
            DataTable dt = new DataTable();
            FileStream file = null;
            IWorkbook Workbook = null;
            using (file = new FileStream(fileName, FileMode.Open, FileAccess.Read))//C#文件流读取文件
            {
                if (fileName.IndexOf(".xlsx") > 0)
                    //把xlsx文件中的数据写入Workbook中
                    Workbook = new XSSFWorkbook(file);

                else if (fileName.IndexOf(".xls") > 0)
                    //把xls文件中的数据写入Workbook中
                    Workbook = new HSSFWorkbook(file);
                if (Workbook != null)
                {
                    ISheet sheet = Workbook.GetSheetAt(0);//读取第一个sheet
                    System.Collections.IEnumerator rows = sheet.GetRowEnumerator();
                    //得到Excel工作表的行 
                    IRow headerRow = sheet.GetRow(0);
                    //得到Excel工作表的总列数  
                    int cellCount = headerRow.LastCellNum;
                    for (int j = 0; j < cellCount; j++)
                    {
                        //得到Excel工作表指定行的单元格  
                        ICell cell = headerRow.GetCell(j);
                        dt.Columns.Add(cell.ToString());
                    }
                    for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        DataRow dataRow = dt.NewRow();
                        if (row.GetCell(0) != null && row.GetCell(0).ToString() != "")
                        {
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {
                                try
                                {
                                    if (row.GetCell(j).CellType == CellType.Formula)
                                    {
                                        dataRow[j] = row.GetCell(j).StringCellValue;
                                    }
                                    else
                                    {
                                        dataRow[j] = row.GetCell(j) + "";
                                    }
                                }
                                catch
                                {
                                    var a = i;
                                }
                            }
                            dt.Rows.Add(dataRow);
                        }
                    }
                }
                return dt;
            }
        }

        /// <summary>
        /// 获取第一个sheet的行内容所对应的列索引名集合
        /// </summary>
        /// <param name="fileName">excel文件全路径</param>
        /// <returns>列索引名集合</returns>
        public static Dictionary<string, string> GetColumnNamesByCellContent(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }
            Dictionary<string, string> dic = new Dictionary<string, string>();
            using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))//C#文件流读取文件
            {
                IWorkbook workbook = null;
                if (fileName.IndexOf(".xlsx") > 0)
                {
                    //把xlsx文件中的数据写入Workbook中
                    workbook = new XSSFWorkbook(file);
                }
                else if (fileName.IndexOf(".xls") > 0)
                {
                    //把xls文件中的数据写入Workbook中
                    workbook = new HSSFWorkbook(file);
                }
                if (workbook != null)
                {
                    ISheet sheet = workbook.GetSheetAt(0);//读取第一个sheet
                    System.Collections.IEnumerator rows = sheet.GetRowEnumerator();

                    while (rows.MoveNext())
                    {
                        IRow currRow = (IRow)rows.Current;
                        int cellCount = currRow.LastCellNum;
                        for (int j = 0; j < cellCount; j++)
                        {
                            //得到Excel工作表指定行的单元格  
                            ICell cell = currRow.GetCell(j);
                            string cellContent = cell?.ToString();
                            if (!string.IsNullOrWhiteSpace(cellContent))
                            {
                                dic.Add(ConvertColumnIndexToColumnName(j), cellContent);
                            }
                        }
                        if (dic.Any())
                        {
                            break;
                        }
                    }

                }
                return dic;
            }
        }

        /// <summary>
        /// 根据索引获取列名
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string ConvertColumnIndexToColumnName(int index)
        {
            index = index + 1;
            int system = 26;
            char[] digArray = new char[100];
            int i = 0;
            while (index > 0)
            {
                int mod = index % system;
                if (mod == 0)
                {
                    mod = system;
                }
                digArray[i++] = (char)(mod - 1 + 'A');
                index = (index - 1) / 26;
            }
            StringBuilder sb = new StringBuilder(i);

            for (int j = i - 1; j >= 0; j--)
            {
                sb.Append(digArray[j]);
            }
            return sb.ToString();

        }
    }
}
