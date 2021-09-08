using BytecodeApi.UI;
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
			get => GetValue(() => Text);
			set => SetValue(() => Text, value);
		}
		public ImageSource Icon
		{
			get => GetValue(() => Icon);
			set => SetValue(() => Icon, value);
		}
		public string HelpFile
		{
			get => GetValue(() => HelpFile);
			set => SetValue(() => HelpFile, value);
		}
		public ICommand Command
		{
			get => GetValue(() => Command);
			set => SetValue(() => Command, value);
		}
		public object CommandParameter
		{
			get => GetValue(() => CommandParameter);
			set => SetValue(() => CommandParameter, value);
		}

		public TextDisplay()
		{
			InitializeComponent();
		}
	}
}