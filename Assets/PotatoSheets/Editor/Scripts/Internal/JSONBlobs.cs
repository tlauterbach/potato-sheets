using System;

namespace PotatoSheets.Editor {
	

	// MetaData BLOBS
	[Serializable]
	internal class SpreadsheetBlob {
		public SheetBlob[] sheets;
	}
	[Serializable]
	internal class SheetBlob {
		public PropertiesBlob properties;
	}
	[Serializable]
	internal class PropertiesBlob {
		public string title;
		public GridPropertiesBlob gridProperties;
	}
	[Serializable]
	internal class GridPropertiesBlob {
		public int rowCount;
		public int columnCount;
		public int frozenRowCount;
	}

	// VALUES BLOBS
	[Serializable]
	internal class ValueRangeBlob {
		public string range;
		public string majorDimension;
		public string[][] values;
	}

	// OAUTH BLOBS
	[Serializable]
	internal class ClientSecretBlob {
		public InstalledBlob installed;
	}
	[Serializable]
	internal class InstalledBlob {
		public string client_id;
		public string project_id;
		public string auth_uri;
		public string token_uri;
		public string auth_provider_x509_cert_url;
		public string client_secret;
		public string[] redirect_uris;
	}
	[Serializable]
	internal class CredentialsBlob {
		public string access_token;
		public int expires_in;
		public string token_type;
		public string scope;
		public string refresh_token;
		public uint last_sign_in;
	}


}
