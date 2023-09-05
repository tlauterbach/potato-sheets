namespace PotatoSheets.Editor {

	public interface ILogger {
		bool HasErrors { get; }
		void LogError(string error);
		void LogWarning(string warning);
	}

}