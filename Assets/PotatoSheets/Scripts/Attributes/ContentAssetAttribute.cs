using System;

namespace PotatoSheets {

	public enum ImportType {
		/// <summary>
		/// Creates a new asset for each row of the the 
		/// worksheet and names the asset after the 
		/// primary key value of the row.
		/// </summary>
		Automatic,
		/// <summary>
		/// Data creation becomes customizable through 
		/// the required Import and LateImport functions 
		/// of the ContentAsset
		/// </summary>
		Custom
	}


	/// <summary>
	/// Use this attribute on a ScriptableObject type to register
	/// it in the Asset Type list of the PotatoSheets Importer.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class ContentAssetAttribute : Attribute {
		public ImportType ImportType { get; }
		public string PrimaryKey { get; }
		public ContentAssetAttribute(ImportType importType, string primaryKey) {
			ImportType = importType;
			PrimaryKey = primaryKey;
		}
	}

}