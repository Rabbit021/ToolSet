using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
using ChineseNameCoding.ViewModels;
using ToolSetsCore;

namespace ChineseNameCoding.Views
{
    [Export("ChineseNameCodingView", typeof(ChineseNameCodingView))]
    public partial class ChineseNameCodingView : ToolSetViewBase
    {
        public ChineseNameCodingView()
        {
            InitializeComponent();
        }

        [Import]
        public ChineseNameCodingVM ImportVM
        {
            set { VM = value; }
        }
    }
}
