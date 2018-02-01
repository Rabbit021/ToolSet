using System.Linq;
using iSagyBIMCore;
using Newtonsoft.Json.Linq;
using ToolSetsCore;

namespace UdpTester.CmdCore
{
    public class ClientCmdInfo : IClientCmdInfo
    {
        public string Cmd { get; set; }
        public string CmdName { get; set; }
        public JArray CmdParams { get; set; }
        public long Time { get; set; }

        public JToken this[string name]
        {
            get
            {
                var nan = "";
                try
                {
                    if (CmdParams == null) return nan;
                    var pair = CmdParams.FirstOrDefault(x => name.StrEqual(x.SelectToken("PKey") + ""));
                    return pair?.SelectToken("PValue");
                }
                catch
                {
                    return nan;
                }
            }
        }
    }
}