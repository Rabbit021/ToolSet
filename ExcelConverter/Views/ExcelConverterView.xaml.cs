using System.ComponentModel.Composition;
using ExcelConverter.ViewModels;
using ToolSetsCore;

namespace ExcelConverter.Views
{
    [Export]
    [Export("ExcelConverterView", typeof(ExcelConverterView))]
    public partial class ExcelConverterView : ToolSetViewBase
    {
        public ExcelConverterView()
        {
            InitializeComponent();
        }

        [Import]
        public ExcelConverterVM ImportVM
        {
            set => VM = value;
        }
    }
}