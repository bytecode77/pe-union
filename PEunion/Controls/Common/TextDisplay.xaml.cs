using BytecodeApi.UI;
using BytecodeApi.UI.Controls;
using System.Windows;
using System.Windows.Media;

namespace PEunion
{
	public partial class TextDisplay : ObservableUserControl
	{
		public static readonly DependencyProperty TextProperty = DependencyPropertyEx.Register(nameof(Text));
		public static readonly DependencyProperty ImageSourceProperty = DependencyPropertyEx.Register(nameof(ImageSource));
		public static readonly DependencyProperty InfoIconProperty = DependencyPropertyEx.Register(nameof(InfoIcon));
		public string Text
		{
			get => GetValue(() => Text);
			set => SetValue(() => Text, value);
		}
		public ImageSource ImageSource
		{
			get => GetValue(() => ImageSource);
			set => SetValue(() => ImageSource, value);
		}
		public string InfoIcon
		{
			get => GetValue(() => InfoIcon);
			set => SetValue(() => InfoIcon, value);
		}

		public TextDisplay()
		{
			InitializeComponent();
		}
	}
}