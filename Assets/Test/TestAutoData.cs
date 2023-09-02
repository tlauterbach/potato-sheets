using PotatoSheets;
using UnityEngine;

[ContentAsset(ImportType.Automatic, "key")]
public class TestAutoData : ScriptableObject {

	[Content("integer")]
	[SerializeField]
	private int m_integer;

	[Content("string")]
	[SerializeField]
	private string m_string;

	[Content("double")]
	[SerializeField]
	private double m_double;

	[Content("assetRef")]
	[SerializeField]
	private TestAutoData m_assetRef;

	[Content("struct")]
	[SerializeField]
	private TestStruct m_struct;

	[Content("class")]
	[SerializeField]
	private TestClass m_class;



}