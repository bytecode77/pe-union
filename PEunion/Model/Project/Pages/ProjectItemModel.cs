using BytecodeApi;
using BytecodeApi.Extensions;
using System.ComponentModel;
using System.IO;

namespace PEunion
{
	public abstract class ProjectItemModel : PageModel
	{
		public ProjectItemSource Source
		{
			get => Get(() => Source, ProjectItemSource.Embedded);
			set => Set(() => Source, value);
		}
		public string SourceId
		{
			get => Get(() => SourceId, () => Create.Guid(GuidFormat.Hyphens));
			set => Set(() => SourceId, value);
		}
		public string SourceEmbeddedPath
		{
			get => Get(() => SourceEmbeddedPath);
			set => Set(() => SourceEmbeddedPath, value);
		}
		public bool SourceEmbeddedCompress
		{
			get => Get(() => SourceEmbeddedCompress);
			set => Set(() => SourceEmbeddedCompress, value);
		}
		public bool SourceEmbeddedEofData
		{
			get => Get(() => SourceEmbeddedEofData);
			set => Set(() => SourceEmbeddedEofData, value);
		}
		public string SourceDownloadUrl
		{
			get => Get(() => SourceDownloadUrl);
			set => Set(() => SourceDownloadUrl, value);
		}

		public ProjectItemModel(PageTemplate pageTemplate, string pageTitle) : base(pageTemplate, pageTitle)
		{
			UpdatePageTitle();
			Changed += ProjectItemModel_Changed;
		}

		private void UpdatePageTitle()
		{
			string sourceName;
			if (this is ProjectMessageBoxItemModel) sourceName = null;
			else if (Source == ProjectItemSource.Embedded && !SourceEmbeddedPath.IsNullOrEmpty()) sourceName = Path.GetFileName(SourceEmbeddedPath);
			else if (Source == ProjectItemSource.Download && !SourceDownloadUrl.IsNullOrWhiteSpace()) sourceName = SourceDownloadUrl;
			else sourceName = null;

			string title;
			string text;

			if (this is ProjectRunPEItemModel)
			{
				title = "RunPE";
				text = sourceName;
			}
			else if (this is ProjectInvokeItemModel)
			{
				title = "Invoke";
				text = sourceName;
			}
			else if (this is ProjectDropItemModel dropItem)
			{
				title = "Drop";
				text = dropItem.Location.GetDescription() + @"\" + (dropItem.FileName.ToNullIfEmpty() ?? sourceName);
			}
			else if (this is ProjectMessageBoxItemModel messageBoxItem)
			{
				title = "Message Box";
				text = messageBoxItem.Title.ToNullIfEmpty();
			}
			else
			{
				throw new InvalidEnumArgumentException();
			}

			PageTitle = title + (text == null ? null : ": " + text);
		}

		private void ProjectItemModel_Changed(object sender, PropertyChangedEventArgs e)
		{
			UpdatePageTitle();
		}
	}
}