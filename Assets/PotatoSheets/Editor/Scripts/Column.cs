using System;
using System.Collections;
using System.Collections.Generic;

namespace PotatoSheets.Editor {

	public struct Column : IReadOnlyList<string> {

		public string FieldName { get { return m_fieldName; } }

		public string this[int index] {
			get {
				if (index < 0 || index >= m_values.Length) {
					throw new IndexOutOfRangeException();
				}
				return m_values[index];
			}
		}
		public int Count { get { return m_values.Length; } }

		private string m_fieldName;
		private string[] m_values;

		public Column(string fieldName, string[] values) {
			m_fieldName = fieldName;
			m_values = values;
		}

		public void Copy(out string[] destination) {
			destination = new string[m_values.Length];
			for (int ix = 0; ix < m_values.Length; ix++) {
				destination[ix] = m_values[ix];
			}
		}

		public IEnumerator<string> GetEnumerator() {
			return ((IEnumerable<string>)m_values).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return m_values.GetEnumerator();
		}
	}


}