namespace PEunion
{
	public sealed class StartupModel : PageModel
	{
		public bool Melt
		{
			get => Get(() => Melt);
			set => Set(() => Melt, value);
		}

		public StartupModel() : base(PageTemplate.Startup, "Startup")
		{
		}
	}
}