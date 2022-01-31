using UnityEngine;

public static class Extensions
{
    public static int Hash(this string some)
    {
        return some.GetHashCode();
    }
    
    public static void Err<T>(this string msg, string method, params System.Type[] attrs)
    {
        Debug.LogError($"[{typeof(T)}] => {typeof(T).GetMethod(method, attrs)}: " + msg);
    }

    public static void Warn<T>(this string msg, string method, params System.Type[] attrs)
    {
        Debug.LogWarning($"[{typeof(T)}] => {typeof(T).GetMethod(method, attrs)}: " + msg);
    }

    public static void Log<T>(this string msg, string method, params System.Type[] attrs)
    {
        Debug.Log($"[{typeof(T)}] => {typeof(T).GetMethod(method, attrs)}: " + msg);
    }
}
