using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace PotatoSheets.Editor {

	internal class ImportState : ILogger {

		public bool HasErrors { get { return m_errors.Count > 0; } }

		public IEnumerable<PotatoSheetsProfile.Profile> Profiles { get; }

		private List<Exception> m_errors;
		private Action m_onComplete;
		private Action<IEnumerable<Exception>> m_onError;
		private Dictionary<WorksheetID, PropertiesBlob> m_metaData;
		private Dictionary<WorksheetID, DataSheet> m_dataSheets;
		private Dictionary<string, AssetBindings> m_assetBindings;

		private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		public ImportState(IEnumerable<PotatoSheetsProfile.Profile> profiles, Action onComplete, Action<IEnumerable<Exception>> onError) {
			m_errors = new List<Exception>();
			m_metaData = new Dictionary<WorksheetID,PropertiesBlob>();
			m_dataSheets = new Dictionary<WorksheetID, DataSheet>();
			m_assetBindings = new Dictionary<string, AssetBindings>();

			Profiles = profiles;
			m_onComplete = onComplete;
			m_onError = onError;
		}

		public void LogError(string error) {
			m_errors.Add(new Exception(error));
		}
		public void LogError(Exception error) {
			m_errors.Add(error);
		}
		public void LogWarning(string warning) {
			Debug.LogWarning(warning);
		}

		public void AddMetaData(string sheetId, SpreadsheetBlob metaData) {
			foreach (SheetBlob worksheet in metaData.sheets) {
				WorksheetID id = new WorksheetID(sheetId, worksheet.properties.title);
				m_metaData.Add(id, worksheet.properties);
			}
		}
		public void AddDataSheet(WorksheetID id, ValueRangeBlob valueRange) {
			int frozenRows = m_metaData[id].gridProperties.frozenRowCount;
			m_dataSheets.Add(id, new DataSheet(id, valueRange, frozenRows));
		}
		public void AddAssetBinding(string assetType, AssetBindings bindings) {
			m_assetBindings.Add(assetType, bindings);
		}


		public bool HasWorksheet(WorksheetID worksheetID) {
			return m_metaData.ContainsKey(worksheetID);
		}
		public DataSheet GetDataSheet(WorksheetID worksheetID) {
			return m_dataSheets[worksheetID];
		}
		public bool HasAssetBinding(string assetType) {
			return m_assetBindings.ContainsKey(assetType);
		}
		public AssetBindings GetAssetBindings(string assetType) {
			return m_assetBindings[assetType];
		}

		public bool TryGetA1Max(WorksheetID worksheetID, out string range) {
			if (m_metaData.TryGetValue(worksheetID, out PropertiesBlob blob)) {
				if (blob.gridProperties.rowCount == 0 || blob.gridProperties.columnCount == 0) {
					range = string.Empty;
					return false;
				}
				range = ToA1Notation(blob.gridProperties.rowCount, blob.gridProperties.columnCount);
				return true;
			}
			range = string.Empty;
			return false;
		}

		public void Complete() {
			if (HasErrors) {
				m_onError?.Invoke(m_errors);
			} else {
				m_onComplete?.Invoke();
			}
		}

		private string ToA1Notation(int rows, int columns) {
			string a1Notation = "";
			while (columns > 0) {
				int remainder = (columns - 1) % ALPHABET.Length;
				a1Notation = string.Concat(ALPHABET[remainder], a1Notation);
				columns = (columns - remainder) / ALPHABET.Length;
			}
			a1Notation = string.Concat(a1Notation, rows.ToString(CultureInfo.InvariantCulture));
			return a1Notation;
		}

	}

}