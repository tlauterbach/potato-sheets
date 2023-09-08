namespace PotatoSheets.Json {


	internal class NumberPattern : IPattern {

		public bool Matches(CharStream stream, out Token token) {
			StringSlice peek = stream.Peek();
			if (peek[^1] != '-' && !char.IsDigit(peek[^1])) {
				token = Token.Empty;
				return false;
			}
			// number section
			MoveNextIf(stream, ref peek, '-');
			if (!MoveNextIf(stream, ref peek, '0')) {
				if (MoveNextIfDigit(stream, ref peek)) {
					while (!stream.IsEndOfStream(peek.Length) && MoveNextIfDigit(stream, ref peek));
				} else {
					throw new UnexpectedCharException(
						$"Invalid character {peek[^1]} in json " +
						$"number. Character after `-' must be a digit."
					);
				}
			}
			// fraction section
			if (MoveNextIf(stream, ref peek, '.')) {
				if (!MoveNextIfDigit(stream, ref peek)) {
					throw new UnexpectedCharException(
						"Invalid character `{0}' in json number. " +
						"Character after a `.' must be a digit."
					);
				}
				while (!stream.IsEndOfStream(peek.Length) && MoveNextIfDigit(stream, ref peek));
			}
			// exponent section
			if (MoveNextIf(stream, ref peek, "Ee")) {
				MoveNextIf(stream, ref peek, "+-");
				if (!MoveNextIfDigit(stream, ref peek)) {
					throw new UnexpectedCharException(
						"Invalid character `{0}' in json number. " +
						"Character after an exponent must be a digit."
					);
				}
				while (!stream.IsEndOfStream(peek.Length) && MoveNextIfDigit(stream, ref peek));
			}
			token = new Token(TokenType.Number, peek);
			return true;

		}

		private bool MoveNextIf(CharStream stream, ref StringSlice current, char c) {
			if (current[^1] == c) {
				if (stream.IsEndOfStream(current.Length+1)) {
					throw new UnexpectedEndOfStreamException();
				}
				current = current.Lengthen(1);
				return true;
			} else {
				return false;
			}
		}
		private bool MoveNextIf(CharStream stream, ref StringSlice current, string chars) {
			if (chars.Contains(current[^1])) {
				if (stream.IsEndOfStream(current.Length + 1)) {
					throw new UnexpectedEndOfStreamException();
				}
				current = current.Lengthen(1);
				return true;
			} else {
				return false;
			}
		}
		private bool MoveNextIfDigit(CharStream stream, ref StringSlice current) {
			if (char.IsDigit(current[^1])) {
				if (stream.IsEndOfStream(current.Length + 1)) {
					throw new UnexpectedEndOfStreamException();
				}
				current = current.Lengthen(1);
				return true;
			} else {
				return false;
			}
		}


	}

}