using System.Collections.Generic;
using System.Linq;
using iSagyBIMCore;
using Newtonsoft.Json.Linq;

namespace UdpTester.CmdCore
{
    public abstract class PraseCmdBase : IPraseCmd
    {
        private string _CmdName;

        public PraseCmdBase(iSagyCommunication communicator, ICmdSender cmdsender)
        {
            Communicator = communicator;
            CmdSender = cmdsender;
        }

        public string CmdName
        {
            get
            {
                if (!string.IsNullOrEmpty(_CmdName))
                    return _CmdName;
                var cmd = GetType().GetCustomAttributes(false).FirstOrDefault() as CmdAttribute;
                if (cmd != null)
                    _CmdName = cmd.Name;
                return _CmdName;
            }
        }

        public iSagyCommunication Communicator { get; set; }
        public ICmdSender CmdSender { get; set; }
        public void FormatCmd(JToken seleccted)
        {
        }

        public virtual void Prase(JToken newValue)
        {
            var cmd = CreateCmd(newValue);
            if (cmd == null) return;
            if (cmd.Type == JTokenType.Array) // 多个命令
            {
                var ary = cmd as JArray;
                var cmds = ary.Select(x => x).ToArray();
                SendMsg(cmds);
            }
            else
            {
                SendMsg(cmd);
            }
        }

        public abstract JToken CreateCmd(JToken newVaue);
        // TODO 
        public virtual JToken CreateCmd(IEnumerable<JToken> points)
        {
            return null;
        }

        public virtual void SendMsg(params object[] cmds)
        {
            var str = JToken.FromObject(cmds).ToString(Newtonsoft.Json.Formatting.Indented);
            Communicator.SendMsg(str);
        }

        public virtual JToken CreateClearCmd(JToken value)
        {
            var cmd = new JObject();
            cmd.Add("CmdName", CmdName);
            cmd.Add("CmdParams", new JArray());
            return cmd;
        }
    }
}