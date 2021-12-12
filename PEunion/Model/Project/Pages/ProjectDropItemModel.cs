using PEunion.Compiler.Project;
using System.ComponentModel;
using System.IO;

namespace PEunion
{
	public sealed class ProjectDropItemModel : ProjectItemModel
	{
		private DropLocation _Location = DropLocation.Temp;
		private string _FileName;
		private bool _FileAttributeHidden;
		private bool _FileAttributeSystem;
		private ExecuteVerb _ExecuteVerb = ExecuteVerb.None;
		public DropLocation Location
		{
			get => _Location;
			set => Set(ref _Location, value);
		}
		public string FileName
		{
			get => _FileName;
			set => Set(ref _FileName, value);
		}
		public bool FileAttributeHidden
		{
			get => _FileAttributeHidden;
			set => Set(ref _FileAttributeHidden, value);
		}
		public bool FileAttributeSystem
		{
			get => _FileAttributeSystem;
			set => Set(ref _FileAttributeSystem, value);
		}
		public ExecuteVerb ExecuteVerb
		{
			get => _ExecuteVerb;
			set => Set(ref _ExecuteVerb, value);
		}

		public ProjectDropItemModel() : base(PageTemplate.DropItem, "Drop")
		{
			Changed += ProjectDropItemModel_Changed;
		}

		private void ProjectDropItemModel_Changed(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(SourceEmbeddedPath))
			{
				FileName = Path.GetFileName(SourceEmbeddedPath);
			}
		}
	}
}