using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace PotatoSheets.Editor {


	internal class AssetBindings {

		public ImportType ImportType { get; }
		public string PrimaryKey { get; }

		private Type m_assetType;
		private Dictionary<string, List<ContentBinding>> m_valueAutoFields;
		private Dictionary<string, List<ContentBinding>> m_assetAutoFields;
		private MethodInfo m_importMethod;
		private MethodInfo m_lateImportMethod;

		private const BindingFlags AUTO_BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField | BindingFlags.SetProperty;

		public AssetBindings(ILogger logger, PotatoSheetsProfile.Profile profile) {
			// must be an existing type
			Type assetType = Type.GetType(profile.AssetType, false);
			if (assetType == null) {
				logger.LogError($"Profile `{profile.ProfileName}' has a non-existing AssetType. Make sure it uses an existing, fully qualified name.");
				return;
			}
			// must be an UnityEngine.Object type
			if (!assetType.IsAsset()) {
				logger.LogError($"{assetType.Name} must inherit from ScriptableObject and have a ContentAssetAttribute");
				return;
			}
			// must include a ContentAsset attribute on the class
			ContentAssetAttribute contentAsset = assetType.GetCustomAttribute<ContentAssetAttribute>();
			if (contentAsset == null) {
				logger.LogError($"{assetType.Name} must inherit from ScriptableObject and have a ContentAssetAttribute");
				return;
			}

			m_assetType = assetType;
			ImportType = contentAsset.ImportType;
			PrimaryKey = contentAsset.PrimaryKey;

			if (ImportType == ImportType.Automatic) {
				// if automatic import type, find all of the Content fields/properties
				SetupAutomatic(logger);
			} else if (ImportType == ImportType.Manual) {
				// if manual import type, find the required Import and PostImport functions
				SetupManual(logger);
			} else {
				logger.LogError($"ImportType `{ImportType}' is not currently implemented");
			}
		}

		public void Import(IImportUtility util) {
			if (ImportType == ImportType.Manual) {
				m_importMethod.Invoke(null, new object[1] { util });
			} else {
				// initially, we ignore UnityEngine.Objects because
				// they are currently in the process of being created
				ImportContent(util, m_valueAutoFields);
			}
		}
		public void LateImport(IImportUtility util) {
			if (ImportType == ImportType.Manual) {
				m_lateImportMethod.Invoke(null, new object[1] { util });
			} else {
				// now that all imported assets are likely to
				// be created, we can do UnityEngine.Objects
				ImportContent(util, m_assetAutoFields);
			}
		}

		private void SetupAutomatic(ILogger logger) {
			
			m_valueAutoFields = new Dictionary<string, List<ContentBinding>>();
			m_assetAutoFields = new Dictionary<string, List<ContentBinding>>();

			foreach (FieldInfo field in m_assetType.GetFields(AUTO_BINDING_FLAGS)) {
				IEnumerable<ContentAttribute> attributes = field.GetCustomAttributes<ContentAttribute>(true);
				if (attributes == null || attributes.Count() <= 0) {
					continue;
				}
				bool isConvertable = StringConverter.IsConvertable(field.FieldType);
				if (!isConvertable) {
					logger.LogWarning($"Field `{field.Name}' of type `{field.FieldType.Name}' " +
						$"on ContentAsset type `{m_assetType.Name}' is not supported for conversion. " +
						$"Its import will be skipped."
					);
					continue;
				}
				bool isAsset = field.FieldType.IsAsset();
				foreach (ContentAttribute attr in attributes) {
					if (isAsset) {
						AddBinding(logger, m_assetAutoFields, attr, field);
					} else {
						AddBinding(logger, m_valueAutoFields, attr, field);
					}
				}
			}
			foreach (PropertyInfo property in m_assetType.GetProperties(AUTO_BINDING_FLAGS)) {
				IEnumerable<ContentAttribute> attributes = property.GetCustomAttributes<ContentAttribute>(true);
				if (attributes == null || attributes.Count() <= 0) {
					continue;
				}
				bool isConvertable = StringConverter.IsConvertable(property.PropertyType);
				if (!isConvertable) {
					logger.LogWarning($"Property `{property.Name}' of type `{property.PropertyType.Name}' " +
						$"on ContentAsset type `{m_assetType.Name}' is not supported for conversion. " +
						$"Its import will be skipped."
					);
					continue;
				}
				bool isAsset = property.PropertyType.IsAsset();
				foreach (ContentAttribute attr in attributes) {
					if (isAsset) {
						AddBinding(logger, m_assetAutoFields, attr, property);
					} else {
						AddBinding(logger, m_valueAutoFields, attr, property);
					}
				}
			}
		}

		private void SetupManual(ILogger logger) {

			m_importMethod = m_assetType.GetMethod("Import", BindingFlags.Public | BindingFlags.Static);
			if (m_importMethod == null) {
				logger.LogError($"ContentAsset `{m_assetType.Name}' requires a public static Import function with exactly one parameter of type IImportUtility");
			} else {
				ParameterInfo[] parameters = m_importMethod.GetParameters();
				if (parameters == null || parameters.Length != 1 || parameters[0].ParameterType != typeof(IImportUtility)) {
					logger.LogError($"ContentAsset `{m_assetType.Name}' requires a public static Import function with exactly one parameter of type IImportUtility");
				}
			}

			m_lateImportMethod = m_assetType.GetMethod("LateImport", BindingFlags.Public | BindingFlags.Static);
			if (m_lateImportMethod == null) {
				logger.LogError($"ContentAsset `{m_assetType.Name}' requires a public static LateImport function with exactly one parameter of type IImportUtility");
			} else {
				ParameterInfo[] parameters = m_lateImportMethod.GetParameters();
				if (parameters == null || parameters.Length != 1 || parameters[0].ParameterType != typeof(IImportUtility)) {
					logger.LogError($"ContentAsset `{m_assetType.Name}' requires a public static LateImport function with exactly one parameter of type IImportUtility");
				}
			}

		}

		private void AddBinding(ILogger logger, Dictionary<string,List<ContentBinding>> bindings, ContentAttribute attr, FieldInfo field) {
			List<ContentBinding> list;
			if (!bindings.TryGetValue(attr.Alias, out list)) {
				list = new List<ContentBinding>();
				bindings.Add(attr.Alias, list);
			}
			list.Add(new ContentBinding(this,logger, field, attr.Delimiter));
		}
		private void AddBinding(ILogger logger, Dictionary<string, List<ContentBinding>> bindings, ContentAttribute attr, PropertyInfo property) {
			List<ContentBinding> list;
			if (!bindings.TryGetValue(attr.Alias, out list)) {
				list = new List<ContentBinding>();
				bindings.Add(attr.Alias, list);
			}
			list.Add(new ContentBinding(this,logger, property, attr.Delimiter));
		}

		private void ImportContent(IImportUtility util, Dictionary<string,List<ContentBinding>> bindings) {
			foreach (Row row in util.DataSheet[PrimaryKey]) {
				// find or create the asset specified by the primary value
				string path = util.BuildAssetPath(row.PrimaryValue);
				UnityEngine.Object asset = util.FindOrCreateAsset(m_assetType, path);

				foreach (KeyValuePair<string, string> item in row) {
					if (bindings.TryGetValue(item.Key, out List<ContentBinding> list)) {
						foreach (ContentBinding binding in list) {
							binding.SetValue(asset, item.Value);
						}
					}
				}
				EditorUtility.SetDirty(asset);
			}
		}


		private class ContentBinding {

			private readonly PropertyInfo m_property;
			private readonly FieldInfo m_field;
			private readonly Type m_type;
			private readonly string m_delimiter;

			public ContentBinding(AssetBindings owner, ILogger logger, PropertyInfo info, string delimiter = default) {
				m_property = info;
				m_type = info.PropertyType;
				if (m_type.IsArray && string.IsNullOrEmpty(delimiter)) {
					logger.LogError($"ContentAttribute on property `{info.Name}' in type " +
						$"`{owner.m_assetType.Name}' requires a non-empty delimiter be set on the attribute");
				}
				m_delimiter = delimiter;
			}
			public ContentBinding(AssetBindings owner, ILogger logger, FieldInfo info, string delimiter = default) {
				m_field = info;
				m_type = info.FieldType;
				if (m_type.IsArray && string.IsNullOrEmpty(delimiter)) {
					logger.LogError($"ContentAttribute on field `{info.Name}' in type " +
						$"`{owner.m_assetType.Name}' requires a non-empty delimiter be set on the attribute");
				}
				m_delimiter = delimiter;
			}
			public void SetValue(object target, string value) {
				object converted;
				if (m_type.IsArray) {
					converted = StringConverter.Convert(m_type, value, m_delimiter);
				} else {
					converted = StringConverter.Convert(m_type, value);
				}
				m_property?.SetValue(target, converted);
				m_field?.SetValue(target, converted);
			}
		}

	}


}