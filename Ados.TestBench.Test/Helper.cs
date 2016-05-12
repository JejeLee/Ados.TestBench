using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
