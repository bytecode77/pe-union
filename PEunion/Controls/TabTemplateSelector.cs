using System;
using System.Windows;
using System.Windows.Controls;

namespace PEunion
{
	public sealed class TabTemplateSelector : DataTemplateSelector
	{
		public DataTemplate ProjectTemplate { get; set; }
		public DataTemplate RtloTemplate { get; set; }
		public DataTemplate LicenseTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is ProjectModel) return ProjectTemplate;
			else if (item is RtloModel) return RtloTemplate;
			else if (item is LicenseModel) return LicenseTemplate;
			else throw new InvalidOperationException();
		}
	}
}