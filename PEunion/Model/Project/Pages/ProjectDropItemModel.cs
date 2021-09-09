using PEunion.Compiler.Project;
using System.ComponentModel;
using System.IO;

namespace PEunion
{
	public sealed class ProjectDropItemModel : ProjectItemModel
	{
		public DropLocation Location
		{
			get => Get(() => Location, DropLocation.Temp);
			set => Set(() => Location, value);
		}
		public string FileName
		{
			get => Get(() => FileName);
			set => Set(() => FileName, value);
		}
		public bool FileAttributeHidden
		{
			get => Get(() => FileAttributeHidden);
			set => Set(() => FileAttributeHidden, value);
		}
		public bool FileAttributeSystem
		{
			get => Get(() => FileAttributeSystem);
			set => Set(() => FileAttributeSystem, value);
		}
		public ExecuteVerb ExecuteVerb
		{
			get => Get(() => ExecuteVerb, ExecuteVerb.None);
			set => Set(() => ExecuteVerb, value);
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