using System;

namespace PotatoSheets {

	/// <summary>
	/// Place over a field or property in an Automatic import type
	/// ContentAsset ScriptableObject and provide the expected field
	/// name from the spreadsheet you will be reading from.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class ContentAttribute : Attribute {

		public string FieldName { get; }

		public ContentAttribute(string fieldName) {
			FieldName = fieldName;
		}

	}

}
