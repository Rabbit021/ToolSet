using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using ToolSetsCore;
using System.Linq;
using Models;

namespace SOHO3Q_Alaram
{
    [Export]
    [Export("AlarmView", typeof(AlarmView))]
    public partial class AlarmView : UserControl
    {
        private readonly List<KeyValuePair<string, string>> RegionAndAlarms = new List<KeyValuePair<string, string>>();

        public AlarmView()
        {
            InitializeComponent();
            this.btnUpdate.Click += BtnUpdate_Click;
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var cstr = $"server={this.server.Text};port={this.port.Text};DataBase={this.database.Text};Uid={this.Uid.Text};Pwd={this.pwd.Text}";
            var context = new DbContextBase(cstr);

            var zoneList = context.Db.Queryable<soho_zone>().ToList();
            var entrancedevice = context.Db.Queryable<entrancedevice>().ToList();

            var json = File.ReadAllText("区域_门禁.json");
            var pairList = JToken.Parse(json).ToObject<List<KeyValuePair<string, string>>>();

            foreach (var itr in pairList)
                RegionAndAlarms.Add(new KeyValuePair<string, string>(itr.Key, itr.Value));

            var reg = new Regex(@"[\u4e00-\u9fa5]");
            foreach (var itr in entrancedevice)
            {
                var name = itr.Name;
                var newName = reg.Replace(name, "").Replace("-", "_");
                var result = RegionAndAlarms.Find(x => newName.Contains(x.Value.Replace("-", "_")));
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