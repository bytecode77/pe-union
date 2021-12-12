using BytecodeApi.Extensions;
using BytecodeApi.IO.FileSystem;
using BytecodeApi.UI;
using BytecodeApi.UI.Dialogs;
using BytecodeApi.UI.Extensions;
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
		private string _DisplayPath;
		private ImageSource _DisplayIcon;
		public string Path
		{
			get => this.GetValue<string>(PathProperty);
			set => SetValue(PathProperty, value);
		}
		public string BaseDirectory
		{
			get => this.GetValue<string>(BaseDirectoryProperty);
			set => SetValue(BaseDirectoryProperty, value);
		}
		public string BaseDirectoryDisplayName
		{
			get => this.GetValue<string>(BaseDirectoryDisplayNameProperty);
			set => SetValue(BaseDirectoryDisplayNameProperty, value);
		}
		public string Extensions
		{
			get => this.GetValue<string>(ExtensionsProperty);
			set => SetValue(ExtensionsProperty, value);
		}
		public string ExtensionsDescription
		{
			get => this.GetValue<string>(ExtensionsDescriptionProperty);
			set => SetValue(ExtensionsDescriptionProperty, value);
		}
		public string DisplayPath
		{
			get => _DisplayPath;
			private set => Set(ref _DisplayPath, value);
		}
		public ImageSource DisplayIcon
		{
			get => _DisplayIcon;
			private set => Set(ref _DisplayIcon, value);
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