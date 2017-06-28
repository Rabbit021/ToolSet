using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using Microsoft.Win32;
using Prism.Mvvm;
using ToolSetsCore;

namespace TextureProcess.ViewModels
{
    [Export(typeof(TextureProcessVM))]
    public class TextureProcessVM : ToolSetVMBase
    {
        public Settings Settings { get; set; } = new Settings();

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
            PublicDatas.Instance.Aggregator?.GetEvent<OpenFolderEvent>().Subscribe(OpenFolder);
        }

        private void OpenFolder()
        {
        }

        public void Save()
        {
            Logger.Log("SaveEvent");
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
                MultiTexureQualityProcess(dialog.FileNames);
        }

        // TODO 并行
        public void MultiTexureQualityProcess(string[] filenames)
        {
            var rst = Parallel.ForEach(filenames, TexureQualityProcess);
        }

        /// <summary>
        ///     图片质量修改
        /// </summary>
        /// <param name="filename"></param>
        public void TexureQualityProcess(string filename)
        {
            try
            {
                var bytes = File.ReadAllBytes(filename);
                using (var inStream = new MemoryStream(bytes))
                {
                    // Image Process
                    using (var factory = new ImageFactory(true))
                    {
                        var img = factory.Load(inStream);
                        img.Resize(new Size { Width = Scale(factory.Image.Width), Height = Scale(factory.Image.Height) });

                        if (!Directory.Exists(Settings.SaveFolder))
                            Directory.CreateDirectory(Settings.SaveFolder);
                        var foramt = GetImageFormat(filename);
                        var path = Path.Combine(Settings.SaveFolder, Path.GetFileName(filename) + "");

                        foramt.Save(path, img.Image, 8);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[Process Error] {filename} ---- {ex.Message}");
            }
        }

        public int Scale(int val)
        {
            var rst = (int)Math.Floor(val * Settings.Resolution / 100.0);
            return Math.Max(rst, 64);
        }

        // KalikoImage
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
}