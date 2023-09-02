using PotatoSheets;
using PotatoSheets.Editor;
using UnityEngine;

[ContentAsset(ImportType.Custom, "key")]
public class TestCustomData : ScriptableObject {

#if UNITY_EDITOR
	public static void Import(DataSheet sheet) {
		
	}

#endif

}