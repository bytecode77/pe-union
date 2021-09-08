using BytecodeApi.Extensions;
using BytecodeApi.Mathematics;
using PEunion.Compiler.Helper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PEunion.Compiler.Compiler
{
	/// <summary>
	/// Provides an obfuscator for assembly (.asm) files.
	/// </summary>
	public sealed class AssemblyObfuscator
	{
		private readonly string[] Registers;
		private readonly string[][] NopCodes;
		private readonly string[][] NopCodesMinimal;

		/// <summary>
		/// Initializes a new instance of the <see cref="AssemblyObfuscator" /> class.
		/// </summary>
		/// <param name="configPath">The path to the stub configuration files.</param>
		public AssemblyObfuscator(string configPath)
		{
			Registers = ReadLineFile(Path.Combine(configPath, "register.txt"));
			NopCodes = ReadCodeBlockFile(Path.Combine(configPath, "nop.txt"));
			NopCodesMinimal = ReadCodeBlockFile(Path.Combine(configPath, "nop_minimal.txt"));
		}

		/// <summary>
		/// Obfuscates an assembly file.
		/// </summary>
		/// <param name="path">The path to the file to obfuscate.</param>
		public void ObfuscateFile(string path)
		{
			string[] lines = File.ReadAllLines(path, Encoding.Default);
			List<string> newLines = new List<string>(lines.Length);

			// Flag, whether to deactivate obfuscation, e.g. in tight loops that would otherwise cause performance issues
			bool obfuscationOff = false;

			// false = not in proc...endp
			// null = in proc, but in local variable declaration block
			// true = in proc
			bool? isProc = false;

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];
				string lineTrimmed = line.SubstringUntil(";").Trim();
				bool isCustomInstruction = false;

				// Add "obfuscation" comment to first line
				if (i == 0)
				{
					line = line.TabIndent(0, 80) + "; --- obfuscation ---";
				}

				if (lineTrimmed == "obfoff")
				{
					// Obfuscation is turned off
					obfuscationOff = true;
					isCustomInstruction = true;
				}
				else if (lineTrimmed == "obfon")
				{
					// Obfuscation is turned back on
					obfuscationOff = false;
					isCustomInstruction = true;
				}
				else if (lineTrimmed.StartsWith("proc"))
				{
					// proc ...
					// Then, either "local ..." or no "local" declarations
					isProc = lines[i + 1].Trim().StartsWith("local") ? (bool?)null : true;
				}
				else if (lineTrimmed == "endp")
				{
					// endp
					isProc = false;
				}
				else if (isProc == null && !lineTrimmed.StartsWith("local"))
				{
					// End of "local ..." declarations
					isProc = true;
				}

				// If inside procedure, add nop-like opcodes
				if (isProc == true &&
					lineTrimmed != "" &&
					lineTrimmed != "obfoff" &&
					lineTrimmed != "obfon" &&
					!lineTrimmed.StartsWith("proc") &&
					!(lineTrimmed.StartsWith(".") && lineTrimmed.EndsWith(":")))
				{
					if (obfuscationOff)
					{
						// Only add one nop-instruction, if obfuscation is turned off
						// This is for performance critical code, such as the decryption routine
						foreach (string obfuscationLine in GetNopCode(true)) newLines.Add(obfuscationLine.TabIndent(80, 0));
					}
					else
					{
						int count = MathEx.Random.Next(1, 3);
						for (int j = 0; j < count; j++)
						{
							foreach (string obfuscationLine in GetNopCode(false)) newLines.Add(obfuscationLine.TabIndent(80, 0));
						}
					}
				}

				if (!isCustomInstruction) newLines.Add(line);
			}

			File.WriteAllLines(path, newLines);
		}

		/// <summary>
		/// Generates nop-like instructions.
		/// </summary>
		/// <param name="minimal"><see langword="true" /> to use instructions from nop_minimal.txt; <see langword="false" /> to use nop.txt.</param>
		/// <returns>A new <see cref="string" />[] with lines of assembly code.</returns>
		private string[] GetNopCode(bool minimal)
		{
			string[] code = MathEx.Random.NextObject(minimal ? NopCodesMinimal : NopCodes).ToArray();
			string[] reg = Registers.SortRandom().ToArray();
			string[] rnd = Enumerable.Range(0, 3).Select(i => "0x" + MathEx.Random.NextInt32().ToString("x8")).ToArray();

			for (int i = 0; i < code.Length; i++)
			{
				for (int j = 0; j < reg.Length; j++)
				{
					code[i] = code[i].Replace("$" + (j + 1), reg[j]);
				}

				for (int j = 0; j < rnd.Length; j++)
				{
					code[i] = code[i].Replace("$rnd" + j, rnd[j]);
				}
			}

			return code;
		}
		private static string[][] ReadCodeBlockFile(string path)
		{
			List<string[]> codes = new List<string[]>();
			List<string> currentCode = new List<string>();

			foreach (string line in File.ReadAllLines(path))
			{
				if (line.Trim().StartsWith(";"))
				{
					if (currentCode.Any()) codes.Add(currentCode.ToArray());
					currentCode.Clear();
				}
				else if (!line.IsNullOrWhiteSpace())
				{
					currentCode.Add(line.SubstringUntil(";").TrimEnd());
				}
			}

			if (currentCode.Any()) codes.Add(currentCode.ToArray());
			return codes.ToArray();
		}
		private static string[] ReadLineFile(string path)
		{
			return File
				.ReadAllLines(path)
				.Select(line => line.Trim())
				.Where(line => line != "")
				.Where(line => !line.StartsWith(";"))
				.Select(line => line.SubstringUntil(";").TrimEnd())
				.ToArray();
		}
	}
}