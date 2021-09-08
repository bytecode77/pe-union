using BytecodeApi;
using BytecodeApi.UI.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PEunion
{
	public abstract class PageModel : ObservableObject
	{
		private static readonly string[] InheritedPropertyNames = typeof(PageModel).GetProperties().Select(property => property.Name).ToArray();

		public PageTemplate PageTemplate
		{
			get => Get(() => PageTemplate);
			set => Set(() => PageTemplate, value);
		}
		public string PageTitle
		{
			get => Get(() => PageTitle);
			set => Set(() => PageTitle, value);
		}
		public bool IsVisible
		{
			get => Get(() => IsVisible, true);
			set => Set(() => IsVisible, value);
		}
		public bool IsSelected
		{
			get => Get(() => IsSelected);
			set => Set(() => IsSelected, value);
		}
		public bool IsExpanded
		{
			get => Get(() => IsExpanded);
			set => Set(() => IsExpanded, value && SubPages?.Any() == true);
		}
		public IEnumerable<PageModel> SubPages
		{
			get => Get(() => SubPages);
			set => Set(() => SubPages, value);
		}
		public event PropertyChangedEventHandler Changed;

		public PageModel(PageTemplate pageTemplate, string pageTitle)
		{
			PageTemplate = pageTemplate;
			PageTitle = pageTitle;
		}
		public PageModel(PageTemplate pageTemplate, string pageTitle, IEnumerable<PageModel> subPages) : this(pageTemplate, pageTitle)
		{
			SubPages = subPages;
			IsExpanded = subPages?.Any() == true;
		}

		protected override void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (CSharp.EqualsNone(e.PropertyName, InheritedPropertyNames))
			{
				Changed?.Invoke(this, e);
			}
		}
	}
}