using BytecodeApi;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace PEunion
{
	public partial class TabRtlo : ObservableUserControl
	{
		private const char RtloCharacter = '\u202e';

		public string SourceFile
		{
			get => Get(() => SourceFile);
			set
			{
				Set(() => SourceFile, value);
				ctrlBrowseSourceFile.Text = Path.GetFileName(value);
				ctrlBrowseSourceFile.IconImageSource = FileEx.GetIcon(value, true).ToBitmapSource();
				DestinationFileName = Path.GetFileNameWithoutExtension(value);
				DestinationOldExtension = DestinationNewExtension = Path.GetExtension(value).TrimStart('.');
			}
		}
		public string DestinationFileName
		{
			get => Get(() => DestinationFileName);
			set
			{
				Set(() => DestinationFileName, value);
				UpdatePreview();
			}
		}
		public string DestinationOldExtension
		{
			get => Get(() => DestinationOldExtension);
			set
			{
				if (value != DestinationOldExtension)
				{
					Set(() => DestinationOldExtension, value);
					RaisePropertyChanged(() => DestinationOldExtensionAlternatives);
					Set(() => DestinationOldExtension, value);
					UpdatePreview();
				}
			}
		}
		public string DestinationNewExtension
		{
			get => Get(() => DestinationNewExtension);
			set
			{
				Set(() => DestinationNewExtension, value);
				UpdatePreview();
			}
		}
		public string DestinationIcon
		{
			get => Get(() => DestinationIcon);
			set
			{
				Set(() => DestinationIcon, value);
				ctrlDestinationIcon.Text = Path.GetFileName(value);
				ctrlDestinationIcon.IconImageSource = File.Exists(value) ? FileEx.GetIcon(value, true).ToBitmapSource() : null;
				ctrlDestinationIcon.IsResetButtonEnabled = value != null;
				UpdatePreview();
			}
		}

		public string[] DestinationOldExtensionAlternatives
		{
			get
			{
				if (DestinationOldExtension == null)
				{
					return null;
				}
				else
				{
					string[][] alternatives = new[]
					{
						new[] { "exe", "scr" },
						new[] { "jpg", "jpeg" },
						new[] { "mid", "midi" }
					};

					return alternatives.FirstOrDefault(alt => alt.Contains(DestinationOldExtension.ToLower())) ?? DestinationOldExtension.CreateSingletonArray();
				}
			}
		}

		public TabRtlo()
		{
			InitializeComponent();
			DataContext = this;
		}

		private void ctrlBrowseSourceFile_FilesSelect(object sender, string[] e)
		{
			if (e.First().Contains(RtloCharacter))
			{
				MessageBoxes.Warning("This file already contains a RTLO character (U+202e).");
			}
			else if (Path.GetExtension(e.First()) == "")
			{
				MessageBoxes.Warning("The source file must have an extension.");
			}
			else
			{
				SourceFile = e.First();
			}
		}
		private void ctrlDestinationIcon_FilesSelect(object sender, string[] e)
		{
			DestinationIcon = e.First();
		}
		private void ctrlDestinationIcon_Reset(object sender, EventArgs e)
		{
			DestinationIcon = null;
		}
		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			if (SourceFile == null)
			{
				MessageBoxes.Warning("Select the source file.");
			}
			else if (!File.Exists(SourceFile))
			{
				MessageBoxes.Warning("'" + SourceFile + "' not found.");
			}
			else if (DestinationFileName.Contains(RtloCharacter))
			{
				MessageBoxes.Warning("There is a RTLO character (U+202e) in the destination file name. It must be removed first.");
			}
			else if (DestinationNewExtension.Contains(RtloCharacter))
			{
				MessageBoxes.Warning("There is a RTLO character (U+202e) in the destination file extension. It must be removed first.");
			}
			else
			{
				string path = Dialogs.OpenFolder();
				if (path != null)
				{
					try
					{
						string fileName = Path.Combine(path, DestinationFileName + RtloCharacter + DestinationNewExtension.Reverse() + "." + DestinationOldExtension);

						if (!File.Exists(fileName) || MessageBoxes.Confirmation("A files named '' already exists in the selected directory.\r\nOverwrite?", true))
						{
							if (DestinationIcon == null)
							{
								File.Copy(SourceFile, fileName, true);
							}
							else
							{
								string tempPath = Path.Combine(path, DestinationFileName + "~.tmp");
								File.Copy(SourceFile, tempPath, true);
								new FileInfo(tempPath).ChangeExecutableIcon(DestinationIcon);
								File.Copy(tempPath, fileName, true);
								File.Delete(tempPath);
							}
						}
					}
					catch (Exception ex)
					{
						MessageBoxes.Error(ex.GetType() + ": " + ex.Message);
					}
				}
			}
		}
		private void lnkCopyCharacterToClipboard_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetDataObject(new string(RtloCharacter, 1));
		}

		private void UpdatePreview()
		{
			if (File.Exists(DestinationIcon)) ctrlPreview.ImageSource = FileEx.GetIcon(DestinationIcon, false).ToBitmapSource();
			else ctrlPreview.ImageSource = File.Exists(SourceFile) ? FileEx.GetIcon(SourceFile, false).ToBitmapSource() : null;

			ctrlPreview.Text = DestinationFileName.IsNullOrEmpty() || DestinationNewExtension.IsNullOrEmpty() ? null : DestinationFileName + DestinationOldExtension?.Reverse() + "." + DestinationNewExtension;
		}
	}
}