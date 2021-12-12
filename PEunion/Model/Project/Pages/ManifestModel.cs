using PEunion.Compiler.Project;

namespace PEunion
{
	public sealed class ManifestModel : PageModel
	{
		private bool _UseNone = true;
		private bool _UseTemplate;
		private bool _UseFile;
		private ManifestTemplate _Template = ManifestTemplate.Default;
		private string _Path;
		public bool UseNone
		{
			get => _UseNone;
			set
			{
				Set(ref _UseNone, value);
				if (UseNone)
				{
					UseTemplate = false;
					UseFile = false;
				}
			}
		}
		public bool UseTemplate
		{
			get => _UseTemplate;
			set
			{
				Set(ref _UseTemplate, value);
				if (UseTemplate)
				{
					UseNone = false;
					UseFile = false;
				}
			}
		}
		public bool UseFile
		{
			get => _UseFile;
			set
			{
				Set(ref _UseFile, value);
				if (UseFile)
				{
					UseNone = false;
					UseTemplate = false;
				}
			}
		}
		public ManifestTemplate Template
		{
			get => _Template;
			set => Set(ref _Template, value);
		}
		public string Path
		{
			get => _Path;
			set => Set(ref _Path, value);
		}

		public ManifestModel() : base(PageTemplate.Manifest, "Manifest")
		{
		}
	}
}