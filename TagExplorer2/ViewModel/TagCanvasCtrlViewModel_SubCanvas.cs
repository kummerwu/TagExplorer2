using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagExplorer2.Model.Config;

namespace TagExplorer2.ViewModel
{
    public class TagCanvasCtrlViewModel_SubCanvas:TagCanvasCtrlViewModel
    {
        public override LayoutMode LayoutMode
        {
            get { return DynamicCfg.Ins.SubCanvasLayoutMode; }
            set
            {
                if (DynamicCfg.Ins.SubCanvasLayoutMode != value)
                {
                    DynamicCfg.Ins.SubCanvasLayoutMode = value;
                    RaiseLayoutChanged();
                }
            }
        }
    }
}
