using System;

namespace PotatoSheets.Editor {

	public struct WorksheetID : IEquatable<WorksheetID> {

		public string SheetID { get { return m_sheetID; } }
		public string WorksheetName { get { return m_worksheetName; } }

		private string m_sheetID;
		private string m_worksheetName;
		private uint m_hash;

		public WorksheetID(string sheetID, string worksheetName) {
			m_sheetID = sheetID;
			m_worksheetName = worksheetName;
			m_hash = Util.FNVHash(string.Concat(m_sheetID, m_worksheetName));
		}

		public bool Equals(WorksheetID other) {
			return other.m_hash == m_hash;
		}

		public override int GetHashCode() {
			return (int)m_hash;
		}
		public override bool Equals(object obj) {
			if (obj is WorksheetID id) {
				return Equals(id);
			} else {
				return false;
			}
		}

	}

}