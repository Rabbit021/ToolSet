using System.ComponentModel.Composition;
using Prism.Modularity;
using Prism.Regions;

namespace ToolSetsCore
{
    public class ToolSetModule : IModule
    {
        [Import]
        public IRegionManager RegionManager;

        public virtual void Initialize()
        {
            Logger.Log(this.GetType().Name + "--->Initialize");
            RegisterPlugins();
            RegisterViews();
            RegisterEvents();
            RaiseInitEvents();
        }

        public virtual void RegisterViews()
        {
        }

        public virtual void RegisterPlugins()
        {
        }
         
        public virtual void RegisterEvents()
        {
        }

        public virtual void RaiseInitEvents()
        {
        }
    }
}