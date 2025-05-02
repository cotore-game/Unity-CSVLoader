using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// CSVをListに格納
/// </summary>
namespace IsogiYama.System
{
    public class CSVLoader
    {

        /// <summary>
        /// Enumを利用したマッピングに対応するCSVの汎用読み込み
        /// </summary>
        /// <typeparam name="TEnum">Enum型（例: ScenarioFields, EnemyFields）</typeparam>
        /// <param name="csvFile">CSVファイル</param>
        /// <param name="dataName">データ名（省略時はファイル名）</param>
        /// <returns>読み込んだデータをまとめた CsvData&lt;TEnum&gt;</returns>
        public CsvData<TEnum> LoadCSV<TEnum>(TextAsset csvFile, string dataName = default)
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

                result.Rows.Add(row);
            }

            Debug.Log($"Loaded CsvData<{typeof(TEnum).Name}> ({result.Rows.Count} 行) for '{result.DataName}'");
            return result;
        }


        // 煩雑な仕様をまとめてファイルを渡せばよいだけにしたメソッド
        public CsvData<ScenarioFields> ReadScenarioCSV(TextAsset file, string filename = default)
        {
            CsvData<ScenarioFields> scenarioData = new CsvData<ScenarioFields>();
            scenarioData = LoadCSV<ScenarioFields>(file);
            scenarioData.DataName = filename;

            return scenarioData;
        }

        // 型変換を処理するヘルパーメソッド
        private object ParseValue(string value)
        {
            if (int.TryParse(value, out int intValue)) return intValue;
            if (float.TryParse(value, out float floatValue)) return floatValue;
            if (bool.TryParse(value, out bool boolValue)) return boolValue;
            return value; // デフォルトは文字列として扱う
        }
    }
}