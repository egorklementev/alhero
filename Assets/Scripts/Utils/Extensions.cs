using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static int Hash(this string some)
    {
        return some.GetHashCode();
    }
    
    public static void Err(this string msg, object obj = null, string method = "", params System.Type[] attrs)
    {
        Debug.LogError($"[{obj?.GetType()}] => {obj?.GetType().GetMethod(method, attrs)}: " + msg);
    }

    public static void Warn(this string msg, object obj = null, string method = "", params System.Type[] attrs)
    {
        Debug.LogWarning($"[{obj?.GetType()}] => {obj?.GetType().GetMethod(method, attrs)}: " + msg);
    }

    public static void Log(this string msg, object obj = null, string method = "", params System.Type[] attrs)
    {
        Debug.Log($"[{obj?.GetType()}] => {obj?.GetType().GetMethod(method, attrs)}: " + msg);
    }

    public static T Last<T>(this List<T> list, int index = 1)
    {
        return list[list.Count - index];
    }

    public static void RemoveLast<T>(this List<T> list)
    {
        list.RemoveAt(list.Count - 1);
    }
}
