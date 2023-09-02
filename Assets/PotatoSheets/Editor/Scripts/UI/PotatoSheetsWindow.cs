using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PotatoSheets.Editor {

	public class PotatoSheetsWindow : EditorWindow {

		[SerializeField]
		private VisualTreeAsset m_uxmlAsset;

		private Authentication m_authenticator;
		private CredentialsBlob m_credentials;

		[MenuItem("Window/Tools/PotatoSheets")]
		public static void Create() {
			EditorWindow window = GetWindow<PotatoSheetsWindow>("Potato Sheets Importer");
		}

		public void CreateGUI() {
			m_uxmlAsset.CloneTree(rootVisualElement);
			
			var clientSecret = rootVisualElement.Q<TextField>("ClientSecretPath");
			clientSecret.value = PotatoSheetsSettings.instance.ClientSecretPath; 
			clientSecret.RegisterValueChangedCallback(HandleClientSecretPathChanged);

			var credentials = rootVisualElement.Q<TextField>("CredentialsPath");
			credentials.value = PotatoSheetsSettings.instance.CredentialsPath;
			credentials.RegisterValueChangedCallback(HandleCredentialsPathChanged);

			var profile = rootVisualElement.Q<ObjectField>("Profile");
			profile.value = PotatoSheetsSettings.instance.Profile;
			profile.RegisterValueChangedCallback(HandleImportSetChanged);

			rootVisualElement.Q<Button>("ProfileCreateNewButton")
				.RegisterCallback<MouseDownEvent>(HandleProfileCreateNewButton, TrickleDown.TrickleDown);
			rootVisualElement.Q("ProfileGroup").style.display =
				PotatoSheetsSettings.instance.Profile != null ? DisplayStyle.Flex : DisplayStyle.None;

			rootVisualElement.Q<Button>("ImportAllButton").RegisterCallback<MouseDownEvent>(HandleImportAllButton, TrickleDown.TrickleDown);
			rootVisualElement.Q<Button>("OAuthWaitCancelButton").RegisterCallback<MouseDownEvent>(HandleOAuthWaitCancelButton, TrickleDown.TrickleDown);

			SetOAuthMessageDisplayed(false);

			m_authenticator = new Authentication();
		}

		public void OnProjectChange() {
			rootVisualElement.Q("ProfileGroup").style.display =
				PotatoSheetsSettings.instance.Profile != null ? DisplayStyle.Flex : DisplayStyle.None;
		}
		public void OnDestroy() {
			m_authenticator?.CancelAuthentication();
		}


		private void SetOAuthMessageDisplayed(bool isDisplayed) {
			rootVisualElement.Q("OAuthWaitMessage").style.display =
				isDisplayed ? DisplayStyle.Flex : DisplayStyle.None;
			rootVisualElement.Q("ProfileGroup").SetEnabled(!isDisplayed);
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
		private void HandleImportSetChanged(ChangeEvent<UnityEngine.Object> ev) {
			PotatoSheetsSettings.instance.Profile = (PotatoSheetsProfile)ev.newValue;
			rootVisualElement.Q("ProfileGroup").style.display =
				PotatoSheetsSettings.instance.Profile != null ? DisplayStyle.Flex : DisplayStyle.None;
		}
		private void HandleProfileCreateNewButton(MouseDownEvent ev) {
			PotatoSheetsProfile asset = CreateInstance<PotatoSheetsProfile>();
			AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath("Assets/NewPotatoSheetsProfile.asset"));
			AssetDatabase.Refresh();
			rootVisualElement.Q<ObjectField>("Profile").value = asset;
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

		#endregion

	}


}
