using BytecodeApi.Mathematics;

namespace PEunion
{
	public sealed class VersionInfoModel : PageModel
	{
		public string FileDescription
		{
			get => Get(() => FileDescription);
			set => Set(() => FileDescription, value);
		}
		public string ProductName
		{
			get => Get(() => ProductName);
			set => Set(() => ProductName, value);
		}
		public int FileVersion1
		{
			get => Get(() => FileVersion1);
			set => Set(() => FileVersion1, MathEx.Map(value, 0, 65535));
		}
		public int FileVersion2
		{
			get => Get(() => FileVersion2);
			set => Set(() => FileVersion2, MathEx.Map(value, 0, 65535));
		}
		public int FileVersion3
		{
			get => Get(() => FileVersion3);
			set => Set(() => FileVersion3, MathEx.Map(value, 0, 65535));
		}
		public int FileVersion4
		{
			get => Get(() => FileVersion4);
			set => Set(() => FileVersion4, MathEx.Map(value, 0, 65535));
		}
		public string ProductVersion
		{
			get => Get(() => ProductVersion);
			set => Set(() => ProductVersion, value);
		}
		public string Copyright
		{
			get => Get(() => Copyright);
			set => Set(() => Copyright, value);
		}
		public string OriginalFilename
		{
			get => Get(() => OriginalFilename);
			set => Set(() => OriginalFilename, value);
		}

		public VersionInfoModel() : base(PageTemplate.VersionInfo, "Version Info")
		{
		}
	}
}