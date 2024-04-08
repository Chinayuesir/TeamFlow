using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using Cysharp.Threading.Tasks;
using QFramework;
using TeamFlow.Utilities;
using UnityEditor;
using UnityEngine;
using Tool = OpenAI.Tool;

namespace TeamFlow
{
    public enum RunningState
    {
        NoStart,
        Started,
        Finished,
    }

    public class TeamFlow : Architecture<TeamFlow>
    {
        public static EasyEvent TeamFlowStart = new EasyEvent();
        public static EasyEvent UpdateInfoFromServer = new EasyEvent();

        public static BindableProperty<string> Result = new BindableProperty<string>("");
        public static BindableProperty<RunningState> TeamFlowState
            = new BindableProperty<RunningState>(RunningState.NoStart);

        public static List<Assistant> Assistants = new List<Assistant>();
        public static List<TeamFlowFile> Files = new List<TeamFlowFile>();

        protected override void Init()
        {
            RegisterUtility(new OpenAIUtility());
            if (PromptFramework.FrameworkDic.Count == 0)
            {
                PromptFramework.LoadFromJson();
            }
            MDViewer.DuplicateCodeEvent.Register(id =>
            {
                EditorGUIUtility.systemCopyBuffer = MDViewer.CodeBlocks[id].codeBlock;
            });
            MDViewer.SaveCodeToFileEvent.Register(id =>
            {
                string defaultName = "DefaultMonoScript";
                string path = "Assets/";
                string extension = "cs";
                string filePath = EditorUtility.SaveFilePanel("Save file as...", path, defaultName, extension);
                if (!string.IsNullOrEmpty(filePath))
                {
                    string content =   MDViewer.CodeBlocks[id].codeBlock;
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(filePath, false))
                        {
                            writer.Write(content);
                        }
                        AssetDatabase.Refresh();
                        Debug.Log($"File successfully saved to: {filePath}");
                    }
                    catch (IOException e)
                    {
                        Debug.LogError($"Failed to save the file: {e.Message}");
                    }
                }
                else
                {
                    Debug.Log("File save action was cancelled or failed.");
                }
            });
        }

        public static void Reset()
        {
            TeamFlowStart = new EasyEvent();
            Result.Value = "";
            TeamFlowState.Value = RunningState.NoStart;
        }

        public static async UniTask SyncFilesAndAssistants()
        {
            await SyncFilesWithServer();
            await SyncAssistantsWithServer();
            UpdateInfoFromServer.Trigger();
        }

        private static async UniTask SyncAssistantsWithServer()
        {
            // 从服务器获取数据
            var assistantList = await Interface.GetUtility<OpenAIUtility>().ListAssistant();
            Assistants.Clear();
            if (assistantList == null) return;

            foreach (var item in assistantList.Items)
            {
                bool codeInterpreterOpen = false;
                bool retrieveOpen = false;
                var toolList = new List<Tool>();
                foreach (var tool in item.Tools)
                {
                    if (tool.Type == "retrieval")
                        retrieveOpen = true;
                    else if (tool.Type == "code_interpreter")
                        codeInterpreterOpen = true;
                    else
                    {
                        toolList.Add(tool);
                    }
                }

                var assistant = new Assistant()
                {
                    ID = item.Id,
                    Name = item.Name,
                    Instructions = item.Instructions,
                    CodeInterpreterOpen = codeInterpreterOpen,
                    RetrieveOpen = retrieveOpen,
                    HasCreated = true,
                    Model = item.Model.ToLower().Contains("gpt-4")
                        ? AssistantModelType.GPT4
                        : AssistantModelType.GPT3_5_Turbo,
                    AssistantFiles = new List<TeamFlowFile>()
                };
                foreach (var fileId in item.FileIds)
                {
                    foreach (var teamFlowFile in Files)
                    {
                        if (fileId == teamFlowFile.FileID)
                        {
                            assistant.AssistantFiles.Add(teamFlowFile);
                            teamFlowFile.AssistantInfoList.Add(item.Name + "---" + item.Id);
                        }
                    }
                }

                Assistants.Add(assistant);
            }

            Debug.Log("可用助手列表同步完毕！");

            // 更新本地数据
            AssistantData localData = Resources.Load<AssistantData>("AssistantData");
            if (localData == null)
            {
                localData = ScriptableObject.CreateInstance<AssistantData>();
                //TODO:硬编码，后续优化
                UnityEditor.AssetDatabase.CreateAsset(localData,
                    "Assets/Scripts/TeamFlow/Data/Resources/AssistantData.asset");
            }

            localData.assistants = Assistants;
            UnityEditor.EditorUtility.SetDirty(localData);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        private static async UniTask SyncFilesWithServer()
        {
            // 从服务器获取数据
            var fileList = await Interface.GetUtility<OpenAIUtility>().ListFilesAsync();
            Files.Clear();
            if (fileList == null) return;
            foreach (var item in fileList)
            {
                TeamFlowFile file = new TeamFlowFile()
                {
                    FileID = item.Id,
                    Created = item.CreatedAt,
                    IsOnServer = true
                };
                file.FileName = DecodeMimeString(item.FileName);
                Files.Add(file);
            }

            Debug.Log("可用文件列表同步完毕！");

            // 更新本地数据
            FileData localData = Resources.Load<FileData>("FileData");
            if (localData == null)
            {
                localData = ScriptableObject.CreateInstance<FileData>();
                //TODO:硬编码，后续优化
                UnityEditor.AssetDatabase.CreateAsset(localData,
                    "Assets/Scripts/TeamFlow/Data/Resources/FileData.asset");
            }

            localData.Files = Files;
            UnityEditor.EditorUtility.SetDirty(localData);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        private static string DecodeMimeString(string mimeString)
        {
            // 判断是否为有效的MIME编码字符串
            if (mimeString.StartsWith("=?") && mimeString.EndsWith("?="))
            {
                try
                {
                    // 将MIME编码的字符串转换为Attachment
                    var attachment = new Attachment(new System.IO.MemoryStream(), mimeString,
                        MediaTypeNames.Application.Octet);
                    // 获取解码后的文件名
                    string decodedString = attachment.Name;
                    return decodedString;
                }
                catch (Exception ex)
                {
                    // 处理解码过程中的错误
                    Debug.Log("Error in decoding: " + ex.Message);
                    return mimeString; // 返回原始字符串
                }
            }
            else
            {
                return mimeString; // 不是有效的MIME编码，直接返回原字符串
            }
        }
    }
}