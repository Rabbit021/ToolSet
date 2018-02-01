using ToolSetsCore;
using Prism.Mef.Modularity;

namespace UdpTester
{
    [ModuleExport(typeof(UdpTesterModule))]
    public class UdpTesterModule : ToolSetModule
    {
        public override void RegisterPlugins()
        {
            base.RegisterPlugins();
            PublicDatas.Instance.AddPlugins(new PluginItem { Header = "UdpTester", ViewName = "UdpTesterView" });
        }
    }
}