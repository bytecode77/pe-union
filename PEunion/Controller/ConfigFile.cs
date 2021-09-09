using BytecodeApi;
using BytecodeApi.FileFormats.Ini;
using BytecodeApi.Threading;
using System;
using System.IO;

namespace PEunion
{
	public sealed class ConfigFile
	{
		private readonly string Path;
		private readonly Deferrer SaveDeferrer;

		public ConfigFile(string fileName)
		{
			Path = System.IO.Path.Combine(ApplicationBase.Path, "Config", fileName);
			SaveDeferrer = new Deferrer(500);
		}

		public IniFile Load()
		{
			return File.Exists(Path) ? IniFile.FromFile(Path) : null;
		}
		public void Save(bool deferred, Func<IniFile> getIniFile)
		{
			Action save = () =>
			{
				Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
				getIniFile().Save(Path);
			};

			if (deferred) SaveDeferrer?.Invoke(save);
			else save();
		}
	}
}