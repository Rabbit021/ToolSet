using System.ComponentModel.Composition;
using ToolSetsCore;

namespace CameraEquipment.ViewModels
{
    [Export]
    [Export("CameraEquipmentVM", typeof(CameraEquipmentVM))]
    public class CameraEquipmentVM : ToolSetVMBase
    {

    }
}