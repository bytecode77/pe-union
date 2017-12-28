using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PEunion
{
	public partial class TextDisplay : UserControl
	{
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextDisplay));
		public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(TextDisplay));
		public static readonly DependencyProperty InfoIconProperty = DependencyProperty.Register(nameof(InfoIcon), typeof(string), typeof(TextDisplay));
		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}
		public ImageSource ImageSource
		{
			get => (ImageSource)GetValue(ImageSourceProperty);
			set => SetValue(ImageSourceProperty, value);
		}
		public string InfoIcon
		{
			get => (string)GetValue(InfoIconProperty);
			set => SetValue(InfoIconProperty, value);
		}

		public TextDisplay()
		{
			InitializeComponent();
		}
	}
}