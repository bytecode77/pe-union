using BytecodeApi.IO.FileSystem;
using PEunion.Compiler.Errors;
using PEunion.Compiler.Project;
using System.ComponentModel;
using System.IO;

namespace PEunion.Compiler.Compiler
{
	/// <summary>
	/// Defines the base class for all compilers.
	/// </summary>
	public abstract class ProjectCompiler
	{
		/// <summary>
		/// Gets the project file associated with this <see cref="ProjectCompiler" /> instance.
		/// </summary>
		public ProjectFile Project { get; private set; }
		/// <summary>
		/// Gets the intermediate directory path to store temporary files during compilations.
		/// </summary>
		public string IntermediateDirectory { get; private set; }
		/// <summary>
		/// Gets the path to the final output file.
		/// </summary>
		public string OutputFileName { get; private set; }
		internal ErrorCollection Errors { get; private set; }
		internal string IntermediateDirectorySource => Path.Combine(IntermediateDirectory, "src");
		internal string IntermediateDirectoryBinaries => Path.Combine(IntermediateDirectory, "bin");

		/// <summary>
		/// Initializes a new instance of the <see cref="ProjectCompiler" /> class.
		/// </summary>
		/// <param name="project">The project file to be associated with this <see cref="ProjectCompiler" /> instance.</param>
		/// <param name="intermediateDirectory">The intermediate directory path to store temporary files during compilations.</param>
		/// <param name="outputFileName">The path to the final output file.</param>
		/// <param name="errors">The <see cref="ErrorCollection" /> to write compilation errors to.</param>
		protected ProjectCompiler(ProjectFile project, string intermediateDirectory, string outputFileName, ErrorCollection errors)
		{
			Project = project;
			IntermediateDirectory = intermediateDirectory;
			OutputFileName = outputFileName;
			Errors = errors;
		}
		/// <summary>
		/// Creates a compiler instance based on the <see cref="StubType" /> of <paramref name="project" />.
		/// </summary>
		/// <param name="project">The project to compile.</param>
		/// <param name="intermediateDirectory">The intermediate directory path to store temporary files during compilations.</param>
		/// <param name="outputFileName">The path to the final output file.</param>
		/// <param name="errors">The <see cref="ErrorCollection" /> to write compilation errors to.</param>
		/// <returns>A new instance of a class that inherits <see cref="ProjectCompiler" />.</returns>
		public static ProjectCompiler Create(ProjectFile project, string intermediateDirectory, string outputFileName, ErrorCollection errors)
		{
			DirectoryEx.DeleteContents(intermediateDirectory, true);
			Directory.CreateDirectory(Path.Combine(intermediateDirectory, "src"));
			Directory.CreateDirectory(Path.Combine(intermediateDirectory, "bin"));

			switch (project.Stub.Type)
			{
				case StubType.Pe32:
					return new Pe32Compiler(project, intermediateDirectory, outputFileName, errors);
				case StubType.DotNet32:
				case StubType.DotNet64:
					return new DotNetCompiler(project, intermediateDirectory, outputFileName, errors);
				default:
					throw new InvalidEnumArgumentException();
			}
		}

		/// <summary>
		/// Compiles the project with the current settings.
		/// </summary>
		public abstract void Compile();
		/// <summary>
		/// Gets the path to an intermediate fie within the src\ subdirectory.
		/// </summary>
		/// <param name="fileName">The path to a file within the src\ subdirectory.</param>
		/// <returns>
		/// The combined path within the src\ subdirectory.
		/// </returns>
		protected string GetIntermediateSourcePath(string fileName)
		{
			return Path.Combine(IntermediateDirectorySource, fileName);
		}
		/// <summary>
		/// Gets the path to an intermediate fie within the bin\ subdirectory.
		/// </summary>
		/// <param name="fileName">The path to a file within the bin\ subdirectory.</param>
		/// <returns>
		/// The combined path within the bin\ subdirectory.
		/// </returns>
		protected string GetIntermediateBinaryPath(string fileName)
		{
			return Path.Combine(IntermediateDirectoryBinaries, fileName);
		}
	}
}