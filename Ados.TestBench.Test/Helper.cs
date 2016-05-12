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
                return System.Reflection.Assembly.GetEntryAssembly().Location;
            } }

        public static string SheetsDir { get { return AppDir + @"\TestSheets"; } }
    }
}
