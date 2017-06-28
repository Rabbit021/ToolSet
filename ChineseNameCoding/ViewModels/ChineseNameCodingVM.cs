using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using ToolSetsCore;

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
            Parallel.ForEach(Records, record =>
            {
                // 重命名
                File.Move(Path.Combine(record.FilePath, record.OriginalName), Path.Combine(record.FilePath, record.EncodingName));
                record.OriginalName = record.EncodingName;
            });
        }

        private void Deconding()
        {
            Parallel.ForEach(Records, record =>
            {
                var originalName = Uri.UnescapeDataString(record.EncodingName);
                record.EncodingName = originalName;
            });
        }

        private void Enconding()
        {
            Parallel.ForEach(Records, record =>
            {
                var encodingName = Uri.EscapeUriString(record.OriginalName);
                record.EncodingName = encodingName;
            });
        }

        private void OpenFolder()
        {
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
                Records = new ObservableCollection<CodingRecord>(dialog.FileNames.Select(x => new CodingRecord
                {
                    OriginalName = Path.GetFileName(x),
                    EncodingName = Path.GetFileName(x),
                    FilePath = Path.GetDirectoryName(x)
                }));
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
    }
}