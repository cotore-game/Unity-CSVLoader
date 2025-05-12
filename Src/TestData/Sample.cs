using System;
using System.Diagnostics;
using UnityEngine;
using UnityCSVLoader;
using UnityCSVLoader.Fields;
using static UnityEngine.EventSystems.EventTrigger;

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
        // ────────────────
        // 1. ScenarioFields を使ってシナリオデータを読み込み
        //    第2引数は表示用のデータ名（省略可）
        // 1. Load scenario data using ScenarioFields
        //    Second argument sets data name for display (optional)
        // ────────────────

        // ZeroAlloc版
        var sw = Stopwatch.StartNew();
        long memBefore = GC.GetTotalMemory(false);

        var scenarioData = CSVLoader.LoadCSV<ScenarioFields>(scenarioCsv, "MainScenario");

        sw.Stop();
        long memAfter = GC.GetTotalMemory(false);
        UnityEngine.Debug.Log(
            $"[ZeroAlloc/Scenario] Loaded {scenarioData.Rows.Count} rows in {sw.ElapsedMilliseconds} ms, " +
            $"Heap Δ = {memAfter - memBefore:N0} bytes"
        );

        // Split版（legacy）
        // GCを一度クリアして同条件に
        GC.Collect();
        GC.WaitForPendingFinalizers();

        sw.Restart();
        memBefore = GC.GetTotalMemory(false);

        var scenarioDataLegacy = CSVLoader.LoadCSVLegacy<ScenarioFields>(scenarioCsv, "MainScenario_Legacy");

        sw.Stop();
        memAfter = GC.GetTotalMemory(false);
        UnityEngine.Debug.Log(
            $"[Legacy/Scenario]    Loaded {scenarioDataLegacy.Rows.Count} rows in {sw.ElapsedMilliseconds} ms, " +
            $"Heap Δ = {memAfter - memBefore:N0} bytes"
        );

        // 取得例
        if (scenarioData.Rows.Count > 0)
        {
            // Rows は各行を表す LineDataリスト を含むクラスです。
            // LineData.Get<T> で同名のカラム値を取得します。
            // Rows is a Class contains list of LineData. Use LineData.Get<T> to retrieve column values.
            var firstLine = scenarioData.Rows[0];
            var command = firstLine.Get<string>(ScenarioFields.Command);
            UnityEngine.Debug.Log($"First scenario command: {command}");
        }

        // ────────────────
        // 2. EnemyFields を使って敵データを読み込み
        // 2.Load enemy data using EnemyFields
        // ────────────────

        // ZeroAlloc版
        sw.Restart();
        memBefore = GC.GetTotalMemory(false);

        var enemyData = CSVLoader.LoadCSV<EnemyFields>(enemyCsv, "EnemyStats");

        sw.Stop();
        memAfter = GC.GetTotalMemory(false);
        UnityEngine.Debug.Log(
            $"[ZeroAlloc/Enemy]    Loaded {enemyData.Rows.Count} rows in {sw.ElapsedMilliseconds} ms, " +
            $"Heap Δ = {memAfter - memBefore:N0} bytes"
        );

        // Split版（legacy）
        GC.Collect();
        GC.WaitForPendingFinalizers();

        sw.Restart();
        memBefore = GC.GetTotalMemory(false);

        var enemyDataLegacy = CSVLoader.LoadCSVLegacy<EnemyFields>(enemyCsv, "EnemyStats_Legacy");

        sw.Stop();
        memAfter = GC.GetTotalMemory(false);
        UnityEngine.Debug.Log(
            $"[Legacy/Enemy]       Loaded {enemyDataLegacy.Rows.Count} rows in {sw.ElapsedMilliseconds} ms, " +
            $"Heap Δ = {memAfter - memBefore:N0} bytes"
        );

        // 各行をループしてフィールド値を取得
        // Loop through each row and get field values
        foreach (var line in enemyData.Rows)
        {
            int id = line.Get<int>(EnemyFields.ID);
            string name = line.Get<string>(EnemyFields.Name);
            float hp = line.Get<int>(EnemyFields.HP);
            int attack = line.Get<int>(EnemyFields.Attack);

            UnityEngine.Debug.Log($"Enemy ID:{id} Name:{name} HP:{hp} Attack:{attack}");
        }
    }
}
