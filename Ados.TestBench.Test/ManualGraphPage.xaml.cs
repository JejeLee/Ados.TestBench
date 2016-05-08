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
using Microsoft.Research.DynamicDataDisplay;

namespace Ados.TestBench.Test
{
    /// <summary>
    /// ManualGraphPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualGraphPage : Page
    {
        public ManualGraphPage(object aModel)
        {
            this.DataContext = aModel;

            InitializeComponent();

            SetLineGraph();
        }

        void SetLineGraph()
        {
            a1m.Description = new PenDescription(Model.A1.Description1);
            a1r.Description = new PenDescription(Model.A1.Description2);
        }

        internal ManualModel Model { get { return (ManualModel)this.DataContext; } }
    }
    
}
