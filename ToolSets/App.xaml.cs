using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using ToolSetsCore;

namespace ToolSets
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            System.Environment.CurrentDirectory = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            this.Init(e);
        }

        void Init(StartupEventArgs e)
        {
            Logger.Log("----------OnStartup Start------------");

            var bootstrapper = new ToolSetBootstrapper();
            bootstrapper.Run();
            Logger.Log("----------OnStartup End------------");
        }

    }
}
