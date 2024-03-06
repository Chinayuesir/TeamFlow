using System.Threading;
using Cysharp.Threading.Tasks;
using OpenAI;
using QFramework;
using Sirenix.OdinInspector;
using TeamFlow.Utilities;
using UnityEngine;
using XNode;

namespace TeamFlow.Nodes
{
    [NodeTitle("普通对话Agent","智能代理",SdfIconType.ChatLeftDots)]
    [NodeWidth(300)]
    [NodeTint("#2D4A60")]
    public class NormalChatAgentNode:BaseNode,IController
    {
        [LabelText("系统提示")]
        [Input] public string system;

        [LabelText("模型选择")]
        public ModelType model=ModelType.GPT3_5_Turbo;
        
        [LabelText("提示词")]
        [TextArea(4,10)]
        [Input] public string prompt;
        
        [LabelText("结果")]
        [Output] public string result;
        
        [LabelText("记忆")]
        [Input(ShowBackingValue.Never)] public NormalMemory normalMemory;
        
        
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "result")
            {
                return result;
            }
            else return null;
        }

        protected override async UniTask RunStepNodeLogic(UniTaskCompletionSource utcs,CancellationTokenSource cts=default)
        {
            if (utcs.Task.Status != UniTaskStatus.Pending) return;
            normalMemory = GetInputValue<NormalMemory>("normalMemory", null);
            result= await this.GetUtility<OpenAIUtility>().GetCompletion(
                    GetInputValue("prompt", this.prompt)
                ,GetInputValue("system", this.system)
                    ,GetInputValue("model", this.model)
                    ,normalMemory,cts!.Token);
            normalMemory.AddMessage(new SimpleMessage(Role.Assistant,result));
            normalMemory.Changed.Trigger();
            Debug.Log(result);
        }

        public IArchitecture GetArchitecture()
        {
            return TeamFlow.Interface;
        }
    }
}