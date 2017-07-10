using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Chinese2Pinyin;
using Prism.Commands;
using Prism.Mvvm;
using ToolSetsCore;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ChineseNameCoding.ViewModels
{
    [Export(typeof(ChineseNameCodingVM))]
    public class ChineseNameCodingVM : ToolSetVMBase
    {
        public ICommand EncondingCommand { get; set; }
        public ICommand VaildateNameCommand { get; set; }
        public ICommand RenameCommand { get; set; }
        protected HashSet<string> UniqueNames { get; set; } = new HashSet<string>();

        public override void RegisterEvents()
        {
            base.RegisterEvents();
            PublicDatas.Instance.Aggregator?.GetEvent<OpenFileEvent>().Subscribe(OpenFile);
            PublicDatas.Instance.Aggregator?.GetEvent<OpenFolderEvent>().Subscribe(OpenFolder);
        }

        public override void UnRegisterEvents()
        {
            base.UnRegisterEvents();
            PublicDatas.Instance.Aggregator?.GetEvent<OpenFileEvent>().Unsubscribe(OpenFile);
            PublicDatas.Instance.Aggregator?.GetEvent<OpenFolderEvent>().Unsubscribe(OpenFolder);
        }

        public override void RegisterCommands()
        {
            base.RegisterCommands();
            EncondingCommand = new DelegateCommand(Enconding);
            VaildateNameCommand = new DelegateCommand(VaildateName);
            RenameCommand = new DelegateCommand(Rename);
        }

        private void Rename()
        {
            Logger.LogState("文件重命名");
            var rst = Parallel.ForEach(Records, record =>
            {
                try
                {
                    // 重命名
                    record.State = 0;
                    File.Move(Path.Combine(record.FilePath, record.OriginalName), Path.Combine(record.FilePath, record.EncodingName));
                    record.OriginalName = record.EncodingName;
                    record.SetState(1);
                }
                catch (Exception exp)
                {
                    record.SetState(2, exp);
                }
            });
            Logger.LogState("文件重命名", true);
        }

        private void VaildateName()
        {
            Logger.Log("验证重名");
            if (Records == null) return;
            var dict = new Dictionary<string, CodingRecord>();
            foreach (var record in Records)
            {
                record.SetState(0);
                var name = Path.GetFileNameWithoutExtension(record.EncodingName);
                CodingRecord exist = null;
                dict.TryGetValue(name, out exist);
                if (exist == null)
                {
                    dict.Add(name, record);
                    record.SetState(1);
                }
                else
                {
                    exist.SetState(2);
                    record.SetState(2);
                    Logger.Log("[重名：]" + name);
                }
            }
        }

        private void Enconding()
        {
            Logger.LogState("文件名中文转拼音");
            UniqueNames.Clear();
            if (Records == null) return;

            Parallel.ForEach(Records, record =>
            {
                try
                {
                    var encodingName = PinyinHelper.GetPinyin(record.EncodingName);
                    record.EncodingName = encodingName;
                    if (UniqueNames.Add(Path.Combine(record.FilePath, encodingName)))
                    {
                        record.SetState(1);
                    }
                    else
                    {
                        record.SetState(2, "转拼音后重名！");
                        Logger.Log($"{Path.Combine(record.FilePath, record.EncodingName)} 转拼音后重名！");
                    }
                }
                catch (Exception exp)
                {
                    record.SetState(2, exp);
                }
            });
            Logger.LogState("文件名中文转拼音", true);
        }

        private void OpenFolder()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                var result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    var files = Directory.GetFiles(fbd.SelectedPath, "*", SearchOption.AllDirectories);
                    LoadRecords(files);
                }
            }
        }

        private void OpenFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "All|*.*";
            dialog.FileOk += Dialog_FileOk;
            dialog.ShowDialog();
        }

        private void Dialog_FileOk(object sender, CancelEventArgs e)
        {
            var dialog = sender as OpenFileDialog;
            if (dialog != null)
                LoadRecords(dialog.FileNames);
        }

        private void LoadRecords(IEnumerable<string> filenames)
        {
            Records = new ObservableCollection<CodingRecord>(filenames.Select(x => new CodingRecord
            {
                OriginalName = Path.GetFileName(x),
                EncodingName = Path.GetFileName(x),
                FilePath = Path.GetDirectoryName(x)
            }));
        }

        public static bool IsHasCHZN(string inputData)
        {
            var RegCHZN = new Regex("[\u4e00-\u9fa5]");
            var m = RegCHZN.Match(inputData);
            return m.Success;
        }

        #region Records

        public ObservableCollection<CodingRecord> Records
        {
            get { return (ObservableCollection<CodingRecord>)GetValue(RecordsProperty); }
            set { SetValue(RecordsProperty, value); }
        }

        public static readonly DependencyProperty RecordsProperty =
            DependencyProperty.Register("Records", typeof(ObservableCollection<CodingRecord>), typeof(ChineseNameCodingVM), new PropertyMetadata((sender, e) =>
            {
                var vm = sender as ChineseNameCodingVM;
                if (vm == null) return;
            }));

        #endregion
    }

    public class CodingRecord : BindableBase, IProcessStatus
    {
        private string _encodingName = string.Empty;
        private string _filePath = string.Empty;
        private string _message = "未处理";
        private string _originalName = string.Empty;
        private int _state; // 0 未处理 1 成功 2，失败

        public int State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        public string OriginalName
        {
            get { return _originalName; }
            set { SetProperty(ref _originalName, value); }
        }

        public string EncodingName
        {
            get { return _encodingName; }
            set { SetProperty(ref _encodingName, value); }
        }

        public string FilePath
        {
            get { return _filePath; }
            set { SetProperty(ref _filePath, value); }
        }

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public void SetState(int state, Exception exp)
        {
            Message = exp.Message;
            State = 2;
        }

        public void SetState(int state, string msg)
        {
            Message = msg;
            State = 2;
        }

        public void SetState(int state)
        {
            State = state;
            switch (state)
            {
                case 0:
                    Message = "未处理";
                    break;
                case 1:
                    Message = "处理成功";
                    break;
                case 2:
                    Message = "处理失败";
                    break;
            }
        }
    }
}