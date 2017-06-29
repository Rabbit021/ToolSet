using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualBasic.Logging;
using Prism.Events;
using ToolSetsCore;

namespace ToolSets
{
    [Export]
    [Export("MainWindow", typeof(MainWindow))]
    public partial class MainWindow : Window, IDisposable
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Unloaded += MainWindow_Unloaded;
            Closing += MainWindow_Closing;
            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        [Import]
        public MainVM VM
        {
            set { DataContext = value; }
            get { return DataContext as MainVM; }
        }

        public void Dispose()
        {
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PublicDatas.Instance.LoadNewPlugins += Instance_LoadNewPlugins;
            PublicDatas.Instance.Aggregator.GetEvent<LogEvent>().Subscribe(LogMessage);
            PublicDatas.Instance.Aggregator.GetEvent<ProgeessEvent>().Subscribe((prgress) =>
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (prgress <= 0 || prgress >= 100)
                            progressInfo.Visibility = Visibility.Collapsed;
                        else
                            progressInfo.Visibility = Visibility.Visible;
                        progressBar.Value = prgress;
                    });
                }
                finally
                {
                }
            });
        }

        private void Instance_LoadNewPlugins(PluginItem itr)
        {
            var root = funitem;
            var fun = new MenuItem { Header = itr.Header, Tag = itr.ViewName };
            fun.Click -= Fun_Click;
            fun.Click += Fun_Click;
            root.Items.Add(fun);
        }

        private void Fun_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem);
            var tag = item?.Tag + "";
            if (string.IsNullOrEmpty(tag)) return;
            VM?.RegionManager.RequestNavigate(Constants.MainRegionName, tag);
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            VM?.OnNavigatedFrom(null);
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
        }

        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            PublicDatas.Instance.Aggregator.GetEvent<OpenFileEvent>().Publish();
        }

        private void savefile_Click(object sender, RoutedEventArgs e)
        {
            PublicDatas.Instance.Aggregator.GetEvent<SaveEvent>().Publish();
        }

        private void openFolder_Click(object sender, RoutedEventArgs e)
        {
            PublicDatas.Instance.Aggregator.GetEvent<OpenFolderEvent>().Publish();
        }

        private void LogMessage(string obj)
        {
            Dispatcher.Invoke(() =>
            {
                tbLog.AppendText(obj);
                tbLog.ScrollToEnd();
            });
        }
    }
}