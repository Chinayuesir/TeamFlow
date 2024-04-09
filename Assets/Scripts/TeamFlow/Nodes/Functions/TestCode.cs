using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OpenAI;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Tool = OpenAI.Tool;

namespace TeamFlow.Nodes
{
    [NodeTitle("代码测试","自定义函数",SdfIconType.Compass)]
    public class TestCode:FunctionNode
    {
        protected override void Init()
        {
            ToolFromFunc = Tool.GetOrCreateTool(typeof(TestCode), nameof(HandleCode));
        }
       
        // TaskCompletionSource用于在异步编译完成后返回结果
        private static TaskCompletionSource<string> compilationTask;

        [Function("测试代码，包括代码的提取、编译，并返回编译中发生的错误")]
        public static async Task<string> HandleCode(
            [FunctionParameter("你打算处理的代码片段")] string code)//,
            //[FunctionParameter("脚本的名称")]string scriptName)
        {
            // 初始化TaskCompletionSource
            compilationTask = new TaskCompletionSource<string>();
            
            // 订阅编译完成事件
            CompilationPipeline.assemblyCompilationFinished += OnCompilationFinished;

            // 定义临时文件的路径
            string tempFilePath = "Assets/TempScript.cs";

            // 将代码写入临时文件
            File.WriteAllText(tempFilePath, code);

            // 强制Unity重新编译
            AssetDatabase.Refresh();

            // 等待编译完成事件的回调函数设置Task的结果
            string result = await compilationTask.Task;

            // 返回编译结果
            return result;
        }

        private static void OnCompilationFinished(string assembly, CompilerMessage[] messages)
        {
            StringBuilder errors = new StringBuilder();

            // 遍历编译信息
            foreach (var message in messages)
            {
                if (message.type == CompilerMessageType.Error)
                {
                    errors.AppendLine(message.message);
                }
            }

            if (errors.Length > 0)
            {
                // 如果有错误，设置Task的结果为错误信息
                compilationTask.SetResult(errors.ToString());
            }
            else
            {
                // // 如果没有错误，保存代码到本地文件中
                // SaveCodeToFile($"Assets/Scripts/Test.cs");

                // 设置Task的结果为成功消息
                compilationTask.SetResult("Code compiled successfully!");
            }

            // 移除事件订阅，避免重复调用
            CompilationPipeline.assemblyCompilationFinished -= OnCompilationFinished;

            // 删除临时文件和meta文件
            DeleteTempScript();
        }

        //// 将代码保存到本地文件的方法
        // private static void SaveCodeToFile(string filePath)
        // {
        //     // 读取临时文件中的代码
        //     string code = File.ReadAllText("Assets/TempScript.cs");
        //
        //     // 将代码写入指定的文件
        //     File.WriteAllText(filePath, code);
        // }

        private static void DeleteTempScript()
        {
            // 定义临时文件的路径
            string tempFilePath = "Assets/TempScript.cs";

            // 删除临时文件
            File.Delete(tempFilePath);

            // 删除临时文件的meta文件
            File.Delete($"{tempFilePath}.meta");

            // 再次强制Unity刷新，确保临时文件已经被完全删除
            AssetDatabase.Refresh();
        }
    }
}