using System;
using ToolSetsCore;
using Prism.Mef.Modularity;

namespace TextureProcess
{
    [ModuleExport(typeof(TextureProcessModule))]
    public class TextureProcessModule : ToolSetModule
    {
        public override void RegisterPlugins()
        {
            base.RegisterPlugins();
            PublicDatas.Instance.AddPlugins(new PluginItem { Header = "图片处理", ViewName = "TextureProcessView" });
        }
    }
}