using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace RoslynCSharp.Editor
{
    [CustomEditor(typeof(AssemblyReferenceAsset))]
    public class AssemblyReferenceAssetInspector : UnityEditor.Editor
    {
        // Methods
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Get instance
            AssemblyReferenceAsset asset = target as AssemblyReferenceAsset;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUIStyle style = new GUIStyle(EditorStyles.largeLabel);
                style.alignment = TextAnchor.MiddleCenter;

                GUILayout.Label("Assembly Info", style);

                // Line
                Rect area = GUILayoutUtility.GetLastRect();
                area.y += EditorGUIUtility.singleLineHeight + 5;
                area.height = 2;

                EditorGUI.DrawRect(area, new Color(0.2f, 0.2f, 0.2f, 0.4f));
                GUILayout.Space(10);

                EditorGUI.BeginDisabledGroup(true);
                {
                    // Assembly name
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("assemblyName"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("assemblyPath"));

                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.PrefixLabel("Last Write Time");
                        EditorGUILayout.TextField((asset.IsValid == true)
                            ? asset.LastWriteTime.ToString()
                            : string.Empty);
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndVertical();


            int widthStretch = 310;

            // Button layout
            GUILayout.Space(10);
            if (Screen.width > widthStretch)
                GUILayout.BeginHorizontal();

            // Select assembly button
            if (GUILayout.Button("Select Assembly File", GUILayout.Height(30)) == true)
            {
                string path = EditorUtility.OpenFilePanel("Open Assembly File", "Assets", "dll");

                if (string.IsNullOrEmpty(path) == false)
                {
                    // Check for file exists
                    if (File.Exists(path) == false)
                    {
                        Debug.LogError("Assembly file does not exist: " + path);
                        return;
                    }

                    // Use relative path if possible
                    string relativePath = path.Replace('\\', '/');
                    relativePath = FileUtil.GetProjectRelativePath(relativePath);

                    if (string.IsNullOrEmpty(relativePath) == false && File.Exists(relativePath) == true)
                        path = relativePath;

                    // Set file path
                    asset.UpdateAssemblyReference(path, Path.GetFileNameWithoutExtension(path));

                    // Mark as dirty
                    EditorUtility.SetDirty(asset);
                }
            }

            if (GUILayout.Button("Select Loaded Assembly", GUILayout.Height(30)) == true)
            {
                GenericMenu menu = new GenericMenu();

                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string menuName = asm.FullName;

                    if (menuName.StartsWith("Unity") == true)
                        menuName = "Untiy Assemblies/" + menuName;
                    else if (menuName.StartsWith("System") == true)
                        menuName = "System Assemblies/" + menuName;

                    menu.AddItem(new GUIContent(menuName), false, (object value) =>
                    {
                        // Get the selected assembly
                        Assembly selectedAsm = (Assembly) value;

                        // Check for location
                        if (string.IsNullOrEmpty(selectedAsm.Location) == true ||
                            File.Exists(selectedAsm.Location) == false)
                        {
                            Debug.LogError(
                                "The selectged assembly could not be referenced because its source location could not be determined. Please add the assembly using the full path!");
                            return;
                        }

                        string path = selectedAsm.Location;

                        // Use relative path if possible
                        string relativePath = path.Replace('\\', '/');
                        relativePath = FileUtil.GetProjectRelativePath(relativePath);

                        if (string.IsNullOrEmpty(relativePath) == false && File.Exists(relativePath) == true)
                            path = relativePath;

                        // Update the assembly
                        asset.UpdateAssemblyReference(path, selectedAsm.FullName);

                        // Mark as dirty
                        EditorUtility.SetDirty(asset);
                    }, asm);
                }

                // SHow the menu
                menu.ShowAsContext();
            }

            if (GUILayout.Button("选择程序集", GUILayout.Height(30)) == true)
            {
                // 获取所有程序集
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                // 创建一个 GenericSelector 来选择程序集
                GenericSelector<Assembly> assemblySelector = new GenericSelector<Assembly>("选择程序集", false,
                    (Assembly assembly) =>
                    {
                        // 返回程序集的完整名称
                        return assembly.FullName;
                    }, assemblies);

                // 设置选择器配置
                assemblySelector.SelectionTree.Config.DrawSearchToolbar = true; // 在选择器中添加搜索栏
                assemblySelector.SelectionTree.Config.AutoFocusSearchBar = true; // 自动聚焦搜索栏
                assemblySelector.SelectionTree.Config.ConfirmSelectionOnDoubleClick = true; // 双击确认选择

                // 分类设置
                assemblySelector.SelectionTree.SortMenuItemsByName(); // 菜单项按字母排序
                assemblySelector.SelectionTree.AddRange(assemblies, (Assembly assembly) =>
                {
                    // 根据程序集的名称分类
                    if (assembly.FullName.Contains("Unity"))
                    {
                        return "Unity Assemblies/" + assembly.FullName;
                    }
                    else if (assembly.FullName.Contains("System"))
                    {
                        return "System Assemblies/" + assembly.FullName;
                    }
                    else if (assembly.FullName.Contains("Microsoft"))
                    {
                        return "Microsoft Assemblies/" + assembly.FullName;
                    }
                    else
                    {
                        return "Other Assemblies/" + assembly.FullName;
                    }
                });

                // 当选择确认后的处理
                assemblySelector.SelectionConfirmed += selectedItems =>
                {
                    Assembly selectedAsm = selectedItems.FirstOrDefault();
                    if (selectedAsm != null)
                    {
                        string path = selectedAsm.Location;
                        if (string.IsNullOrEmpty(path) || !File.Exists(path))
                        {
                            Debug.LogError(
                                "The selected assembly could not be referenced because its source location could not be determined. Please add the assembly using the full path!");
                            return;
                        }

                        string relativePath = FileUtil.GetProjectRelativePath(path.Replace('\\', '/'));
                        if (!string.IsNullOrEmpty(relativePath) && File.Exists(relativePath))
                            path = relativePath;

                        asset.UpdateAssemblyReference(path, selectedAsm.FullName);
                        EditorUtility.SetDirty(asset);
                    }
                };

                // 显示选择器
                Vector2 mousePosition = Event.current.mousePosition;
                assemblySelector.ShowInPopup(mousePosition);
            }

            if (Screen.width > widthStretch)
                GUILayout.EndHorizontal();

            // Check for valid
            if (asset.IsValid == false)
            {
                EditorGUILayout.HelpBox(
                    "The assembly reference is not valid. Select a valid assembly path to reference",
                    MessageType.Warning);
            }
            else if (File.Exists(asset.AssemblyPath) == false)
            {
                EditorGUILayout.HelpBox(
                    "The assembly path does not exists. Referencing will still work but any changes to the assembly will not be detected! Consider selecting a valid assembly path",
                    MessageType.Warning);
            }
        }
    }
}