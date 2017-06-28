using System;
using Prism.Mef.Modularity;
using ToolSetsCore;

namespace ChineseNameCoding
{
    [ModuleExport(typeof(ChineseNameCodingModule))]
    public class ChineseNameCodingModule : ToolSetModule
    {
        public override void RegisterPlugins()
        {
            base.RegisterPlugins();
            PublicDatas.Instance.AddPlugins(new PluginItem { Header = "中文名称转义", ViewName = "ChineseNameCodingView" });
        }
    }
}