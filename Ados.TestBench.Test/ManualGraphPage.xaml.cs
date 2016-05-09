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

        }

        void SetLineGraph()
        {
            a1.AddLineGraph(Model.A1.DataSource, Colors.BlueViolet, 1, Model.A1.Description1);
            a1.AddLineGraph(Model.A1.DataSource2, Colors.OrangeRed, 1, Model.A1.Description2);
            a1.Visible = new Rect(0, Model.A1.Min - (Model.A1.Max - Model.A1.Min)/10, 500, Model.A1.Max + (Model.A1.Max - Model.A1.Min) / 10);

            a2.AddLineGraph(Model.A2.DataSource, Colors.OrangeRed, 1, Model.A2.Description1);
            a2.Visible = new Rect(0, Model.A2.Min - (Model.A2.Max - Model.A2.Min) / 10, 500, Model.A2.Max + (Model.A2.Max - Model.A2.Min) / 10);

            a3.AddLineGraph(Model.A3.DataSource, Colors.BlueViolet, 1, Model.A3.Description1);
            a3.AddLineGraph(Model.A3.DataSource2, Colors.OrangeRed, 1, Model.A3.Description2);
            a3.Visible = new Rect(0, Model.A3.Min - (Model.A3.Max - Model.A3.Min) / 10, 500, Model.A3.Max + (Model.A3.Max - Model.A3.Min) / 10);

            a4.AddLineGraph(Model.A4.DataSource, Colors.BlueViolet, 1, Model.A4.Description1);
            a4.AddLineGraph(Model.A4.DataSource2, Colors.OrangeRed, 1, Model.A4.Description2);
            a4.Visible = new Rect(0, Model.A4.Min - (Model.A4.Max - Model.A4.Min) / 10, 500, Model.A4.Max + (Model.A4.Max - Model.A4.Min) / 10);

            d1.AddLineGraph(Model.D1.DataSource, Colors.OrangeRed, 1, Model.D1.Description1);
            d1.Visible = new Rect(0, Model.D1.Min - (Model.D1.Max - Model.D1.Min) / 10, 500, Model.D1.Max + (Model.D1.Max - Model.D1.Min) / 10);

            d2.AddLineGraph(Model.D2.DataSource, Colors.OrangeRed, 1, Model.D2.Description1);
            d2.Visible = new Rect(0, Model.D2.Min - (Model.D2.Max - Model.D2.Min) / 10, 500, Model.D2.Max + (Model.D2.Max - Model.D2.Min) / 10);

            d3.AddLineGraph(Model.D3.DataSource, Colors.OrangeRed, 1, Model.D3.Description1);
            d3.Visible = new Rect(0, Model.D3.Min - (Model.D3.Max - Model.D3.Min) / 10, 500, Model.D3.Max + (Model.D3.Max - Model.D3.Min) / 10);

            d4.AddLineGraph(Model.D4.DataSource, Colors.OrangeRed, 1, Model.D4.Description1);
            d4.Visible = new Rect(0, Model.D4.Min - (Model.D4.Max - Model.D4.Min) / 10, 500, Model.D4.Max + (Model.D4.Max - Model.D4.Min) / 10);

            d5.AddLineGraph(Model.D5.DataSource, Colors.OrangeRed, 1, Model.D5.Description1);
            d5.Visible = new Rect(0, Model.D5.Min - (Model.D5.Max - Model.D5.Min) / 10, 500, Model.D5.Max + (Model.D5.Max - Model.D5.Min) / 10);

            d6.AddLineGraph(Model.D6.DataSource, Colors.OrangeRed, 1, Model.D6.Description1);
            d6.Visible = new Rect(0, Model.D6.Min - (Model.D6.Max - Model.D6.Min) / 10, 500, Model.D6.Max + (Model.D6.Max - Model.D6.Min) / 10);

            d7.AddLineGraph(Model.D7.DataSource, Colors.OrangeRed, 1, Model.D7.Description1);
            d7.Visible = new Rect(0, Model.D7.Min - (Model.D7.Max - Model.D7.Min) / 10, 500, Model.D7.Max + (Model.D7.Max - Model.D7.Min) / 10);

            Microsoft.Research.DynamicDataDisplay.Navigation.MouseNavigation.zoomY = false;

            ViewportElement2D.ViewAxisXChangedEvent += ViewportElement2D_ViewAxisXChangedEvent;
        }

        private void ViewportElement2D_ViewAxisXChangedEvent(double X, double Width)
        {
            foreach (var c in new ChartPlotter[] { a1, a2, a3, a4, d1, d2, d3, d4, d5, d6, d7})
            {
                var vr = c.Visible;
                vr.X = X;
                vr.Width = Width;
                c.Visible = vr;
            }           
        }

        internal ManualModel Model { get { return (ManualModel)this.DataContext; } }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetLineGraph();
        }
    }
    
}
