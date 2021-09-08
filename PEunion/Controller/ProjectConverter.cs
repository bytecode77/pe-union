using BytecodeApi.Extensions;
using PEunion.Compiler;
using PEunion.Compiler.Errors;
using PEunion.Compiler.Project;
using System;
using System.ComponentModel;
using System.IO;

namespace PEunion
{
	public static class ProjectConverter
	{
		public static ProjectFile ToProjectFile(ProjectModel project)
		{
			ProjectFile file = new ProjectFile();

			file.Stub.Type = project.Stub.Type;
			file.Stub.IconPath = ConvertAbsoluteToRelativePath(project.Stub.IconPath.ToNullIfEmpty());
			file.Stub.Padding = project.Stub.Padding;

			file.Startup.Melt = project.Startup.Melt;

			file.VersionInfo.FileDescription = project.VersionInfo.FileDescription.ToNullIfEmptyOrWhiteSpace();
			file.VersionInfo.ProductName = project.VersionInfo.ProductName.ToNullIfEmptyOrWhiteSpace();
			file.VersionInfo.FileVersion = project.VersionInfo.FileVersion1 != 0 || project.VersionInfo.FileVersion2 != 0 || project.VersionInfo.FileVersion3 != 0 || project.VersionInfo.FileVersion4 != 0 ? project.VersionInfo.FileVersion1 + "." + project.VersionInfo.FileVersion2 + "." + project.VersionInfo.FileVersion3 + "." + project.VersionInfo.FileVersion4 : null;
			file.VersionInfo.ProductVersion = project.VersionInfo.ProductVersion.ToNullIfEmptyOrWhiteSpace();
			file.VersionInfo.Copyright = project.VersionInfo.Copyright.ToNullIfEmptyOrWhiteSpace();
			file.VersionInfo.OriginalFilename = project.VersionInfo.OriginalFilename.ToNullIfEmptyOrWhiteSpace();

			if (project.Manifest.UseTemplate) file.Manifest.Template = project.Manifest.Template;
			else if (project.Manifest.UseFile) file.Manifest.Path = ConvertAbsoluteToRelativePath(project.Manifest.Path.ToNullIfEmpty());

			foreach (ProjectItemModel item in project.Items)
			{
				ProjectSource source;

				if (item is ProjectMessageBoxItemModel)
				{
					source = null;
				}
				else
				{
					switch (item.Source)
					{
						case ProjectItemSource.Embedded:
							file.AddSource(source = new EmbeddedSource
							{
								Path = ConvertAbsoluteToRelativePath(item.SourceEmbeddedPath.ToNullIfEmpty()),
								Compress = item.SourceEmbeddedCompress,
								EofData = item.SourceEmbeddedEofData
							});
							break;
						case ProjectItemSource.Download:
							file.AddSource(source = new DownloadSource
							{
								Url = item.SourceDownloadUrl
							});
							break;
						default:
							throw new InvalidEnumArgumentException();
					}

					source.Id = item.SourceId;
				}

				if (item is ProjectRunPEItemModel)
				{
					file.AddAction(new RunPEAction
					{
						Source = source
					});
				}
				else if (item is ProjectInvokeItemModel)
				{
					file.AddAction(new InvokeAction
					{
						Source = source
					});
				}
				else if (item is ProjectDropItemModel dropItem)
				{
					file.AddAction(new DropAction
					{
						Source = source,
						Location = dropItem.Location,
						FileName = dropItem.FileName,
						FileAttributeHidden = dropItem.FileAttributeHidden,
						FileAttributeSystem = dropItem.FileAttributeSystem,
						ExecuteVerb = dropItem.ExecuteVerb
					});
				}
				else if (item is ProjectMessageBoxItemModel messageBoxItem)
				{
					file.AddAction(new MessageBoxAction
					{
						Title = messageBoxItem.Title,
						Text = messageBoxItem.Text,
						Icon = messageBoxItem.Icon,
						Buttons = messageBoxItem.Buttons,
						OnOk = MessageBoxAction.HasEvent(messageBoxItem.Buttons, MessageBoxEvent.Ok) ? messageBoxItem.OnOk : ActionEvent.None,
						OnCancel = MessageBoxAction.HasEvent(messageBoxItem.Buttons, MessageBoxEvent.Cancel) ? messageBoxItem.OnCancel : ActionEvent.None,
						OnYes = MessageBoxAction.HasEvent(messageBoxItem.Buttons, MessageBoxEvent.Yes) ? messageBoxItem.OnYes : ActionEvent.None,
						OnNo = MessageBoxAction.HasEvent(messageBoxItem.Buttons, MessageBoxEvent.No) ? messageBoxItem.OnNo : ActionEvent.None,
						OnAbort = MessageBoxAction.HasEvent(messageBoxItem.Buttons, MessageBoxEvent.Abort) ? messageBoxItem.OnAbort : ActionEvent.None,
						OnRetry = MessageBoxAction.HasEvent(messageBoxItem.Buttons, MessageBoxEvent.Retry) ? messageBoxItem.OnRetry : ActionEvent.None,
						OnIgnore = MessageBoxAction.HasEvent(messageBoxItem.Buttons, MessageBoxEvent.Ignore) ? messageBoxItem.OnIgnore : ActionEvent.None
					});
				}
				else
				{
					throw new InvalidOperationException();
				}
			}

			return file;

			string ConvertAbsoluteToRelativePath(string absolutePath)
			{
				return project.ProjectPath == null ? absolutePath : RelativePath.AbsoluteToRelativePath(Path.GetDirectoryName(project.ProjectPath), absolutePath);
			}
		}
		public static ProjectModel FromProjectFile(string path, out ErrorCollection errors)
		{
			errors = new ErrorCollection();

			if (ProjectFile.FromFile(path, errors) is ProjectFile file)
			{
				ProjectModel project = new ProjectModel(Path.GetFileName(path));

				project.Stub.Type = file.Stub.Type;
				project.Stub.IconPath = file.Stub.IconPath;
				project.Stub.Padding = file.Stub.Padding;

				project.Startup.Melt = file.Startup.Melt;

				project.VersionInfo.FileDescription = file.VersionInfo.FileDescription;
				project.VersionInfo.ProductName = file.VersionInfo.ProductName;
				if (Version.TryParse(file.VersionInfo.FileVersion, out Version fileVersion))
				{
					project.VersionInfo.FileVersion1 = fileVersion.Major;
					project.VersionInfo.FileVersion2 = fileVersion.Minor;
					project.VersionInfo.FileVersion3 = fileVersion.Build;
					project.VersionInfo.FileVersion4 = fileVersion.Revision;
				}
				project.VersionInfo.ProductVersion = file.VersionInfo.ProductVersion;
				project.VersionInfo.Copyright = file.VersionInfo.Copyright;
				project.VersionInfo.OriginalFilename = file.VersionInfo.OriginalFilename;

				project.Manifest.UseNone = file.Manifest.Template == null && file.Manifest.Template == null;
				project.Manifest.UseTemplate = file.Manifest.Template != null;
				project.Manifest.UseFile = file.Manifest.Path != null;
				if (file.Manifest.Template != null) project.Manifest.Template = file.Manifest.Template.Value;
				if (file.Manifest.Path != null) project.Manifest.Path = file.Manifest.Path;

				foreach (ProjectAction action in file.Actions)
				{
					ProjectItemModel item;

					if (action is RunPEAction)
					{
						item = new ProjectRunPEItemModel();
					}
					else if (action is InvokeAction)
					{
						item = new ProjectInvokeItemModel();
					}
					else if (action is DropAction dropAction)
					{
						item = new ProjectDropItemModel
						{
							Location = dropAction.Location,
							FileName = dropAction.FileName,
							FileAttributeHidden = dropAction.FileAttributeHidden,
							FileAttributeSystem = dropAction.FileAttributeSystem,
							ExecuteVerb = dropAction.ExecuteVerb
						};
					}
					else if (action is MessageBoxAction messageBoxAction)
					{
						item = new ProjectMessageBoxItemModel
						{
							Title = messageBoxAction.Title,
							Text = messageBoxAction.Text,
							Icon = messageBoxAction.Icon,
							Buttons = messageBoxAction.Buttons,
							OnOk = messageBoxAction.OnOk,
							OnCancel = messageBoxAction.OnCancel,
							OnYes = messageBoxAction.OnYes,
							OnNo = messageBoxAction.OnNo,
							OnAbort = messageBoxAction.OnAbort,
							OnRetry = messageBoxAction.OnRetry,
							OnIgnore = messageBoxAction.OnIgnore
						};
					}
					else
					{
						throw new InvalidOperationException();
					}

					if (!(action is MessageBoxAction))
					{
						item.SourceId = action.Source.Id;

						if (action.Source is EmbeddedSource embeddedSource)
						{
							item.Source = ProjectItemSource.Embedded;
							item.SourceEmbeddedPath = embeddedSource.Path;
							item.SourceEmbeddedCompress = embeddedSource.Compress;
							item.SourceEmbeddedEofData = embeddedSource.EofData;
						}
						else if (action.Source is DownloadSource downloadSource)
						{
							item.Source = ProjectItemSource.Download;
							item.SourceDownloadUrl = downloadSource.Url;
						}
						else
						{
							throw new InvalidOperationException();
						}
					}

					project.Items.Add(item);
				}

				return project;
			}
			else
			{
				return null;
			}
		}
	}
}