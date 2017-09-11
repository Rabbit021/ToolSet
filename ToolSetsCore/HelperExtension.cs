using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace ToolSetsCore
{
    public static class StringHelper
    {
        //在字符串结尾去掉指定子字符串,isForce表示是否将子串后的也删除 
        public static string TrimEndString(this string oraStr, string trimEnd, bool isForce = true)
        {
            try
            {
                if ((!isForce) && (!oraStr.EndsWith(trimEnd))) return oraStr;

                int index = oraStr.LastIndexOf(trimEnd);
                if (index == -1) return oraStr;
                var rst = oraStr.Substring(0, index);
                return rst;
            }
            catch
            {
                return oraStr;
            }
        }

        public static string ReplaceSpecChar(this string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            var chars = new Dictionary<string, string>
            {
                {" ","" },
                { "-",""},
                { "（","("},
                { "）",")"}
            };
            foreach (var itr in chars)
                name = name.Replace(itr.Key, itr.Value);
            return name.Trim();
        }

        public static string RemoveSpecChar(this string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            var symbol = new[]
            {
                " ","-","(",")","（","）"
            };
            foreach (var itr in symbol)
                name = name.Replace(itr, "");
            return name.Trim();
        }
    }

    public static class Helper
    {
        public static T GetValue<T>(this JToken token, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new Exception("GetValue name is empty");
            try
            {
                var tk = token?.SelectToken(name);
                if (tk == null)
                    return default(T);
                return tk.Value<T>();
            }
            catch
            {

                return default(T);
            }
        }

    }

    public static class XmlExtension
    {
        private static Dictionary<string, string> SpecCharacters;

        static XmlExtension()
        {
            SpecCharacters = new Dictionary<string, string>();
            SpecCharacters.TryAdd("（", "(");
            SpecCharacters.TryAdd("）", ")");
            SpecCharacters.TryAdd(" ", "");
            SpecCharacters.TryAdd(" ", "");
        }

        public static string NormalizeName(this string str)
        {
            var builder = new StringBuilder(str);
            foreach (var itr in SpecCharacters)
                builder = builder.Replace(itr.Key, itr.Value);
            return builder.ToString().Trim();
        }

        public static string GetAttribte(this XmlNode node, string propName)
        {
            if (node == null || node.Attributes == null) return "没有";
            return node.Attributes[propName]?.Value;
        }

        public static TValue TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            var rst = default(TValue);
            if (dict == null || key == null) return rst;
            dict.TryGetValue(key, out rst);
            return rst;
        }

        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict == null) return false;
            if (dict.ContainsKey(key))
                return false;
            dict.Add(key, value);
            return true;
        }

        public static void AddValue2Row<T>(this DataRow row, string column, T Value)
        {
            var table = row.Table;
            if (!table.Columns.Contains(column))
                table.Columns.Add(column, typeof(T));
            row[column] = Value;
        }
    }

    public static class ExcelHelper
    {
        public static string GetSaveFilePath(string defaultName = "")
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = defaultName;
            dialog.Filter = "Excel File|*.xlsx";
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return string.Empty;
        }

        public static string GetOpenFilePath(string title = "")
        {
            var dialog = new OpenFileDialog() { Filter = "AllFile(*.*)|*.*|Excel File(*.xlsx)|*.xlsx" };
            dialog.Title = title;
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return string.Empty;
        }

        public static string GetOpenFilePath(string filter, string title)
        {
            var dialog = new OpenFileDialog() { Filter = filter };
            dialog.Title = title;
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return string.Empty;
        }

        public static ExcelPackage OpenExcel(string filename)
        {
            var package = new ExcelPackage(new FileInfo(filename));
            return package;
        }

        public static ExcelWorksheet CreateOrGetSheet(this ExcelPackage package, bool CreateNew = true, string sheetName = "sheet0")
        {
            var workbook = package.Workbook;
            var wookSheet = workbook.Worksheets[sheetName];
            if (CreateNew)
            {
                if (wookSheet != null)
                    workbook.Worksheets.Delete(sheetName);
                workbook.Worksheets.Add(sheetName);
                wookSheet = workbook.Worksheets[sheetName];
            }
            else
            {
                if (wookSheet == null)
                    workbook.Worksheets.Add(sheetName);
                wookSheet = workbook.Worksheets[sheetName];
            }
            return wookSheet;
        }

        public static ExcelWorksheet CreateOrGetSheetByIndex(this ExcelPackage package, int index = 1)
        {
            var workbook = package.Workbook;
            var wookSheet = workbook.Worksheets[index];
            return wookSheet;
        }

        public static ExcelRange SetCell(this ExcelWorksheet sheet, int row, int col, object value)
        {
            try
            {
                var cell = sheet.Cells[row, col];
                cell.Value = value;
                cell.AutoFitColumns();
                return cell;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static ExcelTable GetTable(this ExcelWorksheet sheet, string tableName = "table0")
        {
            var table = sheet.Tables[tableName];
            return table;
        }

        public static ExcelTable CreateTable(this ExcelWorksheet sheet, ExcelRange cells, string tableName = "table0")
        {
            var table = sheet.Tables.Add(cells, tableName);
            return table;
        }

        public static int GetColumnIndex(this ExcelTable table, string name)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].Name.StrEqual(name))
                    return table.Columns[i].Id;
            }
            return -1;
        }

        public static object GetCellValue(this ExcelTable table, int row, string name)
        {
            try
            {
                var col = table.GetColumnIndex(name);
                var val = table.WorkSheet.Cells[row, col].Value;
                return val;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        public static object GetCellValue(this ExcelTable table, int row, int col)
        {
            try
            {
                var val = table.WorkSheet.Cells[row, col].Value;
                return val;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        public static bool StrEqual(this string str, string st2)
        {
            return string.Equals(str, st2, StringComparison.CurrentCultureIgnoreCase);
        }

        public static void WriteDataTable(string path, DataTable dt, bool CreateNew = true)
        {
            try
            {
                using (var package = ExcelHelper.OpenExcel(path))
                {
                    var sheet = package.CreateOrGetSheet();

                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        sheet.SetCell(1, col + 1, dt.Columns[col]);
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            var row = dt.Rows[i];
                            sheet.SetCell(i + 2, col + 1, row[col]);
                        }
                    }
                    var start = sheet.Dimension.Start;
                    var end = sheet.Dimension.End;
                    var cells = sheet.Cells[start.Row, start.Column, end.Row, end.Column];
                    sheet.CreateTable(cells);
                    package.Save();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }

        public static void WriteJArray(string path, JArray jArray)
        {
            var dt = jArray.ToDataTable();
            WriteDataTable(path, dt);
        }
    }
}