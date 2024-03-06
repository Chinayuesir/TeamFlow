using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace TeamFlow.Nodes
{
    [NodeTitle("输出字符串","帮助",SdfIconType.Tools)]
    [NodeTint("#414348")]
    public class DebugNode:BaseNode
    {
        [Input]
        [LabelText("输出内容")]
        public string content;
        public override object GetValue(NodePort port)
        {
            return null;
        }

        protected override async UniTask RunStepNodeLogic(UniTaskCompletionSource utcs,CancellationTokenSource cts=default)
        {
            string s=GetInputValue("content", this.content);
            Debug.Log(s);
            await UniTask.Yield();
        }
    }
}