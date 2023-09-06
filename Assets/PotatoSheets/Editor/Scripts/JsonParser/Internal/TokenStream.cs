using System.Collections.Generic;
using System.Linq;

namespace JsonParser {

	internal class TokenStream {

		private Token[] m_input;
		private int m_index;


		public TokenStream(IEnumerable<Token> tokens) {
			Reset(tokens);
		}

		public void Reset(IEnumerable<Token> tokens) {
			m_input = tokens.ToArray();
			m_index = 0;
		}
		public void Reset() {
			m_index = 0;
		}

		public Token Peek() {
			if (IsEndOfStream()) {
				return Token.Empty;
			} else {
				return m_input[m_index];
			}
		}
		public Token Expect(TokenType type) {
			Token peek = Peek();
			if (peek == Token.Empty) {
				throw new UnexpectedEndOfStreamException(type);
			} else if (peek == type) {
				Consume();
				return peek;
			} else {
				throw new UnexpectedTokenException(type, peek.Type);
			}
		}
		public Token Consume() {
			Token peek = Peek();
			m_index++;
			return peek;
		}
		public bool Matches(TokenType type) {
			Token peek = Peek();
			return peek == type;
		}

		public bool IsEndOfStream() {
			return m_index >= m_input.Length;
		}
	}
}