using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PotatoSheets.Editor {

	internal static class StringConverter {

		private delegate object Converter(string value);
		private static readonly Dictionary<Type,Converter> m_simpleConverters = new Dictionary<Type,Converter>() {
			{ typeof(string), ToString },
			{ typeof(int), ToInt32 },
			{ typeof(short), ToInt16 },
			{ typeof(long), ToInt64 },
			{ typeof(uint), ToUInt32 },
			{ typeof(ushort), ToUInt16 },
			{ typeof(ulong), ToUInt64 },
			{ typeof(byte), ToByte },
			{ typeof(sbyte), ToSByte },
			{ typeof(char), ToChar },
			{ typeof(bool), ToBool },
			{ typeof(float), ToSingle },
			{ typeof(double), ToDouble },
			{ typeof(decimal), ToDecimal },
			{ typeof(DateTime), ToDateTime },
			{ typeof(Vector2), ToVector2 },
			{ typeof(Vector3), ToVector3 },
			{ typeof(Vector4), ToVector4 },
			{ typeof(Vector2Int), ToVector2Int },
			{ typeof(Vector3Int), ToVector3Int },
			{ typeof(Bounds), ToBounds },
			{ typeof(BoundsInt), ToBoundsInt },
			{ typeof(Rect), ToRect },
			{ typeof(RectInt), ToRectInt },
			{ typeof(Color), ToColor },
			{ typeof(Color32), ToColor32 },
		};

		public static T Convert<T>(string value) {
			return (T)Convert(typeof(T), value);
		}
		public static T[] Convert<T>(string value, string delimiter) {
			return Convert(typeof(T), value, delimiter) as T[];
		}
		public static object[] Convert(Type type, string value, string delimiter) {
			string[] split = value.Split(delimiter);
			object[] result = new object[split.Length];
			for (int ix = 0; ix < split.Length; ix++) {
				result[ix] = Convert(type, split[ix]);
			}
			return result;
		}
		public static object Convert(Type type, string value) {
			if (m_simpleConverters.TryGetValue(type, out Converter converter)) {
				// simple conversion
				return converter(value);
			} else if (type.IsEnum) {
				// enumeration value
				object obj;
				if (long.TryParse(value, out long result)) {
					try {
						obj = Enum.ToObject(type, result);
					} catch {
						obj = System.Convert.ChangeType(0, type);
					}
				} else if (!Enum.TryParse(type, value, true, out obj)) {
					obj = System.Convert.ChangeType(0, type);
				}
				return obj;
			} else if (type.IsAsset()) {
				// this is a unity asset conversion
				string[] guids = AssetDatabase.FindAssets($"{value} t:{type.FullName}");
				if (guids != null && guids.Length > 0) {
					return AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), type);
				} else {
					return null;
				}
			} else {
				// try using a string constructor
				ConstructorInfo constructor = type.GetConstructor(new Type[1] { typeof(string) });
				return constructor.Invoke(new object[1] { value });
			}
		}

		public static bool IsConvertable<T>() {
			return IsConvertable(typeof(T));
		}
		public static bool IsConvertable(Type type) {
			if (type.IsSZArray) {
				return IsConvertable(type.GetElementType());
			}
			if (m_simpleConverters.ContainsKey(type)) {
				return true;
			}
			if (type.IsEnum) {
				return true;
			}
			if (type.IsAsset()) {
				return true;
			}
			if (type.GetConstructor(new Type[1]{typeof(string)}) != null) {
				return true;
			}
			return false;
		}


		private static float[] SplitCommaSeparatedFloats(string value, int minSize) {
			if (string.IsNullOrEmpty(value)) {
				return new float[minSize];
			}
			string[] split = value.Split(',');
			float[] floats = new float[Math.Max(split.Length, minSize)];
			for (int ix = 0; ix < floats.Length; ix++) {
				if (ix < split.Length && float.TryParse(split[ix], out float result)) {
					floats[ix] = result;
				} else {
					floats[ix] = default(float);
				}
			}
			return floats;
		}

		private static byte FloatToByte(float value) {
			if (value <= 0f) {
				return 0;
			} else if (value >= 255f) {
				return 255;
			} else if (value > 0f && value <= 1f) {
				return (byte)(value * 255);
			} else {
				return (byte)value;
			}
		}

		#region Converters

		private static object ToString(string value) {
			return value;
		}

		private static object ToInt32(string value) {
			if (int.TryParse(value, out int result)) {
				return result;
			} else {
				return default(int);
			}
		}
		private static object ToInt16(string value) {
			if (short.TryParse(value, out short result)) {
				return result;
			} else {
				return default(short);
			}
		}
		private static object ToInt64(string value) {
			if (long.TryParse(value, out long result)) {
				return result;
			} else {
				return default(long);
			}
		}
		private static object ToUInt32(string value) {
			if (uint.TryParse(value, out uint result)) {
				return result;
			} else {
				return default(uint);
			}
		}
		private static object ToUInt16(string value) {
			if (ushort.TryParse(value, out ushort result)) {
				return result;
			} else {
				return default(ushort);
			}
		}
		private static object ToUInt64(string value) {
			if (ulong.TryParse(value, out ulong result)) {
				return result;
			} else {
				return default(ulong);
			}
		}
		private static object ToByte(string value) {
			if (byte.TryParse(value, out byte result)) {
				return result;
			} else {
				return default(byte);
			}
		}
		private static object ToSByte(string value) {
			if (sbyte.TryParse(value, out sbyte result)) {
				return result;	
			} else {
				return default(sbyte);
			}
		}
		private static object ToChar(string value) {
			if (value.Length >= 1) {
				return value[0];
			} else {
				return default(char);
			}
		}
		private static object ToBool(string value) {
			string lower = value.ToLower();
			if (lower == "true") {
				return true;
			} else if (lower == "false") {
				return false;
			} else if (float.TryParse(value, out float result)) {
				return result > 0f;
			} else {
				return default(bool);
			}
		}
		private static object ToSingle(string value) {
			if (float.TryParse(value, out float result)) {
				return result;
			} else {
				return default(float);
			}
		}
		private static object ToDouble(string value) {
			if (double.TryParse(value, out double result)) {
				return result;
			} else {
				return default(double);
			}
		}
		private static object ToDecimal(string value) {
			if (decimal.TryParse(value, out decimal result)) {
				return result;
			} else {
				return default(decimal);
			}
		}

		private static object ToDateTime(string value) {
			if (DateTime.TryParse(value, out DateTime result)) {
				return result;
			} else {
				return default(DateTime);
			}
		}
		private static object ToVector2(string value) {
			float[] floats = SplitCommaSeparatedFloats(value,2);
			return new Vector2(floats[0], floats[1]);
		}
		private static object ToVector3(string value) {
			float[] floats = SplitCommaSeparatedFloats(value, 3);
			return new Vector3(floats[0], floats[1], floats[2]);
		}
		private static object ToVector4(string value) {
			float[] floats = SplitCommaSeparatedFloats(value, 4);
			return new Vector4(floats[0], floats[1], floats[2], floats[3]);
		}
		private static object ToVector2Int(string value) {
			float[] floats = SplitCommaSeparatedFloats(value, 2);
			return new Vector2Int((int)floats[0],(int)floats[1]);
		}
		private static object ToVector3Int(string value) {
			float[] floats = SplitCommaSeparatedFloats(value, 3);
			return new Vector3Int((int)floats[0], (int)floats[1], (int)floats[2]);
		}
		private static object ToRect(string value) {
			float[] floats = SplitCommaSeparatedFloats(value, 4);
			return new Rect(floats[0], floats[1], floats[2], floats[3]);
		}
		private static object ToRectInt(string value) {
			float[] floats = SplitCommaSeparatedFloats(value, 4);
			return new RectInt((int)floats[0], (int)floats[1], (int)floats[2], (int)floats[3]);
		}
		private static object ToBounds(string value) {
			float[] floats = SplitCommaSeparatedFloats(value, 6);
			return new Bounds(new Vector3(floats[0], floats[1], floats[2]), new Vector3(floats[3], floats[4], floats[5]));
		}
		private static object ToBoundsInt(string value) {
			float[] floats = SplitCommaSeparatedFloats(value, 6);
			return new BoundsInt((int)floats[0], (int)floats[1], (int)floats[2], (int)floats[3], (int)floats[4], (int)floats[5]);
		}
		private static object ToColor(string value) {
			if (value.Length > 0 && value[0] == '#') {
				if (ColorUtility.TryParseHtmlString(value, out Color result)) {
					return result;
				} else {
					return default(Color);
				}
			} else {
				float[] floats = SplitCommaSeparatedFloats(value, 3);
				return new Color(floats[0], floats[1], floats[2], floats.Length >= 4 ? floats[3] : 1f);
			}
		}
		private static object ToColor32(string value) {
			if (value.Length > 0 && value[0] == '#') {
				if (ColorUtility.TryParseHtmlString(value, out Color result)) {
					return (Color32)result;
				} else {
					return default(Color32);
				}
			} else {
				float[] floats = SplitCommaSeparatedFloats(value, 3);
				return new Color32(
					FloatToByte(floats[0]),
					FloatToByte(floats[1]),
					FloatToByte(floats[2]),
					floats.Length >= 4 ? FloatToByte(floats[3]) : (byte)255
				);
			}
		}

		#endregion

	}


}