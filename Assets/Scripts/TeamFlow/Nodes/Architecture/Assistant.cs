using System;
using System.Collections.Generic;
using System.Linq;
using OpenAI;
using QFramework;
using Sirenix.OdinInspector;
using TeamFlow.Utilities;
using UnityEngine;

namespace TeamFlow
{
    [Serializable]
    public class Assistant:IController
    {
        [DisableInEditorMode]
        [LabelText("助手ID")]
        public string ID;
        
        [OnValueChanged(nameof(OnAssistantNameChanged))]
        [LabelText("助手名称")]
        public string Name;
        
        [LabelText("助手指令设置")]
        [OnValueChanged(nameof(OnInfoChanged))]
        [TextArea(4,10)]
        public string Instructions;
        
        [OnValueChanged(nameof(OnInfoChanged))]
        [LabelText("模型选择")]
        public AssistantModelType Model=AssistantModelType.GPT3_5_Turbo;
        
        [HorizontalGroup(Width = 0.5f)]
        [LabelText("检索功能")]
        [OnValueChanged(nameof(OnInfoChanged))]
        public bool RetrieveOpen;
        
        [HorizontalGroup(Width = 0.5f)]
        [LabelText("代码解释器")]
        [OnValueChanged(nameof(OnInfoChanged))]
        public bool CodeInterpreterOpen;
        
        
        [HorizontalGroup("group2",Width = 0.8f)]
        [ShowIf(nameof(RetrieveOpen))]
        [LabelText("选择文件")]
        [ValueDropdown(nameof(GetFilesFromServer))]
        public string ToAddFile;
        
        [HorizontalGroup("group2",Width = 0.2f)]
        [ShowIf(nameof(RetrieveOpen))]
        [Button("链接文件")]
        private async void AttackFileToAssistant()
        {
            mOpenAIUtility ??= this.GetUtility<OpenAIUtility>();
            TeamFlowFile toAddFile = TeamFlow.Files.Find(f => f.FileName == ToAddFile);
            var file = await mOpenAIUtility.GetFileInfoAsync(toAddFile.FileID);
            await mOpenAIUtility.AttachAssistantFile(ID, file);
            AssistantFiles.Add(toAddFile);
            toAddFile.AssistantInfoList.Add(Name+"---"+ID);;
        }
        
        [ShowIf(nameof(RetrieveOpen))]
        public List<TeamFlowFile> AssistantFiles;
        
        private IEnumerable<string> GetFilesFromServer()
        {
            return TeamFlow.Files.Select(file => file.FileName);
        }


        [GUIColor(0,1,0)]
        [ShowIf(nameof(mOnInfoChanged))]
        [Button("更新助手信息到服务器")]
        private async void UpdateAssistantToServer()
        { 
            mOpenAIUtility ??= this.GetUtility<OpenAIUtility>();
            var tools = new List<Tool>();
            if(RetrieveOpen) tools.Add(Tool.Retrieval);
            if(CodeInterpreterOpen) tools.Add(Tool.CodeInterpreter);
            var assistant = await mOpenAIUtility.ModifyAssistant(ID,Name,Instructions,Model,tools);
            await TeamFlow.SyncFilesAndAssistants();
            mOnInfoChanged = false;
        }
        
        [ShowIf(nameof(HasCreated))]
        [Button("删除助手")]
        private async void DeleteAssistant()
        {
            if (!HasCreated) return;
            mOpenAIUtility ??= this.GetUtility<OpenAIUtility>();
            await mOpenAIUtility.DeleteAssistant(ID);
            await TeamFlow.SyncFilesAndAssistants();
        }
        
        [HideIf(nameof(HasCreated))]
        [Button("创建助手")]
        private async void CreateAssistant()
        {
            mOpenAIUtility ??= this.GetUtility<OpenAIUtility>();
            if (await mOpenAIUtility.RetrieveAssistant(ID) == null) return;
            var tools = new List<Tool>();
            if(RetrieveOpen) tools.Add(Tool.Retrieval);
            if(CodeInterpreterOpen) tools.Add(Tool.CodeInterpreter);
            var assistant = await mOpenAIUtility.CreateAssistant(Name,Instructions,Model,tools);
            ID = assistant.Id;
            await TeamFlow.SyncFilesAndAssistants();
            HasCreated = true;
            mOnInfoChanged = false;
        }

        private OpenAIUtility mOpenAIUtility;
        private bool mOnInfoChanged=false;
        [HideInInspector]
        public bool HasCreated = false;

        private void OnInfoChanged()
        {
            if (!HasCreated) return;
            mOnInfoChanged = true;
            Debug.Log("数据改变！");
        }
        
        private void OnAssistantNameChanged()
        {
            HasCreated = false;
            Debug.Log("数据改变！");
        }

        public IArchitecture GetArchitecture()
        {
            return TeamFlow.Interface;
        }
    }
}