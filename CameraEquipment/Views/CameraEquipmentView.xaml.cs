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
using CameraEquipment.ViewModels;

namespace CameraEquipment.Views
{
    [Export]
    [Export("CameraEquipmentView", typeof(CameraEquipmentView))]
    public partial class CameraEquipmentView : ToolSetsCore.ToolSetViewBase
    {
        public CameraEquipmentView()
        {
            InitializeComponent();
        }

        [Import]
        public CameraEquipmentVM ImportVM
        {
            set { VM = value; }
            get { return VM as CameraEquipmentVM; }
        }
    }
}
