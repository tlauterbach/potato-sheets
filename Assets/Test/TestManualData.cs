using PotatoSheets;
#if UNITY_EDITOR
using PotatoSheets.Editor;
#endif
using System.Collections.Generic;
using UnityEngine;

[ContentAsset(ImportType.Manual, "key")]
public class TestManualData : ScriptableObject {

	[SerializeField]
	private List<string> m_keys = new List<string>();
	[SerializeField]
	private List<string> m_values = new List<string>();

#if UNITY_EDITOR
	public static void Import(IImportUtility util) {
		Dictionary<string, TestManualData> assets = new Dictionary<string, TestManualData>();

		foreach (string field in util.DataSheet.FieldNames) {
			if (field == "key") {
				continue;
			}
			assets.Add(field, util.FindOrCreateAsset<TestManualData>(util.BuildAssetPath(field)));
		}
		foreach (Row row in util.DataSheet.GetRows("key")) {
			foreach (string field in row.Keys) {
				if (field == "key") {
					continue;
				}
				assets[field].m_keys.Add(row.PrimaryValue);
				assets[field].m_values.Add(row[field]);
			}
		}
	}

	public static void LateImport(IImportUtility util) {

	}

#endif

}