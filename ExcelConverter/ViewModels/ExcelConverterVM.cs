using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using ToolSetsCore;

namespace ExcelConverter.ViewModels
{
    [Export]
    [Export("ExcelConverterVM", typeof(ExcelConverterVM))]
    public class ExcelConverterVM : ToolSetVMBase
    {
        public ICommand ConvertToExcelCommand { get; set; }

        public override void RegisterCommands()
        {
            base.RegisterCommands();
            ConvertToExcelCommand = new DelegateCommand<object>(ConvertToExcel);
        }

        private void ConvertToExcel(object obj)
        {
            try
            {
                if (string.IsNullOrEmpty(JsonFile)) return;
                var defaulfile = Path.GetFileName(Path.ChangeExtension(JsonFile, ".xlsx"));
                defaulfile = ExcelHelper.GetSaveFilePath(defaulfile);
                var jarray = JsonFile.LoadJarrayFile();
                ExcelHelper.WriteJArray(defaulfile, jarray);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
            Logger.Log("Finished ConvertToExcel!");
        }

        #region JsonFile

        public string JsonFile
        {
            get => (string)GetValue(JsonFileProperty);
            set => SetValue(JsonFileProperty, value);
        }

        public static readonly DependencyProperty JsonFileProperty =
            DependencyProperty.Register("JsonFile", typeof(string), typeof(ExcelConverterVM), new PropertyMetadata((sender, e) =>
            {
                var vm = sender as ExcelConverterVM;
            }));

        #endregion
    }
}