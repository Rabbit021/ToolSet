using Prism.Mef.Modularity;
using Prism.Modularity;
using ToolSetsCore;

namespace ExcelConverter
{
    [ModuleExport(typeof(ExcelConverterMoudle ))]
    public class ExcelConverterMoudle : ToolSetModule
    {
        public override void RegisterPlugins()
        {
            base.RegisterPlugins();
            PublicDatas.Instance.AddPlugins(new PluginItem { Header = "Excel转换", ViewName = "ExcelConverterView" });
        }
    }
}