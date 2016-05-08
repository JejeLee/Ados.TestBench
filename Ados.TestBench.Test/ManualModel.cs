using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace Ados.TestBench.Test
{
    internal class ManualModel : INotifyPropertyChanged
    {
        public ManualModel(ControllerModel aCtrlModel)
        {
            try
            {
                _controller = aCtrlModel;

                LoadSettings();

                _cmd.ExecuteHandler = ExecuteCommand;
                _cmd.CanExecuteHandler = CanExecuteCommand;

                LinManager.JobStateChangedEvent += LinManager_JobStateChangedEvent;

                this.ReadDuration = 1000;
                this.UpdateData = true;
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show("ManualModel: 초기화 중 에러 발생:" + e.ToString());
                if (!Extension.IsDesignMode)
                    Environment.Exit(1);
            }
        }

        private void LinManager_JobStateChangedEvent(bool aUnderLoopJob)
        {
            if (aUnderLoopJob == false && UpdateData == false)
            {
                foreach (var s in _statesTmp)
                    _states.Add(s);

                this.Angle = _states.LastOrDefault().DoorAngle;
            }
            _statesTmp.Clear();
        }

        void ExecuteCommand(object aInput)
        {
            if (aInput == null)
                return;

            string input = aInput.ToString();

            switch (input)
            {
                case "readall":
                    {
                        Controller.LinMgr.ReadParameters(ManaulParameterSetting.Settings);
                    }
                    break;
                case "writeall":
                    {
                        Controller.LinMgr.WriteParameters(ManaulParameterSetting.Settings);
                    }
                    break;
                case "dooropen":
                    if (Controller.LinMgr.WriteCommand(1))
                    {
                        Controller.LinMgr.ReadStateLoop(this.ReadDuration);
                    }
                    break;
                case "doorclose":
                    Controller.LinMgr.WriteCommand(2);
                    LinManager.StopLoopJob();
                    
                    break;
            }
        }

        bool CanExecuteCommand(object aInput)
        {
            if (aInput == null)
                return false;

            string input = aInput.ToString();
            if (LinManager.UnderLoopJob)
                return false;

            bool cando = true;

            switch (input)
            {
                case "readall":
                    break;
                case "writeall":
                    break;
                case "dooropen":
                    break;
                case "doorclose":
                    break;
            }
            return cando;
        }

        private void LMgr_ParameterReceived(int aAddr, int aValue)
        {
            var set = ManaulParameterSetting.Settings.First(x => x.Info.Address == aAddr);
            if (set != null)
                set.ReadValue = aValue;
        }

        private void LMgr_StateReceived(StateShot aShot)
        {
            if (UpdateData)
            {
                Angle = aShot.DoorAngle;
                _states.Add(aShot);
            }
            else
            {
                _statesTmp.Add(aShot);
            }
        }

        private void LoadSettings()
        {
            var fi = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
            var dir = fi.DirectoryName;
            if (Extension.IsDesignMode)
                dir = @"f:\prj\mediaever\Ados.TestBench\Ados.TestBench.Test\bin\Debug";

            this.ManaulParameterSetting = ParameterSettings.Load(dir + "\\Settings\\ManualParameters.json");

            this._graphInfos = GraphInfo.Load(dir + "\\Settings\\Graphs.json");
            
        }

        public ObservableCollection<StateShot> StatesData { get { return _states; } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void ClearStates()
        {
            _states.Clear();
            Angle = 0;
        }

        public bool IsActive {
            get { return _active; }
            set {
                if (value != _active)
                {
                    _active = value;
                    if (_active)
                    {
                        LinManager.StateReceivedEvent += LMgr_StateReceived;
                        LinManager.ParameterReceivedEvent += LMgr_ParameterReceived;
                    }
                    else
                    {
                        LinManager.StateReceivedEvent -= LMgr_StateReceived;
                        LinManager.ParameterReceivedEvent -= LMgr_ParameterReceived;
                    }
                }        
            }
        }

        public double Angle { get { return _angle * -1; }
            set {
                if (_angle != value)
                {
                    _angle = value;
                    OnPropertyChanged("Angle");
                    OnPropertyChanged("AngleText");
                }
            }
        }

        public string AngleText
        {
            get { return string.Format("{0} 도", _angle); }
           
        }

        public bool UpdateData { get; set; }

        public DelegateCommand Command { get { return _cmd; } }

        public ControllerModel Controller { get { return _controller; } }

        public ParameterSettings ManaulParameterSetting { get; private set; }

        public int ReadDuration { get; set; }

        private bool _active = false;

        public GraphInfo A1 { get { return _graphInfos["a1"]; } }
        public GraphInfo A2 { get { return _graphInfos["a2"]; } }
        public GraphInfo A3 { get { return _graphInfos["a3"]; } }
        public GraphInfo A4 { get { return _graphInfos["a4"]; } }
        public GraphInfo D1 { get { return _graphInfos["d1"]; } }
        public GraphInfo D2 { get { return _graphInfos["d2"]; } }
        public GraphInfo D3 { get { return _graphInfos["d3"]; } }
        public GraphInfo D4 { get { return _graphInfos["d4"]; } }
        public GraphInfo D5 { get { return _graphInfos["d5"]; } }
        public GraphInfo D6 { get { return _graphInfos["d6"]; } }
        public GraphInfo D7 { get { return _graphInfos["d7"]; } }

        ObservableCollection<StateShot> _states = new ObservableCollection<StateShot>();
        List<StateShot> _statesTmp = new List<StateShot>();
        ControllerModel _controller;
        DelegateCommand _cmd = new DelegateCommand();
        Dictionary<string, GraphInfo> _graphInfos;
        double _angle = 0;
    }
}
