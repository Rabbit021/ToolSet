using System.IO;
using System.Windows;
using System.Windows.Markup;
using Prism.Mef;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;

namespace ToolSets
{
    public class ToolSetBootstrapper : MefBootstrapper
    {
        public override void Run(bool runWithDefaultConfiguration)
        {
            base.Run(runWithDefaultConfiguration);
        }

        protected override DependencyObject CreateShell()
        {
            return this.Container.GetExportedValue<MainWindow>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            App.Current.MainWindow = this.Shell as Window;
            App.Current.MainWindow.Show();
        }

        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(MainWindow).Assembly));
            this.LoadCatalogs();
            this.LoadStyles("ToolSetTheme");
        }

        void LoadCatalogs()
        {
            var config = ConfigurationManager.AppSettings["ModuleConfigFile"];
            //默认文件夹
            if (string.IsNullOrEmpty(config))
            {
                this.LoadDirTreeCatalogs("Plugins");
            }
            else //配置文件
            {
                this.LoadCatalogsFromConfigFile(config);
            }
        }

        void LoadStyles(string dirPath)
        {
            try
            {
                if (!Directory.Exists(dirPath)) return;
                var files = Directory.GetFiles(dirPath, "*.xaml");
                Application.Current.Resources.MergedDictionaries.Clear();
                foreach (var file in files)
                {
                    try
                    {
                        using (FileStream s = new FileStream(file, FileMode.Open))
                        {
                            ResourceDictionary rd = XamlReader.Load(s) as ResourceDictionary;
                            Application.Current.Resources.MergedDictionaries.Add(rd);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        void LoadCatalogsFromConfigFile(string filePath)
        {

            if (!File.Exists(filePath)) return;
            var dllFiles = File.ReadAllLines(filePath);

            foreach (var dllFile in dllFiles)
            {
                if (dllFile.StartsWith("//")) continue;
                if (!File.Exists(dllFile)) continue;
                this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(dllFile));
            }
        }

        void LoadDirTreeCatalogs(string dirPath)
        {
            if (!Directory.Exists(dirPath)) return;
            foreach (var dllFile in Directory.GetFiles(dirPath, "*.dll"))
            {
                var catalog = new AssemblyCatalog(dllFile);
                this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(dllFile));
            }
            foreach (var subDir in Directory.GetDirectories(dirPath))
            {
                LoadDirTreeCatalogs(subDir);
            }
        }
    }
}