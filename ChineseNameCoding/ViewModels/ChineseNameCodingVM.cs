using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
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
        public ICommand DecondingCommand { get; set; }
        public ICommand RenameCommand { get; set; }

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
            DecondingCommand = new DelegateCommand(Deconding);
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
                     File.Move(Path.Combine(record.FilePath, record.OriginalName), Path.Combine(record.FilePath, record.EncodingName));
                     record.OriginalName = record.EncodingName;
                 }
                 catch (Exception exp)
                 {
                     record.Message = exp.Message;
                 }
             });
            Logger.LogState("文件重命名", true);
        }

        private void Deconding()
        {
            Logger.Log("不能够拼音转汉字");
            return;
            if (Records == null) return;
            Parallel.ForEach(Records, record =>
            {
                var originalName = Uri.UnescapeDataString(record.EncodingName);
                record.EncodingName = originalName;
            });
        }

        private void Enconding()
        {
            Logger.LogState("文件名中文转拼音");
            if (Records == null) return;
            Parallel.ForEach(Records, record =>
            {
                try
                {
                    var encodingName = PinyinHelper.GetPinyin(record.EncodingName);
                    record.EncodingName = encodingName;
                    record.Message = "处理成功！";
                }
                catch (Exception exp)
                {
                    record.Message = exp.Message;
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
                    string[] files = Directory.GetFiles(fbd.SelectedPath, "*", SearchOption.AllDirectories);
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
            Records = new ObservableCollection<CodingRecord>(filenames.Where(x => IsHasCHZN(x)).Select(x => new CodingRecord
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

    public class CodingRecord : BindableBase
    {
        private string _encodingName = string.Empty;
        private string _filePath = string.Empty;
        private string _originalName = string.Empty;
        private string _message = "未处理";


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
            set { SetProperty<string>(ref _message, value); }
        }
    }
}