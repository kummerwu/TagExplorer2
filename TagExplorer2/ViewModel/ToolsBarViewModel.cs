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
    class ToolsBarViewModel: INotifyPropertyChanged
    {
        string _searchTxt = "test";
        public string SearchTxt
        {
            get
            {
                return _searchTxt;
            }
            set
            {
                bool oldStatus = string.IsNullOrEmpty(_searchTxt);
                _searchTxt = value;
                bool newStatus = string.IsNullOrEmpty(_searchTxt);
                if(oldStatus!=newStatus)
                {
                    SearchCommand.RaiseCanExcuteChanged();
                }
                RaisePropertyChanged("SearchTxt");
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            // take a copy to prevent thread issues
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private DelegateCommand searchCmd = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand SearchCommand {
            get
            {
                if(searchCmd==null)
                {
                    searchCmd = new DelegateCommand(this.Search, this.CanSearch);
                }
                return searchCmd;
            }
        }
        public void Search(object param)
        {
            MessageBox.Show(SearchTxt);
        }
        public bool CanSearch(object param)
        {
            return !string.IsNullOrEmpty(SearchTxt);
        }
    }
}
