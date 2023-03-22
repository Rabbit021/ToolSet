using System;
using System.Collections.Generic;
using iSagyBIMCore;
using Newtonsoft.Json.Linq;

namespace UdpTester.CmdCore
{
    public interface ICmdSender
    {
        iSagyCommunication Communicator { get; set; }
        event Action<IClientCmdInfo> ProcessResonseEvent;
        IClientCmdInfo CreateCmd();
        IClientCmdInfo CreateCmd(Int64 time);
        IClientCmdInfo CreateCmd(string cmdName);
        IClientCmdInfo CreateCmd(string cmdName, params JObject[] parameters);
        IClientCmdInfo CreateCmd(string cmdName, JArray parameters);
        IClientCmdInfo CreateCmd(string cmdName, IDictionary<string, object> parameters);
        void Send(params IClientCmdInfo[] cmds);
    }

    public static class ClientCmdInfoExt
    {
        public static IClientCmdInfo Append(this IClientCmdInfo cmd, params JObject[] parameters)
        {
            foreach (var itr in parameters)
            {
                cmd.CmdParams.Add(itr);
            }
            return cmd;
        }

        public static IClientCmdInfo SetTime(this IClientCmdInfo cmd, Int64 time)
        {
            cmd.Time = time;
            return cmd;
        }

        public static IClientCmdInfo SetCmdName(this IClientCmdInfo cmd, string cmdName)
        {
            cmd.CmdName = cmdName;
            return cmd;
        }

        public static IClientCmdInfo SetCmdParams(this IClientCmdInfo cmd, JArray parameters)
        {
            cmd.CmdParams = parameters;
            return cmd;
        }

        public static IClientCmdInfo SetCmdParams(this IClientCmdInfo cmd, IDictionary<string, object> parameters)
        {
            try
            {
                var pp = new JArray();
                foreach (var itr in parameters)
                {
                    pp.Add(new JObject
                    {
                        {"PKey", JToken.FromObject( itr.Key)},
                        {"PValue", JToken.FromObject(itr.Value)}
                    });
                }
                cmd.CmdParams = pp;
                return cmd;

            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static IClientCmdInfo AddCmdParams(this IClientCmdInfo cmd, IDictionary<string, object> parameters)
        {
            try
            {
                cmd.CmdParams = cmd.CmdParams ?? new JArray();
                foreach (var itr in parameters)
                {
                    cmd.CmdParams.Add(new JObject
                    {
                        {"PKey", JToken.FromObject( itr.Key)},
                        {"PValue", JToken.FromObject(itr.Value)}
                    });
                }
                return cmd;

            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}