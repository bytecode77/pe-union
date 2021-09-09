using System.ComponentModel;

namespace PEunion.Compiler.Errors
{
	/// <summary>
	/// Defines the source of an error.
	/// </summary>
	public enum ErrorSource
	{
		/// <summary>
		/// The error was generated during project file parsing.
		/// </summary>
		[Description("Project")]
		Project,
		/// <summary>
		/// The error was generated during compilation.
		/// </summary>
		[Description("Compiler")]
		Compiler,
		/// <summary>
		/// The error was generated while assembling the compiled assembly.
		/// </summary>
		[Description("Assembly")]
		Assembly
	}
}