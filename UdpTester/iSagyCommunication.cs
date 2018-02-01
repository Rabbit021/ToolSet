using System;
using Newtonsoft.Json.Linq;

namespace iSagyBIMCore
{
    public interface iSagyCommunication
    {
        event Action<IClientCmdInfo[]> ProcessEvent;
        void Start();
        void SendMsg(string sendMsg);
        void ReceiveMsg(string sendMsg);
        void ProcessCmd(string cmd);
        void Quit();
    }

    public interface IClientCmdInfo : ICommunicationArgs
    {
        Int64 Time { get; set; }
        string CmdName { get; set; }
        JArray CmdParams { get; set; }
        JToken this[string name] { get; }
    }

    public interface ICommunicationArgs
    {
        string Cmd { get; set; }
    }
}