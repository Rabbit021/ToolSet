using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using Prism.Mef;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Reflection;
using System.Resources;

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

            Application.Current.Resources.MergedDictionaries.Clear();
            LoadTheme("ToolSetTheme");
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

        void LoadTheme(string dirPath)
        {
            Application.Current.Resources.MergedDictionaries.Clear();
            LoadStylesFromAssembly("ToolSetTheme");
            LoadStylesFromXaml("ToolSetTheme");
        }

        void LoadStylesFromAssembly(string dirPath)
        {
            if (!Directory.Exists(dirPath)) return;
            var files = Directory.GetFiles(dirPath, "*.dll");
            foreach (var file in files)
            {
                var assembly = Assembly.LoadFrom(file);
                var uri1 = string.Format("pack://application:,,,/{0};Component/Resources.xaml", assembly.GetName().Name);
                var resourceDictionary = new ResourceDictionary { Source = new Uri(uri1) };
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }
        }

        void LoadStylesFromXaml(string dirPath)
        {
            try
            {
                if (!Directory.Exists(dirPath)) return;
                var files = Directory.GetFiles(dirPath, "*.xaml");
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
                        // ignored
                    }
                }
            }
            catch
            {
                // ignored
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