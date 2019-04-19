using BytecodeApi;
using BytecodeApi.Extensions;
using BytecodeApi.UI;
using BytecodeApi.UI.Controls;
using BytecodeApi.UI.Dialogs;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace PEunion
{
	public partial class FileBrowser : ObservableUserControl
	{
		public static readonly DependencyProperty AllowMultipleProperty = DependencyPropertyEx.Register(nameof(AllowMultiple), new PropertyMetadata(AllowMultipleProperty_Changed));
		public static readonly DependencyProperty AllowedExtensionsProperty = DependencyPropertyEx.Register(nameof(AllowedExtensions));
		public static readonly DependencyProperty AllowResetProperty = DependencyPropertyEx.Register(nameof(AllowReset));
		public static readonly DependencyProperty TextProperty = DependencyPropertyEx.Register(nameof(Text));
		public static readonly DependencyProperty IconImageSourceProperty = DependencyPropertyEx.Register(nameof(IconImageSource), new PropertyMetadata(Utility.GetImageResource("ImageDragDrop"), IconImageSourceProperty_Changed));
		public static readonly DependencyProperty IsResetButtonEnabledProperty = DependencyPropertyEx.Register(nameof(IsResetButtonEnabled));
		public bool AllowMultiple
		{
			get => GetValue(() => AllowMultiple);
			set => SetValue(() => AllowMultiple, value);
		}
		public string AllowedExtensions
		{
			get => GetValue(() => AllowedExtensions);
			set => SetValue(() => AllowedExtensions, value);
		}
		public bool AllowReset
		{
			get => GetValue(() => AllowReset);
			set => SetValue(() => AllowReset, value);
		}
		public string Text
		{
			get => GetValue(() => Text);
			set => SetValue(() => Text, value);
		}
		public ImageSource IconImageSource
		{
			get => GetValue(() => IconImageSource);
			set => SetValue(() => IconImageSource, value);
		}
		public bool IsResetButtonEnabled
		{
			get => GetValue(() => IsResetButtonEnabled);
			set => SetValue(() => IsResetButtonEnabled, value);
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
			OnFilesSelect(AllowMultiple ? FileDialogs.OpenMultiple(AllowedExtensionsArray) : Singleton.Array(FileDialogs.Open(AllowedExtensionsArray)));
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
				(AllowedExtensionsArray == null || files.All(file => AllowedExtensionsArray.Any(ext => Path.GetExtension(file).TrimStart('.').CompareTo(ext, SpecialStringComparisons.IgnoreCase) == 0)));
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