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

namespace Ados.TestBench.Test
{
    /// <summary>
    /// ManualPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualPage : Page
    {
        public ManualPage(object aModel)
        {
            this.DataContext = aModel;

            InitializeComponent();

            _listPage = new ManualListPage(this.DataContext);
            _graphPage = new ManualGraphPage(this.DataContext);
            Model.GraphPage = _graphPage;

            this.Loaded += ManualPage_Loaded;
           
        }

        private void ManualPage_Loaded(object sender, RoutedEventArgs e)
        {
            Model.IsActive = true;

            _dataPages.Navigate(_graphPage);
        }

        ManualGraphPage _graphPage;
        ManualListPage _listPage;

        internal ManualModel Model { get { return (ManualModel)DataContext; } }

        private void _dataNaviation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_dataPages == null || _graphPage == null)
                return;

            if (_dataNaviation.SelectedIndex == 0)
                _dataPages.Navigate(_graphPage);
            else
                _dataPages.Navigate(_listPage);
        }

        private void _btnReadSel_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<ParameterSetting>();
            foreach(var p in Model.ManaulParameterSetting.Settings)
            {
                if (p.Use)
                    list.Add(p);
            }
            Model.Controller.LinMgr.ReadParametersAsync(list);
        }

        private void _btnWriteSel_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<ParameterSetting>();
            foreach (var p in Model.ManaulParameterSetting.Settings)
            {
                if (p.Use)
                    list.Add(p);
            }
            Model.Controller.LinMgr.WriteParametersAsync(list);
        }

        private void ReadValue_Click(object sender, RoutedEventArgs e)
        {
            var pset = (ParameterSetting)(e.OriginalSource as Hyperlink).DataContext;
            Model.Controller.LinMgr.ReadParameter(pset.Info.Address);
            
        }

        private void ParamCell_EditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.DisplayIndex == 3)
            {
                var tbox = e.EditingElement as TextBox;
                var input = tbox.Text.Int();
                if (input == int.MinValue)
                    e.Cancel = true;
                else
                {
                    var pset = (ParameterSetting)e.Row.DataContext;
                    pset.WriteValue = input;
                    Model.Controller.LinMgr.WriteParameter(pset);
                }
            }
        }
    }
}
