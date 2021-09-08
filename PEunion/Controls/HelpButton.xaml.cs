using BytecodeApi.UI;
using BytecodeApi.UI.Dialogs;
using System.Windows;

namespace PEunion
{
	public partial class HelpButton
	{
		public static readonly DependencyProperty HelpFileProperty = DependencyPropertyEx.Register(nameof(HelpFile));
		public static readonly DependencyProperty SmallProperty = DependencyPropertyEx.Register(nameof(Small), new PropertyMetadata(true));
		public string HelpFile
		{
			get => GetValue(() => HelpFile);
			set => SetValue(() => HelpFile, value);
		}
		public bool Small
		{
			get => GetValue(() => Small);
			set => SetValue(() => Small, value);
		}

		public HelpButton()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (Help.GetHelpFile(HelpFile, out string html, out string error))
			{
				MainWindowViewModel.Singleton.HelpHtml = html;
			}
			else
			{
				MessageBoxes.Error(error);
			}
		}
	}
}