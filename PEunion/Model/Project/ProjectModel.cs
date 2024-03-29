﻿using BytecodeApi;
using BytecodeApi.Extensions;
using BytecodeApi.IO;
using BytecodeApi.UI.Dialogs;
using PEunion.Compiler.Errors;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PEunion
{
	public sealed class ProjectModel : TabModel
	{
		public string ProjectFileName
		{
			get => Get(() => ProjectFileName);
			set
			{
				Set(() => ProjectFileName, value);
				TabTitle = ProjectFileName;
			}
		}
		public string ProjectPath
		{
			get => Get(() => ProjectPath);
			set => Set(() => ProjectPath, value);
		}

		public override PageModel[] Pages => Get(() => Pages, () =>
		{
			PageModel[] pages = new PageModel[]
			{
				new ProjectPage(new PageModel[]
				{
					Stub,
					Startup,
					VersionInfo,
					Manifest
				}),
				new ItemsPage(Items)
			};

			Stub.IsSelected = true;
			return pages;
		});
		public override PageModel SelectedPage
		{
			get => Get(() => SelectedPage);
			set => Set(() => SelectedPage, value);
		}
		public StubModel Stub => Get(() => Stub, () =>
		{
			StubModel stub = new StubModel();
			stub.Changed += Project_Changed;
			return stub;
		});
		public StartupModel Startup => Get(() => Startup, () =>
		{
			StartupModel startup = new StartupModel();
			startup.Changed += Project_Changed;
			return startup;
		});
		public VersionInfoModel VersionInfo => Get(() => VersionInfo, () =>
		{
			VersionInfoModel versionInfo = new VersionInfoModel();
			versionInfo.Changed += Project_Changed;
			return versionInfo;
		});
		public ManifestModel Manifest => Get(() => Manifest, () =>
		{
			ManifestModel manifest = new ManifestModel();
			manifest.Changed += Project_Changed;
			return manifest;
		});
		public ObservableCollection<ProjectItemModel> Items => Get(() => Items, () =>
		{
			ObservableCollection<ProjectItemModel> items = new ObservableCollection<ProjectItemModel>();
			items.CollectionChanged += Items_CollectionChanged;
			return items;
		});
		public override ErrorModel Errors
		{
			get => Get(() => Errors, () => new ErrorModel(this));
			set => Set(() => Errors, value);
		}

		public ProjectModel(string projectFileName)
		{
			TabIcon = App.GetIcon("Project16");
			ProjectFileName = projectFileName;
		}
		public static ProjectModel Create(string projectFileName)
		{
			ProjectModel project = new ProjectModel(projectFileName);

			project.Stub.Padding = 50;
			project.Manifest.UseTemplate = true;

			project.IsDirty = false;
			project.Errors.Validate(false);
			return project;
		}
		public static ProjectModel FromFile(string path)
		{
			if (ProjectConverter.FromProjectFile(path, out ErrorCollection errors) is ProjectModel project)
			{
				project.IsDirty = false;
				project.ProjectPath = path;
				project.Errors.SetErrors(errors);

				if (errors.Any()) MainWindow.Singleton.BottomPanelSelectedIndex = 0;

				return project;
			}
			else
			{
				return null;
			}
		}

		public bool Save()
		{
			if (ProjectPath == null)
			{
				return SaveAs();
			}
			else
			{
				ProjectConverter.ToProjectFile(this).SaveTo(ProjectPath);
				IsDirty = false;
				Config.Recent.AddProject(ProjectPath);
				return true;
			}
		}
		public bool SaveAs()
		{
			if (FileDialogs.Save(ProjectFileName) is string path)
			{
				ProjectPath = path;
				ProjectFileName = Path.GetFileName(path);
				return Save();
			}
			else
			{
				return false;
			}
		}
		public void Build()
		{
			bool build = false;

			if (ProjectPath == null)
			{
				if (MessageBoxes.Confirmation("Project must be saved in order to build.\r\nSave?"))
				{
					build = SaveAs();
				}
			}
			else
			{
				Save();
				build = true;
			}

			if (build)
			{
				string compilerPath = Path.Combine(ApplicationBase.Path, "peubuild.exe");
				if (File.Exists(compilerPath))
				{
					ErrorCollection errors = Errors.GetProjectErrors();
					Errors.SetErrors(errors);

					if (errors.HasErrors)
					{
						MainWindow.Singleton.BottomPanelSelectedIndex = 0;
						Desktop.Beep(false);
					}
					else if (FileDialogs.Save(Path.ChangeExtension(ProjectPath, "exe")) is string outputPath)
					{
						BuildProgressDialog dialog = new BuildProgressDialog(Path.GetFileName(outputPath), new Process
						{
							StartInfo = new ProcessStartInfo(compilerPath, "\"" + ProjectPath + "\" \"" + outputPath + "\"")
							{
								UseShellExecute = false,
								CreateNoWindow = true,
								WindowStyle = ProcessWindowStyle.Hidden
							}
						});
						dialog.ShowDialog();

						if (dialog.ExitCode is int exitCode)
						{
							string log = Path.Combine(Path.GetDirectoryName(ProjectPath), ".peu", Path.GetFileNameWithoutExtension(ProjectPath), "compile.log");
							if (File.Exists(log))
							{
								Errors.SetErrors(ErrorCollection.FromFile(log));
							}
							else if (exitCode != 0)
							{
								Errors.Errors.Add(new Error(ErrorSource.Compiler, ErrorSeverity.Error, Path.GetFileName(compilerPath) + " returned status code " + exitCode + "."));
							}

							if (Errors.Errors.Any()) MainWindow.Singleton.BottomPanelSelectedIndex = 0;
							if (exitCode != 0) Desktop.Beep(false);
						}
						else
						{
							Errors.Errors.Add(new Error(ErrorSource.Compiler, ErrorSeverity.Message, "Build was canceled."));
							MainWindow.Singleton.BottomPanelSelectedIndex = 0;
						}
					}
				}
				else
				{
					Errors.Errors.Add(new Error(ErrorSource.Compiler, ErrorSeverity.Error, "File '" + Path.GetFileName(compilerPath) + "' not found."));
					MainWindow.Singleton.BottomPanelSelectedIndex = 0;
					Desktop.Beep(false);
				}
			}
		}
		public void CopyVersionInfo(string path)
		{
			if (File.Exists(path))
			{
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(path);

				if (!versionInfo.FileDescription.IsNullOrWhiteSpace() ||
					!versionInfo.ProductName.IsNullOrWhiteSpace() ||
					!versionInfo.FileVersion.IsNullOrWhiteSpace() ||
					!versionInfo.ProductVersion.IsNullOrWhiteSpace() ||
					!versionInfo.LegalCopyright.IsNullOrWhiteSpace() ||
					!versionInfo.OriginalFilename.IsNullOrWhiteSpace())
				{
					VersionInfo.FileDescription = versionInfo.FileDescription?.Trim();
					VersionInfo.ProductName = versionInfo.ProductName?.Trim();
					if (Version.TryParse(versionInfo.FileVersion?.Trim().Replace(',', '.'), out Version fileVersion))
					{
						VersionInfo.FileVersion1 = fileVersion.Major;
						VersionInfo.FileVersion2 = fileVersion.Minor;
						VersionInfo.FileVersion3 = fileVersion.Build;
						VersionInfo.FileVersion4 = fileVersion.Revision;
					}
					else
					{
						VersionInfo.FileVersion1 = 0;
						VersionInfo.FileVersion2 = 0;
						VersionInfo.FileVersion3 = 0;
						VersionInfo.FileVersion4 = 0;
					}
					VersionInfo.ProductVersion = versionInfo.ProductVersion?.Trim();
					VersionInfo.Copyright = versionInfo.LegalCopyright?.Trim();
					VersionInfo.OriginalFilename = versionInfo.OriginalFilename?.Trim();
				}
				else
				{
					MessageBoxes.Warning("File '" + Path.GetFileName(path) + "' does not contain version information.");
				}
			}
			else
			{
				MessageBoxes.Error("File '" + Path.GetFileName(path) + "' not found.");
			}
		}
		public void NewRunPEItem()
		{
			NewItem(new ProjectRunPEItemModel());
		}
		public void NewInvokeItem()
		{
			NewItem(new ProjectInvokeItemModel());
		}
		public void NewDropItem()
		{
			NewItem(new ProjectDropItemModel());
		}
		public void NewMessageBoxItem()
		{
			NewItem(new ProjectMessageBoxItemModel());
		}
		private void NewItem(ProjectItemModel item)
		{
			item.IsSelected = true;
			Items.Add(item);
			Pages.First(page => page.PageTemplate == PageTemplate.Items).IsExpanded = true;
		}
		public void RemoveItem(ProjectItemModel item)
		{
			Items.Remove(item);
			if (Items.None()) Pages.First(page => page.PageTemplate == PageTemplate.Items).IsExpanded = false;
		}
		public void MoveItem(ProjectItemModel item, int direction)
		{
			int index = Items.IndexOf(item);

			switch (direction)
			{
				case -1:
					if (index > 0)
					{
						Items.Remove(item);
						Items.Insert(index - 1, item);
					}
					break;
				case 1:
					if (index < Items.Count - 1)
					{
						Items.Remove(item);
						Items.Insert(index + 1, item);
					}
					break;
				default:
					throw new ArgumentException();
			}
		}

		private void Project_Changed(object sender, PropertyChangedEventArgs e)
		{
			IsDirty = true;
			Errors.Validate(true);
		}
		private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsDirty = true;
			Errors.Validate(true);

			if (e.OldItems != null)
			{
				foreach (ProjectItemModel item in e.OldItems.Cast<ProjectItemModel>()) item.Changed -= Project_Changed;
			}

			if (e.NewItems != null)
			{
				foreach (ProjectItemModel item in e.NewItems.Cast<ProjectItemModel>()) item.Changed += Project_Changed;
			}
		}
	}
}