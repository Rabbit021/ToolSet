using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace ToolSetsCore
{
    public class IndexConverter : IValueConverter
    {
        #region IValueConverter Members
        //Convert the Item to an Index
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var drv = (System.Data.DataRowView)value;
                var dg = (DataGrid)Application.Current.MainWindow.FindName(parameter + "");
                CollectionView cv = (CollectionView)dg.Items;
                int rowindex = cv.IndexOf(drv) + 1;
                return rowindex.ToString();
            }
            catch (Exception e)
            {
                throw new NotImplementedException(e.Message);
            }
        }
        //One way binding, so ConvertBack is not implemented
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class State2BrushConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var rst = (int)value;
                switch (rst)
                {
                    case 0:
                        return Application.Current.FindResource("Normal") as SolidColorBrush;
                    case 1:
                        return Application.Current.FindResource("Success") as SolidColorBrush;
                    case 2:
                        return Application.Current.FindResource("Failed") as SolidColorBrush;
                }

            }
            catch
            {
                return new SolidColorBrush(Colors.White);
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}