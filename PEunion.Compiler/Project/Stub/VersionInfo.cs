using BytecodeApi.Extensions;

namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines executable version information.
	/// </summary>
	public sealed class VersionInfo : ProjectItem
	{
		/// <summary>
		/// Gets or sets the file description of the executables version information.
		/// </summary>
		public string FileDescription { get; set; }
		/// <summary>
		/// Gets or sets the product name of the executables version information.
		/// </summary>
		public string ProductName { get; set; }
		/// <summary>
		/// Gets or sets the file version of the executables version information.
		/// </summary>
		public string FileVersion { get; set; }
		/// <summary>
		/// Gets or sets the product version of the executables version information.
		/// </summary>
		public string ProductVersion { get; set; }
		/// <summary>
		/// Gets or sets the copyright text of the executables version information.
		/// </summary>
		public string Copyright { get; set; }
		/// <summary>
		/// Gets or sets the original filename of the compiled executable.
		/// </summary>
		public string OriginalFilename { get; set; }
		/// <summary>
		/// Gets a <see cref="bool" /> value indicating whether no version information is configured.
		/// </summary>
		public bool IsEmpty =>
			FileDescription.IsNullOrEmpty() &&
			ProductName.IsNullOrEmpty() &&
			FileVersion.IsNullOrEmpty() &&
			ProductVersion.IsNullOrEmpty() &&
			Copyright.IsNullOrEmpty() &&
			OriginalFilename.IsNullOrEmpty();
	}
}