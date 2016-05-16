using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Ados.TestBench.Test
{
    internal class ControllerModel : INotifyPropertyChanged
    {
        public ControllerModel()
        {
            try
            {
                Log.LogEvent += LogReceived;

                LoadSettings();

                _mmodel = new ManualModel(this);
                _amodel = new AutoModel(this);
                _linmgr = new LinManager();

                RefreshDevice();

                _cmd.ExecuteHandler = ExecuteCommand;
                _cmd.CanExecuteHandler = CanExecuteCommand;
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show("ControllerModel: 초기화 중 에러 발생:" + e.ToString());
                if (!Extension.IsDesignMode)
                    Environment.Exit(1);
            }
        }

        void ExecuteCommand(object aInput)
        {
            if (aInput == null)
                return;

            string input = aInput.ToString();

            switch(input)
            {
                case "refreshDevices":
                    RefreshDevice();
                    break;
                case "clearLog":
                    LogSave();
                    break;
                case "updateGraph":
                    Manual.ExecuteCommand(input);
                    break;
            }
        }

        bool CanExecuteCommand(object aInput)
        {
            if (aInput == null)
                return false;

            string input = aInput.ToString();
            bool cando = true;

            switch (input)
            {
                case "refreshDevices":
                    cando = true;
                    break;
                case "clearLog":
                    cando = _logs.Count > 0;
                    break;
            }
            return cando;
        }

        private void LoadSettings()
        {
            var dir = Helper.AppDir;
            if (Extension.IsDesignMode)
                dir = @"f:\prj\mediaever\Ados.TestBench\Ados.TestBench.Test\bin\Debug";
            ParameterInfo.Load(dir + "\\Settings\\Parameters.json");

        }

        public void SaveSettings()
        {
            LogSave();
        }

        public Visibility VisibleAngle
        {
            get
            {
                var grid = (Application.Current.MainWindow as MainWindow)._outMsg;
                var v = (LinManager.UnderLoopJob ||  
                    (grid != null && grid.Uid != "focus") ) ?
                    Visibility.Visible : Visibility.Collapsed;
                return v;
            }
        }

        private const int MAX_LOGS = 10000;
        private const int SAVE_LOGS = 3000;

        public void LogSave()
        {
            var dir = Helper.AppDir+ "\\Log";

            var filename = string.Format(dir + "\\{0}.log", DateTime.Now.ToString("yyyy_MM_dd"));
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (StreamWriter sw = new StreamWriter(filename, true))
            {
                foreach (var log in _logs)
                {
                    sw.WriteLine("{0} <{1}> {2}", log.Time.ToString(), log.IsError ? "E" : "I", log.Message);
                }

                _logs.Clear();
            }
        }

        private void LogReceived(LogData aData)
        {
            if (this.Manual == null || this.Manual.GraphPage == null)
            {
                _logs.Add(aData);
                return;
            }

            if (!this.Manual.GraphPage.CheckAccess())
            {
                this.Manual.GraphPage.Dispatcher.Invoke(() => _logs.Add(aData));
                return;
            }
            //  N 이상일 경우 N/2 개 저장 후 제거.
            if (_logs.Count >= MAX_LOGS && LinManager.UnderLoopJob == false)
            {
                var fi = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
                var dir = fi.DirectoryName + "\\Log";

                var filename = string.Format(dir + "\\{0}.log", DateTime.Now.ToString("yyyy_MM_dd"));
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (StreamWriter sw = new StreamWriter(filename, true))
                {
                    for (int i = 0; i < SAVE_LOGS; i++)
                    {
                        var log = _logs[i];
                        sw.WriteLine("{0} <{1}> {2}", log.Time.ToString(), log.IsError ? "E" : "I", log.Message);
                    }
                    for (int i = 0; i < SAVE_LOGS; i++)
                    {
                        _logs.RemoveAt(0);
                    }
                }

            }
            _logs.Add(aData);

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void RefreshDevice()
        {
            _linmgr.RefreshHardware();
            OnPropertyChanged("Devices");
            if (Devices.Count() > 0)
                CurrentDevice = Devices.First();
        }

        public LinDevice CurrentDevice
        {
            get { return _linmgr.Device; }
            set { _linmgr.Device = value; OnPropertyChanged("CurrentDevice"); }
        }

        public IEnumerable<LinDevice> Devices { get { return _linmgr.Devices; } }

        public ObservableCollection<LogData> LogsData { get { return _logs; } }

        public ManualModel Manual { get { return _mmodel; } }

        public AutoModel Auto { get { return _amodel; } }

        public LinManager LinMgr { get { return _linmgr; } }

        public IEnumerable<ParameterInfo> Parameters { get { return ParameterInfo.Parameters; } }

        public IEnumerable<GraphInfo> Graphs { get { return Manual.GraphInfos; } }

        public DelegateCommand Command { get { return _cmd; } }

        ObservableCollection<LogData> _logs = new ObservableCollection<LogData>();
        ObservableCollection<StateShot> _states = new ObservableCollection<StateShot>();
        ManualModel _mmodel;
        AutoModel _amodel;
        LinManager _linmgr;
        DelegateCommand _cmd = new DelegateCommand();
    }
}
