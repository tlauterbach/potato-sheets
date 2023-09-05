

using System;
using UnityEngine;

namespace PotatoSheets.Editor {


	public interface IImportUtility : ILogger {

		/// <summary>
		/// Object containing imported data retrieved from a GoogleSheet
		/// </summary>
		DataSheet DataSheet { get; }
		/// <summary>
		/// Directory relative to the project folder where any assets should be created
		/// </summary>
		string AssetDirectory { get; }

		string BuildAssetPath(string assetName, string extension = "asset");

		/// <summary>
		/// Finds or Creates an Asset of the specified type at the specified path
		/// </summary>
		/// <typeparam name="T">type of asset to find</typeparam>
		/// <param name="path">path to find or create an asset at</param>
		/// <returns>the found or newly created asset</returns>
		T FindOrCreateAsset<T>(string path) where T : ScriptableObject;
		ScriptableObject FindOrCreateAsset(Type type, string path);

		/// <summary>
		/// Looks for an asset of the given type with the given name in the <see cref="AssetDatabase"/>
		/// </summary>
		/// <typeparam name="T">type of asset to find</typeparam>
		/// <param name="name">name of the asset to find</param>
		/// <param name="asset">the resulting asset if found, null if not found</param>
		/// <returns>TRUE if the asset was found, FALSE if the asset was not found</returns>
		bool FindAssetByName<T>(string name, out T asset) where T : UnityEngine.Object;
		bool FindAssetByName(Type type, string name, out UnityEngine.Object asset);

		/// <summary>
		/// Uses reflection to set any <see cref="ContentAttribute"/> fields to the values
		/// of the provided row data.
		/// </summary>
		/// <param name="target">object to apply data to</param>
		/// <param name="data">data used to populate the target's fields/properties</param>
		void SetContent(object target, Row data);

	}

}