using PEunion.Compiler.Project;

namespace PEunion
{
	public sealed class StubModel : PageModel
	{
		public StubType Type
		{
			get => Get(() => Type, StubType.Pe32);
			set => Set(() => Type, value);
		}
		public string IconPath
		{
			get => Get(() => IconPath);
			set => Set(() => IconPath, value);
		}
		public int Padding
		{
			get => Get(() => Padding);
			set => Set(() => Padding, value);
		}

		public StubModel() : base(PageTemplate.Stub, "Stub")
		{
		}
	}
}