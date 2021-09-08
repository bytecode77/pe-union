using BytecodeApi.Extensions;
using System;
using System.IO;

namespace PEunion.Compiler
{
	/// <summary>
	/// Helper class that converts between absolute and relative paths.
	/// </summary>
	public static class RelativePath
	{
		/// <summary>
		/// Converts a relative path to an absolute path.
		/// </summary>
		/// <param name="baseDirectory">The base directory to use for relative paths.</param>
		/// <param name="relativePath">A relative or absolute path to convert.</param>
		/// <returns>
		/// If <paramref name="relativePath" /> is a relative path, the absolute path starting from <paramref name="baseDirectory" />;
		/// otherwise, the original value of <paramref name="relativePath" />.
		/// </returns>
		public static string RelativeToAbsolutePath(string baseDirectory, string relativePath)
		{
			if (relativePath.IsNullOrEmpty())
			{
				return relativePath;
			}
			else
			{
				return Path.IsPathRooted(relativePath) ? relativePath : Path.Combine(baseDirectory, relativePath);
			}
		}
		/// <summary>
		/// Converts an absolute path to a relative path.
		/// </summary>
		/// <param name="baseDirectory">The base directory to use for relative paths.</param>
		/// <param name="absolutePath">A relative or absolute path to convert.</param>
		/// <returns>
		/// If <paramref name="absolutePath" /> is an absolute path, the original value of <paramref name="absolutePath" />;
		/// otherwise the absolute path starting from <paramref name="baseDirectory" />.
		/// </returns>
		public static string AbsoluteToRelativePath(string baseDirectory, string absolutePath)
		{
			if (absolutePath.IsNullOrEmpty())
			{
				return absolutePath;
			}
			else
			{
				absolutePath = absolutePath.Replace("/", @"\");
				baseDirectory = baseDirectory.Replace("/", @"\");

				if (absolutePath.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase)) return absolutePath.Substring(baseDirectory.Length).TrimStart('\\');
				else return absolutePath;
			}
		}
	}
}