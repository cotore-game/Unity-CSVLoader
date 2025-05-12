using System.Collections.Generic;
using System;
using UnityEngine;
using System.Runtime.CompilerServices;

/// <summary>
/// CSVをListに格納
/// </summary>
namespace UnityCSVLoader
{
    public static class CSVLoader
    {
        /// <summary>
        /// Enumを利用したマッピングに対応するCSVの汎用読み込み
        /// </summary>
        /// <typeparam name="TEnum">Enum型（例: ScenarioFields, EnemyFields）</typeparam>
        /// <param name="csvFile">CSVファイル</param>
        /// <param name="dataName">データ名（省略時はファイル名）</param>
        /// <returns>読み込んだデータをまとめた CsvData&lt;TEnum&gt;</returns>
        public static CsvData<TEnum> LoadCSVLegacy<TEnum>(TextAsset csvFile, string dataName = default)
            where TEnum : struct, Enum
        {
            var result = new CsvData<TEnum>();
            result.DataName = string.IsNullOrEmpty(dataName) ? csvFile.name : dataName;

            // 全行を取得（空行も含む）
            string[] lines = csvFile.text.Split('\n');

            if (lines.Length < 2)
                throw new Exception("CSVファイルが空か、ヘッダーがありません。");

            // ヘッダーを取得
            string headerLine = lines[0];
            string[] headers = headerLine.Split(',');

            // ヘッダー名→列インデックス辞書を作成
            var headerToIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                var name = headers[i].Trim();
                if (headerToIndex.ContainsKey(name))
                    throw new Exception($"ヘッダー名 '{name}' が重複しています。");
                headerToIndex[name] = i;
            }

            // Enum の各メンバーに対して必ずヘッダーがあるかチェックしつつ、Enumインデックスマップを作成
            var enumToIndex = new Dictionary<TEnum, int>();
            foreach (TEnum enumVal in Enum.GetValues(typeof(TEnum)))
            {
                string enumName = enumVal.ToString();
                if (!headerToIndex.TryGetValue(enumName, out int idx))
                    throw new Exception($"CSV のヘッダーに Enum '{enumName}' が見つかりません。");
                enumToIndex[enumVal] = idx;
            }

            // データ行を読み込み
            for (int lineNo = 1; lineNo < lines.Length; lineNo++)
            {
                var raw = lines[lineNo];
                if (string.IsNullOrWhiteSpace(raw)) continue;  // 空行はスキップ

                var values = raw.Split(',');
                if (values.Length != headers.Length)
                    throw new Exception($"行 {lineNo + 1} の列数 ({values.Length}) がヘッダー数 ({headers.Length}) と一致しません。");

                var row = new LineData<TEnum>();

                // Enum列インデックスマップを使って値をセット
                foreach (var kv in enumToIndex)
                {
                    TEnum field = kv.Key;
                    int idx = kv.Value;
                    string str = values[idx].Trim();
                    row[field] = ParseValue(str);
                }

                result.Add(row);
            }

            Debug.Log($"Loaded CsvData<{typeof(TEnum).Name}> ({result.Rows.Count} 行) for '{result.DataName}'");
            return result;
        }

        // 型変換を処理するヘルパーメソッド
        private static object ParseValue(string value)
        {
            if (int.TryParse(value, out int intValue)) return intValue;
            if (float.TryParse(value, out float floatValue)) return floatValue;
            if (bool.TryParse(value, out bool boolValue)) return boolValue;
            return value; // デフォルトは文字列として扱う
        }

        public static CsvData<TEnum> LoadCSV<TEnum>(TextAsset csvFile, string dataName = null)
            where TEnum : struct, Enum
        {
            if (csvFile == null) throw new ArgumentNullException(nameof(csvFile));

            var buffer = csvFile.text.AsMemory();
            var span = buffer.Span;
            int pos = 0;
            int length = span.Length;

            // ヘッダー読み取り
            int lineEnd = IndexOf(span, '\n', ref pos);
            if (lineEnd < 0) throw new Exception("ヘッダーが見つかりません。CSV形式を確認してください。");

            var headerSpan = Trim(span.Slice(0, lineEnd));
            int enumCount = Enum.GetValues(typeof(TEnum)).Length;
            var headerPositions = new (int Start, int Length)[enumCount];
            ParseLine(headerSpan, headerPositions, out int headerCount);

            var enumValues = (TEnum[])Enum.GetValues(typeof(TEnum));
            var enumToIndex = new int[enumCount];
            for (int i = 0; i < enumCount; i++)
            {
                var nameSpan = enumValues[i].ToString().AsSpan();
                enumToIndex[i] = FindColumnIndex(nameSpan, headerPositions, headerSpan, headerCount);
            }

            var result = new CsvData<TEnum> { DataName = string.IsNullOrEmpty(dataName) ? csvFile.name : dataName };

            // データ行ループ
            while (pos < length)
            {
                int start = pos;
                int end = IndexOf(span, '\n', ref pos);
                if (end < 0) end = length;
                var lineSpan = Trim(span.Slice(start, end - start));
                pos++;

                if (lineSpan.IsEmpty) continue;

                var cellPositions = new (int Start, int Length)[headerCount];
                ParseLine(lineSpan, cellPositions, out int cellCount);
                if (cellCount != headerCount)
                    throw new Exception($"列数不一致: ヘッダー={headerCount}, データ行={cellCount}");

                var row = new LineData<TEnum>();
                for (int i = 0; i < enumCount; i++)
                {
                    var posInfo = cellPositions[enumToIndex[i]];
                    var spanVal = lineSpan.Slice(posInfo.Start, posInfo.Length);
                    var trimmed = Trim(spanVal);
                    row[enumValues[i]] = ParseValue(trimmed);
                }

                result.Add(row);
            }

            Debug.Log($"Loaded CsvData<{typeof(TEnum).Name}> ({result.Rows.Count} rows) from '{csvFile.name}'");
            return result;
        }

        private static object ParseValue(ReadOnlySpan<char> span)
        {
            if (int.TryParse(span, out var i)) return i;
            if (float.TryParse(span, out var f)) return f;
            if (bool.TryParse(span, out var b)) return b;
            return span.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IndexOf(ReadOnlySpan<char> span, char target, ref int pos)
        {
            for (int i = pos; i < span.Length; i++)
            {
                if (span[i] == target)
                {
                    pos = i + 1;
                    return i;
                }
            }
            return -1;
        }

        private static void ParseLine(ReadOnlySpan<char> line, (int Start, int Length)[] positions, out int count)
        {
            count = 0;
            int start = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ',')
                {
                    positions[count++] = (start, i - start);
                    start = i + 1;
                }
            }
            positions[count++] = (start, line.Length - start);
        }

        private static int FindColumnIndex(ReadOnlySpan<char> key, (int Start, int Length)[] positions, ReadOnlySpan<char> header, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var span = header.Slice(positions[i].Start, positions[i].Length);
                if (span.SequenceEqual(key)) return i;
            }
            throw new Exception($"ヘッダーに '{key.ToString()}' が見つかりません。");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<char> Trim(ReadOnlySpan<char> s)
        {
            int start = 0, end = s.Length - 1;
            while (start <= end && char.IsWhiteSpace(s[start])) start++;
            while (end >= start && char.IsWhiteSpace(s[end])) end--;
            return s.Slice(start, end - start + 1);
        }
    }
}