using System;
using System.Collections;
using System.Collections.Generic;

namespace PotatoSheets.Editor {

	public class DataSheet {
		public WorksheetID Id { get; }
		public IEnumerable<string> FieldNames { get { return m_fieldNames; } }
		public int RowCount { get { return m_rows.Length; } }
		public int ColumnCount { get { return m_columns.Length; } }

		private readonly string[] m_fieldNames;
		private readonly uint[] m_fieldHashes;
		private readonly string[][] m_rows;
		private readonly string[][] m_columns;

		internal DataSheet(WorksheetID id, ValueRangeBlob valueRange, int frozenRows) {
			
			Id = id;

			// determine the max size of a row to apply to all rows
			int maxLength = 0;
			for (int ix = 0; ix < valueRange.values.Length; ix++) {
				if ((valueRange.values[ix]?.Length ?? 0) > maxLength) {
					maxLength = valueRange.values[ix].Length;
				}
			}
			// we need to remove unused fields (ie, the string is null or empty)
			string[] fields = valueRange.values[Math.Max(0, frozenRows - 1)];
			MakeValidFieldArray(ref fields, maxLength, out int[] removed);
			m_fieldNames = fields;

			m_fieldHashes = new uint[m_fieldNames.Length];
			for (int ix = 0; ix < m_fieldHashes.Length; ix++) {
				m_fieldHashes[ix] = Util.FNVHash(m_fieldNames[ix]);
			}

			// make all of the value ranges into equally sized arrays and ignore empties
			List<string[]> rows = new List<string[]>();
			for (int ix = Math.Max(0, frozenRows - 1) + 1; ix < valueRange.values.Length; ix++) {

				if (IsStringArrayEmpty(valueRange.values[ix])) {
					continue; // this is an empty/invalid row
				}
				string[] row = new string[maxLength];
				CopyStringArray(valueRange.values[ix], row); // maintains original data structure
				MatchFieldArray(ref row, removed);
				rows.Add(row);
			}
			m_rows = rows.ToArray();

			// add all of the rows as columns for extra data simplification
			List<string[]> columns = new List<string[]>();
			if (m_rows.Length > 0) {
				for (int ix = 0; ix < m_rows[0].Length; ix++) {
					columns.Add(new string[m_rows.Length]);
				}
				for (int ix = 0; ix < m_rows.Length; ix++) {
					for (int iy = 0; iy < m_rows[ix].Length; iy++) {
						columns[iy][ix] = m_rows[ix][iy];
					}
				}
			}
			m_columns = columns.ToArray();
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
		public Row GetRow(string primaryKey, int index) {
			if (index < 0 || index >= m_rows.Length) {
				throw new IndexOutOfRangeException();
			}
			return new Row(primaryKey, m_fieldNames, m_fieldHashes, m_rows[index]);
		}

		public IEnumerable<Row> GetRows(string primaryKey) {
			Enumerator enumerator = new Enumerator(this, primaryKey);
			while (enumerator.MoveNext()) {
				yield return enumerator.Current;
			}
		}
		public Column GetColumn(string fieldName) {
			int fieldIndex = GetFieldIndex(fieldName);
			if (fieldIndex == -1) {
				throw new KeyNotFoundException($"No field with name `{fieldName}' exists in the DataSheet");
			}
			return new Column(fieldName, m_columns[fieldIndex]);
		}
		public IEnumerable<Column> GetColumns() {
			for (int ix = 0; ix < m_fieldNames.Length; ix++) {
				yield return new Column(m_fieldNames[ix], m_columns[ix]);
			}
		}


		private static void CopyStringArray(string[] source, string[] destination) {
			for (int ix = 0; ix < source.Length && ix < destination.Length; ix++) {
				destination[ix] = source[ix];
			}
		}

		private static void MakeValidFieldArray(ref string[] fields, int maxLength, out int[] removed) {
			Array.Resize(ref fields, maxLength);
			List<string> fieldsList = new List<string>(fields);
			List<int> removedList = new List<int>();
			for (int ix = fields.Length-1; ix >= 0; ix--) {
				if (string.IsNullOrEmpty(fields[ix])) {
					fieldsList.RemoveAt(ix);
					removedList.Add(ix);
				}
			}
			fields = fieldsList.ToArray();
			removed = removedList.ToArray();
		}

		private static void MatchFieldArray(ref string[] row, int[] removed/*, int primaryIndex*/) {
			// assumption: the row has already been correctly
			// sized to match the max length of all columns

			// remove unused columns from the row (if needed)
			if (removed.Length != 0) {
				List<string> rowList = new List<string>(row);
				foreach (int index in removed) {
					rowList.RemoveAt(index);
				}
				row = rowList.ToArray();
			}
		}

		private static bool IsStringArrayEmpty(string[] array) {
			if (array == null || array.Length == 0) {
				return true;
			}
			bool completelyEmpty = true;
			for (int ix = 0; ix < array.Length; ix++) {
				if (!string.IsNullOrEmpty(array[ix])) {
					completelyEmpty = false;
					break;
				}
			}
			return completelyEmpty;
		}


		private class Enumerator : IEnumerator<Row> {
			public Row Current {
				get {
					return new Row(
						m_primaryKey, 
						m_source.m_fieldNames, 
						m_source.m_fieldHashes, 
						m_source.m_rows[m_index]
					);
				}
			}
			object IEnumerator.Current {
				get { return Current; }
			}

			private DataSheet m_source;
			private string m_primaryKey;
			private int m_index;

			public Enumerator(DataSheet source, string primaryKey) {
				m_source = source;
				m_primaryKey = primaryKey;
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