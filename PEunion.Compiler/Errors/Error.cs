namespace PEunion.Compiler.Errors
{
	/// <summary>
	/// Defines a compiler or project file parsing error.
	/// </summary>
	public sealed class Error
	{
		/// <summary>
		/// Gets the source of the error.
		/// </summary>
		public ErrorSource Source { get; private set; }
		/// <summary>
		/// Gets the severity of the error.
		/// </summary>
		public ErrorSeverity Severity { get; private set; }
		/// <summary>
		/// Gets the error message.
		/// </summary>
		public string Message { get; private set; }
		/// <summary>
		/// Gets optional details about the error.
		/// </summary>
		public string Details { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Error" /> class.
		/// </summary>
		/// <param name="source">The source of the error.</param>
		/// <param name="severity">The severity of the error.</param>
		/// <param name="message">The error message.</param>
		public Error(ErrorSource source, ErrorSeverity severity, string message)
		{
			Source = source;
			Severity = severity;
			Message = message;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="Error" /> class.
		/// </summary>
		/// <param name="source">The source of the error.</param>
		/// <param name="severity">The severity of the error.</param>
		/// <param name="message">The error message.</param>
		/// <param name="details">Optional details about the error.</param>
		public Error(ErrorSource source, ErrorSeverity severity, string message, string details) : this(source, severity, message)
		{
			Details = details;
		}
	}
}