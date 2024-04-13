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

        [LabelText("是否开启循环")]
        public bool IsLoop=false;
        
        [LabelText("循环终止关键词")]
        [ShowIf(nameof(IsLoop))]
        public string EndWord="";

        [LabelText("最大循环次数")]
        [ShowIf(nameof(IsLoop))]
        public int LoopTimes;

        [Input(ShowBackingValue.Never)] 
        [LabelText("校验关键词")]
        [ShowIf(nameof(IsLoop))]
        public string EndWordVerify;
        
        BaseNode foreachStepNode;
        UniTaskCompletionSource childUTCS;

        public override object GetValue(NodePort port)
        {
            return null;
        }

        protected override async UniTask RunStepNodeLogic(UniTaskCompletionSource utcs,CancellationTokenSource cts=default)
        {
            if (utcs.Task.Status != UniTaskStatus.Pending) return;
            
            int loopTimes = 0;
            //保证能执行至少一次
            EndWordVerify = "";
            while (true)
            {
                if (EndWordVerify.Contains(EndWord) || loopTimes >= LoopTimes) break;
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
                loopTimes++;
                EndWordVerify = GetInputValue(nameof(EndWordVerify), EndWordVerify);
            }
            childUTCS = null;
        }
    }
}