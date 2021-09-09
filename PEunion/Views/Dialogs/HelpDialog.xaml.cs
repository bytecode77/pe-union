using BytecodeApi.UI.Dialogs;
using System.Windows;

namespace PEunion
{
	public partial class HelpDialog
	{
		public HelpDialogViewModel ViewModel { get; set; }

		private HelpDialog(string html)
		{
			ViewModel = new HelpDialogViewModel(this, html);
			InitializeComponent();

			if (Config.ViewState.HelpDialogWidth is int width && Config.ViewState.HelpDialogHeight is int height)
			{
				Width = width;
				Height = height;
			}
		}
		public static void Show(string helpFile)
		{
			if (Help.GetHelpFile(helpFile, out string html, out string error))
			{
				new HelpDialog(html).ShowDialog();
			}
			else
			{
				MessageBoxes.Error(error);
			}
		}
		private void HelpDialog_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (WindowState == WindowState.Normal)
			{
				Config.ViewState.HelpDialogWidth = (int)Width;
				Config.ViewState.HelpDialogHeight = (int)Height;
			}
		}
	}
}