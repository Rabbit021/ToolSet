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
    }
}