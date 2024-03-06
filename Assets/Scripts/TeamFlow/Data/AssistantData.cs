using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TeamFlow
{
    [CreateAssetMenu(fileName = "AssistantData", menuName = "TeamFlow/AssistantData", order = 1)]
    public class AssistantData : ScriptableObject
    {
        [Searchable]
        public List<Assistant> assistants;
    
        [HorizontalGroup]
        [Button("添加一个新的助手")]
        private void AddAssistant()
        {
            assistants.Add(new Assistant());
        }
    
        [HorizontalGroup]
        [Button("从服务器获取数据")]
        private async void DownloadInfo()
        {
            await TeamFlow.SyncFilesAndAssistants();
            Debug.Log("获取成功！");
        }
    }
}

