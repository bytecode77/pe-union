using BytecodeApi;
using BytecodeApi.Extensions;
using BytecodeApi.IO;
using BytecodeApi.Mathematics;
using Microsoft.CSharp;
using PEunion.Compiler.Errors;
using PEunion.Compiler.Helper;
using PEunion.Compiler.Project;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PEunion.Compiler.Compiler
{
	/// <summary>
	/// Helper class for the compilation of projects.
	/// </summary>
	internal sealed class CompilerHelper
	{
		private readonly ProjectFile Project;
		private readonly ErrorCollection Errors;

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilerHelper" /> class.
		/// </summary>
		/// <param name="project">The <see cref="ProjectFile" /> associated with this helper class instance.</param>
		/// <param name="errors">The <see cref="ErrorCollection" /> associated with this helper class instance.</param>
		public CompilerHelper(ProjectFile project, ErrorCollection errors)
		{
			Project = project;
			Errors = errors;
		}

		/// <summary>
		/// Validates that all files and directories exist in the specified directory.
		/// </summary>
		/// <param name="directory">The base directory to check for the existence of files and directories.</param>
		/// <param name="files">An array of files and directories. An array element is considered to be a directory, if the <see cref="string" /> ends with "\".</param>
		public void ValidateFiles(string directory, params string[] files)
		{
			foreach (string file in files)
			{
				if (file.EndsWith(@"\"))
				{
					if (!Directory.Exists(Path.Combine(directory, file)))
					{
						Errors.Add(ErrorSource.Compiler, ErrorSeverity.Error, "Directory '" + file.Left(file.Length - 1) + "' not found.");
					}
				}
				else
				{
					if (!File.Exists(Path.Combine(directory, file)))
					{
						Errors.Add(ErrorSource.Compiler, ErrorSeverity.Error, "File '" + file + "' not found.");
					}
				}
			}
		}
		/// <summary>
		/// Validates that the project only uses executables of a specific bitness for RunPE actions.
		/// </summary>
		/// <param name="bitness">The bitness to check. This value must be 32 or 64.</param>
		public void ValidateRunPEBitness(int bitness)
		{
			if (bitness != 32 && bitness != 64) throw new ArgumentException("Argument must be '32' or '64'.", nameof(bitness));

			foreach (RunPEAction action in Project.Actions.OfType<RunPEAction>())
			{
				if (action.Source is EmbeddedSource embeddedSource)
				{
					int bits = ExecutableHelper.GetExecutableBitness(embeddedSource.Path, false);

					if (bits == 0)
					{
						Errors.Add(ErrorSource.Compiler, ErrorSeverity.Error, "File '" + Path.GetFileName(embeddedSource.Path) + "' is not a valid executable.");
					}
					else if (bits == 64 && bitness == 32)
					{
						Errors.Add(ErrorSource.Compiler, ErrorSeverity.Error, "Cannot use 64-bit executable '" + Path.GetFileName(embeddedSource.Path) + "' with RunPE in a 32-bit stub.");
					}
					else if (bits == 32 && bitness == 64)
					{
						Errors.Add(ErrorSource.Compiler, ErrorSeverity.Error, "Cannot use 32-bit executable '" + Path.GetFileName(embeddedSource.Path) + "' with RunPE in a 64-bit stub.");
					}

					if (ExecutableHelper.GetExecutableBitness(embeddedSource.Path, true) != 0)
					{
						Errors.Add(ErrorSource.Compiler, ErrorSeverity.Error, "Cannot use .NET executable '" + Path.GetFileName(embeddedSource.Path) + "' with RunPE. Use a .NET stub with an invoke action instead.");
					}
				}
			}
		}
		/// <summary>
		/// Validates that the project only uses executables of a specific bitness for .NET invoke actions.
		/// </summary>
		/// <param name="bitness">The bitness to check. This value must be 32 or 64.</param>
		public void ValidateInvokeBitness(int bitness)
		{
			if (bitness != 32 && bitness != 64) throw new ArgumentException("Argument must be '32' or '64'.", nameof(bitness));

			foreach (InvokeAction action in Project.Actions.OfType<InvokeAction>())
			{
				if (action.Source is EmbeddedSource embeddedSource)
				{
					int bits = ExecutableHelper.GetExecutableBitness(embeddedSource.Path, true);

					if (bits == 0)
					{
						Errors.Add(ErrorSource.Compiler, ErrorSeverity.Error, "File '" + Path.GetFileName(embeddedSource.Path) + "' is not a valid .NET executable.");
					}
					else if (bits == 64 && bitness == 32)
					{
						Errors.Add(ErrorSource.Compiler, ErrorSeverity.Error, "Cannot use 64-bit .NET executable '" + Path.GetFileName(embeddedSource.Path) + "' with .NET invoke in a 32-bit stub.");
					}
					else if (bits == 32 && bitness == 64)
					{
						Errors.Add(ErrorSource.Compiler, ErrorSeverity.Error, "Cannot use 32-bit .NET executable '" + Path.GetFileName(embeddedSource.Path) + "' with .NET invoke in a 64-bit stub.");
					}
				}
			}
		}
		/// <summary>
		/// Appends EOF data to the compiled stub, if an <see cref="EmbeddedSource" /> has the <see cref="EmbeddedSource.EofData" /> flag.,
		/// </summary>
		/// <param name="outputFileName">The path to the compiled stub to append the EOF data to.</param>
		public void AppendEofData(string outputFileName)
		{
			if (Project.Sources.OfType<EmbeddedSource>().FirstOrDefault(source => source.EofData) is EmbeddedSource eofSource)
			{
				if (ExecutableHelper.ExtractEofData(eofSource.Path) is byte[] eofData)
				{
					using (FileStream file = File.OpenWrite(outputFileName))
					{
						file.Seek(0, SeekOrigin.End);
						file.Write(eofData);
					}
				}
				else
				{
					Errors.Add(ErrorSource.Assembly, ErrorSeverity.Warning, "File '" + Path.GetFileName(eofSource.Path) + "' does not contain EOF data.");
				}
			}
		}
		/// <summary>
		/// Assembles an assembly (.asm) file using the FASM.exe compiler.
		/// </summary>
		/// <param name="inputFileName">The source code file (.asm) to assemble.</param>
		/// <param name="outputFileName">The executable file to write.</param>
		/// <param name="fasmOutput">The output that FASM.exe generated</param>
		/// <returns>
		/// The exit code of FASM.exe, after it has exited.
		/// </returns>
		public int FasmCompile(string inputFileName, string outputFileName, out string fasmOutput)
		{
			Environment.SetEnvironmentVariable("INCLUDE", Path.Combine(ApplicationBase.Path, @"FASM\INCLUDE"), EnvironmentVariableTarget.Process);

			fasmOutput = ProcessEx.ReadProcessOutput
			(
				Path.Combine(ApplicationBase.Path, @"FASM\FASM.exe"),
				"\"" + inputFileName + "\" \"" + outputFileName + "\"",
				true,
				true,
				out int exitCode
			);

			return exitCode;
		}
		/// <summary>
		/// Escapes a single quoted <see cref="string" /> for a definition block in assembler.
		/// </summary>
		/// <param name="str">The <see cref="string" /> to escape.</param>
		/// <returns>The escaped version of <paramref name="str" />.</returns>
		public string FasmEscapeDefinitionString(string str)
		{
			return str
				?.ReplaceMultiple(null, "\r", "\n")
				.Replace("\t", "    ")
				.Replace("'", "''");
		}
		/// <summary>
		/// Assembles a C# (.cs) file using the .NET compiler.
		/// </summary>
		/// <param name="inputFileNames">An array of paths to source code files (.cs) to assemble.</param>
		/// <param name="inputAssemblies">An array of assemblys (.dll) to link.</param>
		/// <param name="manifestFileName">The path to a manifest file, or <see langword="null" />, to compile without a manifest.</param>
		/// <param name="iconFileName">The path to an icon group file, or <see langword="null" />, to compile without an icon.</param>
		/// <param name="outputFileName">The executable file to write.</param>
		/// <param name="is64bit"><see langword="true" /> to compile a 64-bit executable, <see langword="false" /> to compile a 32-bit executable.</param>
		/// <param name="errors">When this method returns, a collection of compiler generated errors.</param>
		/// <returns>
		/// <see langword="true" />, if compilation succeeded;
		/// otherwise, <see langword="false" />.
		/// </returns>
		public bool DotNetCompile(string[] inputFileNames, string[] inputAssemblies, string[] resourceFileNames, string manifestFileName, string iconFileName, string outputFileName, bool is64bit, out string[] errors)
		{
			using (CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } }))
			{
				CompilerParameters parameters = new CompilerParameters
				{
					GenerateExecutable = true,
					OutputAssembly = outputFileName,
					CompilerOptions = new[]
					{
						"/nostdlib",
						"/target:winexe",
						"/platform:" + (is64bit ? "x64" : "x86"),
						manifestFileName == null ? "/nowin32manifest" : "/win32manifest:\"" + manifestFileName + "\"",
						iconFileName == null ? null : "/win32icon:\"" + iconFileName + "\""
					}.ExceptNull().AsString(" ")
				};

				parameters.ReferencedAssemblies.AddRange(inputAssemblies);
				parameters.EmbeddedResources.AddRange(resourceFileNames);

				CompilerResults result = provider.CompileAssemblyFromFile(parameters, inputFileNames);

				errors = result.Errors
					.Cast<CompilerError>()
					.Select(error => Path.GetFileName(error.FileName) + " line " + error.Line + ": " + error.ErrorNumber + " " + error.ErrorText)
					.ToArray();

				return result.Errors.None();
			}
		}
		/// <summary>
		/// Encrypts data using a low-entropy packing scheme.
		/// </summary>
		/// <param name="path">The path to the data to encrypt.</param>
		/// <param name="encryptedPath">The path to write the encrypted data to.</param>
		/// <param name="padding">The percentage of padding to add, where 0 means no extra padding, 100 means to double the size and 200 means to tripple the size. This can be a number between 0 and 1000.</param>
		/// <param name="key">When this function returns, this is the 32-bit decryption key.</param>
		/// <param name="paddingMask">When this function returns, this is the 32-bit padding mask.</param>
		/// <param name="paddingByteCount">When this function returns, this is the number of bytes of to use for a padding block.</param>
		public void EncryptData(string path, string encryptedPath, int padding, out uint key, out uint paddingMask, out int paddingByteCount)
		{
			// 32-bit encryption key
			key = MathEx.RandomNumberGenerator.GetUInt32();

			// paddingMaskBits is a 32-bit number with a certain number of bits set.
			// After each encrypted byte, this mask is rotated right by 1.
			// If the least significant bit is 1, a padding is added to the encrypted data stream.
			// paddingByteCount is the number of consecutive bytes that are used as padding.

			paddingByteCount = 1 + padding / 100;
			int paddingMaskBits = padding * 32 / (100 + padding / 100 * 100);

			paddingMask = 0;
			foreach (int i in Enumerable.Range(0, 32).SortRandom().Take(paddingMaskBits))
			{
				paddingMask = BitCalculator.SetBit(paddingMask, i, true);
			}

			uint currentKey = key;
			uint currentPaddingMask = paddingMask;

			using (FileStream stream = File.OpenRead(path))
			using (FileStream encryptedStream = File.Create(encryptedPath))
			{
				int dataByte;
				while ((dataByte = stream.ReadByte()) != -1)
				{
					// Xor the current byte with the least significant byte of the key.
					// Then, key = (key ror 5) * 7
					encryptedStream.WriteByte((byte)(dataByte ^ currentKey));

					// If the padding mask's least significant bit is 1, add the given number of padding bytes.
					// Then, rotate padding mask right by 1.
					if ((currentPaddingMask & 1) == 1)
					{
						for (int j = 0; j < paddingByteCount; j++)
						{
							encryptedStream.WriteByte(0);
						}
					}

					currentKey = MathEx.Ror(currentKey, 5) * 7;
					currentPaddingMask = MathEx.Ror(currentPaddingMask, 1);
				}
			}
		}
		/// <summary>
		/// Retrieves a comment for a specified <see cref="ActionEvent" />.
		/// </summary>
		/// <param name="actionEvent">The <see cref="ActionEvent" /> to convert.</param>
		/// <returns>
		/// A <see cref="string" /> with the comment that matches the specified <see cref="ActionEvent" />.
		/// </returns>
		public string GetCommentForActionEvent(ActionEvent actionEvent)
		{
			switch (actionEvent)
			{
				case ActionEvent.None: return null;
				case ActionEvent.SkipNextAction: return "skip next action";
				case ActionEvent.Exit: return "exit";
				default: return null;
			}
		}
	}
}