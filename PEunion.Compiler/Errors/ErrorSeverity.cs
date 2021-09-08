using System.ComponentModel;

namespace PEunion.Compiler.Errors
{
	/// <summary>
	/// Defines the severity of a compiler or project file parsing error.
	/// </summary>
	public enum ErrorSeverity
	{
		/// <summary>
		/// The error should inform the user about a suggested improvement or change.
		/// </summary>
		[Description("Message")]
		Message = 1,
		/// <summary>
		/// The error should warn the user about potentially unwanted configuration.
		/// </summary>
		[Description("Warning")]
		Warning = 2,
		/// <summary>
		/// The error is severe and the project cannot be compiled.
		/// </summary>
		[Description("Error")]
		Error = 3
	}
}