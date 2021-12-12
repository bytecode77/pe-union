using BytecodeApi;
using BytecodeApi.Extensions;
using BytecodeApi.FileFormats.Ini;
using BytecodeApi.FileFormats.ResourceFile;
using BytecodeApi.IO.FileSystem;
using BytecodeApi.Text;
using BytecodeApi.UI.Dialogs;
using PEunion.Compiler.Helper;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace PEunion
{
	public sealed class RtloModel : TabModel
	{
		private readonly string[][] ExtensionAlternatives;

		private string _OriginalFilePath;
		private ImageSource _OriginalIcon;
		private ImageSource _DisplayIcon;
		private string _FileName;
		private string _Extension;
		private string _SpoofedExtension;
		private string[] _Extensions;
		private bool _ChangeIcon;
		private string _IconPath;
		public string OriginalFilePath
		{
			get => _OriginalFilePath;
			set
			{
				Set(ref _OriginalFilePath, value);

				if (OriginalFilePath == null)
				{
					OriginalIcon = null;
					FileName = null;
					Extension = null;
					SpoofedExtension = null;
					Extensions = null;
				}
				else if (!Path.HasExtension(OriginalFilePath))
				{
					MessageBoxes.Warning("The selected file must have an extension.");
					OriginalFilePath = null;
				}
				else if (Path.GetFileName(OriginalFilePath).Contains(TextResources.RightToLeftMark))
				{
					MessageBoxes.Warning("The selected file already contains a right-to-left override character (U+202E).");
					OriginalFilePath = null;
				}
				else
				{
					OriginalIcon = FileEx.GetIcon(OriginalFilePath, false).ToBitmapSource();

					string extension = Path.GetExtension(OriginalFilePath).TrimStart('.');
					FileName = Path.GetFileNameWithoutExtension(OriginalFilePath);

					if (ExtensionAlternatives.FirstOrDefault(alt => alt.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase))) is string[] alternative)
					{
						Extensions = alternative;
					}
					else
					{
						Extensions = new[] { extension };
					}

					Extension = extension;
					SpoofedExtension = extension;
				}

				RaisePropertyChanged(nameof(CanChangeIcon));
			}
		}
		public ImageSource OriginalIcon
		{
			get => _OriginalIcon;
			set
			{
				Set(ref _OriginalIcon, value);
				UpdateDisplayIcon();
			}
		}
		public ImageSource DisplayIcon
		{
			get => _DisplayIcon;
			set => Set(ref _DisplayIcon, value);
		}
		public string FileName
		{
			get => _FileName;
			set
			{
				Set(ref _FileName, value);
				RaisePropertyChanged(nameof(IsValid));
			}
		}
		public string Extension
		{
			get => _Extension;
			set
			{
				Set(ref _Extension, value);
				RaisePropertyChanged(nameof(IsValid));
			}
		}
		public string SpoofedExtension
		{
			get => _SpoofedExtension;
			set
			{
				Set(ref _SpoofedExtension, value);
				RaisePropertyChanged(nameof(IsValid));
			}
		}
		public string[] Extensions
		{
			get => _Extensions;
			set => Set(ref _Extensions, value);
		}
		public bool ChangeIcon
		{
			get => _ChangeIcon;
			set
			{
				Set(ref _ChangeIcon, value);
				UpdateDisplayIcon();
			}
		}
		public string IconPath
		{
			get => _IconPath;
			set
			{
				Set(ref _IconPath, value);
				UpdateDisplayIcon();
			}
		}
		public bool IsValid => !FileName.IsNullOrEmpty() && !Extension.IsNullOrEmpty() && !SpoofedExtension.IsNullOrEmpty();
		public bool CanChangeIcon => OriginalFilePath.IsNullOrEmpty() || new[] { "exe", "scr" }.Any(ext => Path.GetExtension(OriginalFilePath).TrimStart('.').Equals(ext, StringComparison.OrdinalIgnoreCase));

		public RtloModel(string tabTitle)
		{
			TabTitle = tabTitle;
			TabIcon = App.GetIcon("Rtlo16");

			ExtensionAlternatives = GetExtensionAlternatives();
		}

		public void Save()
		{
			string icon = CanChangeIcon && ChangeIcon && IconPath != null ? IconPath : null;

			if (!File.Exists(OriginalFilePath))
			{
				MessageBoxes.Error("File '" + Path.GetFileName(OriginalFilePath) + "' not found.");
			}
			else if (FileName.Contains(TextResources.RightToLeftMark))
			{
				MessageBoxes.Warning("The new filename must not contain the right-to-left override character (U+202E).");
			}
			else if (SpoofedExtension.Contains(TextResources.RightToLeftMark))
			{
				MessageBoxes.Warning("The spoofed extension must not contain the right-to-left override character (U+202E).");
			}
			else if (icon != null && !File.Exists(icon))
			{
				MessageBoxes.Error("File '" + Path.GetFileName(icon) + "' not found.");
			}
			else if (icon != null && !IconExtractor.HasIcon(icon))
			{
				MessageBoxes.Error("File '" + Path.GetFileName(icon) + "' is not a valid icon.");
			}
			else
			{
				if (FileDialogs.OpenFolder() is string path)
				{
					string newFileName = Path.Combine(path, FileName + TextResources.RightToLeftMark + SpoofedExtension.Reverse() + "." + Extension);
					if (!File.Exists(newFileName) || MessageBoxes.Confirmation("A file named '" + FileName + Extension.Reverse() + "." + SpoofedExtension + "' already exists in the selected directory.\r\nOverwrite?", true))
					{
						if (icon == null)
						{
							File.Copy(OriginalFilePath, newFileName, true);
						}
						else
						{
							string tempPath = Path.Combine(path, FileName + ".~tmp");
							File.Copy(OriginalFilePath, tempPath, true);

							try
							{
								new ResourceFileInfo(tempPath).ChangeIcon(icon);
								File.Copy(tempPath, newFileName, true);
							}
							finally
							{
								File.Delete(tempPath);
							}
						}
					}
				}
			}
		}
		private string[][] GetExtensionAlternatives()
		{
			string path = Path.Combine(ApplicationBase.Path, @"Config\rtlo_extension_alternatives.ini");

			try
			{
				if (File.Exists(path))
				{
					IniFile ini = IniFile.FromFile(path, Encoding.UTF8, new IniFileParsingOptions
					{
						AllowGlobalProperties = false,
						AllowSectionNameClosingBracket = false,
						DuplicateSectionNameBehavior = IniDuplicateSectionNameBehavior.Duplicate
					});

					return ini.Sections
						.Where(section => section.Name.Equals("alternative", StringComparison.OrdinalIgnoreCase))
						.Select(section => section.Properties.Select(property => property.Value).ToArray())
						.ToArray();
				}
				else
				{
					MessageBoxes.Error("File '" + Path.GetFileName(path) + "' not found.");
					return new string[0][];
				}
			}
			catch
			{
				MessageBoxes.Error("Error reading file '" + Path.GetFileName(path) + "'.");
				return new string[0][];
			}
		}
		private void UpdateDisplayIcon()
		{
			if (CanChangeIcon && ChangeIcon && IconPath != null)
			{
				DisplayIcon = FileEx.GetIcon(IconPath, false).ToBitmapSource();
			}
			else
			{
				DisplayIcon = OriginalIcon;
			}
		}
	}
}