using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

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
                _linmgr = new LinManager();

                _linmgr.RefreshHardware();
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show("초기화 중 에러 발생:" + e.ToString());
                Environment.Exit(1);
            }
        }

        private void LoadSettings()
        {
            var fi = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
            var dir = fi.DirectoryName;

            ParameterInfo.Load(dir + "\\Settings\\Parameters.json");

            this.ManaulParameterSetting = ParameterSettings.Load(dir + "\\Settings\\ManualParameters.json");

            _graphs = GraphInfo.Load(dir + "\\Settings\\Graphs.json");
        }

        private const int MAX_LOGS = 1000;
        private const int SAVE_LOGS = 500;

        public void LogSave()
        {
            var fi = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
            var dir = fi.DirectoryName + "\\Log";

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

        public void GraphSave()
        {
            var fi = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
            var dir = fi.DirectoryName;

            GraphInfo.Save(dir + "\\Settings\\Graphs.json", _graphs);
        }

        private void LogReceived(LogData aData)
        {
            //  1000개 이상일 경우 500개 저장 후 제거.
            if (_logs.Count >= MAX_LOGS)
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

        private void OnPropertyChanged(String propertyName)
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
        }

        public IEnumerable<LinDevice> Devices { get { return _linmgr.Devices; } }

        public ObservableCollection<LogData> LogsData { get { return _logs; } }

        public ManualModel Manual { get { return _mmodel; } }

        public LinManager LinMgr { get { return _linmgr; } }

        public IEnumerable<ParameterInfo> Parameters { get { return ParameterInfo.Parameters; } }

        public ParameterSettings ManaulParameterSetting { get; private set; }

        public Dictionary<string, GraphInfo> Graphs { get { return _graphs; } }

        ObservableCollection<LogData> _logs = new ObservableCollection<LogData>();
        ObservableCollection<StateShot> _states = new ObservableCollection<StateShot>();
        ManualModel _mmodel;
        LinManager _linmgr;
        Dictionary<string, GraphInfo> _graphs = new Dictionary<string, GraphInfo>();
    }
}
