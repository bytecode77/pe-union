using System.ComponentModel;

namespace PEunion
{
	public enum BuildManifest
	{
		[Description("No Manifest")]
		None = 0,
		[Description("Normal (asInvoker)")]
		AsInvoker = 1,
		[Description("UAC (requireAdministrator)")]
		RequireAdministrator = 2
	}
}