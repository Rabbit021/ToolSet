using ToolSetsCore;
using Prism;
using Prism.Mef.Modularity;

namespace CameraEquipment
{
    [ModuleExport(typeof(CameraEquipmentModule))]
    public class CameraEquipmentModule : ToolSetModule
    {
        public override void RegisterPlugins()
        {
            base.RegisterPlugins();
            PublicDatas.Instance.AddPlugins(new PluginItem { Header = "安防摄像头关系处理", ViewName = "CameraEquipmentView" });
        }
    }
}