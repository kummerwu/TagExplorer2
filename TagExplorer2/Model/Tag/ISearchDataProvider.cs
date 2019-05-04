using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagExplorer2.Model.Tag
{
    public interface ISearchDataProvider
    {
        List<AutoCompleteTipsItem> QueryAutoComplete(string searchTerm, bool forceOne = false);
    }
    public class AutoCompleteChooseResult
    {
        public List<AutoCompleteTipsItem> AllItem;
        public AutoCompleteTipsItem SelectedItem;
    }
    public class AutoCompleteTipsItem
    {
        public string Tip;
        public GUTag Tag;

        public int Score;
        //public int TotalItemCount;//所有满足条件的Item。
    }
}
