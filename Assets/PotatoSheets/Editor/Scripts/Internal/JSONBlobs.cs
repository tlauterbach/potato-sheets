using System;

namespace PotatoSheets.Editor {
	

	// SPREADSHEET BLOBS

	[Serializable]
	internal class SpreadsheetBlob {
		public SheetBlob[] sheets;
	}
	[Serializable]
	internal class SheetBlob {
		public PropertiesBlob properties;
		public DataBlob[] data;
	}
	[Serializable]
	internal class PropertiesBlob {
		public string title;
	}
	[Serializable]
	internal class DataBlob {
		public RowDataBlob[] rowData;
	}
	[Serializable]
	internal class RowDataBlob {
		public ValueBlob[] values;
	}
	[Serializable]
	internal class ValueBlob {
		public string formattedValue;
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
	}


}
