using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using iSagyBIMCore;
using Newtonsoft.Json.Linq;
using LZ4;
using ToolSetsCore;
using UdpTester.CmdCore;

namespace iSagyUdpLib
{
    [Export(typeof(iSagyCommunication))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class iSagyUdpServer : iSagyCommunication
    {
        #region Instance

        static iSagyUdpServer instance = new iSagyUdpServer();

        public static iSagyUdpServer Instance
        {
            get
            {
                return instance;
            }
        }

        private iSagyUdpServer()
        {
        }


        #endregion

        int ReceivePort { get; set; }
        int PadPort { get; set; }
        bool IsQuiting { get; set; }
        UdpClient UdpReceiver { get; set; }

        IPEndPoint PadIpEndPoint = new IPEndPoint(IPAddress.Any, 0); //ipad地址
        IPEndPoint U3dIpEndPoint = new IPEndPoint(IPAddress.Any, 0); //U3D地址
        IPEndPoint senderIPEndPoint = new IPEndPoint(IPAddress.Any, 0); //临时地址
        Dictionary<int, List<UdpPacket>> DicUdpPacket = new Dictionary<int, List<UdpPacket>>();

        public void BuidUdpReceiver()
        {
            UdpReceiver = new UdpClient(ReceivePort);
            UdpReceiver.Client.SendBufferSize = UdpPacket.ChunkLength;
            UdpReceiver.Client.ReceiveTimeout = 3000;
            const long IOC_IN = 0x80000000;
            const long IOC_VENDOR = 0x18000000;
            const long SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

            byte[] optionInValue = { Convert.ToByte(false) };
            byte[] optionOutValue = new byte[4];
            UdpReceiver.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
        }

        public void CommandReceivedCompleted(IAsyncResult result)
        {
            if (IsQuiting) return;
            UdpClient u = result.AsyncState as UdpClient;
            try
            {
                byte[] recvData = u.EndReceive(result, ref senderIPEndPoint);
                this.UdpPacketReceived(recvData);
            }
            catch (Exception exp)
            {
                Logger.Log("CommandReceivedCompleted Error:" + exp.Message);
            }
            finally
            {
                if (!IsQuiting)
                {
                    try
                    {
                        if (u != null) u.BeginReceive(CommandReceivedCompleted, u);
                        else if (UdpReceiver != null) UdpReceiver.BeginReceive(CommandReceivedCompleted, UdpReceiver);
                    }
                    catch (Exception exp)
                    {
                        Logger.Log("CommandReceivedCompleted1 Error:" + exp.Message);
                    }
                }
            }
        }
        void UdpPacketReceived(byte[] recvData)
        {
            var pac = UdpPacket.FromArray(recvData);
            if (pac == null) return;
            var seq = pac.Sequence;
            if (!this.DicUdpPacket.Keys.Contains(seq))
            {
                this.DicUdpPacket.Add(seq, new List<UdpPacket>());
                //处理废弃包
                var keys = this.DicUdpPacket.Keys.Where(x => x < seq - 5).ToList();
                foreach (var key in keys)
                {
                    Console.WriteLine(key + ":" + this.DicUdpPacket[key].Count);
                    this.DicUdpPacket.Remove(key);
                }
            }
            var list = this.DicUdpPacket[seq];
            list.Add(pac);
            if (list.Count == pac.Total)//接收完毕
            {
                var datas = UdpPacket.Combine(list);
                this.DicUdpPacket.Remove(pac.Sequence);
                this.ProcessRecvData(senderIPEndPoint, datas);
            }
        }
        public void ProcessRecvData(IPEndPoint senderIPEndPoint, byte[] datas)
        {
            var str = DeCompress(datas);
            this.ProcessCmd(str);
        }

        #region iSagyCommunication

        public void DefaultSetup()
        {
            Setup(int.Parse(ConfigurationManager.AppSettings["ReceivePort"]),
                  int.Parse(ConfigurationManager.AppSettings["PadPort"]),
                  ConfigurationManager.AppSettings["U3dIpEndPoint"]);
        }

        public void Setup(int receivePort, int padPort, string ip)
        {
            this.ReceivePort = receivePort;
            this.PadPort = padPort;
            var strU3dIpEndPoint = ip.Replace("localhost", "127.0.0.1").Split(':');
            this.U3dIpEndPoint = new IPEndPoint(IPAddress.Parse(strU3dIpEndPoint[0]), int.Parse(strU3dIpEndPoint[1]));
        }

        public void Start()
        {
            this.BuidUdpReceiver();
            UdpReceiver.BeginReceive(CommandReceivedCompleted, UdpReceiver);
        }

        public event Action<IClientCmdInfo[]> ProcessEvent;
        public void ProcessCmd(string cmd)
        {
            var cmds = JArray.Parse(cmd).ToObject<ClientCmdInfo[]>();
            ProcessEvent?.Invoke(cmds);
        }

        public void SendMsg(string sendMsg)
        {
            try
            {
                byte[] bytes = Compress(sendMsg);
                var list = UdpPacket.Split(bytes);
                this.SendMessage(list, U3dIpEndPoint);
            }
            catch (Exception exp)
            {
                Logger.Log("Pad2U3D Error:" + exp.Message);
            }
        }

        public void ReceiveMsg(string sendMsg)
        {
            try
            {
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(sendMsg);
                var list = UdpPacket.Split(bytes);
                this.SendMessage(list, PadIpEndPoint);
            }
            catch (Exception exp)
            {
                Logger.Log("Pad2U3D Error:" + exp.Message);
            }
        }
        void SendMessage(List<UdpPacket> listPacket, IPEndPoint dstPt)
        {
            try
            {
                if (dstPt == null) return;
                foreach (UdpPacket udpPacket in listPacket)
                {
                    var bytes = udpPacket.ToArray();
                    UdpReceiver.Client.SendTo(bytes, 0, bytes.Length, SocketFlags.None, dstPt);
                }
            }
            catch (Exception exp)
            {
                Logger.Log("SendMessage Error:" + exp.Message);
            }
        }

        public void Quit()
        {
            try
            {
                IsQuiting = true;
                if (UdpReceiver != null)
                {
                    UdpReceiver.Close();
                }
            }
            catch
            {
            }
        }

        #endregion

        #region LZ4 解压缩
        public byte[] Compress(string str)
        {
            var datas = Encoding.UTF8.GetBytes(str);
            var rst = LZ4Codec.WrapHC(datas, 0, datas.Length);
            return rst;
        }
        public string DeCompress(byte[] datas)
        {
            var unCopress = LZ4Codec.Unwrap(datas);
            return Encoding.UTF8.GetString(unCopress);
        }

        #endregion
    }
    public class UdpPacket
    {
        public const int ChunkLength = 4096;        //分割块的长度
        static int CurrentSequence = 0;

        public int Sequence { get; set; }     //所属组的唯一序列号 包编号  
        public int Index { get; set; }        //消息包的索引  
        public int Total { get; set; }        //分包总数  
        public int DataLength { get; set; }   //分割的数组包大小  
        public int Remainder { get; set; }    //最后剩余的数组的数据长度  
        public byte[] Data { get; set; }        //包的内容数组  

        public UdpPacket(int sequence, int total, int index, int dataLength, int remainder, byte[] data)
        {
            this.Sequence = sequence;
            this.Total = total;
            this.Index = index;
            this.Data = data;
            this.DataLength = dataLength;
            this.Remainder = remainder;
        }

        public byte[] ToArray()
        {
            List<byte> list = new List<byte>();
            list.AddRange(BitConverter.GetBytes(this.Sequence));
            list.AddRange(BitConverter.GetBytes(this.Total));
            list.AddRange(BitConverter.GetBytes(this.Index));
            list.AddRange(BitConverter.GetBytes(this.DataLength));
            list.AddRange(BitConverter.GetBytes(this.Remainder));
            list.AddRange(this.Data);
            return list.ToArray();
        }
        public static UdpPacket FromArray(byte[] oraArr)
        {
            try
            {
                int sequence = BitConverter.ToInt32(oraArr, 0);
                int total = BitConverter.ToInt32(oraArr, 4);
                int index = BitConverter.ToInt32(oraArr, 8);
                int dataLength = BitConverter.ToInt32(oraArr, 12);
                int remainder = BitConverter.ToInt32(oraArr, 16);
                byte[] data = new byte[dataLength];
                if (total == index)//最后一包
                {
                    Buffer.BlockCopy(oraArr, oraArr.Length - remainder, data, 0, remainder);
                }
                else
                {
                    Buffer.BlockCopy(oraArr, oraArr.Length - dataLength, data, 0, dataLength);
                }
                return new UdpPacket(sequence, total, index, dataLength, remainder, data);
            }
            catch (Exception exp)
            {
                return null;
            }
        }

        // 分割UDP数据包   
        public static List<UdpPacket> Split(byte[] datagram)
        {
            if (datagram == null)
                throw new ArgumentNullException("datagram");

            if (CurrentSequence >= int.MaxValue)
                CurrentSequence = 0;
            CurrentSequence++;

            List<UdpPacket> packets = new List<UdpPacket>();

            int chunks = datagram.Length / ChunkLength;
            int remainder = datagram.Length % ChunkLength;
            int total = chunks;
            if (remainder > 0) total++;

            for (int i = 1; i <= chunks; i++)
            {
                byte[] chunk = new byte[ChunkLength];
                Buffer.BlockCopy(datagram, (i - 1) * ChunkLength, chunk, 0, ChunkLength);
                packets.Add(new UdpPacket(CurrentSequence, total, i, ChunkLength, remainder, chunk));
            }
            if (remainder > 0)
            {
                int length = datagram.Length - (ChunkLength * chunks);
                byte[] chunk = new byte[length];
                Buffer.BlockCopy(datagram, ChunkLength * chunks, chunk, 0, length);
                packets.Add(new UdpPacket(CurrentSequence, total, total, ChunkLength, remainder, chunk));
            }

            return packets;
        }
        //组包,还原原始数据
        public static byte[] Combine(List<UdpPacket> list)
        {
            var sortlist = list.OrderBy(x => x.Index).ToList();
            List<byte> dstlist = new List<byte>();
            foreach (var item in sortlist)
                dstlist.AddRange(item.Data);
            return dstlist.ToArray();
        }
    }
}