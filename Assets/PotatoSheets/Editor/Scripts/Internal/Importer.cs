using PotatoSheets.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace PotatoSheets.Editor {


	internal class Importer {

		public event Action<string, float> OnProgressChanged;

		public bool IsImporting { get { return m_importRoutine != null; } }

		private EditorCoroutine m_importRoutine;
		private ImportState m_state;
		private CredentialsBlob m_credentials;
		private CallbackBindings m_callbacks;
		private List<FetchRoutine<SpreadsheetBlob>> m_metaDataRoutines;
		private List<FetchRoutine<ValueRangeBlob>> m_valuesRoutines;
		private JsonParser m_jsonParser = new JsonParser();

		private const string METADATA_ENDPOINT = "https://sheets.googleapis.com/v4/spreadsheets/{0}?fields=sheets.properties(title,gridProperties)";
		private const string VALUES_ENDPOINT = "https://sheets.googleapis.com/v4/spreadsheets/{0}/values/{1}?majorDimension=ROWS";
		

		public void SetCredentials(CredentialsBlob blob) {
			m_credentials = blob;
		}

		public void Import(PotatoSheetsProfile profile, Action onComplete, Action<IEnumerable<Exception>> onError) {
			Import((IEnumerable<PotatoSheetsProfile.Profile>)profile, onComplete, onError);
		}
		public void Import(PotatoSheetsProfile.Profile profile, Action onComplete, Action<IEnumerable<Exception>> onError) {
			Import(new List<PotatoSheetsProfile.Profile>() { profile }, onComplete, onError);
		}

		public void CancelImport() {

		}

		private void Import(IEnumerable<PotatoSheetsProfile.Profile> profiles, Action onComplete, Action<IEnumerable<Exception>> onError) {
			ImportState state = new ImportState(profiles, onComplete, onError);
			// if we're already importing, ignore the request and error
			if (IsImporting) {
				state.LogError("Already trying to Import. Did you forget to Abort?");
			}
			// credentials are required to import
			if (m_credentials == null) {
				state.LogError("No credentials have been assigned. Call `SetCredentials' before importing");
			}
			// confirm that all of the requested profiles can bind properly
			foreach (PotatoSheetsProfile.Profile profile in profiles) {
				if (state.HasAssetBinding(profile.AssetType)) {
					continue; // already included, don't need duplicates
				}
				AssetBindings binding = new AssetBindings(state, profile);
				if (!state.HasErrors) {
					state.AddAssetBinding(profile.AssetType, binding);
				}
			}
			// confirm that all callbacks are bound properly as well
			m_callbacks = new CallbackBindings(state);

			if (state.HasErrors) {
				state.Complete();
				return;
			}

			m_state = state;
			m_importRoutine = EditorCoroutineUtility.StartCoroutine(ImportRoutine(profiles), this);
		}

		private IEnumerator ImportRoutine(IEnumerable<PotatoSheetsProfile.Profile> profiles) {


			// 1. create a set of unique sheets that we need to poll data from
			SetProgress("Building Sheets Request...", 0.1f);
			List<string> sheetIDs = new List<string>();
			List<string> sheetProfiles = new List<string>();
			foreach (PotatoSheetsProfile.Profile profile in profiles) {
				bool idExists = false;
				for (int ix = 0; ix < sheetIDs.Count; ix++) {
					if (StringComparer.Ordinal.Equals(sheetIDs[ix], profile.SheetID)) {
						sheetProfiles[ix] = string.Concat(sheetProfiles[ix], ", ", profile.ProfileName);
						idExists = true;
						break;
					}
				}
				if (!idExists) {
					sheetIDs.Add(profile.SheetID);
					sheetProfiles.Add(profile.ProfileName);
				}
			}
			Assert.AreEqual(sheetIDs.Count, sheetProfiles.Count);

			// 2. send requests for each of the sheet ids to get meta data
			SetProgress("Fetching Meta Data...", 0.2f);
			m_metaDataRoutines = new List<FetchRoutine<SpreadsheetBlob>>();
			for (int ix = 0; ix < sheetIDs.Count; ix++) {
				string url = string.Format(METADATA_ENDPOINT, sheetIDs[ix]);

				m_metaDataRoutines.Add(new FetchRoutine<SpreadsheetBlob>(
					this, sheetProfiles[ix], url, 
					x => JsonUtility.FromJson<SpreadsheetBlob>(x.downloadHandler.text)
				));
			}
			yield return WaitForFetchesToComplete(m_metaDataRoutines);

			// 3. check for errors and end routine if there are any
			if (m_state.HasErrors) {
				Cleanup();
				yield break;
			}

			// 4. add the metadata to the state and poll needed individual worksheets
			SetProgress("Adding Meta Data...", 0.3f);
			for (int ix = 0; ix < sheetIDs.Count; ix++) {
				m_state.AddMetaData(sheetIDs[ix], m_metaDataRoutines[ix].Result);
			}
			m_metaDataRoutines.Clear();
			m_metaDataRoutines = null;

			// 5. validate that all of the needed worksheets exist on their respective spreadsheets
			SetProgress("Validating Worksheets...", 0.4f);
			List<WorksheetID> worksheets = new List<WorksheetID>();
			sheetProfiles.Clear();

			foreach (PotatoSheetsProfile.Profile profile in profiles) {
				WorksheetID id = new WorksheetID(profile.SheetID, profile.WorksheetName);
				int existing = worksheets.IndexOf(id);
				if (existing != -1) {
					sheetProfiles[existing] = string.Concat(sheetProfiles[existing], ", ", profile.ProfileName);
					continue;
				}
				if (m_state.HasWorksheet(id)) {
					worksheets.Add(id);
					sheetProfiles.Add(profile.ProfileName);
				} else {
					m_state.LogError($"Error in profile {profile.ProfileName}: worksheet `{profile.WorksheetName}' does not exist on the specified spreadsheet identity");
				}
			}
			Assert.AreEqual(worksheets.Count, sheetProfiles.Count);

			if (m_state.HasErrors) {
				Cleanup();
				yield break;
			}

			// 6. use the properties information provided in the worksheets set to poll needed spreadsheet values
			SetProgress("Fetching Worksheets...", 0.5f);
			m_valuesRoutines = new List<FetchRoutine<ValueRangeBlob>>();
			for (int ix = 0; ix < worksheets.Count; ix++)  {
				
				if (m_state.TryGetA1Max(worksheets[ix], out string a1Max)) {
					string url = string.Format(VALUES_ENDPOINT,
						worksheets[ix].SheetID,
						$"{worksheets[ix].WorksheetName}!A1:{a1Max}"
					);
					m_valuesRoutines.Add(new FetchRoutine<ValueRangeBlob>(
						this, sheetProfiles[ix], url,
						x => new ValueRangeBlob(m_jsonParser.Parse(x.downloadHandler.text))
					));
				} else {
					m_state.LogError($"Error in {sheetProfiles[ix]}: Could not determine the size of the worksheet. Is the google data bad?");
				}
			}
			if (m_state.HasErrors) {
				Cleanup();
				yield break;
			}
			yield return WaitForFetchesToComplete(m_valuesRoutines);

			Assert.AreEqual(worksheets.Count, m_valuesRoutines.Count);

			// 7. compile the values received and convert them to DataSheet objects
			for (int ix = 0; ix < worksheets.Count; ix++) {
				m_state.AddDataSheet(worksheets[ix], m_valuesRoutines[ix].Result);
			}

			// 8. use added bindings to iterate over datasheets for profiles to do Import stage
			ImportUtility util = new ImportUtility(m_state);
			DataSheet dataSheet;
			AssetBindings bindings;
			float value = 0.6f;
			try {
				foreach (PotatoSheetsProfile.Profile profile in profiles) {
					SetProgress("Executing Import...", value);
					WorksheetID worksheetID = new WorksheetID(profile.SheetID, profile.WorksheetName);
					dataSheet = m_state.GetDataSheet(worksheetID);
					bindings = m_state.GetAssetBindings(profile.AssetType);
					util.Reset(dataSheet, bindings.PrimaryKey, profile.AssetDirectory);
				
					bindings.Import(util);
					value += (1f / profiles.Count()) * 0.15f;
				}
			} catch (Exception e) {
				m_state.LogError(e);
			}
			if (m_state.HasErrors) {
				Cleanup();
				yield break;
			}

			// 9. use added bindings to do the LateImport stage
			try {
				foreach (PotatoSheetsProfile.Profile profile in profiles) {
					SetProgress("Executing Late Import...", value);
					WorksheetID worksheetID = new WorksheetID(profile.SheetID, profile.WorksheetName);
					dataSheet = m_state.GetDataSheet(worksheetID);
					bindings = m_state.GetAssetBindings(profile.AssetType);
					util.Reset(dataSheet, bindings.PrimaryKey, profile.AssetDirectory);

					bindings.LateImport(util);
					value += (1f / profiles.Count()) * 0.15f;
				}
			} catch (Exception e) {
				m_state.LogError(e);
			}
			if (m_state.HasErrors) {
				Cleanup();
				yield break;
			}

			// 10. save the asset database now that everything is imported
			SetProgress("Saving Assets...", 0.9f);
			AssetDatabase.SaveAssets();
			yield return null;

			// 11. send callbacks to ContentCallback attribute holders
			SetProgress("Sending Callbacks...", 0.95f);
			m_callbacks.OnComplete();
			yield return null;

			// x. Cleanup and complete the import process
			Cleanup();

		}

		private void SetProgress(string message, float percent) {
			OnProgressChanged?.Invoke(message, percent);
		}

		private void Cleanup() {
			m_state.Complete();
			if (m_importRoutine != null) {
				EditorCoroutineUtility.StopCoroutine(m_importRoutine);
			}
			CleanupFetchSet(m_metaDataRoutines);
			CleanupFetchSet(m_valuesRoutines);
			m_metaDataRoutines = null;
			m_valuesRoutines = null;

			m_importRoutine = null;
			m_callbacks = null;
		}
		private void CleanupFetchSet<T>(IEnumerable<FetchRoutine<T>> set) {
			if (set != null) {
				foreach (FetchRoutine<T> fetch in set) {
					fetch.Abort();
				}
			}
		}

		private IEnumerator WaitForFetchesToComplete<T>(IEnumerable<FetchRoutine<T>> fetches) {
			while (true) {
				bool isComplete = true;
				foreach (FetchRoutine<T> routine in fetches) {
					if (!routine.IsDone) {
						isComplete = false;
						break;
					}
				}
				if (isComplete) {
					break;
				} else {
					yield return null;
				}
			}
		}

		private class FetchRoutine<T> : IEnumerator {

			public bool IsDone { get; private set; } = false;
			public T Result { get; private set; } = default;
			public object Current { get { return null; } }

			private Importer m_importer;
			private string m_profiles;
			private EditorCoroutine m_routine;
			private UnityWebRequest m_request;
			private UnityWebRequestAsyncOperation m_operation;
			private Func<UnityWebRequest, T> m_converter;

			public FetchRoutine(Importer importer, string profiles, string url, Func<UnityWebRequest,T> converter) {
				m_importer = importer;
				m_profiles = profiles;
				m_converter = converter;
				m_request = UnityWebRequest.Get(url);
				m_request.SetRequestHeader("Authorization", $"Bearer {importer.m_credentials.access_token}");
				m_operation = m_request.SendWebRequest();
				m_routine = EditorCoroutineUtility.StartCoroutine(this, importer);
			}

			public void Abort() {
				if (m_request != null) {
					m_request.Dispose();
					m_request = null;
				}
				if (m_routine != null) {
					EditorCoroutineUtility.StopCoroutine(m_routine);
					m_routine = null;
				}
				m_operation = null;
				IsDone = true;
			}

			public bool MoveNext() {
				if (IsDone) {
					return false;
				} else if (m_operation != null && m_operation.isDone) {
					if (m_request.result == UnityWebRequest.Result.Success) {
						try {
							Result = m_converter(m_request);
						} catch (Exception e) {
							Result = default;
							m_importer.m_state.LogError($"Error parsing {typeof(T).Name}: {e.Message}");
						}
					} else {
						if (m_request.downloadHandler == null) {
							m_importer.m_state.LogError($"Error fetching {m_profiles}: {m_request.error}");
						} else {
							m_importer.m_state.LogError($"Error fetching {m_profiles}: {m_request.error}\n{m_request.downloadHandler.text}");
						}
					}
					IsDone = true;
					m_request.Dispose();
					m_request = null;
					m_operation = null;
					return false;
				} else {
					return true;
				}
			}

			public void Reset() {
				throw new NotImplementedException();
			}
		}


	}

}