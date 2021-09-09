using PEunion.Compiler.UI;
using System.ComponentModel;

namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines a predefined manifest template.
	/// </summary>
	public enum ManifestTemplate
	{
		/// <summary>
		/// Defines a manifest with a requestedExecutionLevel of "asInvoker".
		/// </summary>
		[Description("default")]
		[UiName("Default")]
		Default,
		/// <summary>
		/// Defines a manifest with a requestedExecutionLevel of "requireAdministrator".
		/// </summary>
		[Description("elevated")]
		[UiName("Elevated")]
		Elevated
	}
}