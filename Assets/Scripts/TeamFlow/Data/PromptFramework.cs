using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using QFramework;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using TeamFlow.Utilities;
using UnityEditor;
using UnityEngine;

namespace TeamFlow
{
    public class PromptFramework
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public static Dictionary<string,string> FrameworkDic=new Dictionary<string, string>();
        public static string CurrentFrameworkKey = "ROSES框架";

        /// <summary>
        /// 通过Json加载提示词模板
        /// </summary>
        public static void LoadFromJson()
        {
            TextAsset textAsset = Resources.Load<TextAsset>("PromptFramework");
            string json = textAsset.text;
            var list = JsonConvert.DeserializeObject<List<PromptFramework>>(json);
            foreach (var promptFramework in list)
            {
                FrameworkDic[promptFramework.Name] = promptFramework.Content;
            }
        }
        
        /// <summary>
        /// 打开提示词框架设置面板
        /// </summary>
        [MenuItem("TeamFlow/提示词框架设置")]
        public static void OpenFrameworkSetting()
        {
            var window=OdinEditorWindow.GetWindow<PromptFrameworkSettingWindow>();
            if (FrameworkDic.Count == 0)
            {
                LoadFromJson();
            }
            window.Show();
            window.Content = FrameworkDic[window.CurrentFrameworkKey];
            window.OnClose += () =>
            {
                if (FrameworkDic.ContainsKey(window.CurrentFrameworkKey))
                {
                    CurrentFrameworkKey = window.CurrentFrameworkKey;
                }
                else
                {
                    Debug.LogError("找不到对应的框架");
                }
            };
        }
    }
    
    public class PromptFrameworkSettingWindow : OdinEditorWindow
    {
        [LabelText("当前框架名称")]
        [ValueDropdown(nameof(GetFramework), AppendNextDrawer = true)]
        [OnValueChanged(nameof(OnFrameChanged))]
        public string CurrentFrameworkKey="ROSES框架";
        
        [DisableInEditorMode]
        [LabelText("框架内容")]
        [TextArea(5,10)]
        public string Content;

        private void OnFrameChanged()
        {
            if (PromptFramework.FrameworkDic.ContainsKey(CurrentFrameworkKey))
            {
                Content = PromptFramework.FrameworkDic[CurrentFrameworkKey];
            }
        }

        private IEnumerable<string> GetFramework()
        {
            return PromptFramework.FrameworkDic.Keys;
        }
    }
    
    /// <summary>
    /// 提示词优化窗口重写
    /// </summary>
     public class PromptRefineWindow : OdinEditorWindow, IController
    {
        [TextArea(10, 15)] [PropertyOrder(0)] public string OptimizedPrompt = "";
        private Action<string> mChangePrompt;
        private List<string> mRefinePrompt;
        private List<string> mTranslatePrompt;

        public async void ShowWindow(string originPrompt,string actionName, Action<string> changePrompt)
        {
            OptimizedPrompt = "";
            mChangePrompt = changePrompt;
            var window = GetWindow<PromptRefineWindow>();
            window.titleContent = new GUIContent(actionName);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(500, 250);
            window.Show();
            await Generate(originPrompt,actionName);
            window.OnClose += () => { mChangePrompt = null; };
        }

        private async UniTask Generate(string originPrompt,string actionName)
        {
            var settings=new List<string>();
            if (actionName == "提示词优化")
            {
                mRefinePrompt = new List<string>()
                {
                    "你是一个提示词优化助手",
                    "请你对用户所输入的内容进行优化，使其能够更好地激发AI能力," +
                    $"获取更好的结果。提示词满足如下框架：'''{PromptFramework.FrameworkDic[PromptFramework.CurrentFrameworkKey]}''') " +
                    "你必须根据用户的描述，编写或者优化最符合用户意图的提示词，并直接返回编写好和优化后的提示词，无需说明" +
                    "这样做的理由，或者做一些额外的说明",
                    "好的，我理解了，我将按照提示词编写框架的指导，充分理解用户意图和任务目标，返回给用户修改后的提示词"
                };
                settings = mRefinePrompt;
            }else if (actionName == "提示词转英文")
            {
                mTranslatePrompt= new List<string>()
                {
                    "你是一个提示词翻译助手，负责将用户提供的提示词在不改变原意的情况下，翻译为英文",
                    "请你充分理解用户的描述，并翻译为英文",
                    "好的，我理解了"
                };
                settings = mTranslatePrompt;
            }
            var response = await this.GetUtility<OpenAIUtility>().GetCompletionStreaming(settings, originPrompt
                , partialResponse => { OptimizedPrompt += partialResponse.FirstChoice.Delta.ToString(); });
            OptimizedPrompt = response.FirstChoice.Message;
        }

        [HorizontalGroup(0.5f)]
        [Button("替换原来的提示词", ButtonSizes.Large)]
        [PropertyOrder(1)]
        public void ReplacePrompt()
        {
            mChangePrompt?.Invoke(OptimizedPrompt);
            Close();
        }

        [HorizontalGroup(0.5f)]
        [Button("关闭窗口", ButtonSizes.Large)]
        [PropertyOrder(2)]
        public void CloseWindow()
        {
            Close();
        }

        public IArchitecture GetArchitecture()
        {
            return TeamFlow.Interface;
        }
    }
}