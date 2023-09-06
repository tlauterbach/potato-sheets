namespace JsonParser {


	internal class KeywordPattern : IPattern {

		private readonly TokenType m_type;
		private readonly string m_keyword;

		public KeywordPattern(TokenType type, string keyword) {
			m_type = type;
			m_keyword = keyword;
		}

		public bool Matches(CharStream stream, out Token token) {
			StringSlice peek = stream.Peek(m_keyword.Length);
			if (peek == m_keyword) {
				if (!stream.IsEndOfStream(m_keyword.Length+1)) {
					char next = peek.Lengthen(1)[^1];
					if (char.IsLetterOrDigit(next) || next == '_') {
						token = Token.Empty;
						return false;
					}
				}
				token = new Token(m_type, peek);
				return true;
			}
			token = Token.Empty;
			return false;
		}

	}

}