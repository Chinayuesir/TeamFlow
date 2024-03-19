using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using QFramework;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using TeamFlow.Utilities;
using UnityEditor;
using UnityEngine;
using Tool = OpenAI.Tool;

namespace TeamFlow
{
    [Serializable]
    public class Assistant : IController
    {
        [DisableInEditorMode] [LabelText("助手ID")]
        public string ID;

        [OnValueChanged(nameof(OnAssistantNameChanged))] [LabelText("助手名称")]
        public string Name;

        [LabelText("助手指令设置")]
        [OnValueChanged(nameof(OnInfoChanged))]
        [TextArea(4, 10)]
        [CustomContextMenu("AI优化/提示词优化", "RefinePrompt")]
        [CustomContextMenu("AI优化/提示词转英文", "TranslatePrompt")]
        public string Instructions;


        [OnValueChanged(nameof(OnInfoChanged))] [LabelText("模型选择")]
        public AssistantModelType Model = AssistantModelType.GPT3_5_Turbo;

        [HorizontalGroup(Width = 0.5f)] [LabelText("检索功能")] [OnValueChanged(nameof(OnInfoChanged))]
        public bool RetrieveOpen;

        [HorizontalGroup(Width = 0.5f)] [LabelText("代码解释器")] [OnValueChanged(nameof(OnInfoChanged))]
        public bool CodeInterpreterOpen;


        [HorizontalGroup("group2", Width = 0.8f)]
        [ShowIf(nameof(RetrieveOpen))]
        [LabelText("选择文件")]
        [ValueDropdown(nameof(GetFilesFromServer))]
        public string ToAddFile;

        [HorizontalGroup("group2", Width = 0.2f)]
        [ShowIf(nameof(RetrieveOpen))]
        [Button("链接文件")]
        private async void AttackFileToAssistant()
        {
            mOpenAIUtility ??= this.GetUtility<OpenAIUtility>();
            TeamFlowFile toAddFile = TeamFlow.Files.Find(f => f.FileName == ToAddFile);
            var file = await mOpenAIUtility.GetFileInfoAsync(toAddFile.FileID);
            await mOpenAIUtility.AttachAssistantFile(ID, file);
            AssistantFiles.Add(toAddFile);
            toAddFile.AssistantInfoList.Add(Name + "---" + ID);
            ;
        }

        [ShowIf(nameof(RetrieveOpen))] public List<TeamFlowFile> AssistantFiles;

        private IEnumerable<string> GetFilesFromServer()
        {
            return TeamFlow.Files.Select(file => file.FileName);
        }


        [GUIColor(0, 1, 0)]
        [ShowIf(nameof(mOnInfoChanged))]
        [Button("更新助手信息到服务器")]
        private async void UpdateAssistantToServer()
        {
            mOpenAIUtility ??= this.GetUtility<OpenAIUtility>();
            var tools = new List<Tool>();
            if (RetrieveOpen) tools.Add(Tool.Retrieval);
            if (CodeInterpreterOpen) tools.Add(Tool.CodeInterpreter);
            var assistant = await mOpenAIUtility.ModifyAssistant(ID, Name, Instructions, Model, tools);
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
            if (RetrieveOpen) tools.Add(Tool.Retrieval);
            if (CodeInterpreterOpen) tools.Add(Tool.CodeInterpreter);
            var assistant = await mOpenAIUtility.CreateAssistant(Name, Instructions, Model, tools);
            ID = assistant.Id;
            await TeamFlow.SyncFilesAndAssistants();
            HasCreated = true;
            mOnInfoChanged = false;
        }

        private class SaveAssistant
        {
            public string ID;
            public string Name;
            public string Instructions;
            public string Model;
            public bool RetrieveOpen;
            public bool CodeInterpreterOpen;
        }


        [FoldoutGroup("通过Json读取")]
        [Button("保存助手")]
        private void SaveAssistantToFile()
        {
            var assistantData = new SaveAssistant
            {
                ID = this.ID,
                Name = this.Name,
                Instructions = this.Instructions,
                Model = this.Model.ToString(),
                RetrieveOpen = this.RetrieveOpen,
                CodeInterpreterOpen = this.CodeInterpreterOpen
            };

            string json = JsonConvert.SerializeObject(assistantData, Formatting.Indented);

            // 指定文件路径和名称
            if (!Directory.Exists("Assets/Resources/Assistants"))
            {
                Directory.CreateDirectory("Assets/Resources/Assistants");
            }

            string filePath = $"Assets/Resources/Assistants/{Name}.json";
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, false))
                {
                    writer.Write(json);
                }

                AssetDatabase.Refresh();
                Debug.Log($"File successfully saved to: {filePath}");
            }
            catch (IOException e)
            {
                Debug.LogError($"Failed to save the file: {e.Message}");
            }

            Debug.Log("助手数据已保存到文件");
        }

        [FoldoutGroup("通过Json读取")]
        [Button("加载助手")]
        public void LoadAssistantFromFile()
        {
            string filePath = $"Assets/Resources/Assistants/";
            if (File.Exists(filePath + LoadName))
            {
                string json = File.ReadAllText(filePath + LoadName);
                var assistantData = JsonConvert.DeserializeObject<SaveAssistant>(json);
                this.ID = assistantData.ID;
                this.Name = assistantData.Name;
                this.Instructions = assistantData.Instructions;
                this.Model = Enum.Parse<AssistantModelType>(assistantData.Model);
                this.RetrieveOpen = assistantData.RetrieveOpen;
                this.CodeInterpreterOpen = assistantData.CodeInterpreterOpen;
                Debug.Log("从文件中还原了助手");
            }
            else
            {
                Debug.LogError("指定的文件不存在");
            }
        }

        [FoldoutGroup("通过Json读取")]
        [LabelText("选择本地助手文件")]
        [ValueDropdown("GetAllAssistantNames", AppendNextDrawer = true)]
        public string LoadName;

        private IEnumerable<string> GetAllAssistantNames()
        {
            string filePath = $"Assets/Resources/Assistants/";
            List<string> files = Directory.GetFiles(filePath).ToList();
            return files
                .Where(path => !path.EndsWith(".meta"))
                .Select(path => path.Split('/').Last());
        }

        private OpenAIUtility mOpenAIUtility;
        private bool mOnInfoChanged = false;
        [HideInInspector] public bool HasCreated = false;

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

        private void RefinePrompt()
        {
            var window = OdinEditorWindow.GetWindow<PromptRefineWindow>();
            if (PromptFramework.FrameworkDic.Count == 0)
            {
                PromptFramework.LoadFromJson();
            }
            window.ShowWindow(Instructions,"提示词优化", resultPrompt =>
            {
                Instructions = resultPrompt;
                mOnInfoChanged = true;
            });
        }
        
        private void TranslatePrompt()
        {
            var window = OdinEditorWindow.GetWindow<PromptRefineWindow>();
            window.ShowWindow(Instructions,"提示词转英文", resultPrompt =>
            {
                Instructions = resultPrompt;
                mOnInfoChanged = true;
            });
        }
        

        public IArchitecture GetArchitecture()
        {
            return TeamFlow.Interface;
        }
    }
}