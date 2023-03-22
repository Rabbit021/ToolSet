using System.Collections.Generic;
using iSagyBIMCore;
using Newtonsoft.Json.Linq;

namespace UdpTester.CmdCore
{
    public interface IPraseCmd
    {
        iSagyCommunication Communicator { get; set; }
        ICmdSender CmdSender { get; set; }
        void FormatCmd(JToken seleccted);
        void Prase(JToken newValue);
        JToken CreateCmd(JToken newVaue);
        JToken CreateClearCmd(JToken value);
        JToken CreateCmd(IEnumerable<JToken> points);
        void SendMsg(params object[] cmds);
    }
}