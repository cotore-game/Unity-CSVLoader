using System.Collections.Generic;
using System;

/// <summary>
/// 任意の Enum をキーに、各セルの値を保持する汎用的な 1 行データ
/// </summary>
public sealed class LineData<TEnum> where TEnum : struct, Enum
{
    private readonly Dictionary<TEnum, object> _values = new();

    public object this[TEnum field]
    {
        get => _values.TryGetValue(field, out var val) ? val : null;
        set => _values[field] = value;
    }

    public T Get<T>(TEnum field)
    {
        if (_values.TryGetValue(field, out var val))
        {
            if (val is T t) return t;
            throw new InvalidCastException($"Field {field} is not of type {typeof(T)}");
        }
        throw new KeyNotFoundException($"Field {field} not found");
    }
}
