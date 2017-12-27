namespace PEunion
{
	public class ValidationError
	{
		public ValidationErrorType Type { get; set; }
		public string Source { get; set; }
		public int? Line { get; set; }
		public string Description { get; set; }

		public ValidationError(ValidationErrorType type, string source, int? line, string description)
		{
			Type = type;
			Source = source;
			Line = line;
			Description = description;
		}
		public static ValidationError CreateError(string source, string description) => new ValidationError(ValidationErrorType.Error, source, null, description);
		public static ValidationError CreateWarning(string source, string description) => new ValidationError(ValidationErrorType.Warning, source, null, description);
		public static ValidationError CreateMessage(string source, string description) => new ValidationError(ValidationErrorType.Message, source, null, description);
		public static ValidationError CreateCompileError(int line, string description) => new ValidationError(ValidationErrorType.Error, "Stub.cs", line, description);
	}
}