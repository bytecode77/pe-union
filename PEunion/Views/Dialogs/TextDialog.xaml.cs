using System.Windows;

namespace PEunion
{
	public partial class TextDialog
	{
		public TextDialogViewModel ViewModel { get; set; }

		public TextDialog(string title, string text)
		{
			ViewModel = new TextDialogViewModel(this, title, text);
			InitializeComponent();

			if (Config.ViewState.TextDialogWidth is int width && Config.ViewState.TextDialogHeight is int height)
			{
				Width = width;
				Height = height;
			}
		}
		private void TextDialog_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (WindowState == WindowState.Normal)
			{
				Config.ViewState.TextDialogWidth = (int)Width;
				Config.ViewState.TextDialogHeight = (int)Height;
			}
		}
	}
}