namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines the stub (main program).
	/// </summary>
	public sealed class Stub : ProjectItem
	{
		/// <summary>
		/// Gets or sets the type of the stub.
		/// </summary>
		public StubType Type { get; set; }
		/// <summary>
		/// Gets or sets the path to an icon group file.
		/// </summary>
		public string IconPath { get; set; }
		/// <summary>
		/// Gets or sets the padding for the low-entropy packing scheme.
		/// 0 means no extra padding, 100 means to double the size and 200 means to tripple the size. This can be a number between 0 and 1000.
		/// </summary>
		public int Padding { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Stub" /> class.
		/// </summary>
		public Stub()
		{
			Type = StubType.Pe32;
		}
	}
}