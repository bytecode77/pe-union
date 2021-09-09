namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines an item within an instance of the <see cref="ProjectFile" /> class.
	/// </summary>
	public abstract class ProjectItem
	{
		/// <summary>
		/// Gets the associated <see cref="ProjectFile" /> of this <see cref="ProjectItem" />.
		/// </summary>
		public ProjectFile Project { get; internal set; }
	}
}