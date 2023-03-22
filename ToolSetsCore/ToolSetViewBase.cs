using System;
using Prism.Regions;
using System.Windows.Controls;
using Microsoft.Practices.ServiceLocation;

namespace ToolSetsCore
{
    public class ToolSetViewBase : UserControl, INavigationAware, IRegionMemberLifetime
    {
        public ToolSetViewBase()
        {
        }
        private ToolSetVMBase _VM;

        public ToolSetVMBase VM
        {
            get
            {
                return _VM;
            }
            set
            {
                _VM = value;
                this.DataContext = _VM;
            }
        }

        public bool KeepAlive
        {
            get
            {
                return false;
            }
        }

        //只激发同源的VM View与VM名称是否相同,避免多次激发Loaded事件
        private bool IsSameSource
        {
            get
            {
                try
                {
                    if (this.VM == null) return false;
                    var reg = ServiceLocator.Current.GetInstance<IRegionManager>();

                    var viewname = GetType().Name.TrimEndString("View");
                    var vmname = this.VM.GetType().Name.TrimEndString("VM");
                    var rst = string.Equals(viewname, vmname, StringComparison.CurrentCultureIgnoreCase);
                    return rst;
                }
                catch
                {
                    return false;
                }
            }
        }

        #region INavigationAware
        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!IsSameSource) return;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.VM.OnNavigatedTo(navigationContext);
            }));
        }
        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }
        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {
            if (!IsSameSource) return;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.VM.OnNavigatedFrom(navigationContext);
            }));
        }
        #endregion
    }
}