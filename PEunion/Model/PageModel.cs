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

		private PageTemplate _PageTemplate;
		private string _PageTitle;
		private bool _IsVisible = true;
		private bool _IsSelected;
		private bool _IsExpanded;
		private IEnumerable<PageModel> _SubPages;
		public PageTemplate PageTemplate
		{
			get => _PageTemplate;
			set => Set(ref _PageTemplate, value);
		}
		public string PageTitle
		{
			get => _PageTitle;
			set => Set(ref _PageTitle, value);
		}
		public bool IsVisible
		{
			get => _IsVisible;
			set => Set(ref _IsVisible, value);
		}
		public bool IsSelected
		{
			get => _IsSelected;
			set => Set(ref _IsSelected, value);
		}
		public bool IsExpanded
		{
			get => _IsExpanded;
			set => Set(ref _IsExpanded, value && SubPages?.Any() == true);
		}
		public IEnumerable<PageModel> SubPages
		{
			get => _SubPages;
			set => Set(ref _SubPages, value);
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