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

	private static Dictionary<string, TestManualData> m_assets;

	public static void Import(IImportUtility util) {
		if (m_assets == null) {
			m_assets = new Dictionary<string, TestManualData>();
		}
		foreach (string field in util.DataSheet.FieldNames) {
			if (field == "key" || m_assets.ContainsKey(field)) {
				continue;
			}
			TestManualData data = util.FindOrCreateAsset<TestManualData>(util.BuildAssetPath(field));
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
			TestManualData data = m_assets[column.FieldName];
			data.m_keys.AddRange(keys);
			data.m_values.AddRange(column);
		}
	}

#endif

}