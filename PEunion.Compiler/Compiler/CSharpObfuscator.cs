using BytecodeApi.Extensions;
using BytecodeApi.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PEunion.Compiler.Compiler
{
	/// <summary>
	/// Provides an obfuscator for C# files.
	/// </summary>
	public sealed class CSharpObfuscator
	{
		private static readonly Regex SymbolRegex = new Regex("__(?<Symbol>[a-zA-Z][a-zA-Z0-9]*)");
		private static readonly Regex StringRegex = new Regex("\\/\\*\\*\\/\\\"(?<String>[^\"]*)\\\"");
		private static readonly Regex IntegerRegex = new Regex("\\/\\*\\*\\/(?<Integer>(0[xX][0-9a-fA-F]+)|(-?[0-9]+))");
		private static readonly char[] ObfuscationCharacters = "각갂갃간갅갆갇갈갉갊갋갌갍갎갏감갑값갓갔강갖갗갘같갚갛개객갞갟갠갡갢갣갤갥갦갧갨갩갪갫갬갭갮갯".ToCharArray();

		private readonly Dictionary<string, string> SymbolMapping;

		/// <summary>
		/// Initializes a new instance of the <see cref="CSharpObfuscator" /> class.
		/// </summary>
		public CSharpObfuscator()
		{
			SymbolMapping = new Dictionary<string, string>();
		}
		/// <summary>
		/// Obfuscates a C# file.
		/// </summary>
		/// <param name="path">The path to the file to obfuscate.</param>
		public void ObfuscateFile(string path)
		{
			string code = File.ReadAllText(path);

			// Create a backup as a readable reference
			File.WriteAllText
			(
				Path.ChangeExtension(path, "original~cs"),
				"// Copy of original code (not obfuscated)\r\n\r\n" + code
			);

			// Encrypt string literals that are represented like /**/"....."
			for (Match match; (match = StringRegex.Match(code)).Success;)
			{
				code = code.Left(match.Index) + GenerateString(match.Groups["String"].Value) + code.Substring(match.Index + match.Length);
			}

			// Encrypt integer literals that are represented like /**/1234 or /**/-1234 or 0x1234
			for (Match match; (match = IntegerRegex.Match(code)).Success;)
			{
				code = code.Left(match.Index) + GenerateInteger(match.Groups["Integer"].Value) + code.Substring(match.Index + match.Length);
			}

			// Obfuscate names of symbols (classes, methods, etc.) that are represented like __Name
			for (Match match; (match = SymbolRegex.Match(code)).Success;)
			{
				string symbol = match.Groups["Symbol"].Value;

				// Use a name mapping to achieve consistency across files
				if (!SymbolMapping.ContainsKey(symbol)) SymbolMapping[symbol] = GenerateSymbol();

				code = code.Left(match.Index) + SymbolMapping[symbol] + code.Substring(match.Index + match.Length);
			}

			File.WriteAllText(path, code);
		}
		private string GenerateSymbol()
		{
			return Enumerable
				.Range(0, MathEx.Random.Next(10, 20))
				.Select(i => MathEx.Random.NextObject(ObfuscationCharacters))
				.AsString();
		}
		private string GenerateString(string str)
		{
			byte key = MathEx.Random.NextByte();
			return "__DecryptString(" + key + ", " + Regex.Unescape(str).Select(c => (c ^ key).ToString()).AsString(", ") + ")";
		}
		private string GenerateInteger(string integer)
		{
			int integerValue = integer.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt32(integer.Substring(2), 16) : integer.ToInt32OrDefault();
			int key = MathEx.Random.NextInt32();
			return "__DecryptInt32(" + (integerValue ^ key) + ", " + (key ^ 0x3d69c853) + ")";
		}
	}
}