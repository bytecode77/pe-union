using BytecodeApi.UI;
using BytecodeApi.UI.Extensions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PEunion
{
	public partial class TextDisplay
	{
		public static readonly DependencyProperty TextProperty = DependencyPropertyEx.Register(nameof(Text));
		public static readonly DependencyProperty IconProperty = DependencyPropertyEx.Register(nameof(Icon));
		public static readonly DependencyProperty HelpFileProperty = DependencyPropertyEx.Register(nameof(HelpFile));
		public static readonly DependencyProperty CommandProperty = DependencyPropertyEx.Register(nameof(Command));
		public static readonly DependencyProperty CommandParameterProperty = DependencyPropertyEx.Register(nameof(CommandParameter));
		public string Text
		{
			get => this.GetValue<string>(TextProperty);
			set => SetValue(TextProperty, value);
		}
		public ImageSource Icon
		{
			get => this.GetValue<ImageSource>(IconProperty);
			set => SetValue(IconProperty, value);
		}
		public string HelpFile
		{
			get => this.GetValue<string>(HelpFileProperty);
			set => SetValue(HelpFileProperty, value);
		}
		public ICommand Command
		{
			get => this.GetValue<ICommand>(CommandProperty);
			set => SetValue(CommandProperty, value);
		}
		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		public TextDisplay()
		{
			InitializeComponent();
		}
	}
}