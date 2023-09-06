namespace JsonParser {

	
	internal interface IPattern {
		bool Matches(CharStream stream, out Token token);
	}

}