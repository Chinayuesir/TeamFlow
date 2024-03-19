using System.Collections.Generic;
using QFramework;
using UnityEditor;
using UnityEngine;

namespace TeamFlow
{
    public class DisplayWindow : EditorWindow, IUnRegisterList
    {
        private MDViewer mMarkdownViewer;
        private bool mInited = false;
        
        public void UpdateText(string content)
        {
            if (mMarkdownViewer != null)
            {
                mMarkdownViewer.UpdateText(content);
            }
        }

        private void Init()
        {
            mInited = true;

            GetWindow<DisplayWindow>().minSize=new Vector2(600, 400);
            
            var skin = Resources.Load<GUISkin>("Skin/MarkdownSkinQS");
            mMarkdownViewer = new MDViewer(skin, string.Empty, "");
            TeamFlow.Result.Register(UpdateText).AddToUnregisterList(this);
            //确保第一次打开时能够显示
            UpdateText(TeamFlow.Result.Value);
        }

        private void OnDisable()
        {
            mMarkdownViewer = null;
            this.UnRegisterAll();
        }
        
        public void OnGUI()
        {
            if (!mInited) Init();
            if(mMarkdownViewer.Update())
                Repaint();
            Rect rect = new Rect(0, 0, position.width-10, position.height);
            mMarkdownViewer.DrawWithRect(rect);
        }

        public List<IUnRegister> UnregisterList { get; } = new List<IUnRegister>();
    }
}