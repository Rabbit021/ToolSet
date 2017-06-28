using System.Windows;
using Prism.Regions;
using System.ComponentModel.Composition;
using Prism.Events;

namespace ToolSetsCore
{
    public class ToolSetVMBase : DependencyObject
    {
        public ToolSetVMBase()
        {
            RegisterCommands();
        }

        [Import]
        public IRegionManager RegionManager { get; private set; }

        public virtual void RegisterEvents()
        {
        }

        public virtual void UnRegisterEvents()
        {
        }

        public virtual void RegisterCommands()
        {
        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
            Logger.Log(GetType().Name + "--->Loaded");
            RegisterEvents();
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {
            Logger.Log(GetType().Name + "--->UnLoaded");
            UnRegisterEvents();
        }
    }
}