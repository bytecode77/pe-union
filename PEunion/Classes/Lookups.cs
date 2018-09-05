using BytecodeApi;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PEunion
{
	public static class Lookups
	{
		public static readonly Dictionary<BuildPlatform, string> Platforms = CSharp.GetEnumDescriptionLookup<BuildPlatform>();
		public static readonly Dictionary<BuildManifest, string> Manifests = CSharp.GetEnumDescriptionLookup<BuildManifest>();
		public static readonly Dictionary<BuildObfuscationType, string> Obfuscations = CSharp.GetEnumDescriptionLookup<BuildObfuscationType>();
		public static readonly Dictionary<BuildObfuscationType, string> ObfuscationExamples = new Dictionary<BuildObfuscationType, string>
		{
			{ BuildObfuscationType.None, "processStartInfo" },
			{ BuildObfuscationType.AlphaNumeric, "aYrhWB85JP9I8cEh" },
			{ BuildObfuscationType.Special, "갚갛갖갃감갧갈갓갔" }
		};
		public static readonly Dictionary<int, string> DropLocations = new Dictionary<int, string>
		{
			{ 1, "Temp" },
			{ 2, "Downloads" },
			{ 3, "Desktop" },
			{ 4, "My Documents" },
			{ 5, "Execution Directory" }
		};
		public static readonly Dictionary<MessageBoxButtons, string> MessageBoxButtons = new Dictionary<MessageBoxButtons, string>
		{
			{ System.Windows.Forms.MessageBoxButtons.OK, "OK" },
			{ System.Windows.Forms.MessageBoxButtons.OKCancel, "OK Cancel" },
			{ System.Windows.Forms.MessageBoxButtons.AbortRetryIgnore, "Abort Retry Ignore" },
			{ System.Windows.Forms.MessageBoxButtons.YesNo, "Yes No" },
			{ System.Windows.Forms.MessageBoxButtons.YesNoCancel, "Yes No Cancel" },
			{ System.Windows.Forms.MessageBoxButtons.RetryCancel, "Retry Cancel" }
		};
	}
}