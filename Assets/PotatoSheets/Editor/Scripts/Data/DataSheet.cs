using System;
using System.Collections;
using System.Collections.Generic;

namespace PotatoSheets.Editor {

	public class DataSheet : IReadOnlyList<Row> {
		public string Name { get; }
		public string PrimaryKey { get { return m_fieldNames[m_primaryIndex]; } }
		public IEnumerable<string> FieldNames { get { return m_fieldNames; } }
		public int FieldCount { get { return m_fieldHashes.Length; } }
		public int Count { get { return m_rows.Length; } }

		public Row this[int index] {
			get { return GetRow(index); }
		}

		private readonly int m_primaryIndex;
		private readonly string[] m_fieldNames;
		private readonly uint[] m_fieldHashes;
		private readonly string[][] m_rows;

		public DataSheet(string name, string primaryKey, string[] fieldNames, string[][] rows) {
			Name = name;
			m_fieldNames = fieldNames;
			m_fieldHashes = new uint[m_fieldNames.Length];
			for (int ix = 0; ix < m_fieldHashes.Length; ix++) {
				m_fieldHashes[ix] = Util.FNVHash(m_fieldNames[ix]);
			}
			m_primaryIndex = GetFieldIndex(primaryKey);
			if (m_primaryIndex == -1) {
				throw new ArgumentException(
					$"No string with value `{primaryKey}' exists in fieldNames. " +
					$"Make sure primaryKey is valid before constructing."
				);
			}
			for (int ix = 0; ix < rows.Length; ix++) {
				if (rows[ix].Length != m_fieldNames.Length) {
					throw new ArgumentException(
						"rows contains element without the proper length to match fields"
					);
				}
			}
			m_rows = rows;
		}


		/// <summary>
		/// Polls whether a field with the given name exists in the DataSheet
		/// </summary>
		/// <param name="name">Name to search for</param>
		/// <returns>TRUE if the Field was found, FALSE if not found</returns>
		public bool HasField(string name) {
			return GetFieldIndex(name) != -1;
		}
		public int GetFieldIndex(string name) {
			uint hash = Util.FNVHash(name);
			for (int ix = 0; ix < m_fieldHashes.Length; ix++) {
				if (m_fieldHashes[ix] == hash) {
					return ix;
				}
			}
			return -1;
		}
		public Row GetRow(int index) {
			if (index < 0 || index >= m_rows.Length) {
				throw new IndexOutOfRangeException();
			}
			return new Row(m_primaryIndex, m_fieldHashes, m_fieldNames, m_rows[index]);
		}


		public IEnumerator<Row> GetEnumerator() {
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return new Enumerator(this);
		}


		private class Enumerator : IEnumerator<Row> {
			public Row Current {
				get {
					return new Row(
						m_source.m_primaryIndex, 
						m_source.m_fieldHashes, 
						m_source.m_fieldNames, 
						m_source.m_rows[m_index]
					);
				}
			}
			object IEnumerator.Current {
				get { return Current; }
			}

			private DataSheet m_source;
			private int m_index;

			public Enumerator(DataSheet source) {
				m_source = source;
				m_index = -1;
			}
			public void Dispose() {
				m_source = null;
				m_index = -1;
			}
			public bool MoveNext() {
				m_index++;
				return m_index < m_source.m_rows.Length;
			}
			public void Reset() {
				m_index = -1;
			}
		}

	}

}