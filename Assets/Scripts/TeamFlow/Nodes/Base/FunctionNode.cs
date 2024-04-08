﻿using OpenAI;
using Sirenix.OdinInspector;
using XNode;

namespace TeamFlow.Nodes
{
    public abstract class FunctionNode:Node
    {
        [Required]
        [Input(ShowBackingValue.Never)]
        [LabelText("自定义函数")]
        public FunctionPort CustomFunction;

        public Tool ToolFromFunc { get; protected set; }

        private AssistantNode mAssistantNode;

        #region xNode
        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            //不允许不同类型连接
            if (from.ValueType != to.ValueType) from.Disconnect(to);
            else
            {
                foreach (var item in Inputs)
                {
                    if (item.IsConnected)
                    {
                        mAssistantNode= (AssistantNode)item.Connection.node;
                        mAssistantNode.Assistant.AddCustomFunctionTool(ToolFromFunc);
                    }
                }
            }
        }

        public override void OnRemoveConnection(NodePort port)
        {
            mAssistantNode.Assistant.RemoveCustomFunctionTool(ToolFromFunc);
            mAssistantNode = null;
        }

        protected abstract override void Init();
        #endregion
    }
}