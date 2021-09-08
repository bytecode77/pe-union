using BytecodeApi.Extensions;
using PEunion.Compiler.Errors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PEunion.Compiler.Compiler
{
	/// <summary>
	/// Provides a preprocessor for assembly (.asm) files.
	/// </summary>
	public static class AssemblyPreprocessor
	{
		private static readonly Regex InstructionRegex = new Regex(@"^[ \t]*(?<Instruction>lodstra|lodstrw)[ \t]+(?<Parameters>.*)$");

		/// <summary>
		/// Preprocesses all files in the specified directory.
		/// </summary>
		/// <param name="path">The path of the directory with the files to obfuscate.</param>
		/// <param name="recursive"><see langword="true" /> to include subdirectories; <see langword="false" /> to only include the current directory.</param>
		public static void PreprocessDirectory(string path, bool recursive)
		{
			foreach (string file in Directory.GetFiles(path, "*.asm", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
			{
				PreprocessFile(file);
			}
		}
		/// <summary>
		/// Preprocesses the specified file.
		/// </summary>
		/// <param name="path">The path to the file to preprocess.</param>
		public static void PreprocessFile(string path)
		{
			string[] lines = File.ReadAllLines(path);
			using (AssemblyStream assembly = new AssemblyStream(path))
			{
				assembly.Indent = 4;

				for (int line = 0; line < lines.Length; line++)
				{
					bool isInstruction;
					string instruction;
					string[] parameters;

					try
					{
						isInstruction = ParseInstruction(lines[line], out instruction, out parameters);
					}
					catch
					{
						ParseError();
					}

					if (isInstruction)
					{
						switch (instruction)
						{
							case "lodstra":
								{
									if (parameters.Length != 1) ParseError();

									string str = parameters[0];
									char current = 'a';

									assembly.EmitComment("Load string: " + str);
									assembly.Emit("mov", "ebx, '" + current + "'");

									for (int i = 0; i < str.Length; i++)
									{
										int offset = str[i] - current;
										current = str[i];

										if (offset == 1) assembly.Emit("inc", "bl");
										else if (offset > 1) assembly.Emit("add", "bl, " + offset);
										else if (offset == -1) assembly.Emit("dec", "bl");
										else if (offset < -1) assembly.Emit("sub", "bl, " + -offset);

										assembly.Emit("mov", "byte[eax + " + i + "], bl ;" + str[i]);
									}

									assembly.Emit("mov", "byte[eax + " + str.Length + "], 0");
								}
								break;
							case "lodstrw":
								{
									if (parameters.Length != 1) ParseError();

									string str = parameters[0];
									char current = 'a';

									assembly.EmitComment("Load string: " + str);
									assembly.Emit("mov", "ebx, '" + current + "'");

									for (int i = 0; i < str.Length; i++)
									{
										int offset = str[i] - current;
										current = str[i];

										if (offset == 1) assembly.Emit("inc", "bx");
										else if (offset > 1) assembly.Emit("add", "bx, " + offset);
										else if (offset == -1) assembly.Emit("dec", "bx");
										else if (offset < -1) assembly.Emit("sub", "bx, " + -offset);

										assembly.Emit("mov", "word[eax + " + i * 2 + "], bx ;" + str[i]);
									}

									assembly.Emit("mov", "word[eax + " + str.Length * 2 + "], 0");
								}
								break;
							default:
								ParseError();
								break;
						}
					}
					else
					{
						assembly.WriteLine(lines[line]);
					}

					void ParseError()
					{
						throw new ErrorException("Failed to parse preprocessor instruction in '" + Path.GetFileName(path) + "' at line " + (line + 1) + ".");
					}
				}
			}
		}
		private static bool ParseInstruction(string line, out string instruction, out string[] parameters)
		{
			Match match = InstructionRegex.Match(line);
			if (match.Success)
			{
				instruction = match.Groups["Instruction"].Value;
				string parameterString = match.Groups["Parameters"].Value.Trim();

				if (parameterString == "")
				{
					parameters = new string[0];
				}
				else
				{
					List<string> parsedParameters = new List<string>();
					string currentParameter = null;
					bool quoted = false;

					for (int i = 0; i < parameterString.Length; i++)
					{
						bool isLast = i == parameterString.Length - 1;

						if (parameterString[i] == ',')
						{
							if (isLast)
							{
								throw new ArgumentException();
							}
							else if (quoted)
							{
								currentParameter += parameterString[i];
							}
							else if (currentParameter == null)
							{
								throw new ArgumentException();
							}
							else
							{
								parsedParameters.Add(currentParameter.Trim());
								currentParameter = null;
							}
						}
						else if (parameterString[i] == '\'')
						{
							if (!isLast && parameterString[i + 1] == '\'')
							{
								i++;
								currentParameter += "''";
							}
							else
							{
								quoted = !quoted;
								currentParameter += '\'';

								if (!quoted && !isLast && parameterString[i + 1] != ',')
								{
									throw new ArgumentException();
								}
							}
						}
						else
						{
							currentParameter += parameterString[i];
						}
					}

					if (currentParameter != null)
					{
						parsedParameters.Add(currentParameter.Trim());
					}

					for (int i = 0; i < parsedParameters.Count; i++)
					{
						if (parsedParameters[i].First().IsLetter() && parsedParameters[i].All(c => c.IsLetterOrDigit()))
						{
							// Name literal
						}
						else if (parsedParameters[i].First() == '\'' && parsedParameters[i].Last() == '\'')
						{
							// String literal
							parsedParameters[i] = parsedParameters[i].Substring(1, parsedParameters[i].Length - 2).Replace("''", "'");
						}
						else
						{
							throw new ArgumentException();
						}
					}

					parameters = parsedParameters.ToArray();
				}

				return true;
			}
			else
			{
				instruction = null;
				parameters = null;
				return false;
			}
		}
	}
}