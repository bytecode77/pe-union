using BytecodeApi.Extensions;
using BytecodeApi.FileFormats.Ini;
using PEunion.Compiler.Errors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines a PEunion project.
	/// </summary>
	public sealed class ProjectFile
	{
		private Stub _Stub;
		private Startup _Startup;
		private VersionInfo _VersionInfo;
		private Manifest _Manifest;
		private readonly List<ProjectSource> _Sources;
		private readonly List<ProjectAction> _Actions;
		/// <summary>
		/// Gets information about the stub of this project.
		/// </summary>
		public Stub Stub
		{
			get => _Stub;
			private set
			{
				if (value.Project != null) throw new InvalidOperationException("The item has already been added to a project.");
				if (_Stub != null) _Stub.Project = null;

				_Stub = value;
				_Stub.Project = this;
			}
		}
		/// <summary>
		/// Gets startup parameters of the stub.
		/// </summary>
		public Startup Startup
		{
			get => _Startup;
			private set
			{
				if (value.Project != null) throw new InvalidOperationException("The item has already been added to a project.");
				if (_Startup != null) _Startup.Project = null;

				_Startup = value;
				_Startup.Project = this;
			}
		}
		/// <summary>
		/// Gets the version information of this project.
		/// </summary>
		public VersionInfo VersionInfo
		{
			get => _VersionInfo;
			private set
			{
				if (value.Project != null) throw new InvalidOperationException("The item has already been added to a project.");
				if (_VersionInfo != null) _VersionInfo.Project = null;

				_VersionInfo = value;
				_VersionInfo.Project = this;
			}
		}
		/// <summary>
		/// Gets the manifest of this project.
		/// </summary>
		public Manifest Manifest
		{
			get => _Manifest;
			private set
			{
				if (value.Project != null) throw new InvalidOperationException("The item has already been added to a project.");
				if (_Manifest != null) _Manifest.Project = null;

				_Manifest = value;
				_Manifest.Project = this;
			}
		}
		/// <summary>
		/// Gets a collection of sources of this project.
		/// </summary>
		public ReadOnlyCollection<ProjectSource> Sources { get; private set; }
		/// <summary>
		/// Gets a collection of actions of this project.
		/// </summary>
		public ReadOnlyCollection<ProjectAction> Actions { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ProjectFile" /> class.
		/// </summary>
		public ProjectFile()
		{
			Stub = new Stub();
			Startup = new Startup();
			VersionInfo = new VersionInfo();
			Manifest = new Manifest();
			_Sources = new List<ProjectSource>();
			_Actions = new List<ProjectAction>();
			Sources = new ReadOnlyCollection<ProjectSource>(_Sources);
			Actions = new ReadOnlyCollection<ProjectAction>(_Actions);
		}
		/// <summary>
		/// Loads a <see cref="ProjectFile" /> from a file.
		/// </summary>
		/// <param name="path">The path of the file to load the project from.</param>
		/// <param name="errors">A collection of errors that can occur during project file parsing.</param>
		/// <returns>
		/// A new <see cref="ProjectFile" />, or <see langword="null" />, if parsing failed.
		/// </returns>
		public static ProjectFile FromFile(string path, ErrorCollection errors)
		{
			return FromString(path, File.ReadAllText(path), errors);
		}
		/// <summary>
		/// Loads a <see cref="ProjectFile" /> from a <see cref="string" /> containing INI file content.
		/// </summary>
		/// <param name="path">The path to the project file, or <see langword="null" /> if the project file is not stored on the disk.</param>
		/// <param name="fileContents">A <see cref="string" /> containing the INI file content of this project.</param>
		/// <param name="errors">A collection of errors that can occur during project file parsing.</param>
		/// <returns>
		/// A new <see cref="ProjectFile" />, or <see langword="null" />, if parsing failed.
		/// </returns>
		public static ProjectFile FromString(string path, string fileContents, ErrorCollection errors)
		{
			try
			{
				// Load .peu file and parse INI
				IniFile ini;
				try
				{
					ini = IniFile.FromBinary(fileContents.ToUTF8Bytes(), Encoding.UTF8, new IniFileParsingOptions
					{
						IgnoreErrors = true,
						AllowGlobalProperties = false,
						AllowSectionNameClosingBracket = false,
						DuplicateSectionNameBehavior = IniDuplicateSectionNameBehavior.Duplicate,
						DuplicateSectionNameIgnoreCase = false,
						DuplicatePropertyNameBehavior = IniDuplicatePropertyNameBehavior.Abort,
						DuplicatePropertyNameIgnoreCase = false
					});
				}
				catch (Exception ex)
				{
					throw new ErrorException("INI parsing error.", ex.GetFullStackTrace());
				}

				// INI contains invalid lines
				if (ini.ErrorLines.Any())
				{
					foreach (IniErrorLine errorLine in ini.ErrorLines)
					{
						errors.Add(ErrorSource.Project, ErrorSeverity.Error, "INI parsing error at line " + errorLine.LineNumber, errorLine.Line);
					}

					return null;
				}

				ProjectFile project = new ProjectFile();
				ProjectFileHelper helper = new ProjectFileHelper(path, ini, project, errors);

				helper.ReadIni();
				helper.ValidateProject();

				return project;
			}
			catch (ErrorException ex)
			{
				errors.Add(ErrorSource.Project, ErrorSeverity.Error, ex.Message, ex.Details);
				return null;
			}
			catch (Exception ex)
			{
				errors.Add(ErrorSource.Project, ErrorSeverity.Error, "Unhandled " + ex.GetType() + " while parsing project file.", ex.GetFullStackTrace());
				return null;
			}
		}
		/// <summary>
		/// Saves this <see cref="ProjectFile" /> to a file.
		/// </summary>
		/// <param name="path">The path of the file to save this project to.</param>
		public void SaveTo(string path)
		{
			File.WriteAllText(path, AsString());
		}
		/// <summary>
		/// Gets the INI file content of this <see cref="ProjectFile" />.
		/// </summary>
		/// <returns>
		/// A <see cref="string" /> containing the INI file content of this <see cref="ProjectFile" />.
		/// </returns>
		public string AsString()
		{
			IniFile ini = new IniFile();

			IniSection stubSection = new IniSection("stub");
			stubSection.Properties.Add(new IniProperty("type", Stub.Type.GetDescription()));
			if (!Stub.IconPath.IsNullOrEmpty()) stubSection.Properties.Add(new IniProperty("icon", Stub.IconPath));
			if (Stub.Padding != 0) stubSection.Properties.Add(new IniProperty("padding", Stub.Padding.ToString()));
			ini.Sections.Add(stubSection);

			if (!Startup.IsEmpty)
			{
				IniSection startupSection = new IniSection("startup");
				if (Startup.Melt) startupSection.Properties.Add(new IniProperty("melt", "true"));
				ini.Sections.Add(startupSection);
			}

			if (!VersionInfo.IsEmpty)
			{
				IniSection versionInfoSection = new IniSection("versioninfo");
				if (!VersionInfo.FileDescription.IsNullOrEmpty()) versionInfoSection.Properties.Add(new IniProperty("file_description", EscapeIniString(VersionInfo.FileDescription)));
				if (!VersionInfo.ProductName.IsNullOrEmpty()) versionInfoSection.Properties.Add(new IniProperty("product_name", EscapeIniString(VersionInfo.ProductName)));
				if (!VersionInfo.FileVersion.IsNullOrEmpty()) versionInfoSection.Properties.Add(new IniProperty("file_version", EscapeIniString(VersionInfo.FileVersion)));
				if (!VersionInfo.ProductVersion.IsNullOrEmpty()) versionInfoSection.Properties.Add(new IniProperty("product_version", EscapeIniString(VersionInfo.ProductVersion)));
				if (!VersionInfo.Copyright.IsNullOrEmpty()) versionInfoSection.Properties.Add(new IniProperty("copyright", EscapeIniString(VersionInfo.Copyright)));
				if (!VersionInfo.OriginalFilename.IsNullOrEmpty()) versionInfoSection.Properties.Add(new IniProperty("original_filename", EscapeIniString(VersionInfo.OriginalFilename)));
				ini.Sections.Add(versionInfoSection);
			}

			if (Manifest.Template != null || !Manifest.Path.IsNullOrEmpty())
			{
				IniSection manifestSection = new IniSection("manifest");
				if (Manifest.Template != null) manifestSection.Properties.Add(new IniProperty("template", Manifest.Template.GetDescription()));
				if (!Manifest.Path.IsNullOrEmpty()) manifestSection.Properties.Add(new IniProperty("path", Manifest.Path));
				ini.Sections.Add(manifestSection);
			}

			foreach (ProjectSource source in Sources)
			{
				IniSection sourceSection;

				if (source is EmbeddedSource embeddedSource)
				{
					sourceSection = new IniSection("source.embedded");
					sourceSection.Properties.Add(new IniProperty("id", source.Id));
					sourceSection.Properties.Add(new IniProperty("path", embeddedSource.Path));
					if (embeddedSource.Compress) sourceSection.Properties.Add(new IniProperty("compress", "true"));
					if (embeddedSource.EofData) sourceSection.Properties.Add(new IniProperty("eof", "true"));
				}
				else if (source is DownloadSource downloadSource)
				{
					sourceSection = new IniSection("source.download");
					sourceSection.Properties.Add(new IniProperty("id", source.Id));
					sourceSection.Properties.Add(new IniProperty("url", EscapeIniString(downloadSource.Url)));
				}
				else
				{
					throw new InvalidOperationException();
				}

				ini.Sections.Add(sourceSection);
			}

			foreach (ProjectAction action in Actions)
			{
				IniSection actionSection;

				if (action is RunPEAction)
				{
					actionSection = new IniSection("action.runpe");
					actionSection.Properties.Add(new IniProperty("source", action.Source.Id));
				}
				else if (action is InvokeAction)
				{
					actionSection = new IniSection("action.invoke");
					actionSection.Properties.Add(new IniProperty("source", action.Source.Id));
				}
				else if (action is DropAction dropAction)
				{
					actionSection = new IniSection("action.drop");
					actionSection.Properties.Add(new IniProperty("source", action.Source.Id));
					actionSection.Properties.Add(new IniProperty("location", dropAction.Location.GetDescription()));
					actionSection.Properties.Add(new IniProperty("filename", EscapeIniString(dropAction.FileName)));
					if (dropAction.FileAttributeHidden) actionSection.Properties.Add(new IniProperty("attribute_hidden", "true"));
					if (dropAction.FileAttributeSystem) actionSection.Properties.Add(new IniProperty("attribute_system", "true"));
					if (dropAction.ExecuteVerb != ExecuteVerb.None) actionSection.Properties.Add(new IniProperty("execute", dropAction.ExecuteVerb.GetDescription()));
				}
				else if (action is MessageBoxAction messageBoxAction)
				{
					actionSection = new IniSection("action.messagebox");
					actionSection.Properties.Add(new IniProperty("title", EscapeIniString(messageBoxAction.Title)));
					actionSection.Properties.Add(new IniProperty("text", EscapeIniString(messageBoxAction.Text)));
					actionSection.Properties.Add(new IniProperty("icon", messageBoxAction.Icon.GetDescription()));
					actionSection.Properties.Add(new IniProperty("buttons", messageBoxAction.Buttons.GetDescription()));
					if (messageBoxAction.OnOk != ActionEvent.None) actionSection.Properties.Add(new IniProperty("on_ok", messageBoxAction.OnOk.GetDescription()));
					if (messageBoxAction.OnCancel != ActionEvent.None) actionSection.Properties.Add(new IniProperty("on_cancel", messageBoxAction.OnCancel.GetDescription()));
					if (messageBoxAction.OnYes != ActionEvent.None) actionSection.Properties.Add(new IniProperty("on_yes", messageBoxAction.OnYes.GetDescription()));
					if (messageBoxAction.OnNo != ActionEvent.None) actionSection.Properties.Add(new IniProperty("on_no", messageBoxAction.OnNo.GetDescription()));
					if (messageBoxAction.OnAbort != ActionEvent.None) actionSection.Properties.Add(new IniProperty("on_abort", messageBoxAction.OnAbort.GetDescription()));
					if (messageBoxAction.OnRetry != ActionEvent.None) actionSection.Properties.Add(new IniProperty("on_retry", messageBoxAction.OnRetry.GetDescription()));
					if (messageBoxAction.OnIgnore != ActionEvent.None) actionSection.Properties.Add(new IniProperty("on_ignore", messageBoxAction.OnIgnore.GetDescription()));
				}
				else
				{
					throw new InvalidOperationException();
				}

				ini.Sections.Add(actionSection);
			}

			using (MemoryStream memoryStream = new MemoryStream())
			{
				ini.Save(memoryStream, Encoding.UTF8);
				return memoryStream.ToArray().ToUTF8String();
			}

			string EscapeIniString(string str)
			{
				return str
					?.Replace("\r", @"\r")
					.Replace("\n", @"\n")
					.Replace("\t", @"\t");
			}
		}

		/// <summary>
		/// Adds a source to this project.
		/// </summary>
		/// <param name="source">An object of a class that inherits <see cref="ProjectSource" />.</param>
		public void AddSource(ProjectSource source)
		{
			if (source.Project != null) throw new InvalidOperationException("The item has already been added to a project.");
			source.Project = this;
			_Sources.Add(source);
		}
		/// <summary>
		/// Adds an action to this project.
		/// </summary>
		/// <param name="action">An object of a class that inherits <see cref="ProjectAction" />.</param>
		public void AddAction(ProjectAction action)
		{
			if (action.Project != null) throw new InvalidOperationException("The item has already been added to a project.");
			action.Project = this;
			_Actions.Add(action);
		}
		/// <summary>
		/// Removes the specified source from this project.
		/// </summary>
		/// <param name="source">The <see cref="ProjectSource" /> object to be removed.</param>
		public void RemoveSource(ProjectSource source)
		{
			if (_Sources.Remove(source)) source.Project = null;
		}
		/// <summary>
		/// Removes the specified action from this project.
		/// </summary>
		/// <param name="action">The <see cref="ProjectAction" /> object to be removed.</param>
		public void RemoveAction(ProjectAction action)
		{
			if (_Actions.Remove(action)) action.Project = null;
		}
	}
}