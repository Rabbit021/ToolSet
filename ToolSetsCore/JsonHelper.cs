using System;
using System.Data;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
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

        public static string StrValue(this JToken token, string Key)
        {
            return token.Value<string>(Key);
        }

        public static void SetToken(this JToken root, string path, JToken value)
        {
            if (root == null) return;
            var first = root.SelectToken(path);
            if (first == null)
            {
                var aim = root.CreateIfEmpty(path);
                aim.Replace(value == null ? JValue.CreateNull() : value);
            }
            else
            {
                first.Replace(value == null ? JValue.CreateNull() : value);
            }
        }

        public static JToken CreateIfEmpty(this JToken root, string path)
        {
            try
            {
                var item = root;
                var lstPath = path.Split('.');
                for (var i = 0; i < lstPath.Length; i++)
                {
                    var subPath = lstPath[i];
                    var subItem = item.SelectToken(subPath);
                    if (subItem == null)
                    {
                        if (i < lstPath.Length - 1 && lstPath[i + 1].StartsWith("["))
                            subItem = new JArray();
                        else
                            subItem = new JObject();
                        var arr = item as JArray;
                        if (arr != null)
                        {
                            var len = int.Parse(subPath.TrimStart('[').TrimEnd(']'));
                            for (var j = arr.Count; j < len; j++)
                            {
                                arr.Add(new JObject());
                            }
                            if (arr.Count < len + 1) arr.Add(subItem);
                            else arr[len] = subItem;
                        }
                        else
                        {
                            item[subPath] = subItem;
                        }
                    }
                    item = subItem;
                }
                return item;
            }
            finally
            {
            }
        }

        public static T Prase<T>(this string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                    return default(T);
                return JsonConvert.DeserializeObject<T>(str);
            }
            catch (Exception e)
            {
                return default(T);
            }
        }

        public static T GetValue<T>(this JToken token, string key, T defautlValue)
        {
            try
            {
                if (token == null)
                    return defautlValue;
                var res = token.SelectToken(key);
                if (res == null) return defautlValue;
                return res.ToObject<T>();
            }
            catch
            {
                return defautlValue;
            }
        }

        public static T GetValue<T>(this JToken token, string key)
        {
            try
            {
                if (token == null)
                    return default(T);
                var res = token.SelectToken(key);
                if (res == null) return default(T);
                return res.ToObject<T>();
            }
            catch
            {
                return default(T);
            }
        }

        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == string.Empty) ||
                   (token.Type == JTokenType.Null);
        }
    }
   
}