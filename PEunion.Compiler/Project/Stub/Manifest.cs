namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines an executable manifest.
	/// </summary>
	public sealed class Manifest : ProjectItem
	{
		/// <summary>
		/// Gets or sets a template for this manifest. This value is valid, if <see cref="Path" /> is <see langword="null" />.
		/// </summary>
		public ManifestTemplate? Template { get; set; }
		/// <summary>
		/// Gets or sets a path to a manifest file. This value is valid, if <see cref="Template" /> is <see langword="null" />.
		/// </summary>
		public string Path { get; set; }
	}
}