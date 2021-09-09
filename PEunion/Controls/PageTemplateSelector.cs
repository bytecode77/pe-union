using System;
using System.Windows;
using System.Windows.Controls;

namespace PEunion
{
	public sealed class PageTemplateSelector : DataTemplateSelector
	{
		public DataTemplate ProjectTemplate { get; set; }
		public DataTemplate StubTemplate { get; set; }
		public DataTemplate StartupTemplate { get; set; }
		public DataTemplate VersionInfoTemplate { get; set; }
		public DataTemplate ManifestTemplate { get; set; }
		public DataTemplate ProjectItemsTemplate { get; set; }
		public DataTemplate ProjectRunPEItemTemplate { get; set; }
		public DataTemplate ProjectInvokeItemTemplate { get; set; }
		public DataTemplate ProjectDropItemTemplate { get; set; }
		public DataTemplate ProjectMessageBoxItemTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			switch ((item as PageModel)?.PageTemplate)
			{
				case null: return null;
				case PageTemplate.Project: return ProjectTemplate;
				case PageTemplate.Stub: return StubTemplate;
				case PageTemplate.Startup: return StartupTemplate;
				case PageTemplate.VersionInfo: return VersionInfoTemplate;
				case PageTemplate.Manifest: return ManifestTemplate;
				case PageTemplate.Items: return ProjectItemsTemplate;
				case PageTemplate.RunPEItem: return ProjectRunPEItemTemplate;
				case PageTemplate.InvokeItem: return ProjectInvokeItemTemplate;
				case PageTemplate.DropItem: return ProjectDropItemTemplate;
				case PageTemplate.MessageBoxItem: return ProjectMessageBoxItemTemplate;
				default: throw new InvalidOperationException();
			}
		}
	}
}