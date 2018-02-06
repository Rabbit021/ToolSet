using Prism.Mef.Modularity;
using ToolSetsCore;

namespace SOHO3Q_Alaram
{
    [ModuleExport(typeof(SOHO3QAlaramModule))]
    public class SOHO3QAlaramModule : ToolSetModule
    {
        public override void RegisterPlugins()
        {
            base.RegisterPlugins();
            PublicDatas.Instance.AddPlugins(new PluginItem { Header = "SOHO3Q", ViewName = "AlarmView" });
        }
    }
}