using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public static class Utils
{
    public static void Deconstruct<K, V>(this KeyValuePair<K, V> pair, out K key, out V value)
    {
        key = pair.Key;
        value = pair.Value;
    }

    public static float Avreage(this Vector3 vec)
    {
        return (vec.x + vec.y + vec.z) / 3f;
    }
    public static float SqrDistance(Vector3 v1, Vector3 v2)
    {
        return (v2 - v1).sqrMagnitude;
    }

    public static Color GetColorFromHtml(string html)
    {
        if (ColorUtility.TryParseHtmlString(html, out Color color))
        {
            return color;
        }
        return Color.magenta;
    }

    public static Vector3 Round(this Vector3 vector, int decimals)
    {
        float factor = Mathf.Pow(10, decimals);
        vector *= factor;
        vector.x = Mathf.Round(vector.x);
        vector.y = Mathf.Round(vector.y);
        vector.z = Mathf.Round(vector.z);
        return vector /= factor;
    }

    public static void RemoveAtSwapback<T>(this List<T> list, int index)
    {
        list[index] = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
    }
}
