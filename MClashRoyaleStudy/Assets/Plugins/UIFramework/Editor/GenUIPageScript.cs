using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// 使用规则:
///     必须将UI prefab名称修改为xxPage, 这样才可以生成partial类脚本UIPage UIView,
///     并且在UI框架中能够使用（通过名字定位UI脚本）。
///     UIView中用来后续添加代码, 生成时会覆盖之前的所写的代码, 所以要小心处理(本
///     框架提供了两次确认保护)
/// </summary>
public class GenUIPageScript
{
    public static string SCRIPT_GEN_PATH = "Assets/Scripts/UIScripts";
    public static string SCRIPT_TEMPLATE_PATH = "Assets/Plugins/UIFramework/Template";
    //public static string UI_ROOT_PATH = ""; //NB: Addressables管理的资源文件不需要根路径. 把UI资源做下名称简化保持[资源名==根对象名]即可,但resources管理的文件不行
    public static string UIPAGE_CLASS_DEF = File.ReadAllText(SCRIPT_TEMPLATE_PATH + "/UIPageTemplate.cs.txt", Encoding.UTF8);
    public static string VIEW_CLASS_DEF = File.ReadAllText(SCRIPT_TEMPLATE_PATH + "/UIViewTemplate.cs.txt", Encoding.UTF8);
    public class UIFieldInfo
    {
        #region Declaration
        public string fieldType;
        public string fieldName;
        #endregion
        #region initialization
        public string fieldPath;
        #endregion
        public override string ToString()
        {
            return $"{fieldType} {fieldName}=>{fieldPath}";
        }
    }

    private static string GetFieldType(WidgetID w)
    {
        MonoBehaviour mono;
        if ((mono = w.GetComponents<WidgetID>().FirstOrDefault(x => x != w)) != null)
        {
            return mono.GetType().Name;
        }
        else if ((mono = w.GetComponent<Selectable>()) != null)
        {
            return mono.GetType().Name;
        }
        else if ((mono = w.GetComponent<Graphic>()) != null)
        {
            return mono.GetType().Name;
        }
        else
        {
            return "Transform";
        }
    }

    public static void _Gen(Transform tr, string path, List<UIFieldInfo> list)
    {
        if (tr == null)
        {
            return;
        }
        foreach (Transform c in tr)
        {
            string cPath = (path == string.Empty) ? c.name : path + "/" + c.name;
            var widgets = c.GetComponents<WidgetID>();
            foreach (var w in widgets)
            {
                if (w && w.GetType() == typeof(WidgetID) && w.ignore == false)
                {
                    string fname = c.name.InitialLower();
                    var fieldInfo = list.Find(x => x.fieldName == fname);
                    if (fieldInfo != null)
                    {
                        fname = "_" + c.FullPath();
                    }
                    list.Add(
                            new UIFieldInfo()
                            {
                                fieldName = fname,
                                fieldPath = cPath,
                                fieldType = GetFieldType(w)
                            }
                        );
                    break;
                }
            }
            _Gen(c, cPath, list);
        }
    }
    [MenuItem("GameObject/MCUIFramework/Create UI Page Script", priority = 0)] //priority权重
    public static void Gen()
    {
        if (Selection.activeTransform.parent.gameObject.name != "Canvas (Environment)")
        {
            Debug.LogError("生成UIPage脚本失败，检查以下条件是否满足：1、UI必须是预制体；2、命令执行在预制体根节点上——父节点必须是Canvas (Environment)");
            return;
        }

        var fieldList = new List<UIFieldInfo>();
        _Gen(Selection.activeTransform, string.Empty, fieldList);

        StringBuilder sbFieldDef = new StringBuilder();
        StringBuilder sbFieldInit = new StringBuilder();
        foreach (var field in fieldList)
        {
            sbFieldDef.AppendLine($"\tpublic {field.fieldType} {field.fieldName};");
            sbFieldInit.AppendLine($"\t\t{field.fieldName} = uitransform.Find(\"{field.fieldPath}\").GetComponent<{field.fieldType}>();");
        }
        string sPage = UIPAGE_CLASS_DEF
            .Replace("{ROOT_UI_NAME}", Selection.activeGameObject.name)
            .Replace("{UI_WIDGET_FIELD_LIST}", sbFieldDef.ToString())
            .Replace("{FIELD_INITIALIZATION_LIST}", sbFieldInit.ToString())
            .Replace("{UI_PATH}", /*UI_ROOT_PATH + "/" +*/ Selection.activeGameObject.name)
            ;
        string sView = VIEW_CLASS_DEF
            .Replace("{ROOT_UI_NAME}", Selection.activeGameObject.name);

        string scriptPath = SCRIPT_GEN_PATH + "/" + Selection.activeGameObject.name + ".cs";
        if (File.Exists(scriptPath))
            File.Delete(scriptPath);
        File.WriteAllText(scriptPath, sPage, Encoding.UTF8);

        string viewPath = SCRIPT_GEN_PATH + "/" + Selection.activeGameObject.name.Replace("Page", "View") + ".cs";
        Debug.Log(viewPath);

        // NB: 视图文件不能自动删除并重建，因为可能已经写了很多代码了
        if (File.Exists(viewPath) == false || 
                (
                    EditorUtility.DisplayDialog("文件已存在，是否覆盖？", $"File Name: {viewPath}", "是", "否")
                    && 
                    EditorUtility.DisplayDialog("文件已存在，是否覆盖？", $"File Name: {viewPath}", "是", "否")
                )
            )
        {
            Debug.Log(sView);
            File.WriteAllText(viewPath, sView, Encoding.UTF8);
        }

        AssetDatabase.Refresh();
    }

}

