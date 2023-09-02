using System;
using UnityEngine;

[Serializable]
public struct TestStruct {

	[SerializeField]
	private string m_data;

	public TestStruct(string data) {
		m_data = data;
	}

}