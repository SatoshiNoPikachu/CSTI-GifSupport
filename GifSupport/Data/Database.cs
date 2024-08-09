using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace GifSupport.Data;

/// <summary>
/// 数据库
/// </summary>
public static class Database
{
    /// <summary>
    /// 数据字典
    /// </summary>
    private static readonly Dictionary<Type, Dictionary<string, object>> AllData = new();

    /// <summary>
    /// 获取数据字典
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>数据字典</returns>
    public static Dictionary<string, T> GetData<T>()
    {
        var type = typeof(T);
        return AllData.TryGetValue(type, out var dict)
            ? dict.Where(pair => pair.Value is T).ToDictionary(pair => pair.Key, pair => (T)pair.Value)
            : null;
    }

    /// <summary>
    /// 获取数据字典
    /// </summary>
    /// <param name="type">数据类型</param>
    /// <returns></returns>
    public static Dictionary<string, object> GetData(Type type)
    {
        if (type is null) return null;
        return AllData.TryGetValue(type, out var dict) ? dict : null;
    }

    /// <summary>
    /// 根据键获取数据
    /// </summary>
    /// <param name="key">数据键</param>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>数据</returns>
    public static T GetData<T>(string key)
    {
        var dict = GetData(typeof(T));
        if (dict is null) return default;
        if (!dict.TryGetValue(key, out var data)) return default;

        return data is T obj ? obj : default;
    }

    /// <summary>
    /// 根据多个键获取数据
    /// </summary>
    /// <param name="keys">可迭代的数据键序列</param>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>数据列表</returns>
    public static List<T> GetData<T>(IEnumerable<string> keys)
    {
        var dict = GetData(typeof(T));
        if (dict is null) return null;
        var list = new List<T>();
        foreach (var key in keys)
        {
            if (!dict.TryGetValue(key, out var data)) continue;

            if (data is T obj)
            {
                if (obj is Object unityObj)
                {
                    list.Add(unityObj ? obj : default);
                }
                else
                {
                    list.Add(obj);
                }
            }
            else
            {
                list.Add(default);
            }
        }

        return list;
    }

    /// <summary>
    /// 添加数据
    /// </summary>
    /// <param name="type">数据类型</param>
    /// <param name="objects">数据对象字典</param>
    public static void AddData(Type type, Dictionary<string, object> objects)
    {
        AllData[type] = objects;
    }

    /// <summary>
    /// 清空数据
    /// </summary>
    public static void Clear()
    {
        foreach (var obj in AllData.Values.SelectMany(data => data.Values))
        {
            if (obj is Object unityObj) Object.Destroy(unityObj);
        }

        AllData.Clear();
    }
}