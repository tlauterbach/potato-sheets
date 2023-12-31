﻿using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PotatoSheets.Editor {

	internal class ImportUtility : IImportUtility {

		private enum DirectoryType {
			Empty,
			TrailingSlash,
			NoSlash
		}

		public DataSheet DataSheet { get; private set; }
		public string PrimaryKey { get; private set; }
		public string AssetDirectory { get; private set; }

		public bool HasErrors { get { return m_state.HasErrors; } }

		private ImportState m_state;
		private DirectoryType m_directoryType;


		public ImportUtility(ImportState state) {
			m_state = state;
		}

		public void Reset(DataSheet dataSheet, string primaryKey, string assetDirectory) {
			DataSheet = dataSheet;
			PrimaryKey = primaryKey;
			AssetDirectory = assetDirectory.Trim();
			if (string.IsNullOrEmpty(AssetDirectory)) {
				m_directoryType = DirectoryType.Empty;
			} else {
				char lastChar = AssetDirectory[^1];
				if (lastChar == '\\' || lastChar == '/') {
					m_directoryType = DirectoryType.TrailingSlash;
				} else {
					m_directoryType = DirectoryType.NoSlash;
				}
			}
		}


		public string BuildAssetPath(string assetName, string assetExtension = "asset") {
			switch (m_directoryType) {
				case DirectoryType.Empty:
					return string.Concat($"Assets/{assetName}.{assetExtension}");
				case DirectoryType.NoSlash:
					return string.Concat(AssetDirectory, $"/{assetName}.{assetExtension}");
				case DirectoryType.TrailingSlash:
					return string.Concat(AssetDirectory, $"{assetName}.{assetExtension}");
				default:
					throw new NotImplementedException();
			}
		}

		public T FindOrCreateAsset<T>(string path) where T : ScriptableObject {
			return FindOrCreateAsset(typeof(T), path) as T;
		}

		public ScriptableObject FindOrCreateAsset(Type type, string path) {
			UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(path, type);
			if (asset == null) {
				asset = ScriptableObject.CreateInstance(type);
				string directory = Path.GetDirectoryName(path);
				if (!Directory.Exists(directory)) {
					Directory.CreateDirectory(directory);
				}
				AssetDatabase.CreateAsset(asset, path);
				EditorUtility.SetDirty(asset);
			}
			return asset as ScriptableObject;
		}

		public bool FindAssetByName<T>(string name, out T asset) where T : UnityEngine.Object {
			bool success = FindAssetByName(typeof(T), name, out UnityEngine.Object obj);
			asset = obj as T;
			return success;
		}

		public bool FindAssetByName(Type type, string name, out UnityEngine.Object asset) {
			string[] guids = AssetDatabase.FindAssets($"{name} t:{type.FullName}");
			if (guids == null || guids.Length <= 0) {
				asset = null;
				return false;
			}
			asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), type);
			return true;
		}

		public void LogError(string error) {
			((ILogger)m_state).LogError(error);
		}

		public void LogError(Exception error) {
			((ILogger)m_state).LogError(error);
		}

		public void LogWarning(string warning) {
			((ILogger)m_state).LogWarning(warning);
		}
	}

}