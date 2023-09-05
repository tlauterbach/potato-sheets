using System;

namespace PotatoSheets {

	/// <summary>
	/// Place over a field or property in an Automatic import type
	/// <see cref="ContentAssetAttribute"/> ScriptableObject and provide 
	/// the expected field name from the spreadsheet you will be 
	/// reading from (Alias). If alias is left empty, the name of the
	/// field or property will be used.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
	public class ContentAttribute : Attribute {
		public string Alias { get; }
		public string Delimiter { get; set; } = string.Empty;
		public ContentAttribute() {
			Alias = string.Empty;
		}
		public ContentAttribute(string alias) {
			Alias = alias;
		}
	}

}
