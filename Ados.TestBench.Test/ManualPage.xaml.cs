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

            this.Loaded += ManualPage_Loaded;
           
        }

        private void ManualPage_Loaded(object sender, RoutedEventArgs e)
        {
            _graphPage = new ManualGraphPage(this.DataContext);
            _listPage = new ManualListPage(this.DataContext);

            _dataPages.Navigate(_graphPage);
        }

        ManualGraphPage _graphPage;
        ManualListPage _listPage;

        internal ManualModel Model { get { return (ManualModel)DataContext; } }

        private void _dataNaviation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_graphPage == null)
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
            Model.Controller.LinMgr.ReadParameters(list);
        }

        private void _btnWriteSel_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<ParameterSetting>();
            foreach (var p in Model.ManaulParameterSetting.Settings)
            {
                if (p.Use)
                    list.Add(p);
            }
            Model.Controller.LinMgr.WriteParameters(list);
        }

        private void ReadValue_Click(object sender, RoutedEventArgs e)
        {
            var pset = (ParameterSetting)(e.OriginalSource as Hyperlink).DataContext;
            Model.Controller.LinMgr.ReadParameter(pset.Info.Address);
            
        }

        private void _gridParams_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_gridParams.CurrentCell != null && _gridParams.CurrentCell.Column.DisplayIndex == 3 && e.Key == Key.Enter)
            {
                Model.Controller.LinMgr.WriteParameter(_gridParams.CurrentCell.Item as ParameterSetting);
            }
        }
    }
}
