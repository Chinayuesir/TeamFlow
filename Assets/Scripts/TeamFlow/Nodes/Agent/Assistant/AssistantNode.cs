using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TeamFlow.Utilities;
using XNode;

namespace TeamFlow.Nodes
{
    [NodeTitle("助手", "智能代理",SdfIconType.Robot)]
    [NodeWidth(400)]
    [NodeTint("#2D4A60")]
    public class AssistantNode: BaseOtherNode, IController
    {
        [Output(ShowBackingValue.Never, ConnectionType.Override)]
        [LabelText("助手")]
        public Assistant result;
        
        [LabelText("选择助手")]
        [ValueDropdown(nameof(GetAssistantList), AppendNextDrawer = true)]
        [OnValueChanged("AssistantChanged")]
        public string AssistantName;
        
        [LabelText("当前助手")]
        public Assistant Assistant;
        
        private OpenAIUtility mOpenAIUtility;
        
        /// <summary>
        /// 自定义函数
        /// </summary>
        [LabelText("自定义函数")]
        [Output(ShowBackingValue.Never, ConnectionType.Override, dynamicPortList = true)]
        [OnCollectionChanged(After = "OnDynamicPortListChange")]
        [ListDrawerSettings(ShowIndexLabels = true,CustomAddFunction = "AddFunctionPort")]
        [NonSerialized,OdinSerialize]
        public List<FunctionPort> FunctionPorts=new List<FunctionPort>();
        private FunctionPort AddFunctionPort()
        {
            return new FunctionPort();
        }


        protected override void Init()
        {
            base.Init();
            //服务器更新数据后需要手动触发一次助手变更，否则不刷新
            TeamFlow.UpdateInfoFromServer.Register(AssistantChanged);
            if (AssistantName != null)
            {
                mOpenAIUtility ??= this.GetUtility<OpenAIUtility>();
            }
        }
        
        private void AssistantChanged()
        {
            var asst= TeamFlow.Assistants.Find(assistant => assistant.Name == AssistantName);
            Assistant = asst;
        }

        private IEnumerable<string> GetAssistantList()
        {
            return TeamFlow.Assistants.Select(assistant => assistant.Name);
        }
        

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "result")
            {
                var assistant= TeamFlow.Assistants.Find(assistant => assistant.Name == AssistantName);
                return assistant;
            }
            else return null;
        }

        public IArchitecture GetArchitecture()
        {
            return TeamFlow.Interface;
        }
    }
}