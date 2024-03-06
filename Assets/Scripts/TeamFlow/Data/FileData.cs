using System.Collections.Generic;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TeamFlow
{
    [CreateAssetMenu(fileName = "FileData", menuName = "TeamFlow/FileData", order = 1)]
    public class FileData : ScriptableObject,IController
    {
        [Searchable]
        [BoxGroup("服务器上当前文件")]
        public List<TeamFlowFile> Files;
        
        [Button("增加文件")]
        private void AddFile()
        {
           Files.Add(new TeamFlowFile());
        }
        
        [Button("从服务器获取数据")]
        private async void DownloadInfo()
        {
            await TeamFlow.SyncFilesAndAssistants();
            Debug.Log("获取成功！");
        }
        
        public IArchitecture GetArchitecture()
        {
            return TeamFlow.Interface;
        }
    }
}