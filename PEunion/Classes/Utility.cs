using BytecodeApi;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace PEunion
{
	public static class Utility
	{
		public static string MakePathRelative(string path, string directory)
		{
			if (path.IsNullOrEmpty())
			{
				return path;
			}
			else
			{
				string relative = Uri
					.UnescapeDataString(new Uri(directory.EnsureEndsWith(@"\")).MakeRelativeUri(new Uri(path)).OriginalString)
					.Replace("/", @"\");

				return relative.StartsWith("..") ? path : relative;
			}
		}
		public static string MakePathAbsolute(string path, string directory)
		{
			if (path.IsNullOrEmpty()) return path;
			else if (path.Length >= 2 && path[1] == ':' || path.StartsWith(@"\\")) return path;
			else return Path.Combine(directory, path);
		}
		public static BitmapImage GetImageResource(string name)
		{
			return new BitmapImage((Packs.Application + "/PEunion;component/Resources/" + name + ".png").ToUriOrDefault(UriKind.Absolute));
		}
	}
}