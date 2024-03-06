using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using XNode;

namespace TeamFlow.Nodes
{
    [Node.NodeTitle("条件分支节点","流程控制",SdfIconType.Question)]
    [NodeTint("#414348")]
    public class IfNode : BaseNode
    {
        /// <summary>
        /// 下一个动作
        /// </summary>
        [Output(ShowBackingValue.Never, ConnectionType.Override)]
        [LabelText("结果为真(true)")]
        public UnitaskPort trueNext;

        /// <summary>
        /// 下一个动作
        /// </summary>
        [Output(ShowBackingValue.Never, ConnectionType.Override)]
        [LabelText("结果为假(false)")]
        public UnitaskPort falseNext;
        
        /// <summary>
        /// 结果
        /// </summary>
        [LabelText("结果")]
        [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
        public bool result;

        protected override void Init()
        {
            base.Init();
            showNext = false;
        }

        public override object GetValue(NodePort port)
        {
            return null;
        }

        protected override async UniTask RunStepNodeLogic(UniTaskCompletionSource utcs,CancellationTokenSource cts=default)
        {
            if (utcs.Task.Status != UniTaskStatus.Pending) return;

            if (GetInputPort("result").IsConnected) result = GetInputValue<bool>("result");

            if (result)
            {
                if (GetOutputPort("trueNext").IsConnected)
                {
                    nextStepNode = (BaseNode)GetOutputPort("trueNext").Connection.node;
                }
                else
                {
                    nextStepNode = null;
                }
            }
            else
            {
                if (GetOutputPort("falseNext").IsConnected)
                {
                    nextStepNode = (BaseNode)GetOutputPort("falseNext").Connection.node;
                }
                else
                {
                    nextStepNode = null;
                }

            }

            if (nextStepNode == null)
            {
                utcs.TrySetResult();
                GC.Collect();
            }
            else
            {
                if (utcs.Task.Status == UniTaskStatus.Pending) await nextStepNode.RunStepNode(utcs,cts);
            }
        }
    }
}

