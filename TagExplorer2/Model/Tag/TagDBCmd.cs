using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagExplorer2.Model.Tag
{
    [Serializable]
    public class TagDBCmd : ICmd
    {
        public Guid GetOptID()
        {
            return OptID;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public TagDBCmdType Type;
        public Guid OptID;
        public List<string> Args;

        public TagDBCmd()
        {
            OptID = Guid.NewGuid();
        }
        [JsonIgnore]
        public bool Save = true;
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }


    }
    public interface ICmd
    {
        Guid GetOptID();
    }
    public enum TagDBCmdType
    {
        NewTag,         //Args:SelfTag，Title,
        RemoveTag,      //Args:SelfTag,
        SetParent,      //Args:parent, child ，将child添加到Child最后
        ResetParent,    //Args:parent,child
        ChangeTitle,    //Args:SelfTag,Title
        MergeAlias,     //Args:Tag1,Tag2
        ChangeChildPos, //Args:Tag,offset

        SetParentWithPreBrother,//Args：parent，child，preBrother，新增child到指定兄弟后，20181125 新增SetParentWithPreBrother
        SetPath, //Args:Tag,Path
        ReplacePath, // Args:dst,src
    }
}
