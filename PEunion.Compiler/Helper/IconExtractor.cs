using BytecodeApi.Extensions;
using BytecodeApi.IO;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PEunion.Compiler.Helper
{
	/// <summary>
	/// Helper class that retrieves icons from .ico or .exe files.
	/// </summary>
	public static class IconExtractor
	{
		/// <summary>
		/// Validates whether an .ico or .exe file has an icon.
		/// </summary>
		/// <param name="path">The path to an .ico or .exe file.</param>
		/// <returns>
		/// <see langword="true" />, if the specified file has icon;
		/// otherwise, <see langword="false" />.
		/// </returns>
		public static bool HasIcon(string path)
		{
			bool hasIcon = false;

			using (Icon icon = FromFile(path))
			{
				if (icon != null)
				{
					Icon[] icons = icon.Split();
					hasIcon = icons.Any();
					foreach (Icon i in icons) i.Dispose();
				}
			}

			return hasIcon;
		}
		/// <summary>
		/// Retrieves an <see cref="Icon" /> from an .ico or .exe file.
		/// </summary>
		/// <param name="path">The path to an .ico or .exe file.</param>
		/// <returns>
		/// A new <see cref="Icon" /> object that was loaded from an .ico file or extracted from an .exe file.
		/// If the icon could not be retrieved, <see langword="null" /> is returned.
		/// </returns>
		public static Icon FromFile(string path)
		{
			try
			{
				if (Path.GetExtension(path).Equals(".ico", StringComparison.OrdinalIgnoreCase)) return new Icon(path);
				else if (Path.GetExtension(path).Equals(".exe", StringComparison.OrdinalIgnoreCase)) return FromExecutable(path);
				else return null;
			}
			catch
			{
				return null;
			}
		}
		private static Icon FromExecutable(string path)
		{
			IntPtr module = IntPtr.Zero;

			try
			{
				module = LoadLibraryEx(path, IntPtr.Zero, 2);
				if (module == IntPtr.Zero) throw new Win32Exception();

				Icon result = null;

				EnumResourceNames(module, (IntPtr)14, (mod, type, name, lParam) =>
				{
					byte[] groupIcon = GetDataFromResource(module, (IntPtr)14, name);
					int count = BitConverter.ToUInt16(groupIcon, 4);
					int size = 6 + count * 16 + Enumerable.Range(0, count).Select(i => BitConverter.ToInt32(groupIcon, 6 + i * 14 + 8)).Sum();

					using (MemoryStream memoryStream = new MemoryStream(size))
					{
						using (BinaryWriter writer = new BinaryWriter(memoryStream))
						{
							writer.Write(groupIcon, 0, 6);

							for (int i = 0, offset = 6 + count * 16; i < count; ++i)
							{
								byte[] icon = GetDataFromResource(module, (IntPtr)3, (IntPtr)BitConverter.ToUInt16(groupIcon, 6 + i * 14 + 12));

								writer.Seek(6 + i * 16, SeekOrigin.Begin);
								writer.Write(groupIcon, 6 + i * 14, 8);
								writer.Write(icon.Length);
								writer.Write(offset);

								writer.Seek(offset, SeekOrigin.Begin);
								writer.Write(icon, 0, icon.Length);

								offset += icon.Length;
							}
						}

						using (MemoryStream iconStream = new MemoryStream(memoryStream.ToArray()))
						{
							result = new Icon(iconStream);
						}
					}

					return false;
				}, IntPtr.Zero);

				return result;
			}
			catch
			{
				return null;
			}
			finally
			{
				if (module != IntPtr.Zero) FreeLibrary(module);
			}
		}
		private static byte[] GetDataFromResource(IntPtr module, IntPtr type, IntPtr name)
		{
			IntPtr resourceInfo = FindResource(module, name, type);
			if (resourceInfo == IntPtr.Zero) throw new Win32Exception();

			IntPtr resourceData = LoadResource(module, resourceInfo);
			if (resourceData == IntPtr.Zero) throw new Win32Exception();

			IntPtr resourceLock = LockResource(resourceData);
			if (resourceLock == IntPtr.Zero) throw new Win32Exception();

			uint size = SizeofResource(module, resourceInfo);
			if (size == 0) throw new Win32Exception();

			byte[] buffer = new byte[size];
			Marshal.Copy(resourceLock, buffer, 0, buffer.Length);
			return buffer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
		private delegate bool EnumResourceNamesCallback(IntPtr module, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr LoadLibraryEx(string fileName, IntPtr file, uint flags);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool FreeLibrary(IntPtr module);
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool EnumResourceNames(IntPtr module, IntPtr type, EnumResourceNamesCallback callback, IntPtr lParam);
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr FindResource(IntPtr module, IntPtr name, IntPtr type);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr LoadResource(IntPtr module, IntPtr resourceInfo);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr LockResource(IntPtr resourceData);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern uint SizeofResource(IntPtr module, IntPtr resourceInfo);
	}
}