namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines a file that will be downloaded by the stub.
	/// </summary>
	public sealed class DownloadSource : ProjectSource
	{
		/// <summary>
		/// Gets or sets the URL of the downloaded file.
		/// </summary>
		public string Url { get; set; }
	}
}