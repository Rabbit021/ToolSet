using System;
using System.Data;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ToolSetsCore
{
    public static class JsonHelper
    {
        public static DataTable ToDataTable(this JArray array)
        {
            var dt = new DataTable();

            var header = array.FirstOrDefault() as JObject;
            if (header == null)
                return dt;
            var properties = header.Properties();
            foreach (var prop in properties)
            {
                var col = prop.Name;
                if (!dt.Columns.Contains(col))
                    dt.Columns.Add(col, typeof(string));
            }

            foreach (JObject obj in array)
            {
                var row = dt.NewRow();
                dt.Rows.Add(row);
                foreach (var prop in properties)
                    row[prop.Name] = obj.SelectToken(prop.Name) + "";
            }
            return dt;
        }

        public static JArray LoadJarrayFile(this string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                var token = JToken.Parse(json);
                if (token.Type == JTokenType.Object)
                {
                    return token.SelectToken("RECORDS") as JArray ?? new JArray();
                }
                else
                {
                    return token as JArray;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                return new JArray();
            }
        }
    }
}