using System;
using System.Collections.Generic;
using Prism.Events;

namespace ToolSetsCore
{
    public class PublicDatas
    {
        public static PublicDatas _instance = new PublicDatas();

        public static PublicDatas Instance
        {
            get { return _instance; }
        }

        public IEventAggregator Aggregator { get; set; }
        public HashSet<PluginItem> Plugins { get; set; } = new HashSet<PluginItem>(new PluginItemEqualCompare());
        public event Action<PluginItem> LoadNewPlugins = (itr) => { };
        static PublicDatas()
        {
            Instance.Aggregator = new EventAggregator();
        }

        public void AddPlugins(PluginItem item)
        {
            if (Plugins.Add(item))
                LoadNewPlugins?.Invoke(item);
        }
    }

    public class PluginItem
    {
        public string Header { get; set; }
        public string ViewName { get; set; }
    }

    public class PluginItemEqualCompare : IEqualityComparer<PluginItem>
    {
        public bool Equals(PluginItem x, PluginItem y)
        {
            return x.Header.Equals(y.Header);
        }

        public int GetHashCode(PluginItem obj)
        {
            return obj.GetHashCode();
        }
    }
}