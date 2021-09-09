using PEunion.Compiler.UI;
using System.ComponentModel;

namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines a location where a file can be dropped.
	/// </summary>
	public enum DropLocation
	{
		/// <summary>
		/// The file is dropped in the temp directory.
		/// </summary>
		[Description("temp")]
		[UiName("Temp")]
		Temp = 1,
		/// <summary>
		/// The file is dropped in the executable directory.
		/// </summary>
		[Description("exedir")]
		[UiName("Executable directory")]
		ExecutableDirectory = 2,
		/// <summary>
		/// The file is dropped in the windows directory.
		/// </summary>
		[Description("windir")]
		[UiName("Windows directory")]
		WindowsDirectory = 3,
		/// <summary>
		/// The file is dropped in the system directory.
		/// </summary>
		[Description("system")]
		[UiName("System directory")]
		SystemDirectory = 4,
		/// <summary>
		/// The file is dropped in the program files directory.
		/// </summary>
		[Description("program_files")]
		[UiName("Program Files")]
		ProgramFiles = 5,
		/// <summary>
		/// The file is dropped in the program data directory.
		/// </summary>
		[Description("program_data")]
		[UiName("Program Data")]
		ProgramData = 6,
		/// <summary>
		/// The file is dropped in the downloads directory.
		/// </summary>
		[Description("downloads")]
		[UiName("Downloads")]
		Downloads = 7,
		/// <summary>
		/// The file is dropped in the desktop directory.
		/// </summary>
		[Description("desktop")]
		[UiName("Desktop")]
		Desktop = 8,
		/// <summary>
		/// The file is dropped in the AppData roaming directory.
		/// </summary>
		[Description("appdata_roaming")]
		[UiName("AppData (Roaming)")]
		AppDataRoaming = 9,
		/// <summary>
		/// The file is dropped in the AppData local directory.
		/// </summary>
		[Description("appdata_local")]
		[UiName("AppData (Local)")]
		AppDataLocal = 10,
		/// <summary>
		/// The file is dropped in the C:\ root directory.
		/// </summary>
		[Description("c")]
		[UiName(@"C:\")]
		CDrive = 11
	}
}