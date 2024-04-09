using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using XNode;

namespace TeamFlow.Nodes
{
    [Node.NodeWidth(350)]
    [Node.NodeTitle("顺序执行","流程控制",SdfIconType.ArrowRight)]
    [NodeTint("#414348")]
    public class SequenceNode : BaseNode
    {
        [LabelText("顺序执行任务")]
        [Output(ShowBackingValue.Never, ConnectionType.Override, dynamicPortList = true)]
        [OnCollectionChanged(After = "OnDynamicPortListChange")]
        [ListDrawerSettings(ShowIndexLabels = true,CustomAddFunction = "AddUnitaskPort")]
        [NonSerialized, OdinSerialize]
        public List<UniTaskPort> unitaskPorts=new List<UniTaskPort>();
        
        private UniTaskPort AddUnitaskPort()
        {
            return new UniTaskPort();
        }
        
        BaseNode foreachStepNode;
        UniTaskCompletionSource childUTCS;

        public override object GetValue(NodePort port)
        {
            return null;
        }

        protected override async UniTask RunStepNodeLogic(UniTaskCompletionSource utcs,CancellationTokenSource cts=default)
        {
            if (utcs.Task.Status != UniTaskStatus.Pending) return;

            //具体逻辑
            for (int i = 0; i < DynamicOutputs.ToList().Count; i++)
            {
                if(DynamicOutputs.ToList()[i].IsConnected)
                    foreachStepNode = DynamicOutputs.ToList()[i].Connection.node as BaseNode;
                else
                    foreachStepNode = null;
                childUTCS = new UniTaskCompletionSource();
                if(foreachStepNode!=null)
                    await foreachStepNode.RunStepNode(childUTCS,cts);
            }
            childUTCS = null;
        }
    }
}