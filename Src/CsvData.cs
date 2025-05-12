using System.Collections.Generic;
using System;

/// <summary>
/// 汎用的な CSV 全体を保持するクラス
/// </summary>
public sealed class CsvData<TEnum> where TEnum : struct, Enum
{
    public string DataName { get; set; }
    public IReadOnlyList<LineData<TEnum>> Rows => _rows;

    private readonly List<LineData<TEnum>> _rows = new();
    internal void Add(LineData<TEnum> row) => _rows.Add(row);
}
