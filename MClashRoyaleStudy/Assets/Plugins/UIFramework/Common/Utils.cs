using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
/// <summary>
/// 静态工具类, 储存一些小工具, 拓展方法
/// </summary>
public static class Utils 
{
    /// <summary>
    /// string类 拓展方法 用于初始化字符串使得字符全转化为小写字符
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string InitialLower(this string s) 
    {
        StringBuilder sb = new StringBuilder(s);
        sb[0] = char.ToLower(sb[0]);
        return sb.ToString();
    }

    /// <summary>
    /// string类 拓展方法 用于提供Transform一层一层用斜杠分割的名称字符串
    /// </summary>
    /// <param name="tr"></param>
    /// <returns></returns>
    public static string FullPath(this Transform tr) 
    {
        string s = "";
        do
        {
            s = s + "/" + tr.name;
            tr = tr.parent;
        } while (tr != null);
        return s;
    }
}
