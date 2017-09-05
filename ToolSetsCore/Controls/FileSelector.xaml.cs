using System;
using System.Collections.Generic;
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

namespace ToolSetsCore.Controls
{
    public partial class FileSelector : UserControl
    {
        public FileSelector()
        {
            InitializeComponent();
            this.BtnFile.Click += BtnFile_Click;
            this.tBfile.TextChanged += TBfile_TextChanged;
            this.Loaded += FileSelector_Loaded;
        }

        private void FileSelector_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void TBfile_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void BtnFile_Click(object sender, RoutedEventArgs e)
        {
            MyFile = ExcelHelper.GetOpenFilePath(Filter, "");
        }

        #region MyFile
        public string MyFile
        {
            get { return (string)GetValue(MyFileProperty); }
            set { SetValue(MyFileProperty, value); }
        }

        public static readonly DependencyProperty MyFileProperty =
            DependencyProperty.Register("MyFile", typeof(string), typeof(FileSelector), new PropertyMetadata((sender, e) =>
            {
                var vm = sender as FileSelector;
                vm.tBfile.Text = e.NewValue.ToString();
            }));
        #endregion

        #region Header
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(FileSelector), new PropertyMetadata((sender, e) =>
            {
                var vm = sender as FileSelector;
                vm.Header = e.NewValue.ToString();
            }));
        #endregion


        #region Filter
        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(string), typeof(FileSelector), new PropertyMetadata("All(*.*)|(*.*)", (sender, e) =>
             {
                 var vm = sender as FileSelector;
             }));
        #endregion

    }
}
