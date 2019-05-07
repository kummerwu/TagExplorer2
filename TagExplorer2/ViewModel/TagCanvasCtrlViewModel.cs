using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TagExplorer2.Model.Config;
using TagExplorer2.Model.Tag;

namespace TagExplorer2.ViewModel
{
    public abstract class TagCanvasCtrlViewModel:ViewModelBase
    {
        GUTag curTag = null;

        public abstract LayoutMode LayoutMode { get; set; }
        public bool Is_LRTREE_NO_COMPACT { get { return LayoutMode == LayoutMode.LRTREE_NO_COMPACT; } }
        public bool Is_LRTREE_COMPACT { get { return LayoutMode == LayoutMode.LRTREE_COMPACT; } }
        public bool Is_LRTREE_COMPACT_MORE { get { return LayoutMode == LayoutMode.LRTREE_COMPACT_MORE; } }
        public bool Is_TREE_NO_COMPACT { get { return LayoutMode == LayoutMode.TREE_NO_COMPACT; } }
        public bool Is_TREE_COMPACT { get { return LayoutMode == LayoutMode.TREE_COMPACT; } }
        public bool Is_TREE_COMPACT_MORE { get { return LayoutMode == LayoutMode.TREE_COMPACT_MORE; } }
        protected void RaiseLayoutChanged()
        {
            RaisePropertyChanged("LayoutMode");
            RaisePropertyChanged("Is_LRTREE_NO_COMPACT");
            RaisePropertyChanged("Is_LRTREE_COMPACT");
            RaisePropertyChanged("Is_LRTREE_COMPACT_MORE");
            RaisePropertyChanged("Is_TREE_NO_COMPACT");
            RaisePropertyChanged("Is_TREE_COMPACT");
            RaisePropertyChanged("Is_TREE_COMPACT_MORE");
        }
        private ICommand copyTagFullPath;
        public ICommand CopyTagFullPath
        {
            get
            {
                return copyTagFullPath ?? (copyTagFullPath = new RelayCommand(() =>
                {
                    curTag?.CopyTagFullPathToClipboard();
                }));
            }
        }

        private ICommand copyTagFullPathWithDate;
        public ICommand CopyTagFullPathWithDate
        {
            get
            {
                return copyTagFullPathWithDate ?? (copyTagFullPathWithDate = new RelayCommand(() =>
                {
                    curTag?.CopyTagFullPathWithDateToClipboard();
                }));
            }
        }

        private ICommand changeLayoutMode;
        public ICommand ChangeLayoutMode
        {
            get
            {
                return changeLayoutMode ?? (changeLayoutMode = new RelayCommand<MenuItem>((param) =>
                {
                    string mode = param.Tag as string;
                    LayoutMode tmp;
                    if (!string.IsNullOrEmpty(mode) &&  LayoutMode.TryParse(mode, out tmp)) {
                        LayoutMode = tmp;
                    }
                }));
            }
        }
    }
    
}
