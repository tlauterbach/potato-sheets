using System;

namespace JsonParser {

	internal enum TokenType {
		Null,
		String,
		Number,
		True,
		False,
		OpenSquare,
		CloseSquare,
		OpenCurly,
		CloseCurly,
		Colon,
		Comma
	}
	

	internal readonly struct Token : IEquatable<Token>, IEquatable<TokenType> {

		public static readonly Token Empty = new Token();

		public TokenType Type { get; }
		public StringSlice Value { get; }

		private readonly int m_hashCode;

		public Token(TokenType type, StringSlice value) {
			Type = type;
			Value = value;
			m_hashCode = HashCode.Combine(type, value);
		}

		public static bool operator ==(Token lhs, TokenType rhs) {
			return lhs.Equals(rhs);
		}
		public static bool operator !=(Token lhs, TokenType rhs) {
			return !lhs.Equals(rhs);
		}
		public static bool operator ==(Token lhs, Token rhs) {
			return lhs.Equals(rhs);
		}
		public static bool operator !=(Token lhs, Token rhs) {
			return !lhs.Equals(rhs);
		}
		public static implicit operator TokenType(Token value) {
			return value.Type;
		}


		public bool Equals(Token other) {
			return other.m_hashCode == m_hashCode;
		}
		public bool Equals(TokenType other) {
			return Type == other;
		}
		public override bool Equals(object other) {
			if (other is Token token) {
				return Equals(token);
			} else if (other is TokenType type) {
				return Equals(type);
			} else {
				return false;
			}
		}
		public override int GetHashCode() {
			return m_hashCode;
		}

	}

}