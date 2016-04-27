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
    /// AutoPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SheetEditPage : Page
    {
        public SheetEditPage()
        {
            InitializeComponent();

            _paramPage = new ParamEditPage();
        }

        ParamEditPage _paramPage;

        private void 파라미터편집_클릭(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(_paramPage);
        }
    }
}
