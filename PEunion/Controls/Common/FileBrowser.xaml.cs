using BytecodeApi;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace PEunion
{
	public partial class FileBrowser : ObservableUserControl
	{
		public static readonly DependencyProperty AllowMultipleProperty = DependencyProperty.Register(nameof(AllowMultiple), typeof(bool), typeof(FileBrowser), new PropertyMetadata(AllowMultipleProperty_Changed));
		public static readonly DependencyProperty AllowedExtensionsProperty = DependencyProperty.Register(nameof(AllowedExtensions), typeof(string), typeof(FileBrowser));
		public static readonly DependencyProperty AllowResetProperty = DependencyProperty.Register(nameof(AllowReset), typeof(bool), typeof(FileBrowser));
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(FileBrowser));
		public static readonly DependencyProperty IconImageSourceProperty = DependencyProperty.Register(nameof(IconImageSource), typeof(ImageSource), typeof(FileBrowser), new PropertyMetadata(Utility.GetImageResource("ImageDragDrop"), IconImageSourceProperty_Changed));
		public static readonly DependencyProperty IsResetButtonEnabledProperty = DependencyProperty.Register(nameof(IsResetButtonEnabled), typeof(bool), typeof(FileBrowser));
		public bool AllowMultiple
		{
			get => GetValue<bool>(AllowMultipleProperty);
			set => SetValue(AllowMultipleProperty, value);
		}
		public string AllowedExtensions
		{
			get => GetValue<string>(AllowedExtensionsProperty);
			set => SetValue(AllowedExtensionsProperty, value);
		}
		public bool AllowReset
		{
			get => GetValue<bool>(AllowResetProperty);
			set => SetValue(AllowResetProperty, value);
		}
		public string Text
		{
			get => GetValue<string>(TextProperty);
			set => SetValue(TextProperty, value);
		}
		public ImageSource IconImageSource
		{
			get => GetValue<ImageSource>(IconImageSourceProperty);
			set => SetValue(IconImageSourceProperty, value);
		}
		public bool IsResetButtonEnabled
		{
			get => GetValue<bool>(IsResetButtonEnabledProperty);
			set => SetValue(IsResetButtonEnabledProperty, value);
		}
		private string[] AllowedExtensionsArray => AllowedExtensions.ToNullIfEmpty()?.Split(';');
		public string DragDropText => AllowMultiple ? "Drag one or multiple files here" : "Drag a file here";
		public event EventHandler<string[]> FilesSelect;
		public event EventHandler Reset;

		public FileBrowser()
		{
			InitializeComponent();
		}

		private void UserControl_PreviewDragOver(object sender, DragEventArgs e)
		{
			e.Effects = CheckFiles(e.Data.GetData(DataFormats.FileDrop) as string[]) ? DragDropEffects.Copy : DragDropEffects.None;
			e.Handled = true;
		}
		private void UserControl_Drop(object sender, DragEventArgs e)
		{
			OnFilesSelect(e.Data.GetData(DataFormats.FileDrop) as string[]);
		}
		private void btnBrowse_Click(object sender, RoutedEventArgs e)
		{
			OnFilesSelect(AllowMultiple ? Dialogs.OpenMultiple(AllowedExtensionsArray) : Singleton.Array(Dialogs.Open(AllowedExtensionsArray)));
		}
		private void btnReset_Click(object sender, RoutedEventArgs e)
		{
			OnReset();
		}

		private bool CheckFiles(string[] files)
		{
			return
				files?.Length > 0 &&
				files.All(File.Exists) &&
				(AllowMultiple || files.Length == 1) &&
				(AllowedExtensionsArray == null || files.All(file => AllowedExtensionsArray.Any(ext => Path.GetExtension(file).TrimStart('.').CompareTo(ext, SpecialStringComparison.IgnoreCase) == 0)));
		}
		private static void AllowMultipleProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FileBrowser control = d as FileBrowser;
			control.RaisePropertyChanged(() => control.DragDropText);
		}
		private static void IconImageSourceProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue == null)
			{
				(d as FileBrowser).ClearValue(IconImageSourceProperty);
			}
		}
		protected void OnFilesSelect(string[] files)
		{
			if (CheckFiles(files)) FilesSelect?.Invoke(this, files);
		}
		protected void OnReset()
		{
			Reset?.Invoke(this, EventArgs.Empty);
		}
	}
}