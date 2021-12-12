namespace PEunion
{
	public sealed class StartupModel : PageModel
	{
		private bool _Melt;
		public bool Melt
		{
			get => _Melt;
			set => Set(ref _Melt, value);
		}

		public StartupModel() : base(PageTemplate.Startup, "Startup")
		{
		}
	}
}