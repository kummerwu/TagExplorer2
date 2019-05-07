/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:TagExplorer2"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;


namespace TagExplorer2.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainWindowViewModel>();
            SimpleIoc.Default.Register<ToolsBarViewModel>();
            SimpleIoc.Default.Register<TagCanvasCtrlViewModel>();

        }
        public static TagCanvasCtrlViewModel TagCanvasCtrlViewModelFactory(string name)
        {
            if (name == "MainCanvas") return MainCanvas;
            else return SubCanvas;
        }
        public static TagCanvasCtrlViewModel MainCanvas
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TagCanvasCtrlViewModel>("MainCanvas");
            }
        }
        public static TagCanvasCtrlViewModel SubCanvas
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TagCanvasCtrlViewModel>("SubCanvas");
            }
        }
        public MainWindowViewModel MainWindow
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainWindowViewModel>();
            }
        }
        public ToolsBarViewModel ToolsBar
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ToolsBarViewModel>();
            }
        }
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}