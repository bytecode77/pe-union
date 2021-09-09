using BytecodeApi.FileFormats.PE;
using System;
using System.IO;
using System.Reflection;

namespace PEunion.Compiler.Helper
{
	/// <summary>
	/// Helper class that retrieves information about executable files.
	/// </summary>
	public static class ExecutableHelper
	{
		/// <summary>
		/// Gets the bitness of an executable file.
		/// </summary>
		/// <param name="path">The path to an executable file.</param>
		/// <param name="isDotNet"><see langword="true" />, if the file is a .NET executable; <see langword="false" />, if the file is a native executable.</param>
		/// <returns>
		/// 32 or 64, depending on the executable's bitnes.
		/// Returns 96, if the file is a .NET executable built with AnyCPU, and 0, if the bitness could not be determined.
		/// </returns>
		public static int GetExecutableBitness(string path, bool isDotNet)
		{
			try
			{
				if (isDotNet)
				{
					switch (AssemblyName.GetAssemblyName(path).ProcessorArchitecture)
					{
						case ProcessorArchitecture.X86: return 32;
						case ProcessorArchitecture.Amd64: return 64;
						case ProcessorArchitecture.MSIL: return 96;
						default: return 0;
					}
				}
				else
				{
					PEImage image = PEImage.FromFile(path);

					if (image.OptionalHeader is ImageOptionalHeader32) return 32;
					else if (image.OptionalHeader is ImageOptionalHeader64) return 64;
					else return 0;
				}
			}
			catch
			{
				return 0;
			}
		}
		/// <summary>
		/// Extracts the RawData from an executable that has only a .text section.
		/// </summary>
		/// <param name="path">The path to an executable that contains the shellcode.</param>
		/// <param name="shellCodePath">The path to write the raw shellcode to.</param>
		public static void ExtractShellcode(string path, string shellCodePath)
		{
			using (FileStream file = File.OpenRead(path))
			using (BinaryReader reader = new BinaryReader(file))
			{
				file.Seek(0x3c, SeekOrigin.Begin);
				int ntHeader = reader.ReadInt32();

				file.Seek(ntHeader + 0x14, SeekOrigin.Begin);
				short sizeOfOptionalHeader = reader.ReadInt16();

				file.Seek(ntHeader + 0x18 + sizeOfOptionalHeader, SeekOrigin.Begin);
				byte[] section = reader.ReadBytes(0x28);

				int pointerToRawData = BitConverter.ToInt32(section, 0x14);
				int virtualSize = BitConverter.ToInt32(section, 0x8);

				using (FileStream shellCodeFile = File.Create(shellCodePath))
				{
					file.Seek(pointerToRawData, SeekOrigin.Begin);

					byte[] buffer = new byte[4096];
					int bytesWritten = 0;
					while (bytesWritten < virtualSize)
					{
						int bytesRead = file.Read(buffer, 0, Math.Min(virtualSize - bytesWritten, buffer.Length));
						bytesWritten += bytesRead;

						shellCodeFile.Write(buffer, 0, bytesRead);
					}
				}
			}
		}
		/// <summary>
		/// Extracts the EOF data from an executable file. Returns <see langword="null" />, if the executable does not have EOF data.
		/// </summary>
		/// <param name="path">The path to an executable to extract the EOF data from.</param>
		/// <returns>
		/// A new <see cref="byte" />[] with the EOF data of the executable file, or <see langword="null" />, if the executable does not have EOF data.
		/// </returns>
		public static byte[] ExtractEofData(string path)
		{
			using (FileStream file = File.OpenRead(path))
			using (BinaryReader reader = new BinaryReader(file))
			{
				int fileSize = (int)file.Length;

				file.Seek(0x3c, SeekOrigin.Begin);
				int ntHeader = reader.ReadInt32();

				file.Seek(ntHeader + 0x6, SeekOrigin.Begin);
				short numberOfSections = reader.ReadInt16();

				file.Seek(ntHeader + 0x14, SeekOrigin.Begin);
				short sizeOfOptionalHeader = reader.ReadInt16();

				int executableSize = -1;

				for (short i = 0; i < numberOfSections; i++)
				{
					file.Seek(ntHeader + 0x18 + sizeOfOptionalHeader + i * 0x28, SeekOrigin.Begin);
					byte[] section = reader.ReadBytes(0x28);

					int pointerToRawData = BitConverter.ToInt32(section, 0x14);
					int sizeOfRawData = BitConverter.ToInt32(section, 0x10);
					executableSize = Math.Max(executableSize, pointerToRawData + sizeOfRawData);
				}

				if (executableSize != -1 && fileSize > executableSize)
				{
					file.Seek(executableSize, SeekOrigin.Begin);
					return reader.ReadBytes(fileSize - executableSize);
				}
				else
				{
					return null;
				}
			}
		}
	}
}