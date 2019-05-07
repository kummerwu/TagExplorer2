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
    /// TagCanvasCtrl.xaml 的交互逻辑
    /// </summary>
    public partial class TagCanvasCtrl : UserControl
    {
        
        public TagCanvasCtrl()
        {
            InitializeComponent();
            //此时DataContext为NULL
        }
        public string Type
        {
            set
            {
                DataContext = ViewModelLocator.TagCanvasCtrlViewModelFactory(value);
            }
        }
        
    }
}
