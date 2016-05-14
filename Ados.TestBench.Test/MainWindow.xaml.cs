using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace Ados.TestBench.Test
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            _mainWin = this;

            InitializeComponent();

            this.Loaded += MainWindow_Loaded;

            if (_devList.Items.Count > 0)
                _devList.SelectedIndex =
                    (_devList.Items.Count > Properties.Settings.Default.DeviceIdx)
                    ? Properties.Settings.Default.DeviceIdx : 0;

        }

        DispatcherTimer _timer;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _manualPage = new ManualPage(Model.Manual);
            //_autoPage = new AutoPage(Model.Auto);

            _timer = new DispatcherTimer()
            {
                Interval = new TimeSpan(100 * 10000),
            };

            _modePages.Navigate(_manualPage);

            if (Model.LogsData.Count > 0)
                _outMsg.ScrollIntoView(Model.LogsData.Last());
            Log.LogEvent += Log_LogEvent;

            _timer.Tick += Transfer_Tick;
            _timer.Start();
        }

        private void Log_LogEvent(LogData aData)
        {
            if (!this.CheckAccess())
            {
                Dispatcher.Invoke(() => Log_LogEvent(null));
                return;
            }

            if (!_outMsg.IsFocused && Model.LogsData.Count > 0)
            {
                _outMsg.ScrollIntoView(Model.LogsData.Last());
            }
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
            Model.Manual.UpdateStates();
        }
              
        ControllerModel model { get { return (ControllerModel)this.DataContext; } }

        ManualPage _manualPage;
        //AutoPage _autoPage = null;

        private void testMode_changed(object sender, SelectionChangedEventArgs e)
        {
            if (_manualPage == null)
                return;

            if (_modeNavigation.SelectedIndex == 0)
            {
                _modePages.Navigate(_manualPage);
                //Model.Auto.IsActive = false;
                Model.Manual.IsActive = true;
            }
            else
            {
                //_modePages.Navigate(_autoPage);
                //Model.Manual.IsActive = false;
                //Model.Auto.IsActive = true;
            }

        }

        private void LogScroll_Click(object sender, RoutedEventArgs e)
        {
            if (Model.LogsData.Count > 0)
                _outMsg.ScrollIntoView(Model.LogsData.Last());
        }

        internal ControllerModel Model { get { return (ControllerModel)this.DataContext; } }

        private static MainWindow _mainWin = null;
        public static void MessageBox(string aMessage, string aTitle = "ADOS 메세지 창")
        {
            if (_mainWin != null && !string.IsNullOrEmpty(aMessage))
            {
                var dlg = _mainWin.ShowMessageAsync(aTitle, aMessage, MessageDialogStyle.Affirmative);
            }
        }

        public static async Task<ProgressDialogController> ProgressBox(string aMessage, string aTitle = "ADOS 진행 상태 창")
        {
            if (_mainWin != null && !string.IsNullOrEmpty(aMessage))
            {
                var ctrl = await _mainWin.ShowProgressAsync(aTitle, aMessage, true);
                return ctrl;
            }
            return null;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Model.SaveSettings();
            Model.Manual.SaveSettings();
        }

    }

}
