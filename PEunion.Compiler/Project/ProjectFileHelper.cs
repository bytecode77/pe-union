using BytecodeApi;
using BytecodeApi.Extensions;
using BytecodeApi.FileFormats.Ini;
using PEunion.Compiler.Errors;
using PEunion.Compiler.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Helper class for the parsing of project files.
	/// </summary>
	internal sealed class ProjectFileHelper
	{
		private readonly string ProjectFilePath;
		private readonly IniFile Ini;
		private readonly ProjectFile Project;
		private readonly ErrorCollection Errors;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProjectFileHelper" /> class.
		/// </summary>
		/// <param name="projectFilePath">The path to the project file that is currently processed.</param>
		/// <param name="ini">The <see cref="IniFile" /> associated with this helper class instance.</param>
		/// <param name="project">The <see cref="ProjectFile" /> associated with this helper class instance.</param>
		/// <param name="errors">The <see cref="ErrorCollection" /> associated with this helper class instance.</param>
		public ProjectFileHelper(string projectFilePath, IniFile ini, ProjectFile project, ErrorCollection errors)
		{
			ProjectFilePath = projectFilePath;
			Ini = ini;
			Project = project;
			Errors = errors;
		}

		/// <summary>
		/// Reads the project from the INI file in the <see cref="Ini" /> property.
		/// </summary>
		public void ReadIni()
		{
			// Validate INI sections and properties
			ValidateSectionNames("stub", "startup", "versioninfo", "manifest", "source.embedded", "source.download", "action.runpe", "action.invoke", "action.drop", "action.messagebox");
			ValidateSectionCountOne(true, "stub");
			ValidateSectionCountOne(false, "startup", "versioninfo", "manifest");
			ValidateSectionProperties("stub", new[] { "type" }, new[] { "icon", "padding" });
			ValidateSectionProperties("startup", null, new[] { "melt" });
			ValidateSectionProperties("versioninfo", null, new[] { "file_description", "product_name", "file_version", "product_version", "copyright", "original_filename" });
			ValidateSectionProperties("manifest", null, new[] { "template", "path" });
			ValidateSectionProperties("source.embedded", new[] { "id", "path" }, new[] { "compress", "eof" });
			ValidateSectionProperties("source.download", new[] { "id", "url" }, null);
			ValidateSectionProperties("action.runpe", new[] { "source" }, null);
			ValidateSectionProperties("action.invoke", new[] { "source" }, null);
			ValidateSectionProperties("action.drop", new[] { "source", "location", "filename" }, new[] { "attribute_hidden", "attribute_system", "execute" });
			ValidateSectionProperties("action.messagebox", new[] { "title", "text" }, new[] { "icon", "buttons", "on_ok", "on_cancel", "on_yes", "on_no", "on_abort", "on_retry", "on_ignore" });
			ValidateEmptyProperties();

			// Read sections
			foreach (IniSection section in Ini.Sections)
			{
				switch (section.Name)
				{
					case "stub":
						{
							Project.Stub.Type = ConvertStubType(section.Properties["type"].Value);
							Project.Stub.IconPath = ConvertRelativeToAbsolutePath(section.Properties["icon", null].Value);
							Project.Stub.Padding = GetInt32Value(section.Properties["padding", "0"], 0, 1000);
						}
						break;
					case "startup":
						{
							Project.Startup.Melt = GetBooleanValue(section.Properties["melt", "false"]);
						}
						break;
					case "versioninfo":
						{
							Project.VersionInfo.FileDescription = GetStringValue(section.Properties["file_description", null]);
							Project.VersionInfo.ProductName = GetStringValue(section.Properties["product_name", null]);
							Project.VersionInfo.FileVersion = GetStringValue(section.Properties["file_version", null]);
							Project.VersionInfo.ProductVersion = GetStringValue(section.Properties["product_version", null]);
							Project.VersionInfo.Copyright = GetStringValue(section.Properties["copyright", null]);
							Project.VersionInfo.OriginalFilename = GetStringValue(section.Properties["original_filename", null]);
						}
						break;
					case "manifest":
						{
							Project.Manifest.Template = ConvertManifestTemplate(section.Properties["template", null].Value);
							Project.Manifest.Path = ConvertRelativeToAbsolutePath(section.Properties["path", null].Value);
						}
						break;
					case "source.embedded":
						{
							Project.AddSource(new EmbeddedSource
							{
								Id = section.Properties["id"].Value,
								Path = ConvertRelativeToAbsolutePath(section.Properties["path"].Value),
								Compress = GetBooleanValue(section.Properties["compress", "false"]),
								EofData = GetBooleanValue(section.Properties["eof", "false"])
							});
						}
						break;
					case "source.download":
						{
							Project.AddSource(new DownloadSource
							{
								Id = section.Properties["id"].Value,
								Url = GetStringValue(section.Properties["url"])
							});
						}
						break;
					case "action.runpe":
						{
							Project.AddAction(new RunPEAction
							{
								Source = GetSource(section.Properties["source"].Value)
							});
						}
						break;
					case "action.invoke":
						{
							Project.AddAction(new InvokeAction
							{
								Source = GetSource(section.Properties["source"].Value)
							});
						}
						break;
					case "action.drop":
						{
							Project.AddAction(new DropAction
							{
								Source = GetSource(section.Properties["source"].Value),
								Location = ConvertDropLocation(section.Properties["location"].Value),
								FileName = GetStringValue(section.Properties["filename"]),
								FileAttributeHidden = GetBooleanValue(section.Properties["attribute_hidden", "false"]),
								FileAttributeSystem = GetBooleanValue(section.Properties["attribute_system", "false"]),
								ExecuteVerb = ConvertExecuteVerb(section.Properties["execute", "none"].Value)
							});
						}
						break;
					case "action.messagebox":
						{
							Project.AddAction(new MessageBoxAction
							{
								Title = GetStringValue(section.Properties["title", null]),
								Text = GetStringValue(section.Properties["text", null]),
								Icon = ConvertMessageBoxIcon(section.Properties["icon", "none"].Value),
								Buttons = ConvertMessageBoxButtons(section.Properties["buttons", "ok"].Value),
								OnOk = ConvertActionEvent(section.Properties["on_ok", "none"].Value),
								OnCancel = ConvertActionEvent(section.Properties["on_cancel", "none"].Value),
								OnYes = ConvertActionEvent(section.Properties["on_yes", "none"].Value),
								OnNo = ConvertActionEvent(section.Properties["on_no", "none"].Value),
								OnAbort = ConvertActionEvent(section.Properties["on_abort", "none"].Value),
								OnRetry = ConvertActionEvent(section.Properties["on_retry", "none"].Value),
								OnIgnore = ConvertActionEvent(section.Properties["on_ignore", "none"].Value)
							});
						}
						break;
				}
			}
		}
		/// <summary>
		/// Validates the project file after it has been loaded from an INI file.
		/// </summary>
		public void ValidateProject()
		{
			// Validate stub
			ValidateStubIcon();
			ValidateStubPadding();
			ValidateManifest();

			// Validate sources
			ValidateSources();
			ValidateCompression();
			ValidateEofData();

			// Validate actions
			ValidateActions();
			ValidateRunPE();
			ValidateInvoke();
			ValidateDrop();
			ValidateMessageBox();

			void ValidateStubIcon()
			{
				// Validate stub icon
				if (Project.Stub.IconPath != null)
				{
					// Validate that stub icon has .ico or .exe extension
					string extension = Path.GetExtension(Project.Stub.IconPath);

					if (extension.Equals(".ico", StringComparison.OrdinalIgnoreCase))
					{
						// Validate that stub icon is a valid .ico file
						if (!IconExtractor.HasIcon(Project.Stub.IconPath))
						{
							Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "File '" + Path.GetFileName(Project.Stub.IconPath) + "' is not a valid icon.");
						}
					}
					else if (extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
					{
						// Validate that stub icon is an executable file with an icon
						if (!IconExtractor.HasIcon(Project.Stub.IconPath))
						{
							Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Could not extract icon from '" + Path.GetFileName(Project.Stub.IconPath) + "'.");
						}
					}
					else
					{
						Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Stub icon must have '.ico' or '.exe' extension.");
					}
				}
			}
			void ValidateStubPadding()
			{
				// Validate that stub has padding
				if (Project.Stub.Padding < 50)
				{
					Errors.Add(ErrorSource.Project, ErrorSeverity.Warning, "Padding is less than 50. The compiled binary will be of high-entropy and may be detected as a packer.");
				}
			}
			void ValidateManifest()
			{
				// Validate that manifest is not set to both template and cutom file
				if (Project.Manifest.Template != null && Project.Manifest.Path != null)
				{
					Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Both manifest template and path specified. Please specify only one.");
				}
			}
			void ValidateSources()
			{
				// Validate duplicate source ID's
				for (int i = 0; i < Project.Sources.Count; i++)
				{
					if (Project.Sources.Skip(i + 1).Any(source => source.Id == Project.Sources[i].Id))
					{
						Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Duplicate source ID '" + Project.Sources[i].Id + "'.");
					}
				}

				// Exclude unused sources
				foreach (ProjectSource source in Project.Sources.ToArray())
				{
					if (Project.Actions.None(action => action.Source == source))
					{
						Project.RemoveSource(source);
						Errors.Add(ErrorSource.Project, ErrorSeverity.Warning, "Source '" + source.Id + "' was excluded, because it is not used.");
					}
				}
			}
			void ValidateCompression()
			{
				// Validate that compressed files are smaller than 100 MB
				foreach (EmbeddedSource source in Project.Sources.OfType<EmbeddedSource>())
				{
					if (source.Compress && source.Path != null && new FileInfo(source.Path).Length > 1024 * 1024 * 100)
					{
						Errors.Add(ErrorSource.Project, ErrorSeverity.Warning, "File '" + Path.GetFileName(source.Path) + "' is bigger than 100 MB. It is recommended to disable compression.");
					}
				}
			}
			void ValidateEofData()
			{
				IEnumerable<EmbeddedSource> eofSources = Project.Sources.OfType<EmbeddedSource>().Where(source => source.EofData);

				// Validate that only PE files are used as EOF data sources
				foreach (EmbeddedSource source in eofSources)
				{
					if (new[] { ".exe", ".dll" }.None(extension => Path.GetExtension(source.Path).Equals(extension, StringComparison.OrdinalIgnoreCase)))
					{
						Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Cannot write EOF data of file '" + Path.GetFileName(source.Path) + "'. Only executable files contain EOF data.");
					}
				}

				// Validate that only one file's EOF data is written
				foreach (EmbeddedSource source in eofSources.Skip(1))
				{
					Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Cannot write EOF data of file '" + Path.GetFileName(source.Path) + "'. Only one file's EOF data can be written.");
				}
			}
			void ValidateActions()
			{
				// Validate that at least one action exists
				if (Project.Actions.None())
				{
					Errors.Add(ErrorSource.Project, ErrorSeverity.Warning, "The project is empty.");
				}
			}
			void ValidateRunPE()
			{
				// Validate that RunPE files have an .exe extension
				foreach (RunPEAction action in Project.Actions.OfType<RunPEAction>())
				{
					if (action.Source is EmbeddedSource embeddedSource &&
						embeddedSource.Path != null &&
						!Path.GetExtension(embeddedSource.Path).Equals(".exe", StringComparison.OrdinalIgnoreCase))
					{
						Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "RunPE file must have '.exe' extension.");
					}
				}
			}
			void ValidateInvoke()
			{
				// Validate that invoke files have an .exe extension
				foreach (InvokeAction action in Project.Actions.OfType<InvokeAction>())
				{
					if (action.Source is EmbeddedSource embeddedSource &&
						embeddedSource.Path != null &&
						!Path.GetExtension(embeddedSource.Path).Equals(".exe", StringComparison.OrdinalIgnoreCase))
					{
						Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Invoked file must have '.exe' extension.");
					}
				}

				// Validate that invoke actions are only used in a .NET stub
				if (CSharp.EqualsNone(Project.Stub.Type, StubType.DotNet32, StubType.DotNet64) && Project.Actions.OfType<InvokeAction>().Any())
				{
					Errors.Add(ErrorSource.Project, ErrorSeverity.Error, ".NET invocation is only supported in a .NET stub.");
				}
			}
			void ValidateDrop()
			{
				// Validate that a UAC manifest is included, if files are dropped in privileged directories
				IEnumerable<string> privilegedDropFileNames = Project.Actions
					.OfType<DropAction>()
					.Where(action => CSharp.EqualsAny(action.Location, DropLocation.WindowsDirectory, DropLocation.SystemDirectory, DropLocation.ProgramFiles, DropLocation.CDrive))
					.Select(action =>
					{
						if (action.FileName != null) return action.FileName;
						else if (action.Source is EmbeddedSource embeddedSource && embeddedSource.Path != null) return Path.GetFileName(embeddedSource.Path);
						else return null;
					});

				foreach (string privilegedDropFileName in privilegedDropFileNames)
				{
					if (Project.Manifest.Path != null)
					{
						// Add an error with low severity for custom manifests, because "requireAdministrator" cannot be determined
						Errors.Add(ErrorSource.Project, ErrorSeverity.Message, "File '" + privilegedDropFileName + "' is dropped in a privileged directory. Make sure that your manifest's execution level is 'requireAdministrator'.");
					}
					else if (CSharp.EqualsAny(Project.Manifest.Template, null, ManifestTemplate.Default))
					{
						// Add an error for the default manifest template or if no manifest is included, because in this case, "requireAdministrator" is missing
						Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "File '" + privilegedDropFileName + "' is dropped in a privileged directory. A UAC manifest must be included.");
					}
				}
			}
			void ValidateMessageBox()
			{
				// Validate unused MessageBox events
				foreach (MessageBoxAction action in Project.Actions.OfType<MessageBoxAction>())
				{
					Validate(action.OnOk, MessageBoxEvent.Ok, "on_ok");
					Validate(action.OnCancel, MessageBoxEvent.Cancel, "on_cancel");
					Validate(action.OnYes, MessageBoxEvent.Yes, "on_yes");
					Validate(action.OnNo, MessageBoxEvent.No, "on_no");
					Validate(action.OnAbort, MessageBoxEvent.Abort, "on_abort");
					Validate(action.OnRetry, MessageBoxEvent.Retry, "on_retry");
					Validate(action.OnIgnore, MessageBoxEvent.Ignore, "on_ignore");

					void Validate(ActionEvent actionEvent, MessageBoxEvent e, string eventName)
					{
						if (actionEvent != ActionEvent.None && !MessageBoxAction.HasEvent(action.Buttons, e))
						{
							Errors.Add(ErrorSource.Project, ErrorSeverity.Warning, "MessageBox event '" + eventName + "' will never be raised.");
						}
					}
				}
			}
		}

		/// <summary>
		/// Validates that all sections match a specified set of names.
		/// </summary>
		/// <param name="sectionNames">An array of valid section names.</param>
		public void ValidateSectionNames(params string[] sectionNames)
		{
			foreach (IniSection section in Ini.Sections)
			{
				if (!sectionNames.Contains(section.Name))
				{
					Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Invalid section '" + section.Name + "'.");
				}
			}
		}
		/// <summary>
		/// Validates the sections with the specified names are present only once.
		/// </summary>
		/// <param name="required"><see langword="true" /> to require one occurrence of the section name; <see langword="false" /> to require zero or one ocurrence of the section name.</param>
		/// <param name="sectionNames">An array of section names to check.</param>
		public void ValidateSectionCountOne(bool required, params string[] sectionNames)
		{
			foreach (string sectionName in sectionNames)
			{
				int count = Ini.Sections.Count(section => section.Name == sectionName);

				if (required)
				{
					if (count == 0) Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Section '" + sectionName + "' is required.");
					else if (count > 1) Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Section '" + sectionName + "' may only be declared once.");
				}
				else
				{
					if (count > 1) Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Section '" + sectionName + "' may only be declared once.");
				}
			}
		}
		/// <summary>
		/// Validates all sections with the specified name agains a set of required and optional properties.
		/// No other property names are allowed.
		/// </summary>
		/// <param name="sectionName">THe name of the section to check.</param>
		/// <param name="requiredProperties">An array of property names that are required.</param>
		/// <param name="optionalProperties">An array of property names that are optional.</param>
		public void ValidateSectionProperties(string sectionName, string[] requiredProperties, string[] optionalProperties)
		{
			if (requiredProperties == null) requiredProperties = new string[0];
			if (optionalProperties == null) optionalProperties = new string[0];

			foreach (IniSection section in Ini.Sections.Where(s => s.Name == sectionName))
			{
				foreach (string property in requiredProperties)
				{
					if (!section.HasProperty(property))
					{
						Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Property '" + property + "' required in section '" + sectionName + "'.");
					}
				}

				foreach (IniProperty property in section.Properties)
				{
					if (!requiredProperties.Contains(property.Name) && !optionalProperties.Contains(property.Name))
					{
						Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Invalid property '" + property.Name + "' in section '" + sectionName + "'.");
					}
				}
			}
		}
		/// <summary>
		/// Validates the no properties are empty.
		/// <see cref="string.Empty" /> property values are converted to <see langword="null" />.
		/// </summary>
		public void ValidateEmptyProperties()
		{
			foreach (IniSection section in Ini.Sections)
			{
				foreach (IniProperty property in section.Properties)
				{
					if (property.Value == "") property.Value = null;

					if (property.Value == null)
					{
						if (section.Name == "source.embedded" && property.Name == "path")
						{
							Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Embedded file not specified.");
						}
						else if (section.Name == "source.download" && property.Name == "url")
						{
							Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Download URL not specified.");
						}
						else if (section.Name == "action.drop" && property.Name == "filename")
						{
							Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Drop filename not specified.");
						}
						else if (section.Name == "action.messagebox" && property.Name == "title")
						{
							Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "MessageBox title not specified.");
						}
						else if (section.Name == "action.messagebox" && property.Name == "text")
						{
							Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "MessageBox text not specified.");
						}
						else
						{
							Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Property '" + property.Name + "' in section '" + section.Name + "' is empty.");
						}
					}
				}
			}
		}
		/// <summary>
		/// Gets an unescaped <see cref="string" /> value from an <see cref="IniProperty" />.
		/// </summary>
		/// <param name="property">The <see cref="IniProperty" /> to get the value from.</param>
		/// <returns>
		/// An unescaped <see cref="string" /> from <see cref="IniProperty.Value" />.
		/// </returns>
		public string GetStringValue(IniProperty property)
		{
			return property.Value.IsNullOrEmpty() ? property.Value : Regex.Unescape(property.Value);
		}
		/// <summary>
		/// Gets an <see cref="int" /> value from an <see cref="IniProperty" />.
		/// </summary>
		/// <param name="property">The <see cref="IniProperty" /> to get the value from.</param>
		/// <param name="min">The minimum value of this <see cref="int" />, or <see langword="null" />, if no lower bound is specified.</param>
		/// <param name="max">The maximum value of this <see cref="int" />, or <see langword="null" />, if no upper bound is specified.</param>
		/// <returns>
		/// The converted <see cref="int" /> from <see cref="IniProperty.Value" />.
		/// </returns>
		public int GetInt32Value(IniProperty property, int? min, int? max)
		{
			if (property.Int32Value is int intValue)
			{
				if (min != null && max != null)
				{
					if (intValue < min.Value || intValue > max.Value)
					{
						Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Property '" + property.Name + "' must be in range of " + min + ".." + max + ".");
						return 0;
					}
				}
				else if (min != null)
				{
					if (intValue < min.Value)
					{
						Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Property '" + property.Name + "' must be greater than or equal to " + min + ".");
						return 0;
					}
				}
				else if (max != null)
				{
					if (intValue > max.Value)
					{
						Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Property '" + property.Name + "' must be less than or equal to " + max + ".");
						return 0;
					}
				}

				return intValue;
			}
			else
			{
				Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "'" + property.Value + "' is not a valid integer.");
				return 0;
			}
		}
		/// <summary>
		/// Gets a <see cref="bool" /> value from an <see cref="IniProperty" />. Valid INI values are "yes" or "no".
		/// </summary>
		/// <param name="property">The <see cref="IniProperty" /> to get the value from.</param>
		/// <returns>
		/// The converted <see cref="bool" /> from <see cref="IniProperty.Value" />.
		/// </returns>
		public bool GetBooleanValue(IniProperty property)
		{
			if (property.Value == "true")
			{
				return true;
			}
			else if (property.Value == "false")
			{
				return false;
			}
			else
			{
				Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "'" + property.Value + "' is not a valid boolean.");
				return false;
			}
		}

		/// <summary>
		/// Finds a <see cref="ProjectSource" /> by a specified ID, or <see langword="null" />, if the source with this ID is not found in this project.
		/// </summary>
		/// <param name="id">The ID of the source.</param>
		/// <returns>
		/// The <see cref="ProjectSource" /> with the specified ID, or <see langword="null" />, if the source with this ID is not found in this project.
		/// </returns>
		public ProjectSource GetSource(string id)
		{
			if (Project.Sources.FirstOrDefault(s => s.Id == id) is ProjectSource source)
			{
				return source;
			}
			else
			{
				Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Source '" + id + "' not found.");
				return null;
			}
		}
		/// <summary>
		/// Converts a <see cref="string" /> to a <see cref="StubType" />.
		/// </summary>
		/// <param name="stubType">Type <see cref="string" /> to convert.</param>
		/// <returns>
		/// The converted <see cref="StubType" />, or <see langword="default" />(<see cref="StubType" />), if conversion failed.
		/// If conversion failed, an error is added to <see cref="Errors" />.
		/// </returns>
		public StubType ConvertStubType(string stubType)
		{
			if (EnumEx.FindValueByDescription<StubType>(stubType) is StubType convertedValue)
			{
				return convertedValue;
			}
			else
			{
				Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Stub type '" + stubType + "' invalid.");
				return default;
			}
		}
		/// <summary>
		/// Converts a <see cref="string" /> to a <see cref="ManifestTemplate" />.
		/// </summary>
		/// <param name="manifestTemplate">Type <see cref="string" /> to convert.</param>
		/// <returns>
		/// The converted <see cref="ManifestTemplate" />, or <see langword="null" />, if conversion failed or <paramref name="manifestTemplate" /> is <see langword="null" />.
		/// If conversion failed, an error is added to <see cref="Errors" />.
		/// </returns>
		public ManifestTemplate? ConvertManifestTemplate(string manifestTemplate)
		{
			if (manifestTemplate == null)
			{
				return null;
			}
			else if (EnumEx.FindValueByDescription<ManifestTemplate>(manifestTemplate) is ManifestTemplate convertedValue)
			{
				return convertedValue;
			}
			else
			{
				Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Manifest template '" + manifestTemplate + "' invalid.");
				return default;
			}
		}
		/// <summary>
		/// Converts a <see cref="string" /> to a <see cref="ActionEvent" />.
		/// </summary>
		/// <param name="actionEvent">Type <see cref="string" /> to convert.</param>
		/// <returns>
		/// The converted <see cref="ActionEvent" />, or <see langword="default" />(<see cref="ActionEvent" />), if conversion failed.
		/// If conversion failed, an error is added to <see cref="Errors" />.
		/// </returns>
		public ActionEvent ConvertActionEvent(string actionEvent)
		{
			if (EnumEx.FindValueByDescription<ActionEvent>(actionEvent) is ActionEvent convertedValue)
			{
				return convertedValue;
			}
			else
			{
				Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Action event '" + actionEvent + "' invalid.");
				return default;
			}
		}
		/// <summary>
		/// Converts a <see cref="string" /> to a <see cref="DropLocation" />.
		/// </summary>
		/// <param name="dropLocation">Type <see cref="string" /> to convert.</param>
		/// <returns>
		/// The converted <see cref="DropLocation" />, or <see langword="default" />(<see cref="DropLocation" />), if conversion failed.
		/// If conversion failed, an error is added to <see cref="Errors" />.
		/// </returns>
		public DropLocation ConvertDropLocation(string dropLocation)
		{
			if (EnumEx.FindValueByDescription<DropLocation>(dropLocation) is DropLocation convertedValue)
			{
				return convertedValue;
			}
			else
			{
				Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Drop location '" + dropLocation + "' invalid.");
				return default;
			}
		}
		/// <summary>
		/// Converts a <see cref="string" /> to a <see cref="ExecuteVerb" />.
		/// </summary>
		/// <param name="executeVerb">Type <see cref="string" /> to convert.</param>
		/// <returns>
		/// The converted <see cref="ExecuteVerb" />, or <see langword="default" />(<see cref="ExecuteVerb" />), if conversion failed.
		/// If conversion failed, an error is added to <see cref="Errors" />.
		/// </returns>
		public ExecuteVerb ConvertExecuteVerb(string executeVerb)
		{
			if (EnumEx.FindValueByDescription<ExecuteVerb>(executeVerb) is ExecuteVerb convertedValue)
			{
				return convertedValue;
			}
			else
			{
				Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Execute verb '" + executeVerb + "' invalid.");
				return default;
			}
		}
		/// <summary>
		/// Converts a <see cref="string" /> to a <see cref="MessageBoxIcon" />.
		/// </summary>
		/// <param name="messageBoxIcon">Type <see cref="string" /> to convert.</param>
		/// <returns>
		/// The converted <see cref="MessageBoxIcon" />, or <see langword="default" />(<see cref="MessageBoxIcon" />), if conversion failed.
		/// If conversion failed, an error is added to <see cref="Errors" />.
		/// </returns>
		public MessageBoxIcon ConvertMessageBoxIcon(string messageBoxIcon)
		{
			if (EnumEx.FindValueByDescription<MessageBoxIcon>(messageBoxIcon) is MessageBoxIcon convertedValue)
			{
				return convertedValue;
			}
			else
			{
				Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "MessageBox icon '" + messageBoxIcon + "' invalid.");
				return default;
			}
		}
		/// <summary>
		/// Converts a <see cref="string" /> to a <see cref="MessageBoxButtons" />.
		/// </summary>
		/// <param name="messageBoxButtons">Type <see cref="string" /> to convert.</param>
		/// <returns>
		/// The converted <see cref="MessageBoxButtons" />, or <see langword="default" />(<see cref="MessageBoxButtons" />), if conversion failed.
		/// If conversion failed, an error is added to <see cref="Errors" />.
		/// </returns>
		public MessageBoxButtons ConvertMessageBoxButtons(string messageBoxButtons)
		{
			if (EnumEx.FindValueByDescription<MessageBoxButtons>(messageBoxButtons) is MessageBoxButtons convertedValue)
			{
				return convertedValue;
			}
			else
			{
				Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "MessageBox buttons '" + messageBoxButtons + "' invalid.");
				return default;
			}
		}
		/// <summary>
		/// Converts the specified path to an absolute path, if the path is relative. Relative paths use the project file's base directory.
		/// </summary>
		/// <param name="relativePath">An absolute or a relative path.</param>
		/// <returns>
		/// An absolute path that is valid in conjunction with the base directory of the project file.
		/// </returns>
		public string ConvertRelativeToAbsolutePath(string relativePath)
		{
			string absolutePath = ProjectFilePath == null ? relativePath : RelativePath.RelativeToAbsolutePath(Path.GetDirectoryName(ProjectFilePath), relativePath);

			if (absolutePath.IsNullOrEmpty() || File.Exists(absolutePath))
			{
				return absolutePath;
			}
			else
			{
				Errors.Add(ErrorSource.Project, ErrorSeverity.Error, "File not found: " + absolutePath);
				return null;
			}
		}
	}
}