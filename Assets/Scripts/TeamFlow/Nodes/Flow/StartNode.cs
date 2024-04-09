using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace TeamFlow.Nodes
{
    [NodeTitle("开始")]
    [NodeWidth(200), DisallowMultipleNodes]
    [NodeTint("#BC3030")]
    public class StartNode:Node
    {
        [Output(ShowBackingValue.Never, ConnectionType.Override)]
        [LabelText("下一个动作")]
        public UniTaskPort next;
        
        [Button("初始化本工作流")]
        private async void InitTeamFlow()
        {
            _ = TeamFlow.Interface;
            TeamFlow.Reset();
            TeamFlow.TeamFlowStart.Register(StartGraph);
            await TeamFlow.SyncFilesAndAssistants();
            Debug.Log("当前工作流已初始化完毕！");
        }

        public bool IsStarted
        {
            get => TeamFlow.TeamFlowState.Value == RunningState.Started;
        }

        [DisableIf(nameof(IsStarted))]
        [Button("开始运行")]
        private void Run()
        {
            TeamFlow.TeamFlowState.Value = RunningState.Started;
            TeamFlow.TeamFlowStart.Trigger();
        }
        
        private async void StartGraph()
        {
            Debug.Log("工作流运行中，请等待！");
            await StartStepNode();
            //等待整张图结束后执行的代码
            Debug.Log("工作流运行结束！");
        }

        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            base.OnCreateConnection(from, to);
            if (from.ValueType != to.ValueType) from.Disconnect(to);
        }

        public override object GetValue(NodePort port)
        {
            return null;
        }
        
        #region Unitask

        /// <summary>
        /// 下一个动作
        /// </summary>
        [HideInInspector]
        public BaseNode nextStepNode;
        /// <summary>
        /// 开始新动作
        /// </summary>
        /// <returns></returns>
        private async UniTask StartStepNode()
        {
            await RunStepNode(new UniTaskCompletionSource(),new CancellationTokenSource());
        }

        /// <summary>
        /// 执行动作
        /// </summary>
        /// <param name="utcs"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        protected virtual async UniTask RunStepNode(UniTaskCompletionSource utcs,CancellationTokenSource cts=default)
        {
            await EndStepNode(utcs,cts);
        }

        /// <summary>
        /// 结束当前动作
        /// </summary>
        /// <param name="utcs"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        private async UniTask EndStepNode(UniTaskCompletionSource utcs,CancellationTokenSource cts=default)
        {
            if (GetOutputPort("next").IsConnected)
            {
                nextStepNode = (BaseNode)GetOutputPort("next").Connection.node;
            }
            else
            {
                nextStepNode = null;
            }

            if (nextStepNode == null)
            {
                utcs.TrySetResult();
                TeamFlow.TeamFlowState.Value = RunningState.Finished;
                GC.Collect();
            }
            else
            {
                await nextStepNode.RunStepNode(utcs,cts);
                TeamFlow.TeamFlowState.Value = RunningState.Finished;
            }
        }
        #endregion
        
        private void OnDestroy()
        {
            Debug.Log("StartNode Destroy!");
             TeamFlow.TeamFlowStart.UnRegister(StartGraph);
        }
    }
}