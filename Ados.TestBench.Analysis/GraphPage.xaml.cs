using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.DynamicDataDisplay.Common;

namespace Ados.TestBench.Analysis
{
    /// <summary>
    /// ManualGraphPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GraphPage : Page
    {
        public GraphPage()
        {
            InitializeComponent();
        }
    }

    public class VoltagePointCollection : D3Collection<VoltagePoint>
    {
        public VoltagePointCollection()
        {
        }
    }

    public class VoltagePoint
    {
        public DateTime Date { get; set; }

        public double Voltage { get; set; }

        public VoltagePoint(double voltage, DateTime date)
        {
            this.Date = date;
            this.Voltage = voltage;
        }
    }

}
