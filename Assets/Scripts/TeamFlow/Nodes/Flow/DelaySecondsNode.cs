using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace TeamFlow.Nodes
{
    [NodeTitle("延时节点（秒）","流程控制",SdfIconType.ClockHistory)]
    [NodeTint("#2D4A60")]
    public class DelaySecondsNode:BaseNode
    {
        [Input]
        [LabelText("延迟时间（秒）")]
        public float seconds;
        public override object GetValue(NodePort port)
        {
            return null;
        }

        protected override async UniTask RunStepNodeLogic(UniTaskCompletionSource utcs,CancellationTokenSource cts=default)
        {
            float time = GetInputValue("seconds", this.seconds);
            await UniTask.Delay((int)(1000 * time),true, cancellationToken: cts!.Token);
            Debug.Log("延时节点执行完毕！");
        }
    }
}