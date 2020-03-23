using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Models;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Regions;
using ToolSetsCore;

namespace SOHO3Q_Alaram.ViewModels
{
    [Export(typeof(AlarmVM))]
    public class AlarmVM : ToolSetsCore.ToolSetVMBase
    {
        public override void RegisterCommands()
        {
            base.RegisterCommands();
            UpdateEntraceCommand = new DelegateCommand<object>(UpdateEntrace);
            UpdateDeviceCommand = new DelegateCommand<object>(UpdateDevice);
            UpdateEntraceZoneIdCommand = new DelegateCommand<object>(UpdateEntraceZoneId);
        }


        public ICommand UpdateEntraceCommand { get; set; }
        public ICommand UpdateDeviceCommand { get; set; }
        public ICommand UpdateEntraceZoneIdCommand { get; set; }

        #region Host
        public string Host
        {
            get { return (string)GetValue(HostProperty); }
            set { SetValue(HostProperty, value); }
        }

        public static readonly DependencyProperty HostProperty =
            DependencyProperty.Register("Host", typeof(string), typeof(AlarmVM), new PropertyMetadata("127.0.0.1", (sender, e) =>
             {
                 var vm = sender as AlarmVM;
             }));
        #endregion

        #region Port
        public string Port
        {
            get { return (string)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register("Port", typeof(string), typeof(AlarmVM), new PropertyMetadata("3306", (sender, e) =>
            {
                var vm = sender as AlarmVM;
            }));
        #endregion

        #region database
        public string database
        {
            get { return (string)GetValue(databaseProperty); }
            set { SetValue(databaseProperty, value); }
        }

        public static readonly DependencyProperty databaseProperty =
            DependencyProperty.Register("database", typeof(string), typeof(AlarmVM), new PropertyMetadata("soho3q", (sender, e) =>
             {
                 var vm = sender as AlarmVM;
             }));
        #endregion

        #region Uid
        public string Uid
        {
            get { return (string)GetValue(UidProperty); }
            set { SetValue(UidProperty, value); }
        }

        public static readonly DependencyProperty UidProperty =
            DependencyProperty.Register("Uid", typeof(string), typeof(AlarmVM), new PropertyMetadata("root", (sender, e) =>
             {
                 var vm = sender as AlarmVM;
             }));
        #endregion

        #region pwd
        public string pwd
        {
            get { return (string)GetValue(pwdProperty); }
            set { SetValue(pwdProperty, value); }
        }

        public static readonly DependencyProperty pwdProperty =
            DependencyProperty.Register("pwd", typeof(string), typeof(AlarmVM), new PropertyMetadata("123456", (sender, e) =>
             {
                 var vm = sender as AlarmVM;
             }));
        #endregion



        private DbContextBase GetConnectString()
        {
            var cstr = $"server={this.Host};port={Port};DataBase={database};Uid={Uid};Pwd={pwd}";
            var context = new DbContextBase(cstr);
            return context;
        }

        // 根据模型名称更新ZoneId
        private void UpdateEntraceZoneId(object obj)
        {
            var context = GetConnectString();
            var zoneList = context.Db.Queryable<soho_zone>().ToList();
            var entrancedevices = context.Db.Queryable<entrancedevice>().ToList();

            var json = File.ReadAllText("区域_门禁.json");
            var pairList = JToken.Parse(json).ToObject<List<KeyValuePair<string, string>>>();
            var RegionAndAlarms = new List<KeyValuePair<string, string>>();
            foreach (var itr in pairList)
                RegionAndAlarms.Add(new KeyValuePair<string, string>(itr.Key, itr.Value));


            foreach (var itr in entrancedevices)
            {
                var model_name = itr.ModelName;
                var result = RegionAndAlarms.FirstOrDefault(x => string.Equals(x.Value, model_name));
                var zone = zoneList.FirstOrDefault(x => x.zone_num == result.Key);
                itr.ModelName = result.Value;
                itr.ZoneId = zone?.id + "";
            }
            var rst = context.Db.Updateable<entrancedevice>(entrancedevices)
                .UpdateColumns(x => new { x.ModelName, x.ZoneId })
                .ExecuteCommand();
            Logger.Log($"更新了{rst}条数据！");
        }

        private void UpdateDevice(object obj)
        {
            var context = GetConnectString();
            var zoneList = context.Db.Queryable<soho_zone>().ToList();
            var devices = context.Db.Queryable<soho_device>().ToList();

            var json = File.ReadAllText("区域_FCU.json");
            var lst = new List<KeyValuePair<string, string>>();
            var pairList = JToken.Parse(json).ToObject<List<KeyValuePair<string, string>>>();
            lst.AddRange(pairList);

            json = File.ReadAllText("区域_照明.json");
            pairList = JToken.Parse(json).ToObject<List<KeyValuePair<string, string>>>();
            lst.AddRange(pairList);

            foreach (var device in devices)
            {
                var zonePair = lst.FirstOrDefault(x => x.Value == $"{device.code}");
                var zone = zoneList.FirstOrDefault(x => x.zone_name.Equals("" + zonePair.Key));
                device.zoneid = "" + zone?.id;
            }

            var rst = context.Db.Updateable(devices).UpdateColumns(x => x.zoneid).ExecuteCommand();
            Logger.Log($"更新了{rst}条");
        }

        private void UpdateEntrace(object obj)
        {
            var context = GetConnectString();
            var zoneList = context.Db.Queryable<soho_zone>().ToList();
            var entrancedevice = context.Db.Queryable<entrancedevice>().ToList();

            var json = File.ReadAllText("区域_门禁.json");
            var pairList = JToken.Parse(json).ToObject<List<KeyValuePair<string, string>>>();

            var RegionAndAlarms = new List<KeyValuePair<string, string>>();
            foreach (var itr in pairList)
                RegionAndAlarms.Add(new KeyValuePair<string, string>(itr.Key, itr.Value));

            var reg = new Regex(@"[\u4e00-\u9fa5]");
            foreach (var itr in entrancedevice)
            {
                var name = itr.Name;
                var newName = reg.Replace(name, "").Replace("-", "_");
                var result = RegionAndAlarms.Find(x => newName.IndexOf(x.Value.Replace("-", "_"), StringComparison.CurrentCultureIgnoreCase) > 0);
                if (!string.IsNullOrEmpty(result.Key))
                {
                    var zone = zoneList.FirstOrDefault(x => x.zone_num == result.Key);
                    itr.ModelName = result.Value;
                    itr.ZoneId = zone.id;
                }
            }
            // 更新ModelName和zoneId
            var rst = context.Db.Updateable<entrancedevice>(entrancedevice)
                .UpdateColumns(x => new { x.ModelName, x.ZoneId })
                .ExecuteCommand();

            Logger.Log($"更新了{rst}条");
        }
    }
}