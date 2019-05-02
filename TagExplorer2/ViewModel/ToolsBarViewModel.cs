using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TagExplorer2.Utils;

namespace TagExplorer2.ViewModel
{
    public class ToolsBarViewModel: ViewModelBase
    {
        

        public string InputString { get; set; }
        string _searchTxt = "";
        public string SearchTxt
        {
            get
            {
                return _searchTxt;
            }
            set
            {
                _searchTxt = value;
                RaisePropertyChanged("SearchTxt");
            }
        }
        private DelegateCommand searchCmd = null;
        public DelegateCommand SearchCommand {
            get
            {
                if(searchCmd==null)
                {
                    searchCmd = new DelegateCommand(s=> 
                    {
                        SearchTxt = InputString;
                        MessageBox.Show(SearchTxt);
                    }, 
                    param=>true);
                }
                return searchCmd;
            }
        }
        
    }
}
