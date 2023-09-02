using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PotatoSheets.Editor {

	public class PotatoSheetsWindow : EditorWindow {

		[SerializeField]
		private VisualTreeAsset m_windowUxmlAsset;

		// Internal Data
		private Authentication m_authenticator;
		private CredentialsBlob m_credentials;

		private PotatoSheetsProfile MainProfile {
			get { return PotatoSheetsSettings.instance.Profile; }
		}
		private bool HasProfile {
			get { return PotatoSheetsSettings.instance.Profile != null; }
		}
		private PotatoSheetsProfile.Profile SelectedProfile {
			get {
				if (PotatoSheetsSettings.instance.Profile == null ||
					m_selectedProfileIndex < 0 || 
					m_selectedProfileIndex >= PotatoSheetsSettings.instance.Profile.Count) 
				{
					return null;
				}
				return PotatoSheetsSettings.instance.Profile[m_selectedProfileIndex]; 
			} 
		}
		private int m_selectedProfileIndex = -1;
		private static readonly Color SELECTION_COLOR = new Color32(44,93,135,255);
		private static readonly Color CLEAR_COLOR = new Color(0, 0, 0, 0);
		private static readonly Color DARKEN_COLOR = new Color(0, 0, 0, 0.15f);

		// VisualElements/Controls
		private TextField m_clientSecretPath;
		private TextField m_credentialsPath;
		private ObjectField m_profile;
		private Button m_profileCreateNewButton;
		private ProgressBar m_progressBar;

		private VisualElement m_profileGroup;

		private ScrollView m_profileList;
		private Button m_addButton;
		private Button m_removeButton;
		private Button m_importAllButton;

		private VisualElement m_profileSettingsGroup;
		private TextField m_profileName;
		private TextField m_sheetId;
		private TextField m_worksheetName;
		private DropdownField m_assetType;
		private TextField m_assetDirectory;
		private Button m_importButton;

		private VisualElement m_oauthWaitMessage;
		private Button m_oauthWaitCancelButton;


		[MenuItem("Window/Tools/PotatoSheets")]
		public static void Create() {
			EditorWindow window = GetWindow<PotatoSheetsWindow>("Potato Sheets Importer");
		}

		public void CreateGUI() {
			m_windowUxmlAsset.CloneTree(rootVisualElement);

			// initialize fields

			m_authenticator = new Authentication();

			m_clientSecretPath = rootVisualElement.Q<TextField>("ClientSecretPath");
			m_credentialsPath = rootVisualElement.Q<TextField>("CredentialsPath");
			m_profile = rootVisualElement.Q<ObjectField>("Profile");
			m_profileCreateNewButton = rootVisualElement.Q<Button>("ProfileCreateNewButton");
			m_progressBar = rootVisualElement.Q<ProgressBar>("ProgressBar");
			
			m_profileGroup = rootVisualElement.Q("ProfileGroup");

			m_profileList = rootVisualElement.Q<ScrollView>("ProfileList");
			m_addButton = rootVisualElement.Q<Button>("AddButton");
			m_removeButton = rootVisualElement.Q<Button>("RemoveButton");
			m_importAllButton = rootVisualElement.Q<Button>("ImportAllButton");

			m_profileSettingsGroup = rootVisualElement.Q("ProfileSettingsGroup");
			m_profileName = rootVisualElement.Q<TextField>("ProfileName");
			m_sheetId = rootVisualElement.Q<TextField>("SheetID");
			m_worksheetName = rootVisualElement.Q<TextField>("WorksheetName");
			m_assetType = rootVisualElement.Q<DropdownField>("AssetType");
			m_assetDirectory = rootVisualElement.Q<TextField>("AssetDirectory");

			m_oauthWaitMessage = rootVisualElement.Q("OAuthWaitMessage");
			m_oauthWaitCancelButton = rootVisualElement.Q<Button>("OAuthWaitCancelButton");

			// state setup

			m_clientSecretPath.value = PotatoSheetsSettings.instance.ClientSecretPath;
			m_credentialsPath.value = PotatoSheetsSettings.instance.CredentialsPath;
			HandleProfileChanged(PotatoSheetsSettings.instance.Profile);
			SetOAuthMessageDisplayed(false);

			// Register Callbacks

			m_clientSecretPath.RegisterValueChangedCallback(HandleClientSecretPathChanged);
			m_credentialsPath.RegisterValueChangedCallback(HandleCredentialsPathChanged);
			m_profile.RegisterValueChangedCallback(x => HandleProfileChanged(x.newValue as PotatoSheetsProfile));

			m_profileName.RegisterValueChangedCallback(HandleProfileNameChanged);

			m_addButton.RegisterCallback<MouseDownEvent>(HandleAddProfileButton, TrickleDown.TrickleDown);
			m_removeButton.RegisterCallback<MouseDownEvent>(HandleRemoveProfileButton, TrickleDown.TrickleDown);
			m_profileCreateNewButton.RegisterCallback<MouseDownEvent>(HandleProfileCreateNewButton, TrickleDown.TrickleDown);
			m_importAllButton.RegisterCallback<MouseDownEvent>(HandleImportAllButton, TrickleDown.TrickleDown);
			m_oauthWaitCancelButton.RegisterCallback<MouseDownEvent>(HandleOAuthWaitCancelButton, TrickleDown.TrickleDown);

		}

		public void OnProjectChange() {
			SetProfileGroupDisplayed(PotatoSheetsSettings.instance.Profile != null);
		}
		public void OnDestroy() {
			m_authenticator?.CancelAuthentication();
		}

		private void RefreshProfileList() {
			m_profileList.contentContainer.Clear();
			if (PotatoSheetsSettings.instance.Profile != null) {
				for (int ix = 0; ix < PotatoSheetsSettings.instance.Profile.Count; ix++) {
					Label label = new Label(PotatoSheetsSettings.instance.Profile[ix].ProfileName);
					if (ix == m_selectedProfileIndex) {
						label.style.backgroundColor = SELECTION_COLOR;
					} else {
						label.style.backgroundColor = ((ix % 2) == 0) ? CLEAR_COLOR : DARKEN_COLOR;
					}
					label.style.paddingTop = 2;
					label.style.paddingBottom = 2;
					label.style.paddingLeft = 8;
					m_profileList.contentContainer.Add(label);
					int index = ix;
					label.RegisterCallback<MouseDownEvent>(x => HandleProfileClicked(index), TrickleDown.NoTrickleDown);
				}
			}

			RefreshProfileSettings();
		}

		private void RefreshProfileSettings() {
			PotatoSheetsProfile.Profile profile = SelectedProfile;

			SetProfileSettingsDisplayed(profile != null);
			if (profile == null) {
				
				m_profileName.Unbind();
				m_sheetId.Unbind();
				m_worksheetName.Unbind();
				m_assetType.Unbind();
				m_assetDirectory.Unbind();
			} else {
				SerializedProperty property = new SerializedObject(MainProfile)
					.FindProperty("m_profiles")
					.GetArrayElementAtIndex(m_selectedProfileIndex);

				m_profileName.BindProperty(property.FindPropertyRelative("ProfileName"));
				m_sheetId.BindProperty(property.FindPropertyRelative("SheetID"));
				m_worksheetName.BindProperty(property.FindPropertyRelative("WorksheetName"));
				m_assetType.BindProperty(property.FindPropertyRelative("AssetType"));
				m_assetDirectory.BindProperty(property.FindPropertyRelative("AssetDirectory"));
			}
		}

		private void SetProfileGroupDisplayed(bool isDisplayed) {
			m_profileGroup.style.display = isDisplayed ? DisplayStyle.Flex : DisplayStyle.None;
		}

		private void SetOAuthMessageDisplayed(bool isDisplayed) {
			m_oauthWaitMessage.style.display = isDisplayed ? DisplayStyle.Flex : DisplayStyle.None;
			m_profileGroup.SetEnabled(!isDisplayed);
		}

		private void SetProfileSettingsDisplayed(bool isDisplayed) {
			m_profileSettingsGroup.style.display = isDisplayed ? DisplayStyle.Flex : DisplayStyle.None;
		}

		private void DoAuthentication(Action importCallback) {
			SetOAuthMessageDisplayed(true);
			m_authenticator.Authenticate(PotatoSheetsSettings.instance,
				x => HandleAuthenticationComplete(x,importCallback),
				HandleAuthenticationError
			);
		}
		private void DoImport() {
			
		}
		private void DoImportAll() {
			
		}

		#region Handlers

		private void HandleClientSecretPathChanged(ChangeEvent<string> ev) {
			PotatoSheetsSettings.instance.ClientSecretPath = ev.newValue;
		}
		private void HandleCredentialsPathChanged(ChangeEvent<string> ev) {
			PotatoSheetsSettings.instance.CredentialsPath = ev.newValue;
		}
		private void HandleProfileChanged(PotatoSheetsProfile newProfile) {
			PotatoSheetsSettings.instance.Profile = newProfile;
			SetProfileGroupDisplayed(PotatoSheetsSettings.instance.Profile != null);
			m_profile.value = newProfile;
			m_selectedProfileIndex = 0;
			RefreshProfileList();
		}

		private void HandleProfileCreateNewButton(MouseDownEvent ev) {
			PotatoSheetsProfile asset = CreateInstance<PotatoSheetsProfile>();
			AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath("Assets/NewPotatoSheetsProfile.asset"));
			AssetDatabase.Refresh();
			m_profile.value = asset;
		}


		private void HandleImportAllButton(MouseDownEvent ev) {
			if (m_credentials == null) {
				DoAuthentication(DoImportAll);
			} else {
				DoImportAll();
			}
		}
		private void HandleOAuthWaitCancelButton(MouseDownEvent ev) {
			m_authenticator.CancelAuthentication();
			SetOAuthMessageDisplayed(false);
		}


		private void HandleAuthenticationComplete(CredentialsBlob credentials, Action importCallback) {
			SetOAuthMessageDisplayed(false);
			m_credentials = credentials;
			importCallback();
		}

		private void HandleAuthenticationError(string error) {
			Debug.LogError(error);
			SetOAuthMessageDisplayed(false);
		}


		private void HandleProfileSelectionChanged(IEnumerable<int> indices) {
			foreach (int index in indices) {
				m_selectedProfileIndex = index;
			}
			RefreshProfileSettings();
		}

		private void HandleProfileNameChanged(ChangeEvent<string> ev) {
			m_profileList.contentContainer.Query<Label>().AtIndex(m_selectedProfileIndex).text = ev.newValue;
		}

		private void HandleAddProfileButton(MouseDownEvent ev) {
			if (!HasProfile) {
				return;
			}
			PotatoSheetsSettings.instance.Profile.AddNewProfile();
			m_selectedProfileIndex = PotatoSheetsSettings.instance.Profile.Count - 1;
			RefreshProfileList();
		}

		private void HandleRemoveProfileButton(MouseDownEvent ev) {
			if (!HasProfile || SelectedProfile == null) {
				return;
			}
			MainProfile.RemoveProfile(m_selectedProfileIndex);
			m_selectedProfileIndex = -1;
			RefreshProfileList();
		}

		private void HandleProfileClicked(int index) {
			m_selectedProfileIndex = index;
			RefreshProfileList();
		}


		#endregion

	}


}
