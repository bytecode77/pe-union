using BytecodeApi.Extensions;
using BytecodeApi.IO.FileSystem;
using BytecodeApi.UI;
using BytecodeApi.UI.Dialogs;
using PEunion.Compiler;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace PEunion
{
	public partial class FilePicker
	{
		public static readonly DependencyProperty PathProperty = DependencyPropertyEx.Register(nameof(Path), new PropertyMetadata(Property_Changed));
		public static readonly DependencyProperty BaseDirectoryProperty = DependencyPropertyEx.Register(nameof(BaseDirectory), new PropertyMetadata(Property_Changed));
		public static readonly DependencyProperty BaseDirectoryDisplayNameProperty = DependencyPropertyEx.Register(nameof(BaseDirectoryDisplayName), new PropertyMetadata(Property_Changed));
		public static readonly DependencyProperty ExtensionsProperty = DependencyPropertyEx.Register(nameof(Extensions));
		public static readonly DependencyProperty ExtensionsDescriptionProperty = DependencyPropertyEx.Register(nameof(ExtensionsDescription));
		public string Path
		{
			get => GetValue(() => Path);
			set => SetValue(() => Path, value);
		}
		public string BaseDirectory
		{
			get => GetValue(() => BaseDirectory);
			set => SetValue(() => BaseDirectory, value);
		}
		public string BaseDirectoryDisplayName
		{
			get => GetValue(() => BaseDirectoryDisplayName);
			set => SetValue(() => BaseDirectoryDisplayName, value);
		}
		public string Extensions
		{
			get => GetValue(() => Extensions);
			set => SetValue(() => Extensions, value);
		}
		public string ExtensionsDescription
		{
			get => GetValue(() => ExtensionsDescription);
			set => SetValue(() => ExtensionsDescription, value);
		}
		public string DisplayPath
		{
			get => Get(() => DisplayPath);
			private set => Set(() => DisplayPath, value);
		}
		public ImageSource DisplayIcon
		{
			get => Get(() => DisplayIcon);
			private set => Set(() => DisplayIcon, value);
		}

		public FilePicker()
		{
			InitializeComponent();
		}
		private void FilePicker_PreviewDragOver(object sender, DragEventArgs e)
		{
			if (IsDragDropValid(e.Data, out _))
			{
				e.Effects = DragDropEffects.Copy;
				e.Handled = true;
			}
			else
			{
				e.Effects = DragDropEffects.None;
			}
		}
		private void FilePicker_Drop(object sender, DragEventArgs e)
		{
			if (IsDragDropValid(e.Data, out string path)) Path = path;
		}

		private void TextBox_DragOver(object sender, DragEventArgs e)
		{
			e.Handled = true;
		}
		private void Browse_Click(object sender, RoutedEventArgs e)
		{
			if (FileDialogs.Open(Extensions.ToNullIfEmpty()?.Split(','), ExtensionsDescription.ToNullIfEmpty()) is string path) Path = path;
		}
		private void Clear_Click(object sender, RoutedEventArgs e)
		{
			Path = null;
		}

		private bool IsDragDropValid(IDataObject data, out string path)
		{
			if (data.GetData(DataFormats.FileDrop) is string[] files &&
				files.Length == 1 &&
				File.Exists(files.First()) &&
				(Extensions.IsNullOrEmpty() || Extensions.Split(',').Any(e => System.IO.Path.GetExtension(files.First()).TrimStart('.').Equals(e, StringComparison.OrdinalIgnoreCase))))
			{
				path = files.First();
				return true;
			}
			else
			{
				path = null;
				return false;
			}
		}
		private static void Property_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FilePicker control = (FilePicker)d;

			if (control.BaseDirectory.IsNullOrEmpty())
			{
				control.DisplayPath = control.Path;
			}
			else
			{
				string relativePath = RelativePath.AbsoluteToRelativePath(control.BaseDirectory, control.Path);
				control.DisplayPath = relativePath == control.Path ? control.Path : control.BaseDirectoryDisplayName + relativePath;
			}

			if (e.Property == PathProperty)
			{
				control.DisplayIcon = File.Exists(control.Path) ? FileEx.GetIcon(control.Path, false).ToBitmapSource() : null;
			}
		}
	}
}