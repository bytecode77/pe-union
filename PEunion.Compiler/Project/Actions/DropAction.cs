namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines an action that drops a file on the disk.
	/// </summary>
	public sealed class DropAction : ProjectAction
	{
		/// <summary>
		/// Gets or sets the base directory where the file is dropped.
		/// </summary>
		public DropLocation Location { get; set; }
		/// <summary>
		/// Gets or sets the filename of the dropped file.
		/// </summary>
		public string FileName { get; set; }
		/// <summary>
		/// Gets or sets a <see cref="bool" /> value indicating whether to apply the "hidden" file attribute to the dropped file.
		/// </summary>
		public bool FileAttributeHidden { get; set; }
		/// <summary>
		/// Gets or sets a <see cref="bool" /> value indicating whether to apply the "system" file attribute to the dropped file.
		/// </summary>
		public bool FileAttributeSystem { get; set; }
		/// <summary>
		/// Gets or sets the execute verb, or <see cref="ExecuteVerb.None" />, if the file should not be executed.
		/// </summary>
		public ExecuteVerb ExecuteVerb { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DropAction" /> class.
		/// </summary>
		public DropAction()
		{
			Location = DropLocation.Temp;
			ExecuteVerb = ExecuteVerb.None;
		}

		internal int GetWin32FileAttributes()
		{
			int attributes = 0;
			if (FileAttributeHidden) attributes |= 2;
			if (FileAttributeSystem) attributes |= 4;
			return attributes;
		}
	}
}