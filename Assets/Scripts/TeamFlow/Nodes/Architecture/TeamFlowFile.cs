using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenAI.Assistants;
using QFramework;
using Sirenix.OdinInspector;
using TeamFlow.Utilities;
using UnityEngine;
using Utilities.WebRequestRest;

namespace TeamFlow
{
    [Serializable]
    public class TeamFlowFile:IController
    {
        [DisableInEditorMode]
        [LabelText("文件ID")]
        public string FileID;
        [DisableInEditorMode]
        [LabelText("文件创建时间")]
        public DateTime Created;
        [LabelText("文件名称")]
        public string FileName;
        
        [HideIf(nameof(IsOnServer))]
        [HorizontalGroup]
        [LabelText("文件路径")]
        [FilePath(RequireExistingPath = true,AbsolutePath = true,Extensions
            = "c,cpp,css,csv,docx,gif,html,java,jpeg,jpg,js,json,md,pdf,php,png,pptx,py,rb,tar,tex,ts,txt,xlsx,xml,zip")]
        public string AddFilePath;
        
        [HideInInspector]
        public bool IsOnServer=false;
        
        [HideIf(nameof(IsOnServer))]
        [ShowIf(nameof(HasPath))]
        [HorizontalGroup]
        [Button("上传至服务器")]
        private async void UploadFile()
        {
            if (!File.Exists(AddFilePath)) return;
            ProgressBar = 0;
            var fileResponse = await this.GetUtility<OpenAIUtility>().UploadFileAsync(AddFilePath,OnProgress);
            Debug.Log(Path.GetFileName(AddFilePath));
            Debug.Log("上传成功！");
            await TeamFlow.SyncFilesAndAssistants();
        }
        
        [ShowIf(nameof(HasPath))]
        [LabelText("上传进度")]
        [HideIf(nameof(IsOnServer))]
        [ProgressBar(0, 100)]
        public int ProgressBar = 0;
        
        
        private void OnProgress(Progress p)
        {
            ProgressBar = (int)p.Percentage;
            Debug.Log("文件上传中....");
        }

        [ShowIf(nameof(HasAttachedAtLeastOneAssistant))]
        [HorizontalGroup]
        [LabelText("选择助手")]
        [ValueDropdown(nameof(AssistantNameList))]
        public string AssistantName;

        [HideInInspector]
        public List<string> AssistantInfoList=new List<string>();
        
        private IEnumerable<string> AssistantNameList()
        {
            if (AssistantInfoList.Count==0) return null;
            return AssistantInfoList.Select(assistant => assistant.Split("---")[0]);
        }

        [ShowIf(nameof(HasAttachedAtLeastOneAssistant))]
        [HorizontalGroup]
        [Button("解除链接")]
        public async void UnAttachFileFromAssistant()
        {
            if (AssistantInfoList.Count == 0) return;
            string id = AssistantInfoList.Find(info => info.Split("---")[0] == AssistantName)
                .Split("---")[1];
            var assistant = await this.GetUtility<OpenAIUtility>().RetrieveAssistant(id);
            await assistant.RemoveFileAsync(FileID);
            AssistantInfoList.Remove(AssistantInfoList.Find(s => s.Split("---")[1] == id));
            await TeamFlow.SyncFilesAndAssistants();
        }
        
        [ShowIf(nameof(IsOnServer))]
        [Button("从服务器中删除文件")]
        private async void DeleteFile()
        {
            await this.GetUtility<OpenAIUtility>().DeleteFileAsync(FileID);
            await TeamFlow.SyncFilesAndAssistants();
        }

        /// <summary>
        /// 文件是否至少链接了一个助手
        /// </summary>
        /// <returns></returns>
        private bool HasAttachedAtLeastOneAssistant()
        {
            return AssistantInfoList.Count > 0;
        }

        /// <summary>
        /// 是否已经填写了路径
        /// </summary>
        /// <returns></returns>
        private bool HasPath()
        {
            return AddFilePath != "";
        }

        public IArchitecture GetArchitecture()
        {
            return TeamFlow.Interface;
        }
    }
}