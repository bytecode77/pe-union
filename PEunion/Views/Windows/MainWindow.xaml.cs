using BytecodeApi.UI.Dialogs;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace PEunion
{
	public partial class MainWindow
	{
		public static MainWindow Singleton { get; private set; }
		public MainWindowViewModel ViewModel { get; set; }

		private static readonly int DefaultWindowWidth = (int)Math.Min(SystemParameters.PrimaryScreenWidth * .75, SystemParameters.PrimaryScreenHeight);
		private static readonly int DefaultWindowHeight = (int)(SystemParameters.PrimaryScreenHeight * .75);
		private int _BottomPanelSelectedIndex;
		private GridLength _Splitter1Position;
		private GridLength _Splitter2Position;
		public int BottomPanelSelectedIndex
		{
			get => _BottomPanelSelectedIndex;
			set => Set(ref _BottomPanelSelectedIndex, value);
		}
		public GridLength Splitter1Position
		{
			get => _Splitter1Position;
			set
			{
				Set(ref _Splitter1Position, value);
				Config.ViewState.WindowSplitter1 = (int)Splitter1Position.Value;
			}
		}
		public GridLength Splitter2Position
		{
			get => _Splitter2Position;
			set
			{
				Set(ref _Splitter2Position, value);
				Config.ViewState.WindowSplitter2 = (int)Splitter2Position.Value;
			}
		}

		public MainWindow()
		{
			ViewModel = new MainWindowViewModel(this);
			InitializeComponent();
			MessageBoxes.Window = this;
			ViewModel.IsInitialized = true;

			if (Config.ViewState.WindowX is int x && Config.ViewState.WindowY is int y)
			{
				Left = x;
				Top = y;
			}
			else
			{
				WindowStartupLocation = WindowStartupLocation.CenterScreen;
			}

			if (Config.ViewState.WindowWidth is int width && Config.ViewState.WindowHeight is int height)
			{
				Width = width;
				Height = height;
			}
			else
			{
				Width = DefaultWindowWidth;
				Height = DefaultWindowHeight;
			}

			if (Config.ViewState.WindowMaximized) WindowState = WindowState.Maximized;
			Splitter1Position = new GridLength(Config.ViewState.WindowSplitter1 is int splitter1 ? splitter1 : 250);
			Splitter2Position = new GridLength(Config.ViewState.WindowSplitter2 is int splitter2 ? splitter2 : DefaultWindowHeight / 4);

			Singleton = this;
		}
		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			App.SingleInstance.RegisterWindow(this);
			App.SingleInstance.Activated += SingleInstance_Activated;
		}
		private void MainWindow_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = !ViewModel.SaveChangesBeforeExit();
		}
		private void MainWindow_ContentRendered(object sender, EventArgs e)
		{
			ViewModel.ParseCommandLine(Environment.GetCommandLineArgs());
		}
		private void MainWindow_StateChanged(object sender, EventArgs e)
		{
			if (WindowState != WindowState.Minimized)
			{
				Config.ViewState.WindowMaximized = WindowState == WindowState.Maximized;
			}
		}
		private void MainWindow_LocationChanged(object sender, EventArgs e)
		{
			if (WindowState == WindowState.Normal && Left > -30000 && Top > -30000)
			{
				Config.ViewState.WindowX = (int)Left;
				Config.ViewState.WindowY = (int)Top;
			}
		}
		private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (WindowState == WindowState.Normal)
			{
				Config.ViewState.WindowWidth = (int)Width;
				Config.ViewState.WindowHeight = (int)Height;
			}
		}
		private void MainWindow_DragOver(object sender, DragEventArgs e)
		{
			e.Effects = IsDragDropValid(e.Data, out _) ? DragDropEffects.Copy : DragDropEffects.None;
			e.Handled = true;
		}
		private void MainWindow_Drop(object sender, DragEventArgs e)
		{
			if (IsDragDropValid(e.Data, out string[] paths))
			{
				foreach (string path in paths)
				{
					ViewModel.OpenProjectCommand.Execute(path);
				}
			}
		}

		private void SingleInstance_Activated(object sender, EventArgs e)
		{
			Show();
			if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
			Activate();
		}

		private bool IsDragDropValid(IDataObject data, out string[] paths)
		{
			if (data.GetData(DataFormats.FileDrop) is string[] files &&
				files.Length >= 1 &&
				files.All(file => Path.GetExtension(file).Equals(".peu", StringComparison.OrdinalIgnoreCase)))
			{
				paths = files;
				return true;
			}
			else
			{
				paths = null;
				return false;
			}
		}
	}
}