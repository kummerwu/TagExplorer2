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
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        ToolsBarViewModel vmToolbar = null;
        MainWindowViewModel vmMainWindow = null;
        public MainWindow()
        {
            InitializeComponent();
            vmToolbar = toolBar.DataContext as ToolsBarViewModel;
            vmMainWindow = DataContext as MainWindowViewModel;
            vmToolbar.PropertyChanged += VmToolbar_PropertyChanged;
            
        }

        private void VmToolbar_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e != null && e.PropertyName == "SearchTxt")
            {
                vmMainWindow.SearchTxt = vmToolbar.SearchTxt;
            }
        }

       
    }
}
