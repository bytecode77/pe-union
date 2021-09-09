using PEunion.Compiler.UI;
using System.ComponentModel;

namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines the verb that is used to execute a file using ShellExecute.
	/// </summary>
	public enum ExecuteVerb
	{
		/// <summary>
		/// The file is not executed.
		/// </summary>
		[Description("none")]
		[UiName("Do not execute")]
		None = 0,
		/// <summary>
		/// The file is executed using the "open" verb.
		/// </summary>
		[Description("open")]
		[UiName("Execute (open)")]
		Open = 1,
		/// <summary>
		/// The file is executed using the "runas" verb.
		/// </summary>
		[Description("runas")]
		[UiName("Execute elevated (runas)")]
		Runas = 2
	}
}