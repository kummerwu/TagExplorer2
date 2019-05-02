using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagExplorer2.ViewModel
{
    class MainWindowViewModel: ViewModelBase
    {
        


        private string _searchTxt = "";

        

        public void SetSearchTxt(string txt)
        {
            _searchTxt = txt;
        }
        public string SearchTxt
        {
            get { return _searchTxt; }
            set {
                _searchTxt = value;
                RaisePropertyChanged("Title");
            }
        }
        public string Title
        {
            get
            {
                return "Current Search: " + SearchTxt;
            }
        }
    }
}
