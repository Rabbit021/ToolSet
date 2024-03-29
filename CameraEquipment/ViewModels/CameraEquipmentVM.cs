﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using CameraEquipment.Models;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using ToolSetsCore;
using Prism.Commands;
using Formatting = Newtonsoft.Json.Formatting;

namespace CameraEquipment.ViewModels
{
    [Export]
    [Export("CameraEquipmentVM", typeof(CameraEquipmentVM))]
    public class CameraEquipmentVM : ToolSetVMBase
    {
        public override void RegisterCommands()
        {
            base.RegisterCommands();
            PraseDhCommand = new DelegateCommand<object>(PraseDh);
            ExportDhCommand = new DelegateCommand<object>(Export);
            UpdateCommand = new DelegateCommand<object>(Update);
        }

        public ICommand PraseDhCommand { get; set; }
        public ICommand ExportDhCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand ExportDepartmentCommand { get; set; }

        public Dictionary<string, Channel> Channels { get; set; } = new Dictionary<string, Channel>();
        public Dictionary<string, Channel> Removed { get; set; } = new Dictionary<string, Channel>();
        public Dictionary<string, CameraInfo> CameraDict { get; set; } = new Dictionary<string, CameraInfo>();

        public HashSet<string> SphereCamera = new HashSet<string>();
        public HashSet<string> DeleteCamera = new HashSet<string>();
        public HashSet<string> HasViedoCamera = new HashSet<string>();

        public HashSet<string> NoSpecDeleteCamera = new HashSet<string>();
        public HashSet<string> NoSpecHasViedoCamera = new HashSet<string>();


        #region DhFile
        public string DhFile
        {
            get { return (string)GetValue(DhFileProperty); }
            set { SetValue(DhFileProperty, value); }
        }

        public static readonly DependencyProperty DhFileProperty =
            DependencyProperty.Register("DhFile", typeof(string), typeof(CameraEquipmentVM), new PropertyMetadata((sender, e) =>
            {
                var vm = sender as CameraEquipmentVM;
            }));
        #endregion

        #region FilledChannelFile
        public string FilledChannelFile
        {
            get { return (string)GetValue(FilledChannelFileProperty); }
            set { SetValue(FilledChannelFileProperty, value); }
        }

        public static readonly DependencyProperty FilledChannelFileProperty =
            DependencyProperty.Register("FilledChannelFile", typeof(string), typeof(CameraEquipmentVM), new PropertyMetadata((sender, e) =>
            {
                var vm = sender as CameraEquipmentVM;
            }));
        #endregion

        #region SOHOCameraDeleteFile
        public string SOHOCameraDeleteFile
        {
            get { return (string)GetValue(SOHOCameraDeleteFileProperty); }
            set { SetValue(SOHOCameraDeleteFileProperty, value); }
        }

        public static readonly DependencyProperty SOHOCameraDeleteFileProperty =
            DependencyProperty.Register("SOHOCameraDeleteFile", typeof(string), typeof(CameraEquipmentVM), new PropertyMetadata((sender, e) =>
            {
                var vm = sender as CameraEquipmentVM;
            }));
        #endregion

        #region OldCameraFile
        public string OldCameraFile
        {
            get { return (string)GetValue(OldCameraFileProperty); }
            set { SetValue(OldCameraFileProperty, value); }
        }

        public static readonly DependencyProperty OldCameraFileProperty =
            DependencyProperty.Register("OldCameraFile", typeof(string), typeof(CameraEquipmentVM), new PropertyMetadata((sender, e) =>
            {
                var vm = sender as CameraEquipmentVM;
            }));
        #endregion

        #region ControlFile
        public string ControlFile
        {
            get { return (string)GetValue(ControlFileProperty); }
            set { SetValue(ControlFileProperty, value); }
        }

        public static readonly DependencyProperty ControlFileProperty =
            DependencyProperty.Register("ControlFile", typeof(string), typeof(CameraEquipmentVM), new PropertyMetadata((sender, e) =>
            {
                var vm = sender as CameraEquipmentVM;
            }));
        #endregion

        #region OnlyFileDel 使用文件限制删除
        public bool OnlyFileDel
        {
            get { return (bool)GetValue(OnlyFileDelProperty); }
            set { SetValue(OnlyFileDelProperty, value); }
        }

        public static readonly DependencyProperty OnlyFileDelProperty =
            DependencyProperty.Register("OnlyFileDel", typeof(bool), typeof(CameraEquipmentVM), new PropertyMetadata(true, (sender, e) =>
             {
                 var vm = sender as CameraEquipmentVM;
             }));
        #endregion

