using System;

namespace PotatoSheets {


	/// <summary>
	/// Place on a class to receive callbacks for events that
	/// occur during import, primarily when the Import is over 
	/// (use `static void OnImportComplete()')
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class ContentCallbacksAttribute : Attribute {

		public int Order { get; set; } = 100;

	}

}