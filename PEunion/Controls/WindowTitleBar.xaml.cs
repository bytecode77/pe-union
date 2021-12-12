using BytecodeApi.UI;
using BytecodeApi.UI.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PEunion
{
	public partial class WindowTitleBar
	{
		public static readonly DependencyProperty IconProperty = DependencyPropertyEx.Register(nameof(Icon));
		public static readonly DependencyProperty MainMenuProperty = DependencyPropertyEx.Register(nameof(MainMenu));
		public static readonly DependencyProperty ToolBarProperty = DependencyPropertyEx.Register(nameof(ToolBar));
		public ImageSource Icon
		{
			get => this.GetValue<ImageSource>(IconProperty);
			set => SetValue(IconProperty, value);
		}
		public Menu MainMenu
		{
			get => this.GetValue<Menu>(MainMenuProperty);
			set => SetValue(MainMenuProperty, value);
		}
		public ToolBarTray ToolBar
		{
			get => this.GetValue<ToolBarTray>(ToolBarProperty);
			set => SetValue(ToolBarProperty, value);
		}
		public Window Owner => this.FindParent<Window>(UITreeType.Logical);

		public WindowTitleBar()
		{
			InitializeComponent();
		}

		private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				if (e.ClickCount == 2)
				{
					Owner.WindowState = Owner.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
				}
				else if (e.ButtonState == MouseButtonState.Pressed)
				{
					Owner.DragMove();
				}
			}
		}
		private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				Owner.Close();
			}
			else
			{
				Point offset = new Point(1, 30);
				if (Owner.WindowState == WindowState.Maximized) offset.Offset(6, 6);

				SystemCommands.ShowSystemMenu(Owner, Owner.PointToScreen(offset));
			}
		}
		private void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			Owner.WindowState = WindowState.Minimized;
		}
		private void MaximizeButton_Click(object sender, RoutedEventArgs e)
		{
			Owner.WindowState = WindowState.Maximized;
		}
		private void RestoreButton_Click(object sender, RoutedEventArgs e)
		{
			Owner.WindowState = WindowState.Normal;
		}
		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Owner.Close();
		}
	}
}