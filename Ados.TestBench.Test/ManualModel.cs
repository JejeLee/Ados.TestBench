using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Ados.TestBench.Test
{
    internal class ManualModel : INotifyPropertyChanged
    {
        public ManualModel(ControllerModel aCtrlModel)
        {
            _controller = aCtrlModel;
        }

        private void LMgr_ParameterReceived(int aAddr, int aValue)
        {
            throw new NotImplementedException();
        }

        private void LMgr_StateReceived(StateShot aShot)
        {
            _states.Add(aShot);
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

        public ControllerModel Controller { get { return _controller; } }

        private bool _active = false;

        ObservableCollection<StateShot> _states = new ObservableCollection<StateShot>();
        ControllerModel _controller;
    }
}
