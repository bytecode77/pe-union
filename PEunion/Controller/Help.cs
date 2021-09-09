using BytecodeApi;
using MarkdownSharp;
using System.IO;

namespace PEunion
{
	public static class Help
	{
		public static bool GetHelpFile(string helpFile, out string html, out string errorMessage)
		{
			string templatePath = Path.Combine(ApplicationBase.Path, @"Help\Template.html");
			if (File.Exists(templatePath))
			{
				string path = Path.Combine(ApplicationBase.Path, "Help", helpFile + ".md");
				if (File.Exists(path))
				{
					html = File
						.ReadAllText(templatePath)
						.Replace("{BODY}", new Markdown().Transform(File.ReadAllText(path)))
						.Replace("<p><code>\n", "<pre>\n")
						.Replace("</code></p>\n", "</pre>\n");

					errorMessage = null;
					return true;
				}
				else
				{
					html = null;
					errorMessage = "File '" + Path.GetFileName(path) + "' not found.";
					return false;
				}
			}
			else
			{
				html = null;
				errorMessage = "File '" + Path.GetFileName(templatePath) + "' not found.";
				return false;
			}
		}
	}
}