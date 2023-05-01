using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

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

    public static float Range(this System.Random random, float min, float max)
    {
        return (float)(min + (max - min) * random.NextDouble());
    }

    public static int Range(this System.Random random, int min, int maxExclusive)
    {
        return random.Next(min, maxExclusive);
    }

    public static float Value(this System.Random random)
    {
        return (float)random.NextDouble();
    }

    public static void Localize(this string key, string table, TextMeshProUGUI label, params object[] parameters)
    {
        var op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, key, parameters);
        if (op.IsDone)
        {
            label.text = op.Result;
        }
        else
        {
            op.Completed += (op) => label.text = op.Result;
        }
    }

    public static void LocalizePotion(this Potion pData, TextMeshProUGUI label)
    {
        string[] descs = new string[] { string.Empty, string.Empty, string.Empty };

        var potionOfOp = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("General", "potion_of");
        string potionOf = string.Empty;
        if (potionOfOp.IsDone)
        {
            potionOf = potionOfOp.Result;
        }
        else
        {
            potionOfOp.Completed += (op) =>
            {
                potionOf = op.Result;
            };
        }

        for (int i = 0; i < 3; i++)
        {
            var op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(
                $"PotionDesc{i + 1}", 
                DataController.GetIngredientName(pData.ingredients[pData.titleIDs[i]]));
            if (op.IsDone)
            {
                descs[i] = op.Result;
                label.text = $"{descs[0]} {potionOf} {descs[1]} {descs[2]}";
            }
            else
            {
                op.Completed += (op) =>
                {
                    descs[i] = op.Result;
                    label.text = $"{descs[0]} {potionOf} {descs[1]} {descs[2]}";
                };
            }
        }

    }
}
