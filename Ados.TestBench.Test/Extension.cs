using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ados.TestBench.Test
{
    public static class Extension
    {
        public static int Int (this string s)
        {
            int result = 0;
            if (s.StartsWith("0x"))
            {
                Int32.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out result);
            }
            else
            {
                Int32.TryParse(s, out result);
            }
            return result;
        }

        public static bool IsDesignMode { get { return System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()); } }
    }
}
