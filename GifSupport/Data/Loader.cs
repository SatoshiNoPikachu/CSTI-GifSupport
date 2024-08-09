using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HarmonyLib;
using LitJson;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GifSupport.Data;

/// <summary>
/// 加载器
/// </summary>
public static class Loader
{
    /// <summary>
    /// 加载完成事件
    /// </summary>
    public static event Action LoadCompleteEvent;

    /// <summary>
    /// 加载全部数据
    /// </summary>
    /// <param name="infos">数据信息</param>
    public static void LoadAllData(IEnumerable<DataInfo> infos)
    {
        Database.Clear();
        
        GifLoader.LoadAllGif();

        var warpData = new List<(ScriptableObject, JsonData)>();
        foreach (var info in infos)
        {
            var data = LoadData(info);
            if (data is null) continue;
            warpData.AddRange(data);
        }

        WarpData(warpData);

        LoadCompleteEvent?.Invoke();
    }

    /// <summary>
    /// 加载数据
    /// </summary>
    /// <param name="info">数据信息</param>
    /// <returns>映射数据</returns>
    private static IEnumerable<(ScriptableObject, JsonData)> LoadData(DataInfo info)
    {
        var type = info.Type;
        if (!type.IsSubclassOf(typeof(ScriptableObject))) return null;

        var dataName = info.Name;
        var isWarp = info.IsNeedWarp;

        var dict = new Dictionary<string, object>();

        var allScriptableObjectWithoutGuidTypeDict = ModLoader.ModLoader.AllScriptableObjectWithoutGuidTypeDict;
        var allGuidTypeDict = ModLoader.ModLoader.AllGUIDTypeDict;
        var allGuidDict = ModLoader.ModLoader.AllGUIDDict;

        var isUidObj = type.IsSubclassOf(typeof(UniqueIDScriptable));
        if (isUidObj)
        {
            if (allGuidTypeDict.TryGetValue(type, out var value))
            {
                value.Clear();
            }
            else
            {
                allGuidTypeDict.Add(type, new Dictionary<string, UniqueIDScriptable>());
            }
        }
        else
        {
            if (allScriptableObjectWithoutGuidTypeDict.TryGetValue(type, out var value))
            {
                value.Clear();
            }
            else
            {
                allScriptableObjectWithoutGuidTypeDict.Add(type, new Dictionary<string, ScriptableObject>());
            }
        }

        var modDirs = new DirectoryInfo(BepInEx.Paths.PluginPath).GetDirectories();
        var topName = Path.GetDirectoryName(Plugin.PluginPath);
        var cache = new Dictionary<string, (ScriptableObject, JsonData)>();

        foreach (var modDir in modDirs)
        {
            var path = isUidObj
                ? Path.Combine(modDir.FullName, dataName)
                : Path.Combine(modDir.FullName, "ScriptableObject", dataName);
            if (!Directory.Exists(path)) continue;

            var files = new DirectoryInfo(path).GetFiles("*.json");
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                if (dict.ContainsKey(name))
                {
                    if (modDir.Name == topName)
                    {
                        Plugin.Log.LogWarning($"{Plugin.PluginName} override {dataName} same key {name}.");
                    }
                    else
                    {
                        Plugin.Log.LogWarning($"{modDir.Name} not load {dataName} same key {name}.");
                        continue;
                    }
                }

                var json = File.ReadAllText(file.FullName, Encoding.UTF8);
                var obj = ScriptableObject.CreateInstance(type);
                JsonUtility.FromJsonOverwrite(json, obj);

                if (obj is UniqueIDScriptable uidObj)
                {
                    var uid = uidObj.UniqueID;
                    if (UniqueIDScriptable.AllUniqueObjects.TryGetValue(uid, out var registered))
                    {
                        if (modDir.Name == topName)
                        {
                            Plugin.Log.LogWarning($"{Plugin.PluginName} override {dataName} same uid {uid}.");
                            UniqueIDScriptable.AllUniqueObjects[uid] = uidObj;
                            Object.Destroy(registered);
                        }
                        else
                        {
                            Plugin.Log.LogWarning($"{modDir.Name} not load {dataName} same uid {uid}.");
                            Object.Destroy(obj);
                            continue;
                        }
                    }
                    else
                    {
                        uidObj.Init();
                    }

                    allGuidTypeDict[type][uid] = uidObj;
                    allGuidDict[uid] = uidObj;
                }

                obj.name = name;
                dict[name] = obj;

                var jsonData = JsonMapper.ToObject(json);
                FixData(obj, jsonData);

                if (isWarp) cache[name] = (obj, jsonData);

                if (!isUidObj) allScriptableObjectWithoutGuidTypeDict[type][name] = obj;
            }
        }

        Database.AddData(type, dict);

        return isWarp ? cache.Values.ToList() : null;
    }

    /// <summary>
    /// 修补数据
    /// </summary>
    /// <param name="obj">数据对象</param>
    /// <param name="jsonData">Json数据</param>
    private static void FixData(object obj, JsonData jsonData)
    {
        if (obj is null)
        {
            Plugin.Log.LogWarning("Cannot fix data, because object is null!");
            return;
        }
        
        var type = obj.GetType();

        if (jsonData.IsObject)
        {
            foreach (var fieldName in jsonData.Keys)
            {
                if (fieldName.EndsWith("WarpData") || fieldName.EndsWith("WarpType")) continue;

                var field = AccessTools.Field(type, fieldName);
                if (field is null) continue;
                var fieldType = field.FieldType;
                if (field.GetValue(obj) is not null || fieldType.IsSubclassOf(typeof(Object))) continue;

                var jsonField = jsonData[fieldName];

                if (jsonData[fieldName].IsArray)
                {
                    if (fieldType.IsArray)
                    {
                        var elementType = fieldType.GetElementType();

                        if (elementType is null)
                        {
                            Plugin.Log.LogWarning($"Unable get element type for {fieldType}");
                            continue;
                        }

                        var arr = Array.CreateInstance(elementType, jsonField.Count);
                        if (!elementType.IsSubclassOf(typeof(Object))) FixData(arr, jsonField);
                        field.SetValue(obj, arr);
                    }
                    else
                    {
                        var seq = AccessTools.CreateInstance(fieldType);
                        FixData(seq, jsonField);
                        field.SetValue(obj, seq);
                    }

                    continue;
                }

                var fieldValue = FromJson(jsonField, field.FieldType);
                field.SetValue(obj, fieldValue);
            }
        }
        else if (jsonData.IsArray)
        {
            if (type.IsArray)
            {
                var elementType = type.GetElementType();

                if (elementType is null)
                {
                    Plugin.Log.LogWarning($"Unable get element type for {obj.GetType()}");
                    return;
                }

                var arr = (Array)obj;
                for (var i = 0; i < jsonData.Count; i++)
                {
                    var element = FromJson(jsonData[i], elementType);
                    arr.SetValue(element, i);
                }
            }
            else if (GetIListType(type, out var elementType))
            {
                if (elementType.IsSubclassOf(typeof(Object))) return;

                if (obj is not IList seq)
                {
                    Plugin.Log.LogWarning("Object type is not implement IList interface.");
                    return;
                }

                for (var i = 0; i < jsonData.Count; i++)
                {
                    var element = FromJson(jsonData[i], elementType);
                    seq.Add(element);
                }
            }
            else Plugin.Log.LogWarning("JsonData is Array, but object is not supported type.");
        }
        else Plugin.Log.LogWarning("JsonData type is not supported.");
    }

    /// <summary>
    /// 反序列化对象
    /// </summary>
    /// <param name="data">JsonData</param>
    /// <param name="type">对象类型</param>
    /// <returns>对象</returns>
    private static object FromJson(JsonData data, Type type)
    {
        var obj = JsonUtility.FromJson(data.ToJson(), type);
        FixData(obj, data);
        return obj;
    }

    /// <summary>
    /// 获取IList接口泛型参数类型
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="listType">IList泛型参数类型</param>
    /// <returns>是否实现了泛型IList</returns>
    private static bool GetIListType(Type type, out Type listType)
    {
        foreach (var t in type.GetInterfaces())
        {
            if (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof(IList<>)) continue;

            listType = t.GetGenericArguments()[0];
            return true;
        }

        listType = null;
        return false;
    }

    /// <summary>
    /// 数据映射
    /// </summary>
    /// <param name="data">需映射数据</param>
    private static void WarpData(IEnumerable<(ScriptableObject, JsonData)> data)
    {
        foreach (var (obj, jsonData) in data)
        {
            ModLoader.WarpperFunction.JsonCommonWarpper(obj, jsonData);
        }
    }
}