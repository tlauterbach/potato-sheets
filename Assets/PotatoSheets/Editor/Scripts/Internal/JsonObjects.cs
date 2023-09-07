using JsonParser;
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

		public ValueRangeBlob(JsonBlob blob) {
			range = blob["range"].AsString;
			majorDimension = blob["majorDimension"].AsString;
			JsonBlob array = blob["values"];
			int size = array.Count;
			values = new string[size][];
			for (int ix = 0; ix < size; ix++) {
				int subSize = array[ix].Count;
				values[ix] = new string[subSize];
				for (int iy = 0; iy < subSize; iy++) {
					values[ix][iy] = array[ix][iy].AsString;
				}
			}

		}

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
		public string last_sign_in;
	}


}
