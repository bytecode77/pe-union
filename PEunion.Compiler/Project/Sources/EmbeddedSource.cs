namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines a file that is embedded into the compiled stub.
	/// </summary>
	public sealed class EmbeddedSource : ProjectSource
	{
		/// <summary>
		/// Gets or sets the path to the file that will be embedded.
		/// </summary>
		public string Path { get; set; }
		/// <summary>
		/// Gets or sets a <see cref="bool" /> value indicating whether this file should be compressed.
		/// </summary>
		public bool Compress { get; set; }
		/// <summary>
		/// Gets or sets a <see cref="bool" /> value indicating whether to append EOF data of this source to the compiled stub.
		/// </summary>
		public bool EofData { get; set; }
	}
}