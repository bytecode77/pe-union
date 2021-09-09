namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines startup parameters of the stub.
	/// </summary>
	public sealed class Startup : ProjectItem
	{
		/// <summary>
		/// Gets or sets a <see cref="bool" /> value indicating whether the stub should delete itself upon execution.
		/// </summary>
		public bool Melt { get; set; }
		/// <summary>
		/// Gets a <see cref="bool" /> value indicating whether no startup parameters are configured.
		/// </summary>
		public bool IsEmpty => !Melt;
	}
}