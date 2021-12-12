using PEunion.Compiler.Project;

namespace PEunion
{
	public sealed class StubModel : PageModel
	{
		private StubType _Type = StubType.Pe32;
		private string _IconPath;
		private int _Padding;
		public StubType Type
		{
			get => _Type;
			set => Set(ref _Type, value);
		}
		public string IconPath
		{
			get => _IconPath;
			set => Set(ref _IconPath, value);
		}
		public int Padding
		{
			get => _Padding;
			set => Set(ref _Padding, value);
		}

		public StubModel() : base(PageTemplate.Stub, "Stub")
		{
		}
	}
}