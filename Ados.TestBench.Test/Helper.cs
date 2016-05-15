using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Ados.TestBench.Test
{
    public static class Helper
    {
        public static string AppDir { get {
                var fi = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
                return fi.DirectoryName;
            } }

        public static string SheetsDir { get { return AppDir + @"\TestSheets"; } }

        public static string NewCSVPath()
        {
            var path = AppDir + "\\Results";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            path += "\\"+ DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

            return path;
        }
    }

    public class MultiplyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
               object parameter, CultureInfo culture)
        {
            double result = 1.0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is double)
                    result *= (double)values[i];
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
               object parameter, CultureInfo culture)
        {
            throw new Exception("Not implemented");
        }
    }
}
