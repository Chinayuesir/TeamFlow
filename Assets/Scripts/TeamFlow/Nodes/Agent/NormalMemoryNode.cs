using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OpenAI;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace TeamFlow.Nodes
{
    [Serializable]
    public class SimpleMessage
    {
        public Role Role { get; set; }
        public string Content { get; set; }

        public SimpleMessage() { }

        public SimpleMessage(Role role, string content)
        {
            Role = role;
            Content = content;
        }
    }
    
    [NodeTitle("普通记忆节点","记忆",SdfIconType.Memory)]
    [NodeTint("#2D4A60")]
    public class NormalMemoryNode : BaseOtherNode
    {
        private string mFolderPath = Application.streamingAssetsPath;
        private string mFileName; // 存储文件的名称
        
        protected override void Init()
        {
            base.Init();
            mFileName = "MemoryData_" + mGUID + ".json";
        }

        public void SetFileName(string fileName)
        {
            mFileName = fileName;
        }
        
        [Output] [LabelText("记忆")] public NormalMemory normalMemory;

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "normalMemory")
            {
                LoadFromFile(); // 从文件加载Memory
                return normalMemory;
            }

            return null;
        }

        private void LoadFromFile()
        {
            string fullPath = Path.Combine(mFolderPath, mFileName);
            if (File.Exists(fullPath))
            {
                string fileContent = File.ReadAllText(fullPath);
                normalMemory = JsonConvert.DeserializeObject<NormalMemory>(fileContent) ?? new NormalMemory();
            }
            else
            {
                normalMemory = new NormalMemory
                {
                    Messages = new List<SimpleMessage>()
                };
                Debug.Log("没有找到记忆文件");
            }
            normalMemory.Changed = new EasyEvent();
            normalMemory.Changed.Register(SaveToFile);
        }

        private void SaveToFile()
        {
            string fullPath = Path.Combine(mFolderPath, mFileName);
            string jsonContent = JsonConvert.SerializeObject(normalMemory);
            if (!Directory.Exists(mFolderPath))
            {
                Directory.CreateDirectory(mFolderPath);
            }
            File.WriteAllText(fullPath, jsonContent);
        }
    }
}