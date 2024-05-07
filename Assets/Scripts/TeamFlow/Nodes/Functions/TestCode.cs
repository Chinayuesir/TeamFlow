using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenAI;
using RoslynCSharp;
using RoslynCSharp.Compiler;
using Sirenix.OdinInspector;
using UnityEngine;
using Tool = OpenAI.Tool;

namespace TeamFlow.Nodes
{
    [NodeTitle("代码测试", "自定义函数", SdfIconType.Compass)]
    public class TestCode : FunctionNode
    {
        [LabelText("程序集资源")]
        public AssemblyReferences_SO Assemblies_SO;
        
        private static List<AssemblyReferenceAsset> mAssemblyReferences;

        [Button("加载程序集")]
        private void LoadAssemblies()
        {
            mAssemblyReferences = new List<AssemblyReferenceAsset>();
            mAssemblyReferences = Assemblies_SO.AssemblyReferenceAssets;
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
            var domain = ScriptDomain.CreateDomain("Example");
            // Compile and load code
            foreach (AssemblyReferenceAsset reference in mAssemblyReferences)
            {
                domain.RoslynCompilerService.ReferenceAssemblies.Add(reference);
                Debug.Log(reference.name);
            }
            ScriptAssembly assembly = 
                domain.CompileAndLoadSource(code);
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