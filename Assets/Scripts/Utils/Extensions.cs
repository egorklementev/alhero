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
        string objName = "";
        if (obj is MonoBehaviour mono)
        {
            objName = mono.gameObject.name + "-";
        }
        
        Debug.LogError($"[{objName}{obj?.GetType()}] => {obj?.GetType().GetMethod(method, attrs)}: " + msg);
    }

    public static void Warn(this string msg, object obj = null, string method = "", params System.Type[] attrs)
    {
        string objName = "";
        if (obj is MonoBehaviour mono)
        {
            objName = mono.gameObject.name + "-";
        }

        Debug.LogWarning($"[{objName}{obj?.GetType()}] => {obj?.GetType().GetMethod(method, attrs)}: " + msg);
    }

    public static void Log(this string msg, object obj = null, string method = "", params System.Type[] attrs)
    {
        string objName = "";
        if (obj is MonoBehaviour mono)
        {
            objName = mono.gameObject.name + "-";
        }

        Debug.Log($"[{objName}{obj?.GetType()}] => {obj?.GetType().GetMethod(method, attrs)}: " + msg);
    }

    public static T Last<T>(this List<T> list, int index = 1)
    {
        return list[list.Count - index];
    }

    public static void RemoveLast<T>(this List<T> list)
    {
        list.RemoveAt(list.Count - 1);
    }

    public static bool FindNearestName(this Transform t, string name, out Transform result)
    {
        // TODO: make this algorithm using closest string
        foreach (Transform child in t)
        {
            if (child.name.Contains(name))
            {
                result = child;
                return true;
            }
            
        }
        
        result = null;
        return false;
    }
}
