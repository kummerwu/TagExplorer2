using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagExplorer2.Model.Config;

namespace TagExplorer2.ViewModel
{
    public class TagCanvasCtrlViewModel_MainCanvas : TagCanvasCtrlViewModel
    {
        
        public override LayoutMode LayoutMode
        {
            get { return DynamicCfg.Ins.MainCanvasLayoutMode; }
            set
            {
                if (DynamicCfg.Ins.MainCanvasLayoutMode != value)
                {
                    DynamicCfg.Ins.MainCanvasLayoutMode = value;
                    RaiseLayoutChanged();
                }
            }
        }
    }
}
