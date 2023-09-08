namespace PotatoSheets.Json {

	
	internal interface IPattern {
		bool Matches(CharStream stream, out Token token);
	}

}