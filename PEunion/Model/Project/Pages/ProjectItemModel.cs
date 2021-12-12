using BytecodeApi;
using BytecodeApi.Extensions;
using System.ComponentModel;
using System.IO;

namespace PEunion
{
	public abstract class ProjectItemModel : PageModel
	{
		private ProjectItemSource _Source = ProjectItemSource.Embedded;
		private string _SourceId = Create.Guid(GuidFormat.Hyphens);
		private string _SourceEmbeddedPath;
		private bool _SourceEmbeddedCompress;
		private bool _SourceEmbeddedEofData;
		private string _SourceDownloadUrl;
		public ProjectItemSource Source
		{
			get => _Source;
			set => Set(ref _Source, value);
		}
		public string SourceId
		{
			get => _SourceId;
			set => Set(ref _SourceId, value);
		}
		public string SourceEmbeddedPath
		{
			get => _SourceEmbeddedPath;
			set => Set(ref _SourceEmbeddedPath, value);
		}
		public bool SourceEmbeddedCompress
		{
			get => _SourceEmbeddedCompress;
			set => Set(ref _SourceEmbeddedCompress, value);
		}
		public bool SourceEmbeddedEofData
		{
			get => _SourceEmbeddedEofData;
			set => Set(ref _SourceEmbeddedEofData, value);
		}
		public string SourceDownloadUrl
		{
			get => _SourceDownloadUrl;
			set => Set(ref _SourceDownloadUrl, value);
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