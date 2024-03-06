using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEditor;
using XNode;

namespace TeamFlow.Nodes
{
    [NodeTitle("显示结果","显示结果",SdfIconType.WindowDock)]
    [NodeTint("#414348")]
    public class DisplayResult:BaseNode
    {
        [Input] [LabelText("结果连接在这里")] public string result;
        
        public override object GetValue(NodePort port)
        {
            return null;
        }
        
        [Button("打开结果显示窗口")]
        private void 打开结果显示窗口()
        {
            if (TeamFlow.TeamFlowState.Value==RunningState.Finished)
            {
                var window = EditorWindow.GetWindow<DisplayWindow>();
                TeamFlow.Result.Value=GetInputValue("result", this.result);
                window.UpdateText(TeamFlow.Result.Value);
                window.Show();
            }
        }
        
        protected override async UniTask RunStepNodeLogic(UniTaskCompletionSource utcs, CancellationTokenSource cts = default)
        {
            await UniTask.Yield();
        }
    }
}