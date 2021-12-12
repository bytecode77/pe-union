using BytecodeApi.Mathematics;

namespace PEunion
{
	public sealed class VersionInfoModel : PageModel
	{
		private string _FileDescription;
		private string _ProductName;
		private int _FileVersion1;
		private int _FileVersion2;
		private int _FileVersion3;
		private int _FileVersion4;
		private string _ProductVersion;
		private string _Copyright;
		private string _OriginalFilename;
		public string FileDescription
		{
			get => _FileDescription;
			set => Set(ref _FileDescription, value);
		}
		public string ProductName
		{
			get => _ProductName;
			set => Set(ref _ProductName, value);
		}
		public int FileVersion1
		{
			get => _FileVersion1;
			set => Set(ref _FileVersion1, MathEx.Map(value, 0, 65535));
		}
		public int FileVersion2
		{
			get => _FileVersion2;
			set => Set(ref _FileVersion2, MathEx.Map(value, 0, 65535));
		}
		public int FileVersion3
		{
			get => _FileVersion3;
			set => Set(ref _FileVersion3, MathEx.Map(value, 0, 65535));
		}
		public int FileVersion4
		{
			get => _FileVersion4;
			set => Set(ref _FileVersion4, MathEx.Map(value, 0, 65535));
		}
		public string ProductVersion
		{
			get => _ProductVersion;
			set => Set(ref _ProductVersion, value);
		}
		public string Copyright
		{
			get => _Copyright;
			set => Set(ref _Copyright, value);
		}
		public string OriginalFilename
		{
			get => _OriginalFilename;
			set => Set(ref _OriginalFilename, value);
		}

		public VersionInfoModel() : base(PageTemplate.VersionInfo, "Version Info")
		{
		}
	}
}