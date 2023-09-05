using System;

namespace PotatoSheets {

	public enum ImportType {
		/// <summary>
		/// Creates a new asset for each row of the the 
		/// worksheet and names the asset after the 
		/// primary key value of the row. <see cref="ContentAttribute"/>
		/// must be specified on fields or properties for them 
		/// to be linked when importing.
		/// </summary>
		Automatic,
		/// <summary>
		/// Data creation becomes customizable through 
		/// the required Import and LateImport functions 
		/// of the <see cref="ContentAssetAttribute"/>
		/// </summary>
		Manual
	}


	/// <summary>
	/// Use this attribute on a ScriptableObject type to register
	/// it in the Asset Type list of the PotatoSheets Importer.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class ContentAssetAttribute : Attribute {
		public ImportType ImportType { get; }
		public string PrimaryKey { get; }

		/// <summary>
		/// Specifies that the given class is a ContentAsset and
		/// will make it appear in the PotatoSheets AssetType
		/// dropdown.
		/// </summary>
		/// <param name="importType">ImportType used when this asset type is imported</param>
		/// <param name="primaryKey">primary key used to differentiate assets created</param>
		public ContentAssetAttribute(ImportType importType, string primaryKey) {
			ImportType = importType;
			PrimaryKey = primaryKey;
		}
	}

}