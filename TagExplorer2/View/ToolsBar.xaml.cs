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
using TagExplorer2.ViewModel;

namespace TagExplorer2.View
{
    /// <summary>
    /// ToolsBar.xaml 的交互逻辑
    /// </summary>
    public partial class ToolsBar : UserControl
    {
        public ToolsBar()
        {
            InitializeComponent();
        }

        private void btUp_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as ToolsBarViewModel).SearchTxt = DateTime.Now.ToLongDateString();
        }
    }
}
