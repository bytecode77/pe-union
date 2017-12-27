using System.ComponentModel;

namespace PEunion
{
	public enum BuildPlatform
	{
		[Description("AnyCPU")]
		AnyCPU = 0,
		[Description("x86")]
		Win32 = 32,
		[Description("x64")]
		Win64 = 64,
	}
}