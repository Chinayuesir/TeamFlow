using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenAI;
using OpenAI.Assistants;
using OpenAI.Chat;
using OpenAI.Files;
using OpenAI.Models;
using OpenAI.Threads;
using QFramework;
using TeamFlow.Nodes;
using UnityEngine;
using Message = OpenAI.Chat.Message;
using Progress = Utilities.WebRequestRest.Progress;

namespace TeamFlow.Utilities
{
    public enum ModelType
    {
        GPT3_5_Turbo,
        GPT4,
        GPT4_32K,
        GPT3_5_Turbo_16K
    }
    
    public enum AssistantModelType
    {
        GPT3_5_Turbo,
        GPT4
    }

    public class OpenAIUtility : IUtility
    {
        private readonly OpenAIClient mApi= new OpenAIClient(new OpenAIAuthentication()
            .LoadFromAsset(Resources.Load<OpenAIConfiguration>("OpenAIConfiguration")));

        private string mDefaultSystemMessage = "你是一个有用的问答助手";

        private static Dictionary<ModelType, Model> mModelsDic=new Dictionary<ModelType, Model>()
        {
            {ModelType.GPT4,Model.GPT4},
            {ModelType.GPT4_32K,Model.GPT4_32K},
            {ModelType.GPT3_5_Turbo,Model.GPT3_5_Turbo},
            {ModelType.GPT3_5_Turbo_16K,Model.GPT3_5_Turbo_16K},
        };
        
        private static Dictionary<AssistantModelType, string> mAssistantModelsDic=new Dictionary<AssistantModelType, string>()
        {
            {AssistantModelType.GPT3_5_Turbo,"gpt-3.5-turbo-1106"},
            {AssistantModelType.GPT4,"gpt-4-1106-preview"}
        };
        
        private Model mDefaultModel = mModelsDic[ModelType.GPT3_5_Turbo];
        
        /// <summary>
        /// 普通对话
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="system"></param>
        /// <param name="modelType"></param>
        /// <param name="normalMemory"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async UniTask<string> GetCompletion(string prompt
            , string system = ""
            , ModelType modelType = ModelType.GPT3_5_Turbo
            , NormalMemory normalMemory=null, CancellationToken ct = default)
        {
            system ??= mDefaultSystemMessage;
            var model = mModelsDic[modelType];
            List<Message> requestList = new List<Message>();
            if (normalMemory!.Messages.Count == 0)
            {
                requestList.Add(new Message(Role.System, system));
                requestList.Add(new Message(Role.User, prompt));
                normalMemory.AddMessage(new SimpleMessage(Role.System, system));
                normalMemory.AddMessage(new SimpleMessage(Role.User, prompt));
            }
            else
            {
                requestList.Add(new Message(Role.System, system));
                normalMemory.Messages[0].Content = system;
                for (var i = 1; i < normalMemory.Messages.Count; i++)
                {
                    var msg = new Message( normalMemory.Messages[i].Role, normalMemory.Messages[i].Content);
                    requestList.Add(msg);
                }
                requestList.Add(new Message(Role.User, prompt));
               normalMemory.AddMessage(new SimpleMessage(Role.User,prompt));
            }
            var chatRequest = new ChatRequest(requestList, model);
            var response = await mApi.ChatEndpoint.GetCompletionAsync(chatRequest, ct);
            var choice = response.FirstChoice;
            //Debug.Log($"[{choice.Index}] {choice.Message.Role}: {choice.Message} | Finish Reason: {choice.FinishReason}");
            return choice.Message;
        }


        /// <summary>
        /// 创建Assistant
        /// </summary>
        /// <param name="name"></param>
        /// <param name="instructions"></param>
        /// <param name="assistantModelType"></param>
        /// <param name="tools"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public async UniTask<AssistantResponse> CreateAssistant(
            string name, 
            string instructions,
            AssistantModelType assistantModelType, 
            IEnumerable<Tool> tools = null,
            IEnumerable<string> files = null)
        {
            var request = new CreateAssistantRequest(mAssistantModelsDic[assistantModelType]
                ,name,"",instructions,tools,files);
            var assistant = await mApi.AssistantsEndpoint.CreateAssistantAsync(request);
            return assistant;
        }

        /// <summary>
        /// 创建一个Thread
        /// </summary>
        /// <returns></returns>
        public async UniTask<ThreadResponse> CreateThread()
        {
            var thread = await mApi.ThreadsEndpoint.CreateThreadAsync();
            return thread;
        }

        /// <summary>
        /// 通过传入thread和assistant获取返回
        /// </summary>
        /// <param name="assistant"></param>
        /// <param name="thread"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async UniTask<string> GetAssistantResponse(AssistantResponse assistant,ThreadResponse thread,string content)
        {
            var request = new CreateMessageRequest(content);
            var message = await thread.CreateMessageAsync(request);
            var run = await thread.CreateRunAsync(assistant);
            await run.WaitForRunCompleteAsync();
            var messages= await run.ListMessagesAsync();
            foreach (var messageResponse in messages.Items)
            {
                Debug.Log(messageResponse.PrintContent());
            }
            return messages.Items[0].PrintContent();
        }

        #region File相关

        
        /// <summary>
        /// 列出服务器中所有已经有的文件
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<FileResponse>> ListFilesAsync(Action<Progress> callback=null)
        {
            var fileList = await mApi.FilesEndpoint.ListFilesAsync();
            return fileList;
        }
        
        /// <summary>
        /// 上传文件到服务器后台
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async UniTask<FileResponse> UploadFileAsync(string filePath, Action<Progress> callback=null)
        {
            IProgress<Progress> progress = new Progress<Progress>(callback);
            var fileResponse = await mApi.FilesEndpoint.UploadFileAsync(filePath,"assistants",progress);
            return fileResponse;
        }
        
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async UniTask<bool> DeleteFileAsync(string id)
        {
            var api = new OpenAIClient();
            var isDeleted = await api.FilesEndpoint.DeleteFileAsync(id);
            return isDeleted;
        }
        
        /// <summary>
        /// 通过id检索文件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async UniTask<FileResponse> GetFileInfoAsync(string id)
        {
            var file = await mApi.FilesEndpoint.GetFileInfoAsync(id);
            return file;
        }

        #endregion

        #region Assistant相关
        /// <summary>
        /// 列出所有的Assistant
        /// </summary>
        public async UniTask<ListResponse<AssistantResponse>> ListAssistant()
        {
            var assistantsList = await mApi.AssistantsEndpoint.ListAssistantsAsync();
            return assistantsList;
        }
        /// <summary>
        /// 根据id检索和找回一个Assistant
        /// </summary>
        public async UniTask<AssistantResponse> RetrieveAssistant(string id)
        {
            var assistant = await mApi.AssistantsEndpoint.RetrieveAssistantAsync(id);
            Debug.Log($"{assistant} -> {assistant.CreatedAt}");
            return assistant;
        }

        /// <summary>
        /// 根据id修改Assistant
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newModel"></param>
        /// <returns></returns>
        public async UniTask<AssistantResponse> ModifyAssistant(string id,string name,string instructions
            ,AssistantModelType newModel=AssistantModelType.GPT3_5_Turbo,IEnumerable<Tool> tools = null, 
            IEnumerable<string> files = null)
        {
            var modifyRequest = new CreateAssistantRequest(mAssistantModelsDic[newModel]
                ,name,null,instructions,tools,files);
            var modifiedAssistant = await mApi.AssistantsEndpoint.ModifyAssistantAsync(id, modifyRequest);
            return modifiedAssistant;
        }
        
        /// <summary>
        /// 根据assistant本身修改
        /// </summary>
        /// <param name="assistant"></param>
        /// <param name="newModel"></param>
        /// <returns></returns>
        public async UniTask<AssistantResponse> ModifyAssistant(AssistantResponse assistant,string name,string instructions,
            AssistantModelType newModel=AssistantModelType.GPT3_5_Turbo,IEnumerable<Tool> tools = null, 
            IEnumerable<string> files = null)
        {
            var modifyRequest = new CreateAssistantRequest(mAssistantModelsDic[newModel],name,null,instructions);
            var modifiedAssistant = await assistant.ModifyAsync(modifyRequest);
            return modifiedAssistant;
        }
        
        /// <summary>
        /// 根据id删除Assistant
        /// </summary>
        /// <param name="id"></param>
        public async UniTask<bool> DeleteAssistant(string id)
        {
            var isDeleted = await mApi.AssistantsEndpoint.DeleteAssistantAsync(id);
            return isDeleted;
        }
        
        /// <summary>
        /// 根据assistant本身修改
        /// </summary>
        /// <param name="assistant"></param>
        public async UniTask<bool> DeleteAssistant(AssistantResponse assistant)
        {
            var isDeleted = await assistant.DeleteAsync();
            return isDeleted;
        }

        /// <summary>
        /// 列出assistant的所有文件
        /// </summary>
        /// <param name="id"></param>
        public async UniTask<ListResponse<AssistantFileResponse>> ListAssistantFile(string id)
        {
            var filesList = await mApi.AssistantsEndpoint.ListFilesAsync(id);
            return filesList;
        }
        /// <summary>
        /// 列出assistant的所有文件
        /// </summary>
        /// <param name="assistant"></param>
        public async UniTask<ListResponse<AssistantFileResponse>> ListAssistantFile(AssistantResponse assistant)
        {
            var filesList = await assistant.ListFilesAsync();
            return filesList;
        }

        /// <summary>
        /// 在assistant上附加文件
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filePath"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async UniTask<AssistantFileResponse> AttachAssistantFile(string id,FileResponse file)
        {
            var assistantFile = await mApi.AssistantsEndpoint.AttachFileAsync(id, file);
            return assistantFile;
        }

        /// <summary>
        /// 在assistant上附加文件
        /// </summary>
        /// <param name="assistant"></param>
        /// <param name="filePath"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async UniTask<AssistantFileResponse> AttachAssistantFile(AssistantResponse assistant,FileResponse file)
        {
            var assistantFile = await assistant.AttachFileAsync(file);
            return assistantFile;
        }
        /// <summary>
        /// 上传文件，即将文件上传至服务器以待后续使用
        /// </summary>
        /// <param name="assistant"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async UniTask<AssistantFileResponse> UpLoadFile(AssistantResponse assistant,string filePath)
        {
            var assistantFile = await assistant.UploadFileAsync(filePath);
            return assistantFile;
        }

        /// <summary>
        /// 检索一个文件
        /// </summary>
        /// <param name="assistantID"></param>
        /// <param name="fileID"></param>
        public async UniTask RetrieveFile(string assistantID, string fileID)
        {
            var assistantFile = await mApi.AssistantsEndpoint.RetrieveFileAsync(assistantID, fileID);
            Debug.Log($"{assistantFile.AssistantId}'s file -> {assistantFile.Id}");
        }
        
        /// <summary>
        /// 检索一个文件
        /// </summary>
        /// <param name="assistant"></param>
        /// <param name="fileID"></param>
        public async UniTask RetrieveFile(AssistantResponse assistant, string fileID)
        {
            var assistantFile = await assistant.RetrieveFileAsync(fileID);
            Debug.Log($"{assistantFile.AssistantId}'s file -> {assistantFile.Id}");
        }
        
        /// <summary>
        /// 移除文件，但文件继续保留在组织中，删除请使用DeleteFile
        /// </summary>
        /// <param name="assistantID"></param>
        /// <param name="fileID"></param>
        /// <returns></returns>
        public async UniTask<bool> RemoveFile(string assistantID, string fileID)
        {
            var api = new OpenAIClient();
            var isRemoved = await api.AssistantsEndpoint.RemoveFileAsync(assistantID, fileID);
            return isRemoved;
        }
        /// <summary>
        /// 移除文件，但文件继续保留在组织中，删除请使用DeleteFile
        /// </summary>
        /// <param name="assistant"></param>
        /// <param name="fileID"></param>
        /// <returns></returns>
        public async UniTask<bool> RemoveFile(AssistantResponse assistant, string fileID)
        {
            var isRemoved = await assistant.RemoveFileAsync(fileID);
            return isRemoved;
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="assistant"></param>
        /// <param name="fileID"></param>
        /// <returns></returns>
        public async UniTask<bool> DeleteFile(AssistantResponse assistant,string fileID)
        {
            var isDeleted = await assistant.DeleteFileAsync(fileID);
            return isDeleted;
        }
        #endregion

        #region Thread相关
        /// <summary>
        /// 通过助手创建并运行一个Thread
        /// </summary>
        /// <param name="assistant"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public async UniTask<RunResponse> CreateAndRunThread(AssistantResponse assistant,
            List<OpenAI.Threads.Message> messages)
        {
            var threadRequest = new CreateThreadRequest(messages);
            var run = await assistant.CreateThreadAndRunAsync(threadRequest);
            Debug.Log($"Created thread and run: {run.ThreadId} -> {run.Id} -> {run.CreatedAt}");
            return run;
        }

        /// <summary>
        /// 检索一个线程
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async UniTask<ThreadResponse> RetrieveThread(string id)
        {
            var thread = await mApi.ThreadsEndpoint.RetrieveThreadAsync(id);
            Debug.Log($"Retrieve thread {thread.Id} -> {thread.CreatedAt}");
            return thread;
        }
        /// <summary>
        /// 删除一个线程
        /// </summary>
        /// <param name="thread"></param>
        /// <returns></returns>
        public async UniTask<bool> DeleteThread(ThreadResponse thread)
        {
            var isDeleted = await thread.DeleteAsync();
            return isDeleted;
        }
        
        public async UniTask<bool> DeleteThread(string id)
        {
            var isDeleted = await mApi.ThreadsEndpoint.DeleteThreadAsync(id);
            return isDeleted;
        }

        /// <summary>
        /// 列出线程的消息列表
        /// </summary>
        /// <param name="thread"></param>
        public async UniTask ListMessages(ThreadResponse thread)
        {
            var messageList = await thread.ListMessagesAsync();
            foreach (var message in messageList.Items)
            {
                Debug.Log($"{message.Id}: {message.Role}: {message.PrintContent()}");
            }
        }

        /// <summary>
        /// 列出所有运行
        /// </summary>
        /// <param name="thread"></param>
        public async UniTask ListRuns(ThreadResponse thread)
        {
            var runList = await thread.ListRunsAsync();
            foreach (var run in runList.Items)
            {
                Debug.Log($"[{run.Id}] {run.Status} | {run.CreatedAt}");
            }
        }
        
        

        #endregion
    }
}