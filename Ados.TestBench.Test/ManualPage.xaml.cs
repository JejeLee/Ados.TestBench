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
        public ManualPage()
        {
            InitializeComponent();

            _graphPage = new ManualGraphPage();
            _listPage = new ManualListPage();
            _dataPages.Navigate(_graphPage);
        }

        ManualGraphPage _graphPage;
        ManualListPage _listPage;
    }
}
