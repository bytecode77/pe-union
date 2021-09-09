using BytecodeApi.UI.Data;
using System.Windows.Media;

namespace PEunion
{
	public abstract class TabModel : ObservableObject
	{
		public string TabTitle
		{
			get => Get(() => TabTitle);
			set => Set(() => TabTitle, value);
		}
		public ImageSource TabIcon
		{
			get => Get(() => TabIcon);
			set => Set(() => TabIcon, value);
		}
		public bool IsDirty
		{
			get => Get(() => IsDirty);
			set => Set(() => IsDirty, value);
		}
		public virtual PageModel[] Pages { get; }
		public virtual PageModel SelectedPage { get; set; }
		public virtual ErrorModel Errors { get; set; }
	}
}