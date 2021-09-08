using BytecodeApi.Extensions;
using PEunion.Compiler.Compiler;
using PEunion.Compiler.Errors;
using PEunion.Compiler.Project;
using System;
using System.IO;
using System.Linq;

namespace PEunion.Build
{
	/// <summary>
	/// The commandline compiler that can be invoked using commandline arguments.
	/// </summary>
	public static class Program
	{
		public static int Main(string[] args)
		{
			bool deleteIfFailed = GetArgumentFlag(ref args, "-d");

			if (args.None())
			{
				Console.WriteLine("peubuild.exe input [output] [-d]");
				Console.WriteLine();
				Console.WriteLine("  input   Specifies the input (.peu) file to compile.");
				Console.WriteLine("  output  Specifies the output (.exe) file to save the result to.");
				Console.WriteLine("  -d      Delete the output file, if build has failed.");
				return 1;
			}
			else if (args.Length > 2)
			{
				Console.WriteLine("Too many arguments specified.");
				return 1;
			}
			else if (!Path.GetExtension(args[0]).Equals(".peu", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("Input file must have '.peu' extension.");
				return 1;
			}
			else if (!File.Exists(args[0]))
			{
				Console.WriteLine("File not found: " + args[0]);
				return 1;
			}
			else if (args.Length >= 2 && !Path.GetExtension(args[1]).Equals(".exe", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("Output file must have '.exe' extension.");
				return 1;
			}
			else if (args.Length >= 2 && Directory.Exists(args[1]))
			{
				Console.WriteLine("Output must be a file, but is a directory.");
				return 1;
			}

			string outputPath = args.Length >= 2 ? args[1] : Path.ChangeExtension(args[0], "exe");

			int exitCode = Compile(args[0], outputPath);
			if (exitCode != 0 && deleteIfFailed) File.Delete(outputPath);

			return exitCode;
		}
		private static bool GetArgumentFlag(ref string[] args, string flag)
		{
			if (args.Contains(flag))
			{
				args = args.Where(arg => arg != flag).ToArray();
				return true;
			}
			else
			{
				return false;
			}
		}
		private static int Compile(string input, string outputPath)
		{
			string intermediateDirectory = Path.Combine(Path.GetDirectoryName(input), ".peu", Path.GetFileNameWithoutExtension(input));
			string logPath = Path.Combine(intermediateDirectory, "compile.log");
			ErrorCollection errors = new ErrorCollection();

			ProjectFile project = ProjectFile.FromFile(input, errors);
			if (AssertErrors()) return 1;

			ProjectCompiler compiler = ProjectCompiler.Create
			(
				project,
				intermediateDirectory,
				outputPath,
				errors
			);

			compiler.Compile();
			if (AssertErrors()) return 1;

			try
			{
				errors.WriteToConsole();
				errors.ToFile(logPath);

				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("Successfully compiled " + Path.GetFileName(outputPath) + " (" + new FileInfo(outputPath).Length + " bytes).");
			}
			finally
			{
				Console.ResetColor();
				Console.WriteLine();
			}

			return 0;

			bool AssertErrors()
			{
				if (errors.HasErrors)
				{
					errors.WriteToConsole();
					Directory.CreateDirectory(Path.GetDirectoryName(logPath));
					errors.ToFile(logPath);
					return true;
				}
				else
				{
					return false;
				}
			}
		}
	}
}