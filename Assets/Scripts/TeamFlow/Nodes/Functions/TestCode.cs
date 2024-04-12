using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OpenAI;
using RoslynCSharp;
using RoslynCSharp.Compiler;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Tool = OpenAI.Tool;

namespace TeamFlow.Nodes
{
    [NodeTitle("代码测试", "自定义函数", SdfIconType.Compass)]
    public class TestCode : FunctionNode
    {
        [LabelText("需要引用的程序集资源")]
        public List<AssemblyReferenceAsset> AssemblyReferenceAssets;
        
        private static AssemblyReferenceAsset[] mAssemblyReferences;

        [Button("加载以上所有程序集")]
        private void LoadAssemblies()
        {
            mAssemblyReferences = AssemblyReferenceAssets.ToArray();
            Debug.Log("加载成功！");
        }

        protected override void Init()
        {
            ToolFromFunc = Tool.GetOrCreateTool(typeof(TestCode), nameof(HandleCode));
        }

        // TaskCompletionSource用于在异步编译完成后返回结果
        private static TaskCompletionSource<string> compilationTask;

        [Function("测试代码，包括代码的提取、编译，并返回编译中发生的错误")]
        public static async Task<string> HandleCode(
                [FunctionParameter("你打算处理的代码片段")] string code) //,
            //[FunctionParameter("脚本的名称")]string scriptName)
        {
            Debug.Log($"助手提取到了代码，代码为：{code}");
            
            StringBuilder errors = new StringBuilder();
            var domain = ScriptDomain.CreateDomain("Example Domain");
            // Compile and load code
            ScriptAssembly assembly = 
                domain.CompileAndLoadSource(code, ScriptSecurityMode.UseSettings,mAssemblyReferences);
            
            
            foreach (AssemblyReferenceAsset reference in mAssemblyReferences)
                domain.RoslynCompilerService.ReferenceAssemblies.Add(reference);
            // Check for compiler errors
            if(domain.CompileResult.Success == false)
            {
                // Get all errors
                foreach(CompilationError error in domain.CompileResult.Errors)
                {
                    if(error.IsError == true)
                    {
                        errors.AppendLine(error.Message);
                        Debug.LogError(error.ToString());
                    }
                    else if(error.IsWarning == true)
                    {
                        Debug.LogWarning(error.ToString());
                    }
                }
            }

            if (errors.Length != 0)
                return await Task.FromResult(errors.ToString());
            else return await Task.FromResult("编译成功无错误");
        }
    }
}