> :earth_americas: 日本語バージョンはこちらから [README.md](./README.md)

# UnityCSVLoader

A general-purpose CSV loader UnityPackage.  
Uses enums as keys and provides type-safe access methods, making it easy to load scenario data, status tables, and more.  

---

## Installation

### 1. Via Unity Package Manager (manifest.json)

1. Open `Packages/manifest.json` in your Unity project.  
2. Add the following to the `dependencies` section:

    ```json
    {
      "dependencies": {
        "com.cotore.csvloader": "https://github.com/cotore-game/Unity-CSVLoader.git#v0.0.1"
      }
    }
    ```

3. Restart the Unity Editor; the package will be imported automatically.

### 2. Download and import the `.unitypackage` from Releases

1. Download the UnityPackage from the latest release:  
   https://github.com/cotore-game/Unity-CSVLoader/releases/latest  
2. Drag & drop the downloaded file into the `Assets` folder in the Unity Editor to import.

---

## Defining Headers

Define the CSV headers you want to use as an enum in advance.  
Here’s an example defining “Enemy” fields:

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

## Basic Usage

Below is a sample MonoBehaviour showing how to load and read CSV data:

```csharp
using UnityEngine;
using UnityCSVLoader;
using UnityCSVLoader.Fields;

public class Sample : MonoBehaviour
{
    [Header("CSV File to Load")]
    [SerializeField] private TextAsset enemyCsv;

    private void Start()
    {
        // Load enemy data
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

## API Reference

### `CSVLoader.LoadCSV<TEnum>(TextAsset csvFile, string dataName = null)`

-   **TEnum**: An enum type (e.g. `ScenarioFields`, `EnemyFields`)
    
-   **csvFile**: A Unity `TextAsset` containing the CSV
    
-   **dataName**: Optional; defaults to the file name if omitted
    

```csharp
CsvData<EnemyFields> data = CSVLoader.LoadCSV<EnemyFields>(enemyCsv);

```

Returns a `CsvData<TEnum>` object.

### `CsvData<TEnum>`

```csharp
public class CsvData<TEnum> where TEnum : struct, Enum
{
    public string DataName { get; set; }
    public List<LineData<TEnum>> Rows { get; }
}

```

-   `DataName`: The data name specified when loading
    
-   `Rows`: A list of `LineData<TEnum>` representing each row
    

### `LineData<TEnum>.Get<TValue>(TEnum field)`

Retrieve the value of a cell in a type-safe manner:

```csharp
int hp = line.Get<int>(EnemyFields.HP);
string name = line.Get<string>(EnemyFields.Name);

```

An `InvalidCastException` is thrown if the type does not match.

----------

## Notes

-   CSV header names must **exactly match** the enum names (case-insensitive). Column order does not matter—the mapping is automatic.
    
-   An exception is thrown if the number of columns is mismatched or if duplicate headers exist.
    
-   Empty lines are skipped automatically.
    

----------

That’s all for setup and basic usage.  
If you have any questions or encounter bugs, please feel free to open an issue on GitHub.
