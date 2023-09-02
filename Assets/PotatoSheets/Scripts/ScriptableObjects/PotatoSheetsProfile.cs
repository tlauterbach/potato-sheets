using System;
using System.Collections.Generic;
using UnityEngine;

namespace PotatoSheets {

	public class PotatoSheetsProfile : ScriptableObject {

		[SerializeField]
		private List<Profile> m_profiles;
		[HideInInspector,SerializeField]
		private int m_nextProfileIndex = 0;

		public string GetNextProfileName() {
			return $"Profile{m_nextProfileIndex++}";
		}

		public void AddNewProfile() {
			m_profiles.Add(new Profile() {
				ProfileName = GetNextProfileName()
			});
		}


		[Serializable]
		private class Profile {
			public string ProfileName;
			public string SheetID;
			public string WorksheetName;
			public string AssetType;
			public string AssetDirectory;
		}


	}


}