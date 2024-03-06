using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace TeamFlow.Nodes
{
    [NodeTitle("添加前后缀","帮助",SdfIconType.Tools)]
    [NodeTint("#414348")]
    public class EasyStringNode:BaseOtherNode
    {
        [Input]
        [TextArea(5,5)]
        [LabelText("前缀")]
        public string prefix;
        
        [Input]
        [TextArea(5,5)]
        [LabelText("目标文本")]
        public string content;
        
        [Input]
        [TextArea(5,5)]
        [LabelText("后缀")]
        public string suffix;
        
        [Output(ShowBackingValue.Never, ConnectionType.Override)]
        [LabelText("结果")]
        public string result;
        
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "result")
            {
                StringBuilder sb = new StringBuilder();
                if (GetInputPort("content").IsConnected 
                    || GetInputPort("prefix").IsConnected
                || GetInputPort("suffix").IsConnected)
                {
                    string s = GetInputValue("prefix", this.prefix);
                    if ( s!= "")
                    {
                        sb.Append(s + "\n");
                    }
                    sb.Append(GetInputValue("content",this.content));
                    s = GetInputValue("suffix", this.suffix);
                    if ( s!= "")
                    {
                        sb.Append("\n"+s);
                    } 
                    return sb.ToString();
                }   
            }
            return null;
        }
    }
}