        #region CameraGroupFile
        public string CameraGroupFile
        {
            get { return (string)GetValue(CameraGroupFileProperty); }
            set { SetValue(CameraGroupFileProperty, value); }
        }

        public static readonly DependencyProperty CameraGroupFileProperty =
            DependencyProperty.Register("CameraGroupFile", typeof(string), typeof(CameraEquipmentVM), new PropertyMetadata((sender, e) =>
            {
                var vm = sender as CameraEquipmentVM;
            }));
        #endregion

        // 导出转换后的DH文件
        private void Export(object args)
        {
            if (Channels.Count == 0)
            {
                MessageBox.Show("Wait");
                return;
            }
            var dialog = new SaveFileDialog();
            dialog.Filter = "Excel File|*.xlsx";
            dialog.FileName = "channels";
            if (dialog.ShowDialog() == true)
            {
                var path = dialog.FileName;
                if (string.IsNullOrEmpty(dialog.FileName))
                    return;
                Export(path, Channels);
                Export($@"{Path.GetDirectoryName(path)}\{Path.GetFileNameWithoutExtension(path)}_Removed.xlsx", Removed);

                var lst = new JArray();
                foreach (var itr in Channels)
                {
                    var obj = new JObject
                    {
                        {"name", itr.Value.ChannelName},
                        {"processedName", itr.Value.ChannelName.NormalizeName()},
                        {"Channel", itr.Value.ChannelId},
                        {"buildingName", itr.Value.Building.name},
                        {"DVR", itr.Value.DVRGroup.name},
                        {"IP", itr.Value.Ip},
                        {"Num", itr.Value.ChannelNum},
                        { "Remark",itr.Value.Remark}
                    };
                    lst.Add(obj);
                }
                File.WriteAllText($@"{Path.GetDirectoryName(path)}\{Path.GetFileNameWithoutExtension(path)}.json", lst.ToString(Formatting.Indented));
            }
            Log("Finished Export");
        }

        // 解析DH文件
        private void PraseDh(object obj)
        {
            Log("Start PraseDh!");
            DoPrase(DhFile);
            Log("Finished PraseDh!");
        }

        #region 转换DHFile

        public void DoPrase(string filename)
        {
            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
            {
                Log("DHFile 不存在");
                return;
            }

            // Prase DeleteCamera 
            var rst = LoadJarrayFile(SOHOCameraDeleteFile);
            foreach (var itr in rst)
            {
                var name = itr.GetValue<string>("通道名称");
                var uid = itr.GetValue<string>("唯一标识码");

                if (!string.IsNullOrEmpty(uid))
                    CameraDict.TryAdd(uid, new CameraInfo { name = name, uid = uid });
                if (string.IsNullOrEmpty(name)) continue;
                DeleteCamera.Add(name.ReplaceSpecChar());
                NoSpecDeleteCamera.Add(name.ReplaceSpecChar().RemoveSpecChar());

                if (!string.IsNullOrEmpty(uid))
                {
                    HasViedoCamera.Add(name.ReplaceSpecChar());
                    NoSpecHasViedoCamera.Add(name.ReplaceSpecChar().RemoveSpecChar());
                }
            }

            var doc = new XmlDocument();
            doc.Load(filename);
            // 解析摄像头结构
            var Organization = doc.SelectSingleNode("Organization");
            PraseDepartment(Organization);
            ExportDepartment();

            // 填充摄像头信息
            var deviceroot = Organization?.SelectSingleNode("Device");
            PraseDevice(deviceroot);

            RemoveDestoryed();
        }

        private void ExportDepartment()
        {
            //var path = ExcelHelper.GetSaveFilePath("Organization");
            //Export(path, Channels);
        }

        public void PraseDepartment(XmlNode Organization)
        {
            var systemNode = Organization?.SelectSingleNode("Department"); // 望京SOHO
            var systemDep = new Department(systemNode);

            var buildingNodes = systemNode?.SelectNodes("Department"); // T1 ,T2 ,T3

            if (buildingNodes != null)
            {
                foreach (XmlNode buildingNode in buildingNodes)
                {
                    var building = new Department(buildingNode);

                    var dvrs = buildingNode.SelectNodes("Department"); // DVR Group
                    foreach (XmlNode group in dvrs)
                    {
                        var dvrgroup = new Department(group);
                        var channels = group.SelectNodes("Channel");
                        var device = group.SelectSingleNode("Device");
                        var deviceId = device.GetAttribte("id");

                        foreach (XmlNode node in channels)
                        {
                            var channel = PraseChannel(node);
                            channel.DeviceId = deviceId;
                            channel.Product = systemDep;
                            channel.Building = building;
                            channel.DVRGroup = dvrgroup;
                            Channels.TryAdd(channel.ChannelId, channel);
                        }
                    }
                }
            }
        }

