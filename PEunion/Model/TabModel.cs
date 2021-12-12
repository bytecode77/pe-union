using BytecodeApi.UI.Data;
using System.Windows.Media;

namespace PEunion
{
	public abstract class TabModel : ObservableObject
	{
		private string _TabTitle;
		private ImageSource _TabIcon;
		private bool _IsDirty;
		public string TabTitle
		{
			get => _TabTitle;
			set => Set(ref _TabTitle, value);
		}
		public ImageSource TabIcon
		{
			get => _TabIcon;
			set => Set(ref _TabIcon, value);
		}
		public bool IsDirty
		{
			get => _IsDirty;
			set => Set(ref _IsDirty, value);
		}
		public virtual PageModel[] Pages { get; }
		public virtual PageModel SelectedPage { get; set; }
		public virtual ErrorModel Errors { get; set; }
	}
}