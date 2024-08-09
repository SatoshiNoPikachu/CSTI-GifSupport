using System;

namespace GifSupport.Data;

/// <summary>
/// 数据信息
/// </summary>
/// <param name="type">类型</param>
/// <param name="name">名称</param>
/// <param name="isNeedWarp">是否需要映射</param>
public struct DataInfo(Type type, string name, bool isNeedWarp = true)
{
    public readonly Type Type = type;
    public readonly string Name = name;
    public readonly bool IsNeedWarp = isNeedWarp;
}