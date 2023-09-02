using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PotatoSheets {

	public class PotatoSheetsProfile : ScriptableObject, IReadOnlyCollection<PotatoSheetsProfile.Profile> {

		[Serializable]
		public class Profile {
			public string ProfileName;
			public string SheetID;
			public string WorksheetName = "Sheet1";
			public string AssetType = "None";
			public string AssetDirectory = "Assets/Data";
		}

		public Profile this[int index] {
			get {
				if (index < 0 || index >= Count) {
					throw new IndexOutOfRangeException();
				}
				return m_profiles[index];
			}
		}
		public int Count { get { return m_profiles.Count; } }

		[SerializeField]
		private List<Profile> m_profiles;
		[HideInInspector,SerializeField]
		private int m_nextProfileIndex = 0;

		public void AddNewProfile() {
			m_profiles.Add(new Profile() {
				ProfileName = $"Profile{m_nextProfileIndex++}"
			});
		}
		public void RemoveProfile(int index) {
			m_profiles.RemoveAt(index);
		}

		public IEnumerator<Profile> GetEnumerator() {
			return ((IEnumerable<Profile>)m_profiles).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return ((IEnumerable)m_profiles).GetEnumerator();
		}
	}


}