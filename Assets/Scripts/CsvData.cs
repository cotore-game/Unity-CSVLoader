using System.Collections.Generic;
using System;

/// <summary>
/// 汎用的な CSV 全体を保持するクラス
/// </summary>
[Serializable]
public class CsvData<TEnum> where TEnum : struct, Enum
{
    public string DataName { get; set; }
    public List<LineData<TEnum>> Rows { get; } = new List<LineData<TEnum>>();
}
