namespace PotatoSheets.Json {

	internal class StringPattern : IPattern {

		public bool Matches(CharStream stream, out Token token) {
			StringSlice peek = stream.Peek();
			if (peek[0] == '"') {
				while (!stream.IsEndOfStream(peek.Length + 1)) {
					peek = peek.Lengthen(1);
					if (peek[^1] == '"' && peek[^2] != '\\') {
						token = new Token(TokenType.String, peek);
						return true;
					}
				}
				throw new UnexpectedEndOfStreamException('"');
			}
			token = Token.Empty;
			return false;
		}

	}


}