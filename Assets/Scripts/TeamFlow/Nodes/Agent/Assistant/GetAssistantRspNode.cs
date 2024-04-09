using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using OpenAI.Assistants;
using OpenAI.Threads;
using QFramework;
using Sirenix.OdinInspector;
using TeamFlow.Utilities;
using UnityEditor;
using UnityEngine;
using XNode;

namespace TeamFlow.Nodes
{
    [NodeTitle("获取助手回复", "智能代理",SdfIconType.ChatLeftFill)]
    [NodeWidth(300)]
    [NodeTint("#2D4A60")]
    public class GetAssistantRspNode : BaseNode, IController
    {
        [LabelText("助手")] [Input]
        public Assistant assistant;
        [LabelText("提示词")] [TextArea(4, 10)] [Input]
        public string prompt;
        [LabelText("结果")] [Output] public string result;
        private OpenAIUtility mOpenAIUtility;

        [LabelText("开启新线程")]
        public bool RunningOnNewThread = false;

        public string ThreadID {
            get
            {
                if (!EditorPrefs.HasKey(mThreadIDKey))
                {
                    EditorPrefs.SetString(mThreadIDKey,"");
                }
                return EditorPrefs.GetString(mThreadIDKey);
            }
            set => EditorPrefs.SetString(mThreadIDKey,value);
        }
        
        [DisableInEditorMode]
        [SerializeField]
        private string mThreadIDKey = "";
        
        protected override void Init()
        {
            base.Init();
            mThreadIDKey = "ThreadID_" + mGUID;
        }

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "result")
            {
                return result;
            }
            else return null;
        }

        protected override async UniTask RunStepNodeLogic(UniTaskCompletionSource utcs,
            CancellationTokenSource cts = default)
        {
            if (utcs.Task.Status != UniTaskStatus.Pending)
            {
                return;
            }
            mOpenAIUtility ??= this.GetUtility<OpenAIUtility>();

            ThreadResponse thread=null;
            AssistantResponse assistantRsp=null;
            
            if (RunningOnNewThread)
            {
                await mOpenAIUtility.DeleteThread(ThreadID);
                ThreadID = "";
            }

            if (ThreadID == "")
            {
                thread=await CreateThreadAndSetID();
            }
            else
            {
                try
                {
                    thread = await mOpenAIUtility.RetrieveThread(ThreadID);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    thread=await CreateThreadAndSetID();
                }
                thread??=await CreateThreadAndSetID();
            }
            try
            {
                string id = GetInputValue(nameof(assistant), assistant).ID;
                assistantRsp = await mOpenAIUtility.RetrieveAssistant(id);
                // 获取响应并赋值给result
                result = await mOpenAIUtility.GetAssistantResponse(assistantRsp, thread,
                    GetInputValue(nameof(prompt), prompt));
            }
            catch (Exception ex)
            {
                // 异常处理，可以根据需要记录日志或者设置错误信息
                result = $"Error: {ex.Message}";
            }
        }

        private async UniTask<ThreadResponse> CreateThreadAndSetID()
        {
            var thread = await mOpenAIUtility.CreateThread();
            ThreadID = thread.Id;
            return thread;
        }

        public IArchitecture GetArchitecture()
        {
            return TeamFlow.Interface;
        }
    }
}