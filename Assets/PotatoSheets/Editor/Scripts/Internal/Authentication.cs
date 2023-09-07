using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;
using System.Collections;

namespace PotatoSheets.Editor {

	internal partial class Authentication {

		public bool IsAuthenticating { get; private set; } = false;

		private string m_credentialsPath;
		private HttpListener m_listener;
		private EditorCoroutine m_listenerRoutine;
		private ClientSecretBlob m_clientSecretBlob;

		private Action<CredentialsBlob> m_onComplete;
		private Action<string> m_onError;

		private const string SCOPES = "https://www.googleapis.com/auth/spreadsheets.readonly";

		public void Authenticate(PotatoSheetsSettings settings, Action<CredentialsBlob> onComplete, Action<string> onError) {

			if (IsAuthenticating) {
				onError("Already trying to Authenticate. Did you forget to cancel?");
				return;
			}

			// see if we already have existing credentials
			string credentialsPath = Path.Combine(Application.dataPath, settings.CredentialsPath);
			bool refreshToken = false;

			CredentialsBlob credentials = null;
			if (File.Exists(credentialsPath)) {
				// load the credentials
				try {
					credentials = JsonUtility.FromJson<CredentialsBlob>(File.ReadAllText(credentialsPath));
				} catch (Exception e) {
					onError($"Error reading credentials: {e.Message}");
					return;
				}
				if (string.IsNullOrEmpty(credentials.access_token)) {
					onError($"No access token could be loaded from the credentials. You may have to delete your credentials file.");
					return;
				}
				// check the timestamps to see if we need to do a refresh
				if (DateTime.TryParse(credentials.last_sign_in, out DateTime lastSignIn)) {
					if (DateTime.Now < lastSignIn + TimeSpan.FromSeconds(credentials.expires_in)) {
						// we do not need to refresh
						onComplete(credentials);
						return;
					}
				}
				// we need to refresh our token
				refreshToken = true;
			}

			// the client secret file must exist to get credentials
			string clientSecretPath = Path.Combine(Application.dataPath, settings.ClientSecretPath);
			if (!File.Exists(clientSecretPath)) {
				onError($"No client secret file exists at: {clientSecretPath}");
				return;
			}
			try {
				m_clientSecretBlob = JsonUtility.FromJson<ClientSecretBlob>(File.ReadAllText(clientSecretPath));
			} catch (Exception e) {
				onError($"Error reading client secret: {e.Message}");
				m_clientSecretBlob = null;
				return;
			}


			// start the asynchronus process of getting credentials
			m_onComplete = onComplete;
			m_onError = onError;
			m_credentialsPath = credentialsPath;
			IsAuthenticating = true;

			if (refreshToken) {
				// call the endpoint to get a refreshed token
				{
					string url = CreateURL(m_clientSecretBlob.installed.token_uri,
						new Dictionary<string, string>() {
							{ "client_id", m_clientSecretBlob.installed.client_id },
							{ "client_secret", m_clientSecretBlob.installed.client_secret },
							{ "scope", SCOPES },
							{ "refresh_token", credentials.refresh_token },
							{ "grant_type", "refresh_token" }
					});
					UnityWebRequest request = UnityWebRequest.Post(url, string.Empty);
					request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

					// actually send the request
					UnityWebRequestAsyncOperation operation = request.SendWebRequest();
					if (operation.isDone) {
						RefreshCallback(request, credentials.refresh_token);
					} else {
						operation.completed += x => RefreshCallback(request, credentials.refresh_token);
					}
				}

			} else {
				// Create an HTTP Listener to wait for a response from google
				{
					if (m_listener != null) {
						m_listener.Stop();
					}
					m_listener = new HttpListener();
					for (int ix = 0; ix < m_clientSecretBlob.installed.redirect_uris.Length; ix++) {
						string prefix = m_clientSecretBlob.installed.redirect_uris[ix];
						if (prefix[^1] != '/') {
							m_listener.Prefixes.Add(m_clientSecretBlob.installed.redirect_uris[ix] + '/');
						} else {
							m_listener.Prefixes.Add(m_clientSecretBlob.installed.redirect_uris[ix]);
						}

					}
					m_listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
					m_listener.Start();
					m_listenerRoutine = EditorCoroutineUtility.StartCoroutine(WaitForOAuth(), this);
				}

				// build the OAuth URL to send client info to and start sign in flow
				{
					string url = CreateURL(m_clientSecretBlob.installed.auth_uri,
						new Dictionary<string, string>() {
							{ "client_id", m_clientSecretBlob.installed.client_id },
							{ "redirect_uri", m_clientSecretBlob.installed.redirect_uris[0] },
							{ "response_type", "code" },
							{ "access_type", "offline" },
							{ "scope", SCOPES }
					});
					Application.OpenURL(url);
				}
			}

		}

		public void CancelAuthentication() {
			m_onError?.Invoke("Authentication Aborted");
			Reset();
		}

		private void Reset() {
			m_onComplete = null;
			m_onError = null;

			m_listener?.Abort();
			m_listener = null;

			m_clientSecretBlob = null;
			m_credentialsPath = null;

			if (m_listenerRoutine != null) {
				EditorCoroutineUtility.StopCoroutine(m_listenerRoutine);
			}
			m_listenerRoutine = null;

			IsAuthenticating = false;
		}

		private IEnumerator WaitForOAuth() {
			Task<HttpListenerContext> task = m_listener.GetContextAsync();
			while (!task.IsCompleted) {
				yield return null;
			}
			OAuthCallback(task.Result);
		}

		private void OAuthCallback(HttpListenerContext context) {

			string code = string.Empty;
			if (context.Request.QueryString.AllKeys.Length > 0) {
				foreach (var key in context.Request.QueryString.AllKeys) {
					if (StringComparer.Ordinal.Equals("code", key)) {
						code = context.Request.QueryString.GetValues(key)[0];
					}
				}
			}
			context.Response.StatusCode = 200;
			context.Response.StatusDescription = "OK";
			context.Response.Close();
			
			if (string.IsNullOrEmpty(code)) {
				m_onError?.Invoke("Did not receive code in OAuth process?");
				Reset();
			} else {
				RequestToken(m_clientSecretBlob, code);
			}
			
			m_listener.Stop();
			m_listener = null;
		}

		private void RequestToken(ClientSecretBlob blob, string code) {
			string url = CreateURL(blob.installed.token_uri, new Dictionary<string, string>() {
				{ "client_id", blob.installed.client_id },
				{ "client_secret", blob.installed.client_secret },
				{ "code", code },
				{ "grant_type", "authorization_code" },
				{ "redirect_uri", blob.installed.redirect_uris[0] }
			});

			UnityWebRequest request = UnityWebRequest.Post(url, string.Empty);
			request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

			// actually send the request
			UnityWebRequestAsyncOperation operation = request.SendWebRequest();
			if (operation.isDone) {
				TokenCallback(request);
			} else {
				operation.completed += x => TokenCallback(request);
			}
		}

		private void TokenCallback(UnityWebRequest request) {
			if (request.result == UnityWebRequest.Result.Success) {

				CredentialsBlob result = JsonUtility.FromJson<CredentialsBlob>(request.downloadHandler.text);
				result.last_sign_in = DateTime.Now.ToString();

				File.WriteAllText(m_credentialsPath, JsonUtility.ToJson(result));

				m_onComplete?.Invoke(result);
			} else {
				m_onError?.Invoke("Error: " + request.error + ": " + request.downloadHandler.text);
			}
			request.Dispose();
			Reset();
		}

		private void RefreshCallback(UnityWebRequest request, string refreshToken) {
			if (request.result == UnityWebRequest.Result.Success) {

				CredentialsBlob result = JsonUtility.FromJson<CredentialsBlob>(request.downloadHandler.text);
				result.last_sign_in = DateTime.Now.ToString();
				// rewrite the refresh token to our blob
				result.refresh_token = refreshToken;

				File.WriteAllText(m_credentialsPath, JsonUtility.ToJson(result));

				m_onComplete?.Invoke(result);

			} else {
				m_onError?.Invoke("Error: " + request.error + ": " + request.downloadHandler.text);
			}
			request.Dispose();
			Reset();
		}


		private static string CreateURL(string baseURL, Dictionary<string,string> arguments) {

			StringBuilder builder = new StringBuilder();
			builder.Append(baseURL).Append('?');
			foreach (KeyValuePair<string, string> kvp in arguments) {
				builder.Append(kvp.Key).Append('=').Append(kvp.Value).Append('&');
			}
			// remove trailing ampersand or ?
			builder.Length--;

			return builder.ToString();
		}

	}

}