using System.Collections;
using System.Collections.Generic;

namespace PotatoSheets.Editor {

	/// <summary>
	/// Dictionary containing the values of a specific row. Can be
	/// used to poll specific information about a row by using
	/// field names as keys to access values in the row.
	/// </summary>
	public readonly struct Row : IReadOnlyDictionary<string,string> {

		public string PrimaryValue { get { return m_values[m_primaryIndex]; } }

		public IEnumerable<string> Keys { get { return m_fieldNames; } }
		public IEnumerable<string> Values { get { return m_values; } }

		public int Count { get { return m_values.Length; } }

		public string this[string key] {
			get { 
				if (TryGetValue(key, out string value)) {
					return value;
				} else {
					throw new KeyNotFoundException();
				}
			}
		}

		private readonly int m_primaryIndex;
		private readonly uint[] m_fieldHashes;
		private readonly string[] m_fieldNames;
		private readonly string[] m_values;


		public Row(int primaryIndex, uint[] fieldHashes, string[] fieldNames, string[] values) {
			m_primaryIndex = primaryIndex;
			m_fieldHashes = fieldHashes;
			m_fieldNames = fieldNames;
			m_values = values;
		}

		public int FindFieldIndex(string key) {
			uint hash = Util.FNVHash(key);
			for (int ix = 0; ix < m_fieldHashes.Length; ix++) {
				if (m_fieldHashes[ix] == hash) {
					return ix;
				}
			}
			return -1;
		}

		public bool ContainsKey(string key) {
			return FindFieldIndex(key) != -1;
		}

		public bool TryGetValue(string key, out string value) {
			int fieldIndex = FindFieldIndex(key);
			if (fieldIndex == -1) {
				value = string.Empty;
				return false;
			}
			value = m_values[fieldIndex];
			return true;
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
			return new Enumerator(m_fieldNames, m_values);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return new Enumerator(m_fieldNames, m_values);
		}


		private class Enumerator : IEnumerator<KeyValuePair<string, string>> {
			public KeyValuePair<string, string> Current {
				get { return new KeyValuePair<string, string>(m_keys[m_index], m_values[m_index]); }	
			}
			object IEnumerator.Current {
				get { return Current; }
			}

			private string[] m_keys;
			private string[] m_values;
			private int m_index;

			public Enumerator(string[] keys, string[] values) {
				m_keys = keys;
				m_values = values;
				m_index = -1;
			}

			public void Dispose() {
				m_keys = null;
				m_values = null;
				m_index = -1;
			}

			public bool MoveNext() {
				m_index++;
				return m_index < m_keys.Length && m_index < m_values.Length;
			}

			public void Reset() {
				m_index = -1;
			}
		}

	}

}