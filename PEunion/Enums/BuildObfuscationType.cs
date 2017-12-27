using System.ComponentModel;

namespace PEunion
{
	public enum BuildObfuscationType
	{
		[Description("None")]
		None = 0,
		[Description("Alphanumeric")]
		AlphaNumeric = 1,
		[Description("Special")]
		Special = 2
	}
}