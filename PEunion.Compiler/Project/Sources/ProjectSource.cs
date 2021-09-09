using BytecodeApi;

namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines the base of a data source for a <see cref="ProjectFile" />.
	/// </summary>
	public abstract class ProjectSource : ProjectItem
	{
		/// <summary>
		/// Gets the unique identifier that is used during compilation.
		/// </summary>
		public int AssemblyId => Project.Sources.IndexOf(this) + 1;
		/// <summary>
		/// Gets or sets the guid that is parsed from the project file.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ProjectSource" /> class.
		/// </summary>
		public ProjectSource()
		{
			Id = Create.Guid(GuidFormat.Hyphens);
		}
	}
}