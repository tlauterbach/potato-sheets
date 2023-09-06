using System.Collections.Generic;

namespace JsonParser {

	public enum JsonType {
		Null,
		String,
		Number,
		Boolean,
		Object,
		Array
	}

	public class JsonBlob {

		public JsonType Type { get; }

		public JsonBlob this[string name] {
			get {
				if (IsObject && m_objectChildren.TryGetValue(name, out JsonBlob value)) {
					return value;
				} else {
					return null;
				}
			}
		}
		public JsonBlob this[int index] {
			get {
				if (IsArray && index >= 0 && index < (m_arrayChildren?.Count ?? -1)) {
					return m_arrayChildren[index];
				} else {
					return null;
				}
			}
		}
		public IEnumerable<string> Keys {
			get {
				if (IsObject) {
					foreach (string key in m_objectChildren.Keys) {
						yield return key;
					}
				} else {
					yield break;
				}
			}
		}
		public int Count {
			get {
				if (IsArray) {
					return m_arrayChildren.Count;
				} else if (IsObject) {
					return m_objectChildren.Count;
				} else {
					return 0;
				}
			}
		}

		#region Type Helpers

		public bool IsNull {
			get { return Type == JsonType.Null; }
		}
		public bool IsString {
			get { return Type == JsonType.String; }
		}
		public bool IsNumber {
			get { return Type == JsonType.Number; }
		}
		public bool IsBoolean {
			get { return Type == JsonType.Boolean; }
		}
		public bool IsObject {
			get { return Type == JsonType.Object; }
		}
		public bool IsArray {
			get { return Type == JsonType.Array; }
		}

		public string AsString {
			get {
				if (IsString) {
					return m_string;
				} else {
					return string.Empty;
				}
			}
		}
		public double AsDouble {
			get {
				if (IsNumber) {
					return m_number;
				} else {
					return default;
				}
			}
		}
		public int AsInt32 {
			get {
				if (IsNumber) {
					return (int)m_number;
				} else {
					return default;
				}
			} 
		}
		public bool AsBoolean {
			get {
				if (IsBoolean || IsNumber) {
					return m_number > 0f;
				} else {
					return default;
				}
			}
		}


		#endregion

		private Dictionary<string, JsonBlob> m_objectChildren;
		private List<JsonBlob> m_arrayChildren;
		private double m_number;
		private string m_string;

		#region Constructors and Implicit Conversions

		private JsonBlob(JsonType type) {
			Type = type;
			if (IsArray) {
				m_arrayChildren = new List<JsonBlob>();
			} else if (IsObject) {
				m_objectChildren = new Dictionary<string, JsonBlob>();
			}
		}
		public JsonBlob(int value) : this(JsonType.Number) {
			m_number = value;
		}
		public JsonBlob(double value) : this(JsonType.Number) {
			m_number = value;
		}
		public JsonBlob(float value) : this(JsonType.Number) {
			m_number = value;
		}
		public JsonBlob(string value): this(JsonType.String) {
			m_string = value;
		}
		public JsonBlob(bool value) : this(JsonType.Boolean) {
			m_number = value ? 1f : 0f;
		}
		public JsonBlob(Dictionary<string,JsonBlob> value) : this(JsonType.Object) {
			foreach (KeyValuePair<string,JsonBlob> pair in value) {
				m_objectChildren.Add(pair.Key, pair.Value);
			}
		}
		public JsonBlob(IEnumerable<JsonBlob> value) : this(JsonType.Array) {
			m_arrayChildren.AddRange(value);
		}

		public static implicit operator JsonBlob(int value) {
			return new JsonBlob(value);
		}
		public static implicit operator JsonBlob(double value) {
			return new JsonBlob(value);
		}
		public static implicit operator JsonBlob(float value) {
			return new JsonBlob(value);
		}
		public static implicit operator JsonBlob(string value) {
			return new JsonBlob(value);
		}
		public static implicit operator JsonBlob(bool value) {
			return new JsonBlob(value);
		}


		public static JsonBlob CreateObject() {
			return new JsonBlob(JsonType.Object);
		}
		public static JsonBlob CreateArray() {
			return new JsonBlob(JsonType.Array);
		}
		public static JsonBlob CreateNull() {
			return new JsonBlob(JsonType.Null);
		}


		#endregion


		public void AddChild(string name, JsonBlob blob) {
			if (IsObject) {
				m_objectChildren.Add(name, blob);
			}
		}
		public void AddChild(JsonBlob blob) {
			if (IsArray) {
				m_arrayChildren.Add(blob);
			}
		}
		public bool ContainsKey(string name) {
			if (IsObject) {
				return m_objectChildren.ContainsKey(name);
			} else {
				return false;
			}
		}

		public bool IsType(JsonType type) {
			return Type == type;
		}

	}

}