using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using iSagyBIMCore;
using iSagyUdpLib;
using Newtonsoft.Json.Linq;
using ToolSetsCore;
using UdpTester.CmdCore;

namespace UdpTester.Views
{
    [Export]
    [Export("UdpTesterView", typeof(UdpTesterView))]
    public partial class UdpTesterView : ToolSetViewBase
    {
        public UdpTesterView()
        {
            InitializeComponent();
            this.Loaded += UdpTesterView_Loaded;
            this.Unloaded += UdpTesterView_Unloaded;
            this.sender.Click += Sender_Click;
            this.btnRefresh.Click += BtnRefresh_Click;
            this.DataContext = this;
            this.cmdLsb.SelectionChanged += CmdLsb_SelectionChanged;
        }

        private void CmdLsb_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var cmd = cmdLsb.SelectedItem as CmdFile;
            if (cmd == null)
                return;
            this.msg.Text = cmd.CmdContent;
        }

        private void UdpTesterView_Loaded(object sender, RoutedEventArgs e)
        {

            StartUdp();

        }
        private void UdpTesterView_Unloaded(object sender, RoutedEventArgs e)
        {
            QuitUdp();
        }

        public void StartUdp()
        {
            try
            {
                iSagyUdpServer.Instance.ProcessEvent -= Instance_ProcessEvent;
                iSagyUdpServer.Instance.ProcessEvent += Instance_ProcessEvent;
                iSagyUdpServer.Instance.DefaultSetup();
                iSagyUdpServer.Instance.Start();
            }
            catch (Exception exception)
            {
                Logger.Log(exception.StackTrace);
            }
        }

        public void QuitUdp()
        {
            try
            {
                iSagyUdpServer.Instance.Quit();
            }
            catch (Exception exception)
            {
                Logger.Log(exception.StackTrace);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            var dir = tbPath.Text;
            if (string.IsNullOrEmpty(dir))
            {
                dir = AppDomain.CurrentDomain.BaseDirectory;
                tbPath.Text = dir;
            }
            if (!Directory.Exists(dir))
                return;
            // FileWatcher(dir, "*.json");
            LoadCmds();
        }

        private void Instance_ProcessEvent(IClientCmdInfo[] e)
        {
            var cmd = JToken.FromObject(e).ToString(Newtonsoft.Json.Formatting.Indented);
            Dispatcher.BeginInvoke(new Action(() => { ReceivedMsg.Text = cmd; }));
        }

        private void Sender_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cmd = this.msg.Text;
                var token = JToken.Parse(cmd);
                ClientCmdInfo[] cmds = null;
                switch (token.Type)
                {
                    case JTokenType.Array:
                        cmds = token.ToObject<ClientCmdInfo[]>();
                        foreach (var itr in cmds)
                        {
                            itr.SetTime(Environment.TickCount);
                        }
                        break;
                    case JTokenType.Object:
                        var client = token.ToObject<ClientCmdInfo>();
                        client.SetTime(Environment.TickCount);
                        cmds = new ClientCmdInfo[]
                        {
                            client
                        };
                        break;
                    default:
                        Logger.Log("命令不符合格式");
                        return;
                }
                var cmdstr = JToken.FromObject(cmds).ToString(Newtonsoft.Json.Formatting.None);
                iSagyUdpServer.Instance.SendMsg(cmdstr);
            }
            catch (Exception exception)
            {
                Logger.Log("命令不符合格式");
            }
        }

        public void LoadCmds()
        {
            var dir = tbPath.Text;
            Dispatcher.Invoke(new Action(() =>
                {
                    var files = Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories);
                    var rst = new List<CmdFile>();
                    foreach (var file in files)
                    {
                        var cmd = new CmdFile();
                        cmd.FileName = file.Replace(dir, "");
                        using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (var reander = new StreamReader(fs, System.Text.Encoding.UTF8))
                            {
                                cmd.CmdContent = reander.ReadToEnd();
                            }
                        }
                        rst.Add(cmd);
                    }
                    rst = rst.OrderBy(x => x.FileName).ToList();
                    Cmds = new ObservableCollection<CmdFile>(rst);
                }));
        }

        #region Cmds

        public ObservableCollection<CmdFile> Cmds
        {
            get { return (ObservableCollection<CmdFile>)GetValue(CmdsProperty); }
            set { SetValue(CmdsProperty, value); }
        }

        public static readonly DependencyProperty CmdsProperty =
            DependencyProperty.Register("Cmds", typeof(ObservableCollection<CmdFile>), typeof(UdpTesterView), new PropertyMetadata((sender, e) =>
            {
                var vm = sender as UdpTesterView;
                if (vm == null) return;
            }));

        #endregion
        public void FileWatcher(string path, string filter)
        {
            var watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.Filter = filter;

            watcher.Changed += new FileSystemEventHandler(OnProcess);
            watcher.Created += new FileSystemEventHandler(OnProcess);
            watcher.Deleted += new FileSystemEventHandler(OnProcess);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.EnableRaisingEvents = true;
        }

        private void OnRenamed(object o, RenamedEventArgs e)
        {
            LoadCmds();
        }

        private void OnProcess(object o, FileSystemEventArgs e)
        {
            LoadCmds();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            LoadCmds();
        }
    }

    public class CmdFile : DependencyObject
    {
        #region FileName

        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }

        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(CmdFile), new PropertyMetadata((sender, e) =>
            {
                var vm = sender as CmdFile;
                if (vm == null) return;
            }));

        #endregion

        #region CmdContent

        public string CmdContent
        {
            get { return (string)GetValue(CmdContentProperty); }
            set { SetValue(CmdContentProperty, value); }
        }

        public static readonly DependencyProperty CmdContentProperty =
            DependencyProperty.Register("CmdContent", typeof(string), typeof(CmdFile), new PropertyMetadata((sender, e) =>
            {
                var vm = sender as CmdFile;
                if (vm == null) return;
            }));

        #endregion
    }
}
