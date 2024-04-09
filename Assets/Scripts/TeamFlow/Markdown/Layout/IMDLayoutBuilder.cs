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
using System.Text;
using UnityEngine;

namespace QFramework
{
    internal class IMDLayoutBuilder : IMDBuilder
    {
        public void Text(string text, MDStyle style, string link, string tooltip)
        {
            if (mCurrentContent == null)
            {
                NewContentBlock();
            }

            mContext.Apply(style);

            if (style.Size > 0)
            {
                if (mCurrentContent.ID == null)
                {
                    mCurrentContent.ID = "#";
                }
                else
                {
                    mCurrentContent.ID += "-";
                }

                mCurrentContent.ID += text.Trim().Replace(' ', '-').ToLower();
            }

            mStyle = style;
            mLink = link;
            mTooltip = tooltip;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (ch == '\n')
                {
                    AddWord();
                    NewLine();
                }
                // 中文字符的处理，这里假设你有某种方式来识别中文字符
                else if (IsChineseCharacter(ch))
                {
                    mWord.Append(ch);
                    AddWord(); // 中文字符后直接添加单词，因为中文不使用空格
                }
                else if (char.IsWhiteSpace(ch))
                {
                    mWord.Append(' ');
                    AddWord();
                }
                else
                {
                    mWord.Append(ch);
                }
            }

            AddWord();
        }
        
        // 定义IsChineseCharacter函数来判断字符是否为中文
        bool IsChineseCharacter(char ch)
        {
            int code = (int)ch;
            return (code >= 0x4E00 && code <= 0x9FFF)
                   || (code >= 0x3400 && code <= 0x4DBF)
                   || (code >= 0x20000 && code <= 0x2A6DF)
                   || (code >= 0x2A700 && code <= 0x2B73F)
                   || (code >= 0x2B740 && code <= 0x2B81F)
                   || (code >= 0x2B820 && code <= 0x2CEAF)
                   || (code >= 0x2CEB0 && code <= 0x2EBEF);
        }


        //------------------------------------------------------------------------------

        public void Image(string url, string alt, string title)
        {
            var payload = new GUIContent();
            var content = new MDContentImage(payload, mStyle, mLink);

            content.URL = url;
            content.Alt = alt;
            payload.tooltip = !string.IsNullOrEmpty(title) ? title : alt;

            AddContent(content);
        }

        //------------------------------------------------------------------------------

        public void NewLine()
        {
            if (mCurrentContent != null && mCurrentContent.IsEmpty)
            {
                return;
            }

            NewContentBlock();
        }

        public void Space()
        {
            if (CurrentBlock is MDBlockSpace || CurrentBlock is MDBlockContainer)
            {
                return;
            }

            AddBlock(new MDBlockSpace(mIndent));
        }

        public void HorizontalLine()
        {
            if (CurrentBlock is MDBlockLine)
            {
                return;
            }

            AddBlock(new MDBlockLine(mIndent));
        }


        //------------------------------------------------------------------------------

        public void Indent()
        {
            NewLine();

            mIndent += mContext.IndentSize;

            if (mCurrentContent != null)
            {
                mCurrentContent.Indent = mIndent;
            }
        }

        public void Outdent()
        {
            NewLine();

            mIndent = Mathf.Max(mIndent - mContext.IndentSize, 0.0f);

            if (mCurrentContent != null)
            {
                mCurrentContent.Indent = mIndent;
            }
        }

        public void Prefix(string text, MDStyle style)
        {
            mContext.Apply(style);

            if (mCurrentContent == null)
            {
                return;
            }

            var payload = new GUIContent(text);
            var content = new MDContentText(payload, style, null);
            content.Location.size = mContext.CalcSize(payload);

            mCurrentContent.Prefix(content);
        }


        //------------------------------------------------------------------------------

        public void StartBlock(bool quoted)
        {
            Space();
            mCurrentContainer = AddBlock(new MDBlockContainer(mIndent) { Highlight = true, Quoted = quoted });
            CurrentBlock = null;
        }

        public void EndBlock()
        {
            mCurrentContainer.RemoveTrailingSpace();
            mCurrentContainer = mCurrentContainer.Parent as MDBlockContainer ?? mDocument;
            CurrentBlock = null;

            Space();
        }

        //------------------------------------------------------------------------------

        public void StartTable()
        {
            Space();
            mCurrentContainer = AddBlock(new MDBlockContainer(mIndent) { Quoted = false, Highlight = false });
            CurrentBlock = null;
        }

        public void EndTable()
        {
            mCurrentContainer.RemoveTrailingSpace();
            mCurrentContainer = mCurrentContainer.Parent as MDBlockContainer ?? mDocument;
            CurrentBlock = null;

            Space();
        }


        public void StartTableRow(bool isHeader)
        {
            mCurrentContainer = AddBlock(new MDBlockContainer(mIndent)
            {
                Quoted = false, Highlight = false, Horizontal = true, IsTableHeader = isHeader, IsTableRow = !isHeader
            });
            CurrentBlock = null;
        }

        public void EndTableRow()
        {
            mCurrentContainer.RemoveTrailingSpace();
            mCurrentContainer = mCurrentContainer.Parent as MDBlockContainer ?? mDocument;
            CurrentBlock = null;
        }

        ////////////////////////////////////////////////////////////////////////////////
        // private


        MDContext mContext;

        MDStyle mStyle;
        string mLink;
        string mTooltip;
        StringBuilder mWord;
        float mIndent;

        MDBlockContainer mDocument;
        MDBlockContainer mCurrentContainer;
        MDBlock mCurrentBlock;
        MDBlockContent mCurrentContent;

        MDBlock CurrentBlock
        {
            get { return mCurrentBlock; }

            set
            {
                mCurrentBlock = value;
                mCurrentContent = mCurrentBlock as MDBlockContent;
            }
        }


        //------------------------------------------------------------------------------

        public IMDLayoutBuilder(MDContext context)
        {
            mContext = context;

            mStyle = new MDStyle();
            mLink = null;
            mTooltip = null;
            mWord = new StringBuilder(1024);

            mIndent = 0.0f;

            mDocument = new MDBlockContainer(mIndent);
            mCurrentContainer = mDocument;
            mCurrentBlock = null;
            mCurrentContent = null;
        }

        public MDLayout GetLayout()
        {
            return new MDLayout(mContext, mDocument);
        }

        //------------------------------------------------------------------------------

        void AddContent(MDContent content)
        {
            if (mCurrentContent == null)
            {
                NewContentBlock();
            }

            mCurrentContent.Add(content);
        }

        T AddBlock<T>(T block) where T : MDBlock
        {
            CurrentBlock = mCurrentContainer.Add(block);
            return block;
        }

        void NewContentBlock()
        {
            AddBlock(new MDBlockContent(mIndent));

            mStyle.Clear();
            mContext.Apply(mStyle);
        }

        void AddWord()
        {
            if (mWord.Length == 0)
            {
                return;
            }

            var payload = new GUIContent(mWord.ToString(), mTooltip);
            var content = new MDContentText(payload, mStyle, mLink);
            content.CalcSize(mContext);

            AddContent(content);

            mWord.Length = 0;
        }
    }
}
#endif