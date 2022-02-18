using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Data;
using Excel;

public class Table2Script
{
    public enum TypeHint 
    {
        Enum,
        Array
    }
    //这些常量的意义?
    public const int META_NAME = 0;
    public const int META_VALUE = 1;
    public const int COMMET = 3;
    public const int FIELD_NAME = 4;
    public const int FIELD_TYPE = 5;
    public const int NAMESPACES = 2;
    public const int FIELD_HINT = 6;
    public const int DATA_ROW_START = 7;
    public const string CLASS_DEF = @"
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
{NAMESPACE_LIST}

[Serializable]
public partial class {ROW_NAME}
{
{FIELD_LIST}
}

[Serializable]
public partial class {CLASS_NAME}
{
    public List<{ROW_NAME}> list = new List<{ROW_NAME}>();
    public {CLASS_NAME}() 
    {
        {ROW_LIST}
    }
    public static {CLASS_NAME} instance = new {CLASS_NAME}();
}
";

    public static string[][] LoadCSV(string pathName) 
    {
        string text = File.ReadAllText(pathName);
        string[] lines = text.Split(new string[] { "\r\n"},StringSplitOptions.RemoveEmptyEntries);
        Debug.Log(lines.Length);
        string[][] data = new string[lines.Length][];
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            data[i] = SplitCSV(line);
        }
        return data;
    }

    static string[][] LoadXls(string filePath)
    {
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        IExcelDataReader excelReader = null;
        var fi = new FileInfo(filePath);
        if (fi.Extension == ".xls")
        {
            excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
        }
        else if (fi.Extension == ".xlsx")
        {
            excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        }
        else
        {
            Debug.Log("无法读取非Excel文件");
            return null;
        }

        DataSet result = excelReader.AsDataSet();
        //Tables[0] 下标0表示excel文件中第一张表的数据
        int columnNum = result.Tables[0].Columns.Count;
        int rowNum = result.Tables[0].Rows.Count;
        DataRowCollection coll = result.Tables[0].Rows;

        string[][] data = new string[rowNum][]; // 支持锯齿数组
        for (int row = 0; row < rowNum; row++)
        {
            int colNum = coll[row].ItemArray.Length;
            string[] cols = new string[colNum];
            //Debug.Log(line);
            for (int col = 0; col < colNum; col++)
            {
                cols[col] = coll[row].ItemArray[col].ToString();
            }
            data[row] = cols;
        }

        return data;
    }


    private static string[] SplitCSV(string s)
    {
        Regex regex = new Regex("\".*?\"");
        var a = regex.Matches(s).Cast<Match>().Select(m => m.Value).ToList(); // a存放所有带双引号的字符串
        var b = regex.Replace(s, "%_%"); // b是从s抽取的双引号之外的字符串
        var c = b.Split(','); // 把b用逗号分割好
                              // 处理双引号字符串
        for (int i = 0, j = 0; i < c.Length && j < a.Count; i++)
        {
            if (c[i] == "%_%") // 遇到双引号切割符就知道一个双引号字符串结束了，于是加入c
            {
                c[i] = a[j++].Replace("\"", ""); // 加入前去掉双引号
            }
        }
        return c;
    }

    [MenuItem("Tools/CSVToScript")]
    public static void _Csv2Script() 
    {
        string inPath = Application.dataPath + "/DataGenerate/ConfigTables";
        string outPath = Application.dataPath + "/DataGenerate/ConfigScripts";
        foreach (var fname in Directory.EnumerateFiles(inPath, "*.csv"))
        {
            string[][] data = LoadCSV(fname);
            if (data.Length == 0)
            {
                continue;
            }
            GenerateScript(data, outPath);
        }
        AssetDatabase.Refresh();
        Debug.Log("代码生成完毕");
    }


    [MenuItem("Tools/ExcelToScript")]
    public static void _Xls2Script()
    {
        string inPath = Application.dataPath + "/DataGenerate/ConfigTables";
        string outPath = Application.dataPath + "/DataGenerate/ConfigScripts";
        foreach (string fname in Directory.EnumerateFiles(inPath, "*.xls"))
        {
            string[][] data = LoadXls(fname);
            GenerateScript(data, outPath);
        }
        AssetDatabase.Refresh();
        Debug.Log("代码生成完毕");
    }



    static void GenerateScript(string[][] data, string path) 
    {
        string className = data[META_VALUE][1];
        Debug.Log(className);
        string rowName = data[META_VALUE][2];
        StringBuilder rowList = new StringBuilder();
        for (int row = DATA_ROW_START; row < data.Length; row++)
        {
            StringBuilder fieldValues = new StringBuilder();
            for (int col = 1; col < data[row].Length; col++)
            {
                string fieldHint = data[FIELD_HINT][col];
                string fieldName = data[FIELD_NAME][col];
                string fieldType = data[FIELD_TYPE][col];
                string fieldValue = data[row][col];
                fieldValue = ProcessFieldValueByHint(fieldValue, fieldType, fieldHint);
                fieldValues.AppendLine($"\t\t\t{fieldName} = {fieldValue},");
            }
            rowList.AppendLine($"\t\tlist.Add(new {rowName}(){{\r\n{fieldValues}\t\t}});\r\n");
        }
        StringBuilder fieldDefs = new StringBuilder();
        for (int col = 1; col < data[FIELD_NAME].Length; col++)
        {
            string fieldHint = data[FIELD_HINT][col];
            fieldDefs.AppendLine($"\t\tpublic {data[FIELD_TYPE][col]}{(fieldHint.Contains("Array") ? "[]" : "")} {data[FIELD_NAME][col]};\r\n");
        }

        StringBuilder namespaceList = new StringBuilder();
        for (int col = 1; col < data[NAMESPACES].Length; col++)
        {
            if (string.IsNullOrEmpty(data[NAMESPACES][col]))
                continue;
            namespaceList.AppendLine($"using {data[NAMESPACES][col]};\r\n");
        }

        StringBuilder classImpl = new StringBuilder(CLASS_DEF);
        classImpl.Replace("{CLASS_NAME}", className);
        classImpl.Replace("{FIELD_LIST}", fieldDefs.ToString());
        classImpl.Replace("{ROW_NAME}", rowName);
        classImpl.Replace("{ROW_LIST}", rowList.ToString());
        classImpl.Replace("{NAMESPACE_LIST}", namespaceList.ToString());

        string script = classImpl.ToString();
        string fname = path + $"/{className}.cs";
        File.Delete(fname);//删除之前的
        File.WriteAllText(fname, script);//创建新的
    }
    static string ProcessFieldValueByHint(string fieldValue, string fieldType, string fieldHint) 
    {
        switch (fieldHint)
        {
            case "Enum":
                return $"{fieldType}.{fieldValue}";
            case "ValArray":
                {
                    string[] elems = fieldValue.Split('|');
                    string sval = "";
                    for (int i = 0; i < elems.Length; i++)
                    {
                        if (i != 0)
                        {
                            sval += ", ";
                        }
                        sval += ProcessFieldValueByType(elems[i],fieldType);
                    }
                    return $"new []{{{sval}}}";//?
                }
            case "RefArray": 
                {
                    string[] elems = fieldValue.Split('|');

                    string sVal = "";
                    for (int i = 0; i < elems.Length; i++)
                    {
                        if (i != 0)
                            sVal += ", ";
                        sVal += $"new {fieldType}({ProcessFieldValueByType(elems[i], fieldType)})";
                    }
                    return $"new []{{{sVal}}}";
                }
            default:
                return ProcessFieldValueByType(fieldValue,fieldType);
        }
    }

    static string ProcessFieldValueByType(string fieldValue,string fieldType) 
    {
        switch (fieldType)
        {
            case "string":
                return $"\"{fieldValue}\"";
            case "float":
                return $"{fieldValue}f";
            case "Vector3":
                return Regex.Replace(fieldValue, @"([+|-]?\d+\.?\d*),\s*([+|-]?\d+\.?\d*),\s*([+|-]?\d+\.?\d*)", @"$1f, $2f, $3f");
        }
        return fieldValue;
    }


}