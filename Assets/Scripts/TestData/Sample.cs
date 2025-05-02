using UnityEngine;
using UnityCSVLoader;
using UnityCSVLoader.Fields;

/// <summary>
/// サンプル：CSV ローダーの使い方 / Sample: How to use CSV Loader
/// </summary>
public class Sample : MonoBehaviour
{
    [Header("読み込む CSV ファイル / CSV Files to Load")]
    [SerializeField] private TextAsset scenarioCsv;
    [SerializeField] private TextAsset enemyCsv;

    private void Start()
    {
        // インスタンス化してから使用してください。CSVLoader は static ではありません。
        // Instantiate: CSVLoader is not static.
        var csvLoader = new CSVLoader();

        // 1. ScenarioFields を使ってシナリオデータを読み込み
        //    第2引数は表示用のデータ名（省略可）
        // 1. Load scenario data using ScenarioFields
        //    Second argument sets data name for display (optional)
        var scenarioData = csvLoader.LoadCSV<ScenarioFields>(scenarioCsv, "MainScenario");
        Debug.Log($"Scenario '{scenarioData.DataName}' loaded. Rows: {scenarioData.Rows.Count}");

        if (scenarioData.Rows.Count > 0)
        {
            // Rows は各行を表す LineDataリスト を含むクラスです。
            // LineData.Get<T> で同名のカラム値を取得します。
            // Rows is a Class contains list of LineData. Use LineData.Get<T> to retrieve column values.
            var firstLine = scenarioData.Rows[0];
            var command = firstLine.Get<string>(ScenarioFields.Command);
            Debug.Log($"First scenario command: {command}");
        }

        // 2. EnemyFields を使って敵データを読み込み
        // 2. Load enemy data using EnemyFields
        var enemyData = csvLoader.LoadCSV<EnemyFields>(enemyCsv, "EnemyStats");
        Debug.Log($"Enemy data '{enemyData.DataName}' loaded. Rows: {enemyData.Rows.Count}");

        // 各行をループしてフィールド値を取得
        // Loop through each row and get field values
        foreach (var line in enemyData.Rows)
        {
            int id = line.Get<int>(EnemyFields.ID);
            string name = line.Get<string>(EnemyFields.Name);
            float hp = line.Get<int>(EnemyFields.HP);
            int attack = line.Get<int>(EnemyFields.Attack);

            Debug.Log($"Enemy ID:{id} Name:{name} HP:{hp} Attack:{attack}");
        }
    }
}