        public void PraseDevice(XmlNode deviceroot)
        {
            var devices = deviceroot.SelectNodes("Device");
            foreach (XmlNode devicenode in devices)
            {
                var ip = devicenode.GetAttribte("ip");
                var deviceIp = devicenode.GetAttribte("deviceIp");
                var unodes = devicenode.SelectNodes("UnitNodes");
                foreach (XmlNode unode in unodes)
                {
                    var num = unode.GetAttribte("channelnum");
                    var channels = unode.SelectNodes("Channel");
                    foreach (XmlNode cnode in channels)
                    {
                        var channel = PraseChannel(cnode);
                        var chn = Channels.TryGetValue(channel.ChannelId);
                        if (chn != null)
                        {
                            chn.ChannelName = channel.ChannelName;
                            chn.CameraType = channel.CameraType;
                            chn.ChannelSN = channel.ChannelSN;

                            chn.Ip = ip;
                            chn.ChannelNum = num;
                        }
                    }
                }
            }
        }

        public Channel PraseChannel(XmlNode node)
        {
            var rst = new Channel();
            try
            {
                rst.ChannelId = node.GetAttribte("id");
                rst.ChannelName = node.GetAttribte("name");
                rst.CameraType = node.GetAttribte("cameraType");
                rst.ChannelType = node.GetAttribte("channelType");
                rst.ChannelSN = node.GetAttribte("channelSN");
                return rst;
            }
            catch (Exception e)
            {
                Log(e.Message);
                return rst;
            }
        }

        //  移除已替换的摄像头
        public void RemoveDestoryed()
        {
            var lst = Channels.Values.ToList();

            foreach (var itr in lst)
            {
                // 移除的Name
                var cname = itr.ChannelName.ReplaceSpecChar();
                if (DeleteCamera.Contains(cname) && !HasViedoCamera.Contains(cname))
                {
                    RemoveChannel(itr.ChannelId, "删除文件匹配");
                }
                // 去除特殊字符在删除
                cname = cname.RemoveSpecChar();
                if (NoSpecDeleteCamera.Contains(cname) && !NoSpecHasViedoCamera.Contains(cname))
                {
                    RemoveChannel(itr.ChannelId, "删除文件匹配，排除特殊字符");
                }

                // 移除CameraType 空的
                if (string.IsNullOrEmpty(itr.CameraType))
                {
                    RemoveChannel(itr.ChannelId, "删除CameraType为空");
                }

                // 移除视频分析,解码器,备用
                if (itr.Building.name.Contains("视频分析") || itr.DVRGroup.name.Contains("解码器") || itr.ChannelName?.Contains("备用") == true)
                {
                    RemoveChannel(itr.ChannelId, "删除视屏分析,解码器,备用");
                }
            }

            if (!OnlyFileDel)
            {
                // 同楼同名删除DVR
                var building = Channels.Values.ToList().GroupBy(x => x.Building.name);
                foreach (var itr in building)
                {
                    var groups = itr.GroupBy(x => x.ChannelName.NormalizeName());
                    foreach (var group in groups)
                    {
                        if (group.Count() > 2)
                        {
                            Log($"{group.Key}重名多余两个");
                        }
                        var build = group.Select(x => x.Building.name.Trim()).Distinct();
                        if (group.Count() >= 2 && build.Count() == 1)   // 同一个建筑重名
                        {
                            var ids = group.Where(x => x.DVRGroup.name.Contains("DVR"))?.Select(x => x.ChannelId)?.ToList();
                            foreach (var id in ids)
                            {
                                RemoveChannel(id, "同楼重名");
                            }
                        }
                    }
                }

                //// 同楼 末尾数字同名删除DVR
                //building = Channels.Values.ToList().GroupBy(x => x.Building.name);
                //foreach (var itr in building)
                //{
                //    var numGroups = itr.GroupBy(x => GetDigital(GetLastStr(x.ChannelName)));
                //    foreach (var group in numGroups)
                //    {
                //        if (group.Key < 0) continue;
                //        var buildGroups = group.GroupBy(x => x.Building.name.NormalizeName());
                //        if (group.Count() >= 2 && buildGroups.Count() == 1)
                //        {
                //            var ids = group.Where(x => x.DVRGroup.name.Contains("DVR")).Select(x => x.ChannelId).ToList();
                //            foreach (var id in ids)
                //            {
                //                // SN 相同
                //                var rst = Channels.TryGetValue(id);
                //                if (string.IsNullOrEmpty(rst.ChannelSN))
                //                {
                //                    RemoveChannel(id, "同楼,末尾数字重复,SN码空");
                //                }
                //                else if (!rst.ChannelName.EndsWith(GetSNCode(rst.ChannelSN)))
                //                {
                //                    RemoveChannel(id, "同楼,末尾数字重复,SN不匹配");
                //                }
                //            }
                //        }
                //    }
                //}
            }

            Vailidate();
        }

        public void Vailidate()
        {
            foreach (var itr in Channels)
            {
                if (!itr.Value.ChannelName.EndsWith(GetSNCode(itr.Value.ChannelSN))) // 同楼末尾数字重复，但是SN不同
                    itr.Value.Remark = "SN 结尾不同，注意！";
            }
        }

        public string GetSNCode(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            var length = str.Length - 1;
            var rst = str.Substring(1, length);
            return int.Parse(rst).ToString();
        }

        private string GetLastStr(string str)
        {
            var rst = str?.Trim().Split(' ').Last();
            return rst;
        }
        private int GetDigital(string str)
        {
            int rst = -1;
            if (int.TryParse(str, out rst))
            {
                return Math.Abs(rst);
            }
            return rst;
        }
        private void RemoveChannel(string ChannelId, string log = "")
        {
            var channel = Channels.TryGetValue(ChannelId);
            if (channel == null) return;
            channel.Remark = log;
            Removed.TryAdd(ChannelId, channel);
            Channels.Remove(ChannelId);
        }
        private void Export(string path, Dictionary<string, Channel> channels)
        {
            if (string.IsNullOrEmpty(path))
                return;
            var dt = new DataTable();
            foreach (var itr in channels)
            {
                var row = dt.NewRow();

                var type = itr.Value.GetType();
                var props = type.GetProperties();
                foreach (var prop in props)
                {
                    var val = prop.GetValue(itr.Value, null);
                    var rst = val as Department;
                    if (rst != null)
                        row.AddValue2Row(prop.Name, rst.name);
                    else
                        row.AddValue2Row(prop.Name, val?.ToString());
                }
                dt.Rows.Add(row);
            }
            ExcelHelper.WriteDataTable(path, dt);
        }

        #endregion

        #region 更新摄像头

        private void Update(object obj)
        {
            try
            {
                if (!File.Exists(OldCameraFile) || !File.Exists(FilledChannelFile))
                {
                    Log("OldCameraFile或FilledChannelFile 不存在");
                    return;
                }

                if (File.Exists(ControlFile))
                {
                    var array = LoadJarrayFile(ControlFile);
                    foreach (var itr in array)
                    {
                        SphereCamera.Add(itr.GetValue<string>("c_channel"));
                    }
                }

                var cameras = LoadJarrayFile(OldCameraFile);
                var filed = LoadJarrayFile(FilledChannelFile);

                var dt = new DataTable();
                var jr = new JArray();
                foreach (var itr in filed) // 新摄像头
                {
                    var row = dt.NewRow();
                    ToDataRow(itr, cameras, row);
                    dt.Rows.Add(row);
                    jr.Add(ToJObject(itr, cameras));
                }


                var dname = Path.ChangeExtension(OldCameraFile, ".xlsx");
                dname = ExcelHelper.GetSaveFilePath(dname);
                if (!string.IsNullOrEmpty(dname))
                {
                    var jsonpath = Path.Combine(Path.GetDirectoryName(OldCameraFile), Path.GetFileNameWithoutExtension(OldCameraFile) + "-update.json");
                    File.WriteAllText(jsonpath, jr.ToString());
                }
                ExcelHelper.WriteDataTable(dname, dt);

                // TODO 导出模型匹配结果
                UpgradeCameraGroup(jr);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
            finally
            {
                Log("Finished Update ");
            }
        }

        private JObject ToJObject(JToken itr, JArray cameras)
        {
            var row = new JObject();

            var camera = GetCameraByChannel(cameras, itr.GetValue<string>("Channel"));
            if (camera != null)
            {
                row.Add("c_id", camera.GetValue<string>("c_id"));
                row.Add("c_code", camera.GetValue<string>("c_code"));
                row.Add("c_operation", "Update");
            }
            else
            {
                string c_id = Guid.NewGuid().ToString();
                row.Add("c_id", c_id);
                row.Add("c_code", "");
                row.Add("c_operation", "Add");
            }
            row.Add("c_storey", itr.GetValue<string>("storey"));
            row.Add("c_buildingName", itr.GetValue<string>("buildingName"));
            row.Add("c_header", itr.GetValue<string>("name"));
            row.Add("c_english_name", itr.GetValue<string>("EnglishName"));
            row.Add("c_dvr", itr.GetValue<string>("DVR"));
            row.Add("c_num", itr.GetValue<string>("Num"));
            row.Add("c_ip", itr.GetValue<string>("IP"));
            row.Add("c_channel", itr.GetValue<string>("Channel"));
            row.Add("c_render_option", "CCTV");
            row.Add("c_building_id", itr.GetValue<string>("buildingId"));
            row.Add("c_storey_id", itr.GetValue<string>("storeyId"));
            row.Add("AlarmLimit", "22:00-7:00");
            row.Add("CanControl", "false");
            row.Add("pointLocation", itr.SelectToken("pointLocation"));
            return row;
        }

        private DataRow ToDataRow(JToken itr, JArray cameras, DataRow row)
        {
            var camera = GetCameraByChannel(cameras, itr.GetValue<string>("Channel"));
            if (camera != null)
            {
                row.AddValue2Row("c_id", camera.GetValue<string>("c_id"));
                row.AddValue2Row("c_code", camera.GetValue<string>("c_code"));
                row.AddValue2Row("c_operation", "Update");
            }
            else
            {
                string c_id = Guid.NewGuid().ToString();
                row.AddValue2Row("c_id", c_id);
                row.AddValue2Row("c_code", "");
                row.AddValue2Row("c_operation", "Add");
            }
            row.AddValue2Row("c_storey", itr.GetValue<string>("storey"));
            row.AddValue2Row("c_buildingName", itr.GetValue<string>("buildingName"));
            row.AddValue2Row("c_header", itr.GetValue<string>("name"));
            row.AddValue2Row("c_english_name", itr.GetValue<string>("EnglishName"));
            row.AddValue2Row("c_dvr", itr.GetValue<string>("DVR"));
            row.AddValue2Row("c_num", itr.GetValue<string>("Num"));
            row.AddValue2Row("c_ip", itr.GetValue<string>("IP"));
            row.AddValue2Row("c_channel", itr.GetValue<string>("Channel"));
            row.AddValue2Row("c_render_option", "CCTV");
            row.AddValue2Row("c_building_id", itr.GetValue<string>("buildingId"));
            row.AddValue2Row("c_storey_id", itr.GetValue<string>("storeyId"));
            row.AddValue2Row("AlarmLimit", "22:00-7:00");
            row.AddValue2Row("CanControl", SphereCamera.Contains(row["c_channel"] + ""));
            row.AddValue2Row("pointLocation", itr.SelectToken("pointLocation").ToString());

            return row;
        }

        private JToken GetCameraByChannel(JArray cameras, string channel)
        {
            foreach (var camera in cameras)
            {
                var rst = camera.GetValue<string>("c_channel");
                if (string.IsNullOrEmpty(rst)) continue;
                if (rst.StrEqual(channel))
                    return camera;
            }
            return null;
        }

        public JArray LoadJarrayFile(string path)
        {
            var json = File.ReadAllText(path);
            var token = JToken.Parse(json);
            if (token.Type == JTokenType.Object)
            {
                return token.SelectToken("RECORDS") as JArray ?? new JArray();
            }
            else
            {
                return token as JArray;
            }
        }
        #endregion

        #region  更新摄像头分组

        public void UpgradeCameraGroup(JArray jr)
        {
            if (File.Exists(ControlFile)) return;
            var group = LoadJarrayFile(CameraGroupFile);
            var cameraIds = jr.Select(x => x.GetValue<string>("c_id")).ToArray();
            for (var i = 0; i < group.Count; i++)
            {
                var itr = group[i];
                var id = itr.GetValue<string>("c_camera_id");
                if (!cameraIds.Contains(id))
                    group.Remove(itr);
            }
            var path = ExcelHelper.GetSaveFilePath();
            ExcelHelper.WriteJArray(path, group);
        }

        #endregion

        public void Log(string msg)
        {
            Logger.Log(msg);
        }
    }
}