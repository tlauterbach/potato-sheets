using UnityEditor;
using UnityEngine;

namespace PotatoSheets.Editor {

	[FilePath("PotatoSheets/Settings.asset", FilePathAttribute.Location.PreferencesFolder)]
	internal class PotatoSheetsSettings : ScriptableSingleton<PotatoSheetsSettings> {

		public string ClientSecretPath {
			get { return m_clientSecretPath; }
			set {
				m_clientSecretPath = value;
				Save(true);
			}
		}
		public string CredentialsPath {
			get { return m_credentialsPath; }
			set {
				m_credentialsPath = value;
				Save(true);
			}
		}
		public PotatoSheetsProfile Profile {
			get { return m_profile; }
			set {
				m_profile = value;
				Save(true);
			}
		}

		[SerializeField]
		private string m_clientSecretPath = "../tools/potato-sheets/client-secret.json";
		[SerializeField]
		private string m_credentialsPath = "../tools/potato-sheets/credentials.json";
		[SerializeField]
		private PotatoSheetsProfile m_profile = null;

	}

}