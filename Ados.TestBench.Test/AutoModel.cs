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
    internal class AutoModel : INotifyPropertyChanged
    {
        public AutoModel(ControllerModel aCtrlModel)
        {
            try
            {
                _controller = aCtrlModel;

                LoadSettings();

                _cmd.ExecuteHandler = ExecuteCommand;
                _cmd.CanExecuteHandler = CanExecuteCommand;
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show("ManualModel: 초기화 중 에러 발생:" + e.ToString());
                if (!Extension.IsDesignMode)
                    Environment.Exit(1);
            }
        }

        void ExecuteCommand(object aInput)
        {
            string input = aInput.ToString();

            switch (input)
            {
                case "":
                    break;
            }
        }

        bool CanExecuteCommand(object aInput)
        {
            string input = aInput.ToString();
            bool cando = true;

            switch (input)
            {
                case "":
                    cando = true;
                    break;
            }
            return cando;
        }

        private void LMgr_ParameterReceived(int aAddr, int aValue)
        {
            throw new NotImplementedException();
        }

        private void LMgr_StateReceived(StateShot aShot)
        {
            _states.Add(aShot);
        }

        private void LoadSettings()
        {
            var fi = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
            var dir = fi.DirectoryName;
            if (Extension.IsDesignMode)
                dir = @"f:\prj\mediaever\Ados.TestBench\Ados.TestBench.Test\bin\Debug";

            this.ManaulParameterSetting = ParameterSettings.Load(dir + "\\Settings\\ManualParameters.json");
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

        public DelegateCommand Command { get { return _cmd; } }

        public ControllerModel Controller { get { return _controller; } }

        public ParameterSettings ManaulParameterSetting { get; private set; }

        private bool _active = false;

        ObservableCollection<StateShot> _states = new ObservableCollection<StateShot>();
        ControllerModel _controller;
        DelegateCommand _cmd = new DelegateCommand();
    }
}
