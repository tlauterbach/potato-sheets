using System;
using UnityEngine;

[Serializable]
public class TestClass {

	[SerializeField]
	private string m_data;

	public TestClass(string data) {
		m_data = data;
	}

}