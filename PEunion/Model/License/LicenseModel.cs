using BytecodeApi;
using System.IO;

namespace PEunion
{
	public sealed class LicenseModel : TabModel
	{
		private string _Text;
		public string Text
		{
			get => _Text;
			set => Set(ref _Text, value);
		}

		public LicenseModel()
		{
			TabTitle = "License Agreement";

			string path = Path.Combine(ApplicationBase.Path, "LICENSE.md");
			string path3rdParty = Path.Combine(ApplicationBase.Path, "LICENSE-3RD-PARTY.md");

			if (!File.Exists(path))
			{
				Text = "File '" + Path.GetFileName(path) + "' not found.";
			}
			else if (!File.Exists(path3rdParty))
			{
				Text = "File '" + Path.GetFileName(path3rdParty) + "' not found.";
			}
			else
			{
				Text =
					File.ReadAllText(path).Trim() +
					"\r\n\r\n\r\n\r\n" +
					"--------------------------------------------------------------------------------\r\n" +
					"--                             3rd party licenses                             --\r\n" +
					"--------------------------------------------------------------------------------\r\n" +
					"\r\n" +
					File.ReadAllText(path3rdParty).Trim();
			}
		}
	}
}