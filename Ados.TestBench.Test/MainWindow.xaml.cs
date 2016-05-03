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
using MahApps.Metro.Controls;
using System.Windows.Threading;

namespace Ados.TestBench.Test
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            _manualPage = new ManualPage();
            _autoPage = new AutoPage();

            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = new TimeSpan(100 * 10000),
            };
            timer.Tick += Transfer_Tick;
            timer.Start();


            _modePages.Navigate(_manualPage);

            if (_devList.Items.Count > 0)
                _devList.SelectedIndex =
                    (_devList.Items.Count > Properties.Settings.Default.DeviceIdx)
                    ? Properties.Settings.Default.DeviceIdx : 0;

        }


        private void Transfer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now.Ticks;

            if (LinManager.IsRxError)
            {
                _rxLamp.Fill = (Brush)this.Resources["rampError"];
            }
            else
            {
                _rxLamp.Fill = LinManager.RxTics > now ?
                    (Brush)this.Resources["rampActive"] : (Brush)this.Resources["rampIdle"];
            }
            if (LinManager.IsTxError)
            {
                _txLamp.Fill = (Brush)this.Resources["rampError"];
            }
            else
            {
                _txLamp.Fill = LinManager.TxTics > now ?
                    (Brush)this.Resources["rampActive"] : (Brush)this.Resources["rampIdle"];
            }
        }

        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            model.RefreshDevice();
        }

        private void cbDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            model.LinMgr.Device = (LinDevice)_devList.SelectedItem;
        }

        private void outMsg_Loaded(object sender, RoutedEventArgs e)
        {
            _outMsg.ItemsSource = model.LogsData;
        }


        ControllerModel model { get { return (ControllerModel)this.DataContext; } }

        ManualPage _manualPage;
        AutoPage _autoPage;

        private void testMode_changed(object sender, SelectionChangedEventArgs e)
        {
            if (_modeNavigation.SelectedIndex == 0)
                _modePages.Navigate(_manualPage);
            else
                _modePages.Navigate(_autoPage);
        }
    }
}
