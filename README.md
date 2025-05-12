> :earth_americas: For English documentation, see [README_EN.md](./README_EN.md)

# UnityCSVLoader

汎用的な CSV ローダーを提供する UnityPackage です。  
`Enum` をキーにしたマッピング＋型安全な取得メソッドで、シナリオデータやステータスデータなどを簡単に読み込めます。

---

## インストール

### 1. Unity Package Manager（manifest.json）を使う場合

1. Unity プロジェクトの `Packages/manifest.json` を開く  
2. `dependencies` セクションに以下を追加してください:

    ```json
    {
      "dependencies": {
        "com.cotore.csvloader": "https://github.com/cotore-game/Unity-CSVLoader.git#v0.0.1"
      }
    }
    ```

3. Unity エディタを再起動すると、自動でパッケージがインポートされます。

### 2. Releases から `.unitypackage` をダウンロードしてインポート

1. 以下の URL から UnityPackage をダウンロード
	https://github.com/cotore-game/Unity-CSVLoader/releases/latest 
3. Unity エディタ上で `Assets` フォルダにドラッグ＆ドロップしてインポート

---

## ヘッダー定義

扱いたい CSV のヘッダーをあらかじめ `enum` として定義します。  
以下は例として「敵ステータス」を定義したものです。

```csharp
namespace UnityCSVLoader.Fields
{
    public enum EnemyFields
    {
        ID,
        Name,
        HP,
        Attack
    }
}

```

----------

## 基本的な使い方

以下は MonoBehaviour 内で CSV を読み込むサンプルコードです。

```csharp
using UnityEngine;
using UnityCSVLoader;
using UnityCSVLoader.Fields;

public class Sample : MonoBehaviour
{
    [Header("読み込む CSV ファイル")]
    [SerializeField] private TextAsset enemyCsv;

    private void Start()
    {
        // 敵データを読み込み
        var enemyData = CSVLoader.LoadCSV<EnemyFields>(enemyCsv, "EnemyStats");
        Debug.Log($"Enemy data '{enemyData.DataName}' loaded. Rows: {enemyData.Rows.Count}");

        foreach (var line in enemyData.Rows)
        {
            int id       = line.Get<int>(EnemyFields.ID);
            string name  = line.Get<string>(EnemyFields.Name);
            float hp     = line.Get<float>(EnemyFields.HP);
            int attack   = line.Get<int>(EnemyFields.Attack);

            Debug.Log($"Enemy ID:{id} Name:{name} HP:{hp} Attack:{attack}");
        }
    }
}

```

----------

## API リファレンス

### `CSVLoader.LoadCSV<TEnum>(TextAsset csvFile, string dataName = null)`

-   **TEnum**: `enum` 型（例: `ScenarioFields`, `EnemyFields`）
    
-   **csvFile**: Unity 上で読み込んだ `TextAsset`
    
-   **dataName**: 任意。省略時はファイル名。
    
戻り値は `CsvData<TEnum>`。

### `CsvData<TEnum>`

```csharp
public class CsvData<TEnum> where TEnum : struct, Enum
{
    public string DataName { get; set; }
    public List<LineData<TEnum>> Rows { get; }
}

```

-   `DataName`: 読み込み時に指定したデータ名
    
-   `Rows`: 各行を表す `LineData<TEnum>` のリスト
    

### `LineData<TEnum>.Get<TValue>(TEnum field)`

各セルの値を型安全に取得します。

```csharp
int hp = line.Get<int>(EnemyFields.HP);
string name = line.Get<string>(EnemyFields.Name);

```

型が異なる場合は `InvalidCastException` がスローされます。

----------

## 注意点

-   CSV のヘッダー名は `enum` 名と**完全一致**（大文字小文字は無視）している必要があります。列の順番は宣言順でなくても自動的にマッピングされるので問題ありません。
    
-   列数が不一致の場合やヘッダー重複がある場合、例外が発生します。
    
-   空行は自動でスキップされます。
    

----------

以上でセットアップと基本的な使い方の説明は完了です。  
不明点やバグがあれば GitHub Issue までお気軽にどうぞ。
