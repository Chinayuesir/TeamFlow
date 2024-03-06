using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using XNode;

namespace TeamFlow.Nodes
{
    public enum ParallelType
    {
        /// <summary>
        /// 所有完成
        /// </summary>
        [LabelText("所有完成(All)")]
        All,
        /// <summary>
        /// 任一完成
        /// </summary>
        [LabelText("任一完成(Any)")]
        Any,
    }
    
    [Node.NodeTitle("并行节点","流程控制",SdfIconType.Justify)]
    [NodeTint("#414348")]
    public class ParallelNode:BaseNode
    {
        /// <summary>
        /// 类型
        /// </summary>
        [LabelText("类型")]
        [LabelWidth(30)]
        public ParallelType multiple;

        /// <summary>
        /// 并行任务
        /// </summary>
        [LabelText("并行任务")]
        [Output(ShowBackingValue.Never, ConnectionType.Override, dynamicPortList = true)]
        [OnCollectionChanged(After = "OnDynamicPortListChange")]
        [ListDrawerSettings(ShowIndexLabels = true,CustomAddFunction = "AddUnitaskPort")]
        [NonSerialized,OdinSerialize]
        public List<UnitaskPort> unitaskPorts=new List<UnitaskPort>();

        private UnitaskPort AddUnitaskPort()
        {
            return new UnitaskPort();
        }


        private List<UniTask> uniTasks = new List<UniTask>();

        private List<UniTaskCompletionSource> utcsList = new List<UniTaskCompletionSource>();

        private CancellationTokenSource cts;
        
        
        public override object GetValue(NodePort port)
        {
            return null;
        }

        protected override async UniTask RunStepNodeLogic(UniTaskCompletionSource utcs,CancellationTokenSource cts=default)
        {
            if (utcs.Task.Status != UniTaskStatus.Pending) return;
            this.cts = new CancellationTokenSource();
            uniTasks.Clear();
            utcsList.Clear();

            foreach (var item in DynamicOutputs)
            {
                if (item.IsConnected)
                {
                    BaseNode stepNode = (BaseNode)item.Connection.node;
                    //uniTasks.Add(stepNode.StartStepNode());
                    UniTaskCompletionSource utcsChild = new UniTaskCompletionSource();
                    uniTasks.Add(stepNode.RunStepNode(utcsChild,cts));
                    utcsList.Add(utcsChild);
                }
            }
            switch (multiple)
            {
                case ParallelType.All:
                    await UniTask.WhenAll(uniTasks);
                    break;
                case ParallelType.Any:
                    await UniTask.WhenAny(uniTasks);
                    break;
            }

            uniTasks.Clear();
            utcsList.ForEach((x) => { 
                x.TrySetCanceled();
                x = null;
            });
            cts.Cancel();
            cts = null;
            utcsList.Clear();

            GC.Collect();
        }
    }
}