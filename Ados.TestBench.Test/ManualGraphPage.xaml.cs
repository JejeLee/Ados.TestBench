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
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

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

        public void UpdateGraphInfo()
        {
            Rect r;

            r = a1.Visible;
            r.Y = Model.A1.Min - (Model.A1.Max - Model.A1.Min) / 10;
            r.Height = (Model.A1.Max - Model.A1.Min) + (Model.A1.Max - Model.A1.Min) / 10;
            a1.Visible = r;

            r = a2.Visible;
            r.Y = Model.A2.Min - (Model.A2.Max - Model.A2.Min) / 10;
            r.Height = (Model.A2.Max - Model.A2.Min) + (Model.A2.Max - Model.A2.Min) / 10;
            a2.Visible = r;

            r = a3.Visible;
            r.Y = Model.A3.Min - (Model.A3.Max - Model.A3.Min) / 10;
            r.Height = (Model.A3.Max - Model.A3.Min) + (Model.A3.Max - Model.A3.Min) / 10;
            a3.Visible = r;

            r = a4.Visible;
            r.Y = Model.A4.Min - (Model.A4.Max - Model.A4.Min) / 10;
            r.Height = (Model.A4.Max - Model.A4.Min) + (Model.A4.Max - Model.A4.Min) / 10;
            a4.Visible = r;

            int min = int.MaxValue, max = int.MinValue;
            foreach (var dd in new GraphInfo[] { Model.D1, Model.D2, Model.D3, Model.D4, Model.D5, Model.D6, Model.D7 })
            {
                min = Math.Min(min, dd.Min);
                max = Math.Max(max, dd.Max);
            }
            r = d1.Visible;
            r.Y = min - 1;
            r.Height = max - min + 2;
            d1.Visible = r;
        }

        public void UpdateTimeScroll(StateShot aShot)
        {
            Rect r;
            r = a1.Visible;

            double base10ms = GraphInfo.TimeUnit(aShot.Time) - StateShot.TimeBase - r.Width;
            if (base10ms <= 0)
                return;

            r.X = base10ms;
            a1.Visible = r;
        }

        void SetLineGraph()
        {
            double maxh = GraphInfo.TimeRange / 1000.0;
            
            a1.AddLineGraph(Model.A1.DataSource, new Pen(Brushes.BlueViolet, 1),
                new CirclePointMarker { Size = 3.5, Fill = Brushes.Blue }, new PenDescription(Model.A1.Description1));
            a1.AddLineGraph(Model.A1.DataSource2, new Pen(Brushes.OrangeRed, 1),
                new CirclePointMarker { Size = 3.5, Fill = Brushes.Blue }, new PenDescription(Model.A1.Description2));

            a2.AddLineGraph(Model.A2.DataSource, new Pen(Brushes.OrangeRed, 1),
                new CirclePointMarker { Size = 3.5, Fill = Brushes.Blue }, new PenDescription(Model.A2.Description1));

            a3.AddLineGraph(Model.A3.DataSource, new Pen(Brushes.BlueViolet, 1),
                new CirclePointMarker { Size = 3.5, Fill = Brushes.Blue }, new PenDescription(Model.A3.Description1));
            a3.AddLineGraph(Model.A3.DataSource2, new Pen(Brushes.OrangeRed, 1),
                new CirclePointMarker { Size = 3.5, Fill = Brushes.Blue }, new PenDescription(Model.A3.Description2));

            a4.AddLineGraph(Model.A4.DataSource, new Pen(Brushes.BlueViolet, 1),
                new CirclePointMarker { Size = 3.5, Fill = Brushes.Blue }, new PenDescription(Model.A4.Description1));
            a4.AddLineGraph(Model.A4.DataSource2, new Pen(Brushes.OrangeRed, 1),
                new CirclePointMarker { Size = 3.5, Fill = Brushes.Blue }, new PenDescription(Model.A4.Description2));

            d1.AddLineGraph(Model.D1.DataSource, new Pen(Brushes.OrangeRed, 1),
                new CirclePointMarker { Size = 3.5, Fill = Brushes.Blue }, new PenDescription(Model.D1.Description1));
            d1.AddLineGraph(Model.D2.DataSource, new Pen(Brushes.DarkGray, 1),
                new CirclePointMarker { Size = 3.5, Fill = Brushes.Blue }, new PenDescription(Model.D2.Description1));
            d1.AddLineGraph(Model.D3.DataSource, new Pen(Brushes.CadetBlue, 1), 
                new CirclePointMarker { Size = 3.5, Fill = Brushes.Blue }, new PenDescription(Model.D3.Description1));
            d1.AddLineGraph(Model.D4.DataSource, new Pen(Brushes.DarkBlue, 1), 
                new CirclePointMarker { Size = 3.5, Fill = Brushes.Blue }, new PenDescription(Model.D4.Description1));
            d1.AddLineGraph(Model.D5.DataSource, new Pen(Brushes.IndianRed, 1), 
                new CirclePointMarker { Size = 3.5, Fill = Brushes.Blue }, new PenDescription(Model.D5.Description1));
            d1.AddLineGraph(Model.D6.DataSource, new Pen(Brushes.GreenYellow, 1), 
                new CirclePointMarker { Size = 3.5, Fill = Brushes.Blue }, new PenDescription(Model.D6.Description1));
            d1.AddLineGraph(Model.D7.DataSource, new Pen(Brushes.Green, 1), 
                new CirclePointMarker { Size = 3.5, Fill = Brushes.Blue }, new PenDescription(Model.D7.Description1));

            Rect r = new Rect();
            r.X = 0; r.Width = maxh;

            a1.Visible = r;
            a2.Visible = r;
            a3.Visible = r;
            a4.Visible = r;
            d1.Visible = r;

            UpdateGraphInfo();

            Microsoft.Research.DynamicDataDisplay.Navigation.MouseNavigation.zoomY = false;

            ViewportElement2D.ViewAxisXChangedEvent += ViewportElement2D_ViewAxisXChangedEvent;
        }

        private void ViewportElement2D_ViewAxisXChangedEvent(double X, double Width)
        {
            foreach (var c in new ChartPlotter[] { a1, a2, a3, a4, d1})
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
