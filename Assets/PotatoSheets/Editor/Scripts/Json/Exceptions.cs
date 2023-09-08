using System;

namespace PotatoSheets.Json {


	public class JsonParseException : InvalidOperationException {
		internal JsonParseException(string message) : base(message) { }
	}

	public class UnexpectedEndOfStreamException : JsonParseException {
		internal UnexpectedEndOfStreamException() : base(
			$"Unexpected end of Stream"
		) {}
		internal UnexpectedEndOfStreamException(TokenType expected) : base(
			$"Expected token {expected} but have reached the end of the stream"
		) {}
		internal UnexpectedEndOfStreamException(char expected) : base(
			$"Expected char {expected} but have reached the end of the stream"
		) {}
	}

	public class UnexpectedTokenException : JsonParseException {
		internal UnexpectedTokenException(TokenType expected, TokenType received) : base(
			$"Unexpected token {received} in stream. Was expecting {expected}"
		) {}
		internal UnexpectedTokenException(TokenType received) : base(
			$"Unexpected token {received} in stream."
		){ }
	}

	public class UnexpectedCharException : JsonParseException {
		internal UnexpectedCharException(string message) : base(message) { }
	}

}