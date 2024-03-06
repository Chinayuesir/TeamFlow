/****************************************************************************
 * Copyright (c) 2019 Gwaredd Mountain UNDER MIT License
 * Copyright (c) 2022 liangxiegame UNDER MIT License
 *
 * https://github.com/gwaredd/UnityMarkdownViewer
 * http://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace QFramework
{
    internal class MDBlockContainer : MDBlock
    {
        public bool Quoted = false;
        public bool Highlight = false;
        public bool Horizontal = false;
        public bool IsTableRow = false;
        public bool IsTableHeader = false;

        List<MDBlock> mBlocks = new List<MDBlock>();

        public MDBlockContainer(float indent) : base(indent)
        {
        }

        public MDBlock Add(MDBlock block)
        {
            block.Parent = this;
            mBlocks.Add(block);
            return block;
        }

        public override MDBlock Find(string id)
        {
            if (id.Equals(ID, StringComparison.Ordinal))
            {
                return this;
            }

            foreach (var block in mBlocks)
            {
                var match = block.Find(id);

                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        public override void Arrange(MDContext context, Vector2 pos, float maxWidth)
        {
            Rect.position = new Vector2(pos.x + Indent, pos.y);
            Rect.width = maxWidth - Indent - context.IndentSize;

            var paddingBottom = 0.0f;
            var paddingVertical = 0.0f;

            if (Highlight || IsTableHeader || IsTableRow)
            {
                GUIStyle style;

                if (Highlight)
                {
                    style = GUI.skin.GetStyle(Quoted ? "blockquote" : "blockcode");
                }
                else
                {
                    style = GUI.skin.GetStyle(IsTableHeader ? "th" : "tr");
                }

                pos.x += style.padding.left;
                pos.y += style.padding.top;

                maxWidth -= style.padding.horizontal;
                paddingBottom = style.padding.bottom;
                paddingVertical = style.padding.vertical;
            }

            if (Horizontal)
            {
                Rect.height = 0;
                maxWidth = mBlocks.Count == 0 ? maxWidth : maxWidth / mBlocks.Count;

                foreach (var block in mBlocks)
                {
                    block.Arrange(context, pos, maxWidth);
                    pos.x += block.Rect.width;
                    Rect.height = Mathf.Max(Rect.height, block.Rect.height);
                }

                Rect.height += paddingVertical;
            }
            else
            {
                foreach (var block in mBlocks)
                {
                    block.Arrange(context, pos, maxWidth);
                    pos.y += block.Rect.height;
                }

                Rect.height = pos.y - Rect.position.y + paddingBottom;
            }
        }

        private int mCodeBlockID = -1;
        public override void Draw(MDContext context)
        {
            if (Highlight && !Quoted)
            {
                GUI.Box(Rect, string.Empty, GUI.skin.GetStyle("blockcode"));

                mCodeBlockID = MDViewer.CodeBlockID;
                MDViewer.CodeBlockID++;
                
                // 绘制按钮，并在鼠标悬浮时显示提示
                if (GUI.Button(new Rect(Rect.width - 20, Rect.y + 10, 20, 20), new GUIContent(
                            SdfIcons.CreateTransparentIconTexture(SdfIconType.ArrowsAngleExpand, 
                                Color.white, 20, 20, 0),
                            "复制代码到剪贴板"  // 这里是悬浮提示文本
                        ),
                        GUI.skin.GetStyle("button")))
                {
                    // 复制代码段文本到剪贴板的逻辑
                    //CopyTextToClipboard(mBlocks[0]);
                    MDViewer.DuplicateCodeEvent.Trigger(mCodeBlockID);

                    if (MDViewer.CodeBlocks.Count == 0)
                    {
                        // 正则表达式，用于匹配代码块。匹配 ``` 后的任何非换行字符（语言标识），然后是代码块直到下一个 ```
                        string pattern = @"```(.*?)\n([\s\S]*?)```";

                        // 执行匹配操作
                        MatchCollection matches = Regex.Matches(MDViewer.Text, pattern);

                  

                        foreach (Match match in matches)
                        {
                            // 确保捕获组存在
                            if (match.Groups.Count > 2) 
                            {
                                string language = match.Groups[1].Value.Trim(); // 提取语言标识
                                string codeBlock = match.Groups[2].Value; // 提取代码块内容
                                MDViewer.CodeBlocks.Add((language, codeBlock));
                            }
                        }
                    }
                    
                    EditorGUIUtility.systemCopyBuffer = MDViewer.CodeBlocks[mCodeBlockID].codeBlock;
                    Debug.Log("language:"+MDViewer.CodeBlocks[mCodeBlockID].language);
                    Debug.Log("code:"+MDViewer.CodeBlocks[mCodeBlockID].codeBlock);
                }

            }
            else if (IsTableHeader)
            {
                GUI.Box(Rect, string.Empty, GUI.skin.GetStyle("th"));
            }
            else if (IsTableRow)
            {
                var parentBlock = Parent as MDBlockContainer;
                if (parentBlock == null)
                {
                    GUI.Box(Rect, string.Empty, GUI.skin.GetStyle("tr"));
                }
                else
                {
                    var idx = parentBlock.mBlocks.IndexOf(this);
                    GUI.Box(Rect, string.Empty, GUI.skin.GetStyle(idx % 2 == 0 ? "tr" : "trl"));
                }
            }

            mBlocks.ForEach(block => block.Draw(context));

            if (Highlight && Quoted)
            {
                GUI.Box(Rect, string.Empty, GUI.skin.GetStyle("blockquote"));
            }
        }

        public void RemoveTrailingSpace()
        {
            if (mBlocks.Count > 0 && mBlocks[mBlocks.Count - 1] is MDBlockSpace)
            {
                mBlocks.RemoveAt(mBlocks.Count - 1);
            }
        }
    }
}
#endif