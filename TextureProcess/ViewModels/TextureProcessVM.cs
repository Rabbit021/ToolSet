using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using Prism.Commands;
using Prism.Mvvm;
using ToolSetsCore;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Size = System.Drawing.Size;

namespace TextureProcess.ViewModels
{
    [Export(typeof(TextureProcessVM))]
    public class TextureProcessVM : ToolSetVMBase
    {
        public Settings Settings { get; set; } = new Settings();

        public ICommand ProcessCommand { get; set; }

        public ICommand SelectSaveFolderCommand { get; set; }

        public override void RegisterCommands()
        {
            base.RegisterCommands();
            ProcessCommand = new DelegateCommand(MultiTexureQualityProcess);
            SelectSaveFolderCommand = new DelegateCommand(SelectSaveFolder);
        }

        private void SelectSaveFolder()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                var result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    Settings.SaveFolder = fbd.SelectedPath;
            }
        }

        public override void RegisterEvents()
        {
            base.RegisterEvents();
            PublicDatas.Instance.Aggregator?.GetEvent<OpenFileEvent>().Subscribe(OpenFile);
            PublicDatas.Instance.Aggregator?.GetEvent<SaveEvent>().Subscribe(Save);
            PublicDatas.Instance.Aggregator?.GetEvent<OpenFolderEvent>().Subscribe(OpenFolder);
        }

        public override void UnRegisterEvents()
        {
            base.UnRegisterEvents();
            PublicDatas.Instance.Aggregator?.GetEvent<OpenFileEvent>().Unsubscribe(OpenFile);
            PublicDatas.Instance.Aggregator?.GetEvent<SaveEvent>().Unsubscribe(Save);
            PublicDatas.Instance.Aggregator?.GetEvent<OpenFolderEvent>().Unsubscribe(OpenFolder);
        }

        public void Save()
        {
            Logger.Log("SaveEvent");
        }

        private void OpenFolder()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                var result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    var files = Directory.GetFiles(fbd.SelectedPath, "*", SearchOption.AllDirectories);
                    LoadSelectedFiles(files);
                }
            }
        }

        public void OpenFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "All|*|jpg|*.jpg|png|*.png|tif|*.tif|tga|*.tga";
            dialog.FileOk += Dialog_FileOk;
            dialog.ShowDialog();
        }

        private void Dialog_FileOk(object sender, CancelEventArgs e)
        {
            var dialog = sender as OpenFileDialog;
            if (dialog != null)
                LoadSelectedFiles(dialog.FileNames);
        }

        // TODO 并行
        public void MultiTexureQualityProcess()
        {
            Logger.LogState("处理图片");
            var progressEvent = PublicDatas.Instance.Aggregator.GetEvent<ProgeessEvent>();
            progressEvent.Publish(0);

            var count = 0;
            var lst = SelectedFiles?.ToList() ?? new List<ProcessFile>();
            var fileCount = lst.Count;

            var worker = new BackgroundWorker();
            worker.DoWork += (sender, e) => { Parallel.ForEach(lst, x => { TexureQualityProcess(x, () => { progressEvent.Publish(count++ * 100.0 / fileCount); }); }); };
            worker.RunWorkerCompleted += (sender, e) =>
            {
                Logger.LogState("处理图片", true);
                progressEvent.Publish(100);
            };
            worker.RunWorkerAsync();
        }

        /// <summary>
        ///     图片质量修改
        /// </summary>
        /// <param name="filename"></param>
        public void TexureQualityProcess(ProcessFile proc, Action callback)
        {
            var filename = proc.Filename;
            var path = Path.Combine(Settings.SaveFolder, Path.GetFileName(filename) + "");
            var ext = Path.GetExtension(filename) + "".ToLower();
            if (!Directory.Exists(Settings.SaveFolder))
                Directory.CreateDirectory(Settings.SaveFolder);
            try
            {
                if (!File.Exists(filename))
                    return;
                if (ext.Equals(".jpg") || ext.Equals("jpeg"))
                {
                    var bytes = File.ReadAllBytes(filename);
                    using (var inStream = new MemoryStream(bytes))
                    {
                        using (var factory = new ImageFactory(true))
                        {
                            var img = factory.Load(inStream);
                            img.Quality(Settings.Quality);
                            img.Save(path);
                        }
                    }
                }
                else
                {
                    File.Copy(filename, path, true);
                }
                proc.SetState(1);
            }
            catch (Exception ex)
            {
                proc.SetState(2, ex);
            }
            callback?.Invoke();
        }

        public int Scale(int val)
        {
            var rst = (int)Math.Floor(val * Settings.Resolution / 100.0);
            return Math.Max(rst, 64);
        }

        [Obsolete]
        private ISupportedImageFormat GetImageFormat(string filename)
        {
            var ext = Path.GetExtension(filename) + "".ToLower();
            ISupportedImageFormat format = new JpegFormat { Quality = Settings.Quality };
            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                    return new JpegFormat { Quality = Settings.Quality };
                case ".png":
                    return new PngFormat { Quality = Settings.Quality };
                case ".tif":
                    return new TiffFormat { Quality = Settings.Quality };
            }
            return format;
        }

        private void LoadSelectedFiles(IEnumerable<string> filenames)
        {
            if (filenames == null) return;
            SelectedFiles = new ObservableCollection<ProcessFile>(filenames.Select(x => new ProcessFile { Filename = x, Message = string.Empty }));
        }

        #region SelectedFiles

        public ObservableCollection<ProcessFile> SelectedFiles
        {
            get { return (ObservableCollection<ProcessFile>)GetValue(SelectedFilesProperty); }
            set { SetValue(SelectedFilesProperty, value); }
        }

        public static readonly DependencyProperty SelectedFilesProperty =
            DependencyProperty.Register("SelectedFiles", typeof(ObservableCollection<ProcessFile>), typeof(TextureProcessVM), new PropertyMetadata((sender, e) =>
            {
                var vm = sender as TextureProcessVM;
                if (vm == null) return;
            }));

        #endregion
    }

    public class Settings : BindableBase
    {
        private int _quality = 100;
        private int _resolution = 100;
        private string _SaveFolder = Path.Combine(Environment.CurrentDirectory, "Processed");

        public string SaveFolder
        {
            get { return _SaveFolder; }
            set { SetProperty(ref _SaveFolder, value); }
        }

        public int Quality
        {
            get { return _quality; }
            set { SetProperty(ref _quality, value); }
        }

        public int Resolution
        {
            get { return _resolution; }
            set { SetProperty(ref _resolution, value); }
        }
    }

    public class ProcessFile : BindableBase, IProcessStatus
    {
        private string _filename = string.Empty;
        private string _message = "未处理";
        private int _state; // 0 未处理 1 成功 2，失败

        public int State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        public string Filename
        {
            get { return _filename; }
            set { SetProperty(ref _filename, value); }
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