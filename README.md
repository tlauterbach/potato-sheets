# potato-sheets
A light-weight Google Sheets importer for Unity scriptable objects

| Package Name | Package Version | Unity Version |
|-----|-----|-----|
| com.potatointeractive.sheets | 1.0.0 | 2021.3.x |

[Changelog](CHANGELOG.md)

# Overview
_PotatoSheets_ is an open source (MIT License) Google Sheets importer for Unity meant to quickly download data from Google Spreadsheets into your own Scriptable Objects data from the Editor to be used by your project at runtime.

## Key Benefits
* _PotatoSheets_ is configurable to each organization's _Google Cloud_, meaning that you can supply your own **Client Secrets** and restrict usage of the importer to people within your organization.
  * A detailed installation guide to set up _Google Cloud_ is provided in the **Installation** section.
* With both _Automatic_ and _Manual_ import types, engineers can customize as much or as little with how your data is created.
  * An explanation of the difference between and uses of both _Automatic_ and _Manual_ imports are in the **Usage** section.
* _PotatoSheets_ **does not** have dependencies on Google's hefty NuGet libraries/.dlls or Newtonsoft.Json (both of which can cause conflicts with other packages) or any plugins and instead opts to use Google's REST API to manually handle importing.
  * The only dependencies for this package are as follows:
    | Dependency | Version |
    |-----|-----|
    | com.unity.editorcoroutines | 1.0.0 |
    | com.unity.textmeshpro | 3.0.6 |
    | com.unity.ugui | 1.0.0 |
    | com.unity.modules.jsonserialize | 1.0.0 |
    | com.unity.modules.imgui | 1.0.0 |
    | com.unity.modules.ui | 1.0.0 |
    | com.unity.modules.uielements | 1.0.0 |
    | com.unity.modules.uielementsnative | 1.0.0 |
    | com.unity.modules.unitywebrequest | 1.0.0 |

# Installation

## OpenUPM
This project is available as an Open UPM Package: [![openupm](https://img.shields.io/npm/v/com.potatointeractive.sheets?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.potatointeractive.sheets/)

Visit [Open UPM](https://openupm.com) to learn more about the Open Unity Package Manager project and how to install the package in your Unity Project.

## Google Cloud
In order for _PotatoSheets_ to interface with Google's Sheets API, you need to have a Google account with a _Google Cloud_ configuration. Following are instructions on how to set up _Google Cloud_ to work with _PotatoSheets_.

### Sign-In and First Time Setup
1. Access `https://console.cloud.google.com/` and sign in with a Google Account
   * You will be prompted to accept their terms and conditions
2. Select **APIs & Services** > **Enabled APIs & Services**
3. Choose **Select a Project**
4. Choose **Create New Project**
5. Make a name for your new project. _PotatoSheets_ should work fine.
   * If you have permission from your admin(s) or pay for an organization, select it for the **Location** field (otherwise, leave it as **No Organization**)
6. Press **Create** and the project will take a moment to be created

### Enable Google Sheets API
1. Once redirected or by accessing the **Enabled APIs & Services** section, click **ENABLE APIS AND SERVICES**
2. From the **API Library**, search for `Google Sheets`
3. Select the **Google Sheets API**
4. Click **Enable** to enable the API

### Configure OAuth Consent Screen
1. Select **OAuth consent screen** from the **APIs & Services** menu, then choose your **User Type**
   * If you have an organization, you should select **Internal**, as it is likely you'll only be using _PotatoSheets_ internally
   * If you don't have an organization, you can still use _PotatoSheets_ by selecting **External** and manually add accounts that can access this project. Your app will most likely always remain in testing mode and not need to be verified.
2. Click **Create**
3. Enter in an **App name** (_PotatoSheets_ should work fine), a support email (likely your own or your group/company's support email address), and enter the same email for the developer contact information
4. Click **SAVE AND CONTINUE** at the bottom
5. Next, you will be adding *Scopes*. Click **ADD OR REMOVE SCOPES**
6. Page through the scopes and find the **Google Sheets API** and select the `../auth/spreadsheets.readonly` option
7. Click **UPDATE** at the bottom of the Scopes menu
8. Click **SAVE AND CONTINUE** of the Edit app registration page
9. Next, you will be adding *Test Users*. Click **ADD USERS** and enter any google email addresses that will need to use PotatoSheets
    * This step may look different or not exist(?) if you selected **Internal** in Step 1 of this section
10. Click **SAVE AND CONTINUE**

### Credentials Setup
1. Select **Credentials** from the **APIs & Services** menu, then click **Create Credentials** > **OAuth client ID**
2. Select **Desktop app** from the dropdown selection menu and name it (_PotatoSheets Client_ is fine)
3. Click **CREATE**
4. Download the **Client secret** JSON file by clicking **DOWNLOAD JSON**
   * This option can also be selected by clicking on the *OAuth 2.0 Client IDs* link and clicking the **DOWNLOAD JSON** button
5. Place your downloaded JSON file in your project folder under `project-name/tools/potato-sheets/` and rename it to `client-secret.json`
   * the folder `tools` should be in the same directory as your project's `Assets` folder
   * **NOTE:** you will want to make sure each user downloads this client secret file individually and NOT commit it to source control

# Usage

Before you can import any data, you need to create a data class that inherits from `ScriptableObject` and add the `ContentAsset` attribute to it. `ContentAsset` requires two arguments: **ImportType** and **PrimaryKey**
```csharp
using PotatoSheets;
using UnityEngine;

[ContentAsset(ImportType.Automatic, "primaryKey")]
public class MyDataClass : ScriptableObject {

}
```
Your **ImportType** options are *Automatic* and *Manual*, and you primary key will be the 'field name' of the identifying column of your data on your spreadsheet (such as `"id"`, `"key"`, etc). For *Automatic* imports, the primary key value will become the name of your imported asset, so it is good practice to include the type of data in your primary key (such as `Enemy_Cougar` or `Spell_Fire1`)

## Automatic Importing
*Automatic* importing is relatively simple; after you've created your class, all you need to do is mark up your fields or properties with the `Content` attribute.

Consider that this example data...

![](img/potato_sheets_04.png)

...could look something like this in code:
```csharp
using PotatoSheets;
using UnityEngine;

[ContentAsset(ImportType.Automatic, "id")]
public class EnemyData : ScriptableObject {

  [Content("name")]
  public string Name;

  [Content("health")]
  public int Health {
    get { return m_health; }
    set { m_health = value; }
  }
  [SerializeField]
  private int m_health;

  [Content("atk")]
  [Content("attackMultiplier")]
  public float AttackMultiplier;
}
```
String parameters in the `Content` attribute would be the name of the column in GoogleSheets that the data will be read from. It is valid to have multiple `Content` attributes for a field, in case different worksheets use different column titles. The furthest column to the right will usally be the one that 'wins' if there are multiple columns in the same worksheet per field/property.

Each row of data from the spreadsheet will create a new asset named using the *primary key*. So in our example, `Enemy_Tiger.asset`, `Enemy_Snake.asset`, etc. would be created.


### Supported Automatic Import Field/Property Types
| Group | Types |
|-----|-----|
| System Structs | `string` `int` `uint` `short` `ushort` `long` `ulong` `byte` `sbyte` `char` `float` `double` `decimal` `DateTime` |
| Unity Structs | `Vector2` `Vector3` `Vector4` `Vector2Int` `Vector3Int` `Bounds` `BoundsInt` `Rect` `RectInt` `Color` `Color32` |
| Unity Classes | `UnityEngine.Object`† `ScriptableObject`†† |
| Enums | `int` `uint` `short` `ushort` `long` `ulong` `byte` `sbyte`|
| User Structs and Classes | requires a constructor with `string` parameter |

† `UnityEngine.Object` includes built-in Unity asset types such as `Material`, `Texture2D`, and `TextAsset`

†† `ScriptableObject` includes any user-created classes that inherit from `ScriptableObject` so that imported assets can link to eachother

Currently, Lists and Dictionaries (or any other Generic Collections) are not supported for *Automatic* importing. However, you can support them yourself with *Manual* importing if you need to handle them.

### Array Support
Arrays are supported, but you need to change a couple of things about your data and asset class in order to handle them.

Consider:

![](img/potato_sheets_05.png)

The values within the **Items** column need to be separated by a character (a 'delimiter') you know you don't need in processing the data. In the example, it is a comma, but it can be any character you wish. You then need to specify the delimiter in your `Content` attribute to let the importer know how to split your data.

```csharp
using PotatoSheets;
using UnityEngine;

[ContentAsset(ImportType.Automatic,"id")]
public class EnemyData : ScriptableObject {
  
  [Content("name")]
  public string Name;

  [Content("items", Delimiter = ",")]
  public string[] ItemNames;

}
```

### Complex Automatic Import Example

![](img/potato_sheets_06.png) 

![](img/potato_sheets_07.png)

```csharp
using PotatoSheets;
using UnityEngine;

[ContentAsset(ImportType.Automatic,"id")]
public class EnemyData : ScriptableObject {
  
  [Content("name")]
  public string Name;

  [Content("health")]
  public uint Health;

  [Content("baseAttack")]
  public uint BaseAttack;

  [Content("attacks", Delimiter = "|")]
  public EnemyAttack[] Attacks;

  [Content("sprite")]
  public Sprite BattleSprite;

  [Content("droppedItems", Delimiter = "/")]
  public ItemData[] DroppedItems;

}

[ContentAsset(ImportType.Automatic,"id")]
public class ItemData : ScriptableObject {
  [Content("name")]
  public string Name;

  [Content("worth")]
  public uint Worth;

  [Content("consumable")]
  public bool IsConsumable;

  [Content("searchTags", Delimiter = "|")]
  public string[] SearchTags;
  
}

[Serializable]
public class EnemyAttack {

  public string Verb;
  public float AtkMultiplier;

  public EnemyAttack(string data) {
    string[] split = data.Split(",");
    Verb = split[0].Trim();
    AtkMultiplier = float.Parse(split[1].Trim());
  }
}

```

## Manual Importing
In certain cases, you may need quite a bit more control over _what_ happens with all of your imported data for a `ContentAsset`. In such a case, you will need to do _Manual_ importing. There are slightly different things you need to do with your data class to use _Manual_ importing:

```csharp
using PotatoSheets;
using UnityEditor;
#if UNITY_EDITOR
using PotatoSheets.Editor;
#endif

[ContentAsset(ImportType.Manual, "key")]
public class MyDataClass : ScriptableObject {

#if UNITY_EDITOR
  public static void Import(IImportUtility util) {
    // static Import method is required for Manual imports and occurs first
  }
  public static void LateImport(IImportUtility util) {
    // static LateImport method is also required, and occurs after all ContentAssets have completed the Import stage
  }
#endif
}
```
### IImportUtility
Your main way of handling imported data is via the `IImportUtility` interface provided by the `Import` and `LateImport` functions. It contains several helper functions to make the process easier.
| Property | Returns | Usage |
|-----|-------|-----|
| DataSheet | `DataSheet` | Object containing imported data retreived from a GoogleSheets worksheet. Read more usage tips in the [DataSheet](#DataSheet) section. |
| PrimaryKey | `string` | The primary key value specified in the `ContentAsset` attribute of this class. |
| AssetDirectory | `string` | Directory relative to the project folder where assets should be created (set from the [PotatoSheets Window](#PotatoSheets-Window)) |

| Function | Returns | Usage |
|-----|-----|-----|
| BuildAssetPath | `string` | Makes a valid asset path using the `AssetDirectory` and provided assetName and extension parameters |
| FindOrCreateAsset | `ScriptableObject` | Finds or Creates a `ScriptableObject` of a given type at the given path. |
| FindAssetByName | `bool`, `out UnityEngine.Object` | Looks for the asset of a given type and given name within your project's `AssetDatabase`. This can be built-in asset types like `Material` or `TextAsset` as well as other `ScriptableObject`s. Returns `TRUE` if the asset was found, `FALSE` if it was not. The asset itself will be in the output parameter |

### DataSheet
The `DataSheet` property of `IImportUtility` contains all of the data that was retrieved from a worksheet specified in the [PotatoSheets Window](#PotatoSheets-Window). You can get `Rows` and `Columns` to examine and serialize the data you've been given.

| Property | Returns | Usage |
|-----|-----|-----|
| Id | `WorksheetID` | returns a struct that gives the Spreadsheet ID and Worksheet Name of the `DataSheet` |
| FieldNames | `IEnumerable<string>` | A set of all of the field names that were specified on the Worksheet |
| RowCount | `int` | The number of rows in the `DataSheet` (not including frozen rows) |
| ColumnCount | `int` | The number of columns in the `DataSheet` |

| Function| Returns | Usage |
|-----|------|------|
| HasField | `bool` | Returns `TRUE` if the given `fieldName` exists on the `DataSheet`, `FALSE` if it does not |
| GetFieldIndex | `int` | Returns the index of the given `fieldName` if it exists, `-1` if it does not |
| GetRow | `Row` | Returns the `Row` of values of the specified `index` and `primaryKey` |
| GetRows | `IEnumerable<Row>` | Returns all `Row`s of values on the `DataSheet` |
| GetColumn | `Column` | Returns the `Column` of values of the given `fieldName` |
| GetColumns | `IEnumerable<Column>` | Returns all `Column`s of values on the `DataSheet` |


### Manual Import Example
A good example of something that likely needs _Manual_ importing is **Localization** data being in the same worksheet but requiring assets per column. 

![](img/potato_sheets_08.png)

```csharp
using PotatoSheets;
#if UNITY_EDITOR
using PotatoSheets.Editor;
#endif
using System.Collections.Generic;
using UnityEngine;

[ContentAsset(ImportType.Manual, "key")]
public class Localization : ScriptableObject {

  [SerializeField]
  private List<string> m_keys = new List<string>();
  [SerializeField]
  private List<string> m_values = new List<string>();

#if UNITY_EDITOR

  private static Dictionary<string, TestManualData> m_assets;

  public static void Import(IImportUtility util) {
    if (m_assets == null) {
      m_assets = new Dictionary<string, TestManualData>();
    }
    foreach (string field in util.DataSheet.FieldNames) {
      if (field == "key" || m_assets.ContainsKey(field)) {
        continue;
      }
      Localization data = util.FindOrCreateAsset<Localization>(util.BuildAssetPath(field));
      data.m_keys.Clear();
      data.m_values.Clear();
      m_assets.Add(field, data);
    }
  }

  public static void LateImport(IImportUtility util) {
    util.DataSheet.GetColumn("key").Copy(out string[] keys);

    foreach (Column column in util.DataSheet.GetColumns()) {
      if (column.FieldName == "key") {
        continue;
      }
      Localization data = m_assets[column.FieldName];
      data.m_keys.AddRange(keys);
      data.m_values.AddRange(column);
    }
  }
#endif
}
```

## PotatoSheets Window
Finally, you will need to link the Unity Editor to Google Sheets by using the _PotatoSheets_ editor window from inside Unity.

Because the `com.potatointeractive.sheets` package is installed in your project, you can access the _PotatoSheets_ window from Unity by selecting **Window** > **Tools** > **Potato Sheets**
![Opening PotatoSheets window in Unity](img/potato_sheets_01.png)

If this is your first use, it should look something like this:
![PotatoSheets window](img/potato_sheets_02.png)

### Import Profile Setup
1. Press the **Create New** button next to the *Profile* object selector.
   * Feel free to rename the newly created profile asset and place it in any folder you wish
2. You will see the **Profile List** appear
   * The **Profile List** will only appear when you have selected a valid **PotatoSheetsProfile** object
3. Press the **+** button next to **Profile List** to add a new profile
   * The **Profile Settings** will appear when you have selected a *Profile* from the **ProfileList**
   * Selected *Profiles* can be removed by pressing the **-** button next to **Profile List**
4. Now you can set information for the importer to import your content
   | Field | Usage |
   |-----|-----|
   | Profile Name | The name of the profile as it will appear in the **Profile List** |
   | SheetID | The unique id of your Google Spreadsheet in Google Drive. The id can be located by opening the spreadsheet and reading the url between the `d/` and `/edit/` section. |
   | Worksheet Name | This is the name of the sheet _within_ your spreadsheet that you will be importing. The name is located at the bottom of the screen when viewing a spreadsheet. |
   | Asset Type | A comprehensive list of all ScriptableObject classes in your project that have the `[ContentAsset]` attribute. This will be the type used to create data when this profile is imported. |
   | Asset Directory | The path relative to your project folder where assets will be created on import. |
5. After you have configured a *Profile*, you can import it by clicking the **Import** or **Import All** buttons





