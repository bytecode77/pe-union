using PEunion.Compiler.Project;

namespace PEunion
{
	public sealed class ManifestModel : PageModel
	{
		public bool UseNone
		{
			get => Get(() => UseNone, true);
			set
			{
				Set(() => UseNone, value);
				if (UseNone)
				{
					UseTemplate = false;
					UseFile = false;
				}
			}
		}
		public bool UseTemplate
		{
			get => Get(() => UseTemplate);
			set
			{
				Set(() => UseTemplate, value);
				if (UseTemplate)
				{
					UseNone = false;
					UseFile = false;
				}
			}
		}
		public bool UseFile
		{
			get => Get(() => UseFile);
			set
			{
				Set(() => UseFile, value);
				if (UseFile)
				{
					UseNone = false;
					UseTemplate = false;
				}
			}
		}
		public ManifestTemplate Template
		{
			get => Get(() => Template, ManifestTemplate.Default);
			set => Set(() => Template, value);
		}
		public string Path
		{
			get => Get(() => Path);
			set => Set(() => Path, value);
		}

		public ManifestModel() : base(PageTemplate.Manifest, "Manifest")
		{
		}
	}
}