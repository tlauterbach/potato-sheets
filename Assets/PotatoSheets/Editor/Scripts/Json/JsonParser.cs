using System.Collections.Generic;

namespace PotatoSheets.Json {

	public class JsonParser {

		private static readonly List<IPattern> m_patterns = new List<IPattern>() {
			new SymbolPattern(TokenType.OpenCurly,		"{"),
			new SymbolPattern(TokenType.CloseCurly,		"}"),
			new SymbolPattern(TokenType.OpenSquare,		"["),
			new SymbolPattern(TokenType.CloseSquare,	"]"),
			new SymbolPattern(TokenType.Comma,			","),
			new SymbolPattern(TokenType.Colon,			":"),
			new StringPattern(),
			new NumberPattern(),
			new KeywordPattern(TokenType.True,			"true"),
			new KeywordPattern(TokenType.False,			"false"),
			new KeywordPattern(TokenType.Null,			"null"),
		};
		private CharStream m_charStream;
		private TokenStream m_tokenStream;

		public JsonBlob Parse(string input) {
			Tokenize(input);

			JsonBlob root;
			if (m_tokenStream.IsEndOfStream()) {
				root = null;
			} else if (m_tokenStream.Matches(TokenType.OpenCurly)) {
				root = ParseObject();
			} else if (m_tokenStream.Matches(TokenType.OpenSquare)) {
				root = ParseArray();
			} else {
				throw new UnexpectedTokenException(m_tokenStream.Peek());
			}
			return root;
		}


		private void Tokenize(string input) {
			ResetCharStream(input);
			List<Token> tokens = new List<Token>();

			while (!m_charStream.IsEndOfStream()) {
				if (char.IsWhiteSpace(m_charStream.Peek()[0])) {
					m_charStream.Advance();
					continue;
				}
				bool foundPattern = false;
				foreach (IPattern pattern in m_patterns) {
					if (pattern.Matches(m_charStream, out Token token)) {
						m_charStream.Advance(token.Value.Length);
						tokens.Add(token);
						foundPattern = true;
						break;
					}
				}
				if (!foundPattern) {
					throw new UnexpectedCharException($"Unexpected character `{m_charStream.Peek()}' in Json string");
				}
			}
			ResetTokenStream(tokens);
		}

		private JsonBlob ParseObject() {

			m_tokenStream.Expect(TokenType.OpenCurly);
			JsonBlob obj = JsonBlob.CreateObject();
			bool isFirst = true;
			while (!m_tokenStream.IsEndOfStream() && !m_tokenStream.Matches(TokenType.CloseCurly)) {
				if (isFirst) {
					isFirst = false;
				} else {
					m_tokenStream.Expect(TokenType.Comma);
				}
				string name = RemoveDoubleQuotes(
					m_tokenStream.Expect(TokenType.String).Value.ToString()
				);
				m_tokenStream.Expect(TokenType.Colon);
				obj.AddChild(name, ParseValue());				
			}
			m_tokenStream.Expect(TokenType.CloseCurly);
			return obj;
		}

		private JsonBlob ParseArray() {
			m_tokenStream.Expect(TokenType.OpenSquare);
			JsonBlob obj = JsonBlob.CreateArray();
			bool isFirst = true;
			while (!m_tokenStream.IsEndOfStream() && !m_tokenStream.Matches(TokenType.CloseSquare)) {
				if (isFirst) {
					isFirst = false;
				} else {
					m_tokenStream.Expect(TokenType.Comma);
				}
				obj.AddChild(ParseValue());
			}
			m_tokenStream.Expect(TokenType.CloseSquare);
			return obj;
		}

		private JsonBlob ParseValue() {
			if (m_tokenStream.IsEndOfStream()) {
				throw new UnexpectedEndOfStreamException();
			} else if (m_tokenStream.Matches(TokenType.OpenCurly)) {
				return ParseObject();
			} else if (m_tokenStream.Matches(TokenType.OpenSquare)) {
				return ParseArray();
			} else if (m_tokenStream.Matches(TokenType.Number)) {
				return new JsonBlob(
					double.Parse(m_tokenStream.Consume().Value.ToString())
				);
			} else if (m_tokenStream.Matches(TokenType.String)) {
				return new JsonBlob(
					RemoveDoubleQuotes(m_tokenStream.Consume().Value.ToString())
				) ;
			} else if (m_tokenStream.Matches(TokenType.True)) {
				m_tokenStream.Consume();
				return new JsonBlob(true);
			} else if (m_tokenStream.Matches(TokenType.False)) {
				m_tokenStream.Consume();
				return new JsonBlob(false);
			} else if (m_tokenStream.Matches(TokenType.Null)) {
				m_tokenStream.Consume();
				return JsonBlob.CreateNull();
			} else {
				throw new UnexpectedTokenException(m_tokenStream.Peek());
			}
		}


		private void ResetCharStream(string input) {
			if (m_charStream == null) {
				m_charStream = new CharStream(input);
			} else {
				m_charStream.Reset(input);
			}
		}
		private void ResetTokenStream(IEnumerable<Token> input) {
			if (m_tokenStream == null) {
				m_tokenStream = new TokenStream(input);
			} else {
				m_tokenStream.Reset(input);
			}
		}

		private static string RemoveDoubleQuotes(string input) {
			return input.Substring(1, input.Length - 2);
		}
	}


}