using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Threading;
using System.Diagnostics;

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

                this.AutoDuration = 1000;
                this.AutoRepeat = 1;
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
            if (GraphPage == null)
                return;

            if (!GraphPage.CheckAccess())
            {
                GraphPage.Dispatcher.BeginInvoke(new Action(
                    () => LinManager_JobStateChangedEvent(aUnderLoopJob)));
                return;
            }

            if (aUnderLoopJob == false)
            {
                this.CursorVisible = Visibility.Visible;
            }
            else
            {
                this.CursorVisible = Visibility.Collapsed;
            }
            OnPropertyChanged("CursorVisible");
            Controller.OnPropertyChanged("VisibleAngle");
        }

        public void UpdateStates()
        {
            if (_qstates.Count > 0 && (UpdateData || LinManager.UnderLoopJob == false))
            {
                lock (_osync)
                {
                    _states.AddItems(_qstates);
                    _qstates.Clear();
                }

                this.Angle = _states.LastOrDefault().DoorAngle;
                this.GraphPage.UpdateTimeScroll(_states.LastOrDefault());
            }
        }

        public Visibility CursorVisible
        {
            get;
            set;
        }

        public void ExecuteCommand(object aInput)
        {
            if (aInput == null)
                return;

            string input = aInput.ToString();

            if (!CanExecuteCommand(input))
            {
                Log.i("Can't not run Command <" + input + ">, it is busy...!");
                return;
            }

            Log.i("Run Command <" + input + ">");

            switch (input)
            {
                case "readall":
                    {
                        Controller.LinMgr.ReadParametersAsync(ManaulParameterSetting.Settings);
                    }
                    break;
                case "writeall":
                    {
                        Controller.LinMgr.WriteParametersAsync(ManaulParameterSetting.Settings);
                    }
                    break;
                case "dooropen":
                    Controller.LinMgr.WriteCommand(LinManager.DOOR_OPEN);
                    break;
                case "doorclose":
                    Controller.LinMgr.WriteCommand(LinManager.DOOR_STOP);
                    LinManager.StopLoopJob();
                    break;
                case "updateGraph":
                    UpdateGraph();
                    break;
                case "autostart":
                    AutoRunAsync();
                    break;
                case "autosave":
                    AutoSave();
                    break;
                case "autoexcel":
                    AutoExcel();
                    break;
                case "doorstates":
                    ReadStates();
                    break;
                case "clearstates":
                    ClearStates();
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
                case "autostart":
                    break;
                case "autosave":
                    break;
                case "autoexcel":
                    break;
                case "doorstates":
                    break;
                case "clearstates":
                    break;
            }
            return cando;
        }

        void AutoSave()
        {
            if (_states.Count > 0)
            {
                _csvPath = StateShot.SaveCsv(Helper.NewCSVPath(), this.StatesData);
            }
        }

        void AutoExcel()
        {
            AutoSave();
            if (_csvPath != null)
            {
                ProcessStartInfo psi = new ProcessStartInfo(_csvPath);
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
        }

        void ReadStates()
        {
            Controller.LinMgr.ReadStateLoopAsync(this.AutoDuration);
        }

        string _csvPath = null;

        public int AutoDuration { get; set; }
        public int AutoRepeat { get; set; }

        void AutoRunAsync()
        {
            ClearStates();

            Task.Run(() => AutoRun());
        }

        void AutoRun()
        {
            int duration = this.AutoDuration;
            int repeat = this.AutoRepeat;

            try
            {
                LinManager.UnderLoopJob = true;

                while (repeat-- > 0 )
                {
                    Controller.LinMgr.WriteCommand(2); // door close
                    Thread.Sleep(100);

                    Controller.LinMgr.WriteCommand(1); // door open
                    //Thread.Sleep(100);

                    Controller.LinMgr.ReadStateLoop(duration, false); 
                }
                Controller.LinMgr.WriteCommand(2); // door close
            }
            catch(Exception e)
            {
                Log.e("AutoRun 에외 발생: " + e.ToString());
            }
            finally
            {
                Controller.LinMgr.WriteCommand(2); // door close
                LinManager.UnderLoopJob = false;
            }
        }

        private void LMgr_ParameterReceived(int aAddr, int aValue)
        {
            if (GraphPage == null)
                return;

            if (!GraphPage.CheckAccess())
            {
                GraphPage.Dispatcher.Invoke(() => {
                    LMgr_ParameterReceived( aAddr,  aValue);
                    return;
                });
            }

            var set = ManaulParameterSetting.Settings.First(x => x.Info.Address == aAddr);
            if (set != null)
                set.ReadValue = aValue;
        }

        private void LMgr_StateReceived(StateShot aShot)
        {

            if (_qstates.Count == 0 && _states.Count == 0)
                StateShot.TimeBase = GraphInfo.TimeUnit(aShot.Time);

            Angle = aShot.DoorAngle;
            lock (_osync)
            {
                _qstates.Add(aShot);
            }
        }

        private void LoadSettings()
        {
            var dir = Helper.AppDir;
            if (Extension.IsDesignMode)
                dir = @"f:\prj\mediaever\Ados.TestBench\Ados.TestBench.Test\bin\Debug";

            this.ManaulParameterSetting = ParameterSettings.Load(dir + "\\Settings\\ManualParameters.json");

            this._graphInfos = GraphInfo.Load(dir + "\\Settings\\Graphs.json");

            foreach(var g in _graphInfos)
            {
                g.SetDataSource(_states);
            }
            
        }

        public void SaveSettings()
        {
            GraphInfo.Save(_graphInfos);
            this.ManaulParameterSetting.Save();

            this.ManaulParameterSetting.Save();
        }


        void UpdateGraph()
        {
            //this.GraphPage.UpdateGraphInfo();

            GraphInfo.Save(_graphInfos);
        }

        public List<GraphInfo> GraphInfos { get { return _graphInfos; } }

        public ObservableCollection<StateShot> StatesData { get { return _states; } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(String propertyName)
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
            _csvPath = null;
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

        public double Angle { get { return _angle * 1; }
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

        public GraphInfo GetGraph(string aName)
        {
            return _graphInfos.First(x => x.Name == aName);
        }

        public bool UpdateData { get; set; }

        public DelegateCommand Command { get { return _cmd; } }

        public ControllerModel Controller { get { return _controller; } }

        public ParameterSettings ManaulParameterSetting { get; private set; }

        public ManualGraphPage GraphPage { get; set; } 

        private bool _active = false;

        public GraphInfo A1 { get { return GetGraph("a1"); } }
        public GraphInfo A2 { get { return GetGraph("a2"); } }
        public GraphInfo A3 { get { return GetGraph("a3"); } }
        public GraphInfo A4 { get { return GetGraph("a4"); } }
        public GraphInfo D1 { get { return GetGraph("d1"); } }
        public GraphInfo D2 { get { return GetGraph("d2"); } }
        public GraphInfo D3 { get { return GetGraph("d3"); } }
        public GraphInfo D4 { get { return GetGraph("d4"); } }
        public GraphInfo D5 { get { return GetGraph("d5"); } }
        public GraphInfo D6 { get { return GetGraph("d6"); } }
        public GraphInfo D7 { get { return GetGraph("d7"); } }

        FastObservableCollection<StateShot> _states = new FastObservableCollection<StateShot>();
        List<StateShot> _qstates = new List<StateShot>();
        ControllerModel _controller;
        DelegateCommand _cmd = new DelegateCommand();
        List<GraphInfo> _graphInfos;
        object _osync = new object();
        double _angle = 0;
    }
}
