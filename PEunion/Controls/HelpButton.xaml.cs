using BytecodeApi.UI;
using BytecodeApi.UI.Dialogs;
using BytecodeApi.UI.Extensions;
using System.Windows;

namespace PEunion
{
	public partial class HelpButton
	{
		public static readonly DependencyProperty HelpFileProperty = DependencyPropertyEx.Register(nameof(HelpFile));
		public static readonly DependencyProperty SmallProperty = DependencyPropertyEx.Register(nameof(Small), new PropertyMetadata(true));
		public string HelpFile
		{
			get => this.GetValue<string>(HelpFileProperty);
			set => SetValue(HelpFileProperty, value);
		}
		public bool Small
		{
			get => this.GetValue<bool>(SmallProperty);
			set => SetValue(SmallProperty, value);
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