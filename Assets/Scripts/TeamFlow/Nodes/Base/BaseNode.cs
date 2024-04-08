using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace TeamFlow.Nodes
{
   /// <summary>
    /// 基础动作节点
    /// </summary>
    [NodeTitle("","",SdfIconType.None,false)]
    public abstract class BaseNode : Node
    {
        [SerializeField]
        [HideInInspector]
        protected string mGUID="";

        protected override void Init()
        {
            base.Init();
            if(mGUID=="")  mGUID = Guid.NewGuid().ToString();
        }
        /// <summary>
        /// 上一个动作
        /// </summary>
        [Required]
        [Input(ShowBackingValue.Never)]
        [LabelText("上一个动作")]
        public UnitaskPort previous;
        /// <summary>
        /// 下一个动作
        /// </summary>
        [Output(ShowBackingValue.Never, ConnectionType.Override)]
        [LabelText("下一个动作")]
        [ShowIf("showNext")]
        public UnitaskPort next;
        
        /// <summary>
        /// 是否显示下一个接口
        /// </summary>
        [HideInInspector]
        public bool showNext = true;
        
        #region xNode

        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            //不允许不同类型连接
            if (from.ValueType != to.ValueType) from.Disconnect(to);
        }

        public abstract override object GetValue(NodePort port);
        

        #endregion

        #region Unitask

        /// <summary>
        /// 下一个动作
        /// </summary>
        [HideInInspector]
        public BaseNode nextStepNode;
        
        protected abstract UniTask RunStepNodeLogic(UniTaskCompletionSource utcs,CancellationTokenSource cts=default);

        /// <summary>
        /// 执行动作
        /// </summary>
        /// <param name="utcs"></param>
        /// <returns></returns>
        public async UniTask RunStepNode(UniTaskCompletionSource utcs,CancellationTokenSource cts=default)
        {
            await RunStepNodeLogic(utcs,cts);
            await EndStepNode(utcs,cts);
        }
        /// <summary>
        /// 结束当前动作
        /// </summary>
        /// <param name="utcs"></param>
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
                GC.Collect();
            }
            else
            {
                if (utcs.Task.Status == UniTaskStatus.Pending) await nextStepNode.RunStepNode(utcs,cts);
            }
        }
        #endregion
    }
}