namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines the base of an action for a <see cref="ProjectFile" />.
	/// </summary>
	public abstract class ProjectAction : ProjectItem
	{
		/// <summary>
		/// Gets the unique identifier that is used during compilation.
		/// </summary>
		public int AssemblyId => Project.Actions.IndexOf(this) + 1;
		/// <summary>
		/// Gets or sets the associated <see cref="ProjectSource" />.
		/// </summary>
		public ProjectSource Source { get; set; }
	}
}