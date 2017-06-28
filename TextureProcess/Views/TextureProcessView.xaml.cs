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
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using TextureProcess.ViewModels;
using ToolSetsCore;

namespace TextureProcess.Views
{
    [Export("TextureProcessView", typeof(TextureProcessView))]
    public partial class TextureProcessView : ToolSetsCore.ToolSetViewBase
    {
        public TextureProcessView()
        {
            InitializeComponent();

            var arg = ServiceLocator.Current.GetInstance<IEventAggregator>();
            arg.GetEvent<SaveEvent>().Subscribe(() =>
            {
                Logger.Log("SaveEvent");
            });
        }

        [Import]
        public TextureProcessVM ImportVM
        {
            set { VM = value; }
        }
    }
}
