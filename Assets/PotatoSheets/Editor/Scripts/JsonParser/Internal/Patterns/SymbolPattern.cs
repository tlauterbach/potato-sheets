namespace JsonParser {

	internal class SymbolPattern : IPattern {

		private readonly TokenType m_type;
		private readonly string m_symbol;

		public SymbolPattern(TokenType type, string symbol) {
			m_type = type;
			m_symbol = symbol;
		}

		public bool Matches(CharStream stream, out Token token) {
			StringSlice peek = stream.Peek(m_symbol.Length);
			if (peek == m_symbol) {
				token = new Token(m_type, peek);
				return true;
			} else {
				token = Token.Empty;
				return false;
			}
		}

	}

}