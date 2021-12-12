using BytecodeApi;
using BytecodeApi.Extensions;
using BytecodeApi.IO.FileSystem;
using BytecodeApi.UI;
using BytecodeApi.UI.Dialogs;
using PEunion.Compiler.Errors;
using PEunion.Compiler.Helper;
using PEunion.Compiler.Project;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PEunion
{
	public sealed class MainWindowViewModel : ViewModelBase
	{
		public static MainWindowViewModel Singleton { get; private set; }
		public MainWindow View { get; set; }

		private DelegateCommand _NewProjectCommand;
		private DelegateCommand<string> _OpenProjectCommand;
		private DelegateCommand<TabModel> _CloseCommand;
		private DelegateCommand _CloseAllCommand;
		private DelegateCommand _SaveProjectCommand;
		private DelegateCommand _SaveProjectAsCommand;
		private DelegateCommand _SaveAllCommand;
		private DelegateCommand _ExitCommand;
		private DelegateCommand _BuildProjectCommand;
		private DelegateCommand _RtloCommand;
		private DelegateCommand _ChangelogCommand;
		private DelegateCommand _AboutCommand;
		private DelegateCommand<PageTemplate> _SelectPageCommand;
		private DelegateCommand _NewRunPEItemCommand;
		private DelegateCommand _NewInvokeItemCommand;
		private DelegateCommand _NewDropItemCommand;
		private DelegateCommand _NewMessageBoxItemCommand;
		private DelegateCommand _MoveItemUpCommand;
		private DelegateCommand _MoveItemDownCommand;
		private DelegateCommand _FilePropertiesCommand;
		private DelegateCommand _OpenFileLocationCommand;
		private DelegateCommand _RemoveItemCommand;
		private DelegateCommand _CopyStubIconCommand;
		private DelegateCommand _CopyVersionInfoFromItemCommand;
		private DelegateCommand _CopyVersionInfoFromFileCommand;
		private DelegateCommand _ManifestPreviewCommand;
		private DelegateCommand<ProjectMessageBoxItemModel> _MessageBoxPreviewCommand;
		private DelegateCommand<Error> _ShowErrorDetailsCommand;
		private DelegateCommand<RtloModel> _RtloSaveCommand;
		public DelegateCommand NewProjectCommand => _NewProjectCommand ?? (_NewProjectCommand = new DelegateCommand(NewProjectCommand_Execute));
		public DelegateCommand<string> OpenProjectCommand => _OpenProjectCommand ?? (_OpenProjectCommand = new DelegateCommand<string>(OpenProjectCommand_Execute));
		public DelegateCommand<TabModel> CloseCommand => _CloseCommand ?? (_CloseCommand = new DelegateCommand<TabModel>(CloseCommand_Execute));
		public DelegateCommand CloseAllCommand => _CloseAllCommand ?? (_CloseAllCommand = new DelegateCommand(CloseAllCommand_Execute));
		public DelegateCommand SaveProjectCommand => _SaveProjectCommand ?? (_SaveProjectCommand = new DelegateCommand(SaveProjectCommand_Execute, SaveProjectCommand_CanExecute));
		public DelegateCommand SaveProjectAsCommand => _SaveProjectAsCommand ?? (_SaveProjectAsCommand = new DelegateCommand(SaveProjectAsCommand_Execute, SaveProjectAsCommand_CanExecute));
		public DelegateCommand SaveAllCommand => _SaveAllCommand ?? (_SaveAllCommand = new DelegateCommand(SaveAllCommand_Execute, SaveAllCommand_CanExecute));
		public DelegateCommand ExitCommand => _ExitCommand ?? (_ExitCommand = new DelegateCommand(ExitCommand_Execute));
		public DelegateCommand BuildProjectCommand => _BuildProjectCommand ?? (_BuildProjectCommand = new DelegateCommand(BuildProjectCommand_Execute, BuildProjectCommand_CanExecute));
		public DelegateCommand RtloCommand => _RtloCommand ?? (_RtloCommand = new DelegateCommand(RtloCommand_Execute));
		public DelegateCommand ChangelogCommand => _ChangelogCommand ?? (_ChangelogCommand = new DelegateCommand(ChangelogCommand_Execute));
		public DelegateCommand AboutCommand => _AboutCommand ?? (_AboutCommand = new DelegateCommand(AboutCommand_Execute));
		public DelegateCommand<PageTemplate> SelectPageCommand => _SelectPageCommand ?? (_SelectPageCommand = new DelegateCommand<PageTemplate>(SelectPageCommand_Execute));
		public DelegateCommand NewRunPEItemCommand => _NewRunPEItemCommand ?? (_NewRunPEItemCommand = new DelegateCommand(NewRunPEItemCommand_Execute));
		public DelegateCommand NewInvokeItemCommand => _NewInvokeItemCommand ?? (_NewInvokeItemCommand = new DelegateCommand(NewInvokeItemCommand_Execute));
		public DelegateCommand NewDropItemCommand => _NewDropItemCommand ?? (_NewDropItemCommand = new DelegateCommand(NewDropItemCommand_Execute));
		public DelegateCommand NewMessageBoxItemCommand => _NewMessageBoxItemCommand ?? (_NewMessageBoxItemCommand = new DelegateCommand(NewMessageBoxItemCommand_Execute));
		public DelegateCommand MoveItemUpCommand => _MoveItemUpCommand ?? (_MoveItemUpCommand = new DelegateCommand(MoveItemUpCommand_Execute, MoveItemUpCommand_CanExecute));
		public DelegateCommand MoveItemDownCommand => _MoveItemDownCommand ?? (_MoveItemDownCommand = new DelegateCommand(MoveItemDownCommand_Execute, MoveItemDownCommand_CanExecute));
		public DelegateCommand FilePropertiesCommand => _FilePropertiesCommand ?? (_FilePropertiesCommand = new DelegateCommand(FilePropertiesCommand_Execute, FilePropertiesCommand_CanExecute));
		public DelegateCommand OpenFileLocationCommand => _OpenFileLocationCommand ?? (_OpenFileLocationCommand = new DelegateCommand(OpenFileLocationCommand_Execute, OpenFileLocationCommand_CanExecute));
		public DelegateCommand RemoveItemCommand => _RemoveItemCommand ?? (_RemoveItemCommand = new DelegateCommand(RemoveItemCommand_Execute, RemoveItemCommand_CanExecute));
		public DelegateCommand CopyStubIconCommand => _CopyStubIconCommand ?? (_CopyStubIconCommand = new DelegateCommand(CopyStubIconCommand_Execute));
		public DelegateCommand CopyVersionInfoFromItemCommand => _CopyVersionInfoFromItemCommand ?? (_CopyVersionInfoFromItemCommand = new DelegateCommand(CopyVersionInfoFromItemCommand_Execute));
		public DelegateCommand CopyVersionInfoFromFileCommand => _CopyVersionInfoFromFileCommand ?? (_CopyVersionInfoFromFileCommand = new DelegateCommand(CopyVersionInfoFromFileCommand_Execute));
		public DelegateCommand ManifestPreviewCommand => _ManifestPreviewCommand ?? (_ManifestPreviewCommand = new DelegateCommand(ManifestPreviewCommand_Execute));
		public DelegateCommand<ProjectMessageBoxItemModel> MessageBoxPreviewCommand => _MessageBoxPreviewCommand ?? (_MessageBoxPreviewCommand = new DelegateCommand<ProjectMessageBoxItemModel>(MessageBoxPreviewCommand_Execute));
		public DelegateCommand<Error> ShowErrorDetailsCommand => _ShowErrorDetailsCommand ?? (_ShowErrorDetailsCommand = new DelegateCommand<Error>(ShowErrorDetailsCommand_Execute));
		public DelegateCommand<RtloModel> RtloSaveCommand => _RtloSaveCommand ?? (_RtloSaveCommand = new DelegateCommand<RtloModel>(RtloSaveCommand_Execute, RtloSaveCommand_CanExecute));

		private bool _IsInitialized;
		private TabModel _SelectedTab;
		private string[] _RecentProjects;
		private string _HelpHtml;
		public bool IsInitialized
		{
			get => _IsInitialized;
			set => Set(ref _IsInitialized, value);
		}
		public ObservableCollection<TabModel> Tabs { get; private set; }
		public TabModel SelectedTab
		{
			get => _SelectedTab;
			set => Set(ref _SelectedTab, value);
		}
		public string[] RecentProjects
		{
			get => _RecentProjects;
			set => Set(ref _RecentProjects, value);
		}
		public string HelpHtml
		{
			get => _HelpHtml;
			set
			{
				Set(ref _HelpHtml, value);
				View.BottomPanelSelectedIndex = 1;
			}
		}

		public MainWindowViewModel(MainWindow view)
		{
			View = view;
			Singleton = this;

			if (Help.GetHelpFile("Default", out string html, out _)) HelpHtml = html;
			View.BottomPanelSelectedIndex = 0;

			Tabs = new ObservableCollection<TabModel>();
			RecentProjects = Config.Recent.Projects ?? new string[0];

			NewProjectCommand.Execute();
			Config.Recent.ProjectsChanged += Config_Recent_ProjectsChanged;
		}

		public void ParseCommandLine(string[] args)
		{
			foreach (string arg in args)
			{
				if (Path.GetExtension(arg).Equals(".peu", StringComparison.OrdinalIgnoreCase) && File.Exists(arg))
				{
					OpenProjectCommand.Execute(arg);
				}
			}
		}
		public bool SaveChangesBeforeExit()
		{
			IEnumerable<ProjectModel> unsaved = Tabs.OfType<ProjectModel>().Where(project => project.IsDirty);

			if (unsaved.Any())
			{
				return new SaveChangesDialog
				(
					unsaved.Select(project => project.ProjectFileName + " *").ToArray(),
					() => unsaved.All(project => project.Save())
				).ShowDialog() == true;
			}
			else
			{
				return true;
			}
		}
		private void AddTab(TabModel tab)
		{
			Tabs.Add(tab);
			SelectedTab = tab;
		}

		private void NewProjectCommand_Execute()
		{
			Regex nameIndexRegex = new Regex("^New Project \\d{0,3}$", RegexOptions.IgnoreCase);

			int[] nameIndexes = Tabs
				.OfType<ProjectModel>()
				.Select(project => Path.GetFileNameWithoutExtension(project.ProjectFileName))
				.Where(name => nameIndexRegex.IsMatch(name))
				.Select(name => name.SubstringFrom(" ", true).ToInt32OrNull())
				.ExceptNull()
				.SortDescending()
				.ToArray();

			AddTab(ProjectModel.Create("New Project " + (nameIndexes.Any() ? nameIndexes.First() + 1 : 1) + ".peu"));
		}
		private void OpenProjectCommand_Execute(string parameter)
		{
			if ((parameter ?? FileDialogs.Open(new[] { "peu" }, "PEunion files")) is string path)
			{
				if (Tabs.OfType<ProjectModel>().FirstOrDefault(tab => tab.ProjectPath?.Equals(path, StringComparison.OrdinalIgnoreCase) == true) is ProjectModel existingProject)
				{
					SelectedTab = existingProject;
				}
				else if (!File.Exists(path))
				{
					MessageBoxes.Error("File '" + Path.GetFileName(path) + "' not found.");
				}
				else if (File.ReadLines(path).FirstOrDefault() is string firstLine && firstLine.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
				{
					MessageBoxes.Error("Project file '" + Path.GetFileName(path) + "' is from an older version of PEunion.");
				}
				else if (ProjectModel.FromFile(path) is ProjectModel project)
				{
					if (Tabs.OfType<ProjectModel>().Count() == 1 &&
						Tabs.OfType<ProjectModel>().First() is ProjectModel initialProject &&
						initialProject.ProjectPath == null &&
						!initialProject.IsDirty)
					{
						Tabs.Remove(initialProject);
					}

					AddTab(project);
					Config.Recent.AddProject(path);
				}
				else
				{
					MessageBoxes.Error("Could not open '" + Path.GetFileName(path) + "'.");
				}
			}
		}
		private void CloseCommand_Execute(TabModel tab)
		{
			bool close = true;

			if ((tab ?? SelectedTab) is ProjectModel project && project.IsDirty)
			{
				close = new SaveChangesDialog
				(
					new[] { project.ProjectFileName + " *" },
					() => project.Save()
				).ShowDialog() == true;
			}

			if (close)
			{
				Tabs.Remove(tab ?? SelectedTab);
				if (Tabs.None()) NewProjectCommand.Execute();
			}
		}
		private void CloseAllCommand_Execute()
		{
			ProjectModel[] unsaved = Tabs.OfType<ProjectModel>().Where(project => project.IsDirty).ToArray();

			if (unsaved.Any())
			{
				bool saved = new SaveChangesDialog
				(
					unsaved.Select(project => project.ProjectFileName + " *").ToArray(),
					() => unsaved.All(project =>
					{
						if (project.Save())
						{
							Tabs.Remove(project);
							return true;
						}
						else
						{
							return false;
						}
					})
				).ShowDialog() == true;

				if (saved) Tabs.Clear();
			}
			else
			{
				Tabs.Clear();
			}

			if (Tabs.None())
			{
				NewProjectCommand.Execute();
			}
		}
		private void SaveProjectCommand_Execute()
		{
			((ProjectModel)SelectedTab).Save();
		}
		private bool SaveProjectCommand_CanExecute()
		{
			return SelectedTab is ProjectModel;
		}
		private void SaveProjectAsCommand_Execute()
		{
			((ProjectModel)SelectedTab).SaveAs();
		}
		private bool SaveProjectAsCommand_CanExecute()
		{
			return SelectedTab is ProjectModel;
		}
		private void SaveAllCommand_Execute()
		{
			foreach (ProjectModel project in Tabs.OfType<ProjectModel>())
			{
				if (!project.Save()) break;
			}
		}
		private bool SaveAllCommand_CanExecute()
		{
			return Tabs.OfType<ProjectModel>().Any();
		}
		private void ExitCommand_Execute()
		{
			View.Close();
		}
		private void BuildProjectCommand_Execute()
		{
			((ProjectModel)SelectedTab).Build();
		}
		private bool BuildProjectCommand_CanExecute()
		{
			return SelectedTab is ProjectModel;
		}
		private void RtloCommand_Execute()
		{
			Regex nameIndexRegex = new Regex("^Right-to-Left Override \\d{0,3}$", RegexOptions.IgnoreCase);

			int[] nameIndexes = Tabs
				.OfType<RtloModel>()
				.Select(rtlo => rtlo.TabTitle)
				.Where(name => nameIndexRegex.IsMatch(name))
				.Select(name => name.SubstringFrom(" ", true).ToInt32OrNull())
				.ExceptNull()
				.SortDescending()
				.ToArray();

			AddTab(new RtloModel("Right-to-Left Override " + (nameIndexes.Any() ? nameIndexes.First() + 1 : 1)));
		}
		private void ChangelogCommand_Execute()
		{
			HelpDialog.Show(@"App\Changelog");
		}
		private void AboutCommand_Execute()
		{
			if (new AboutDialog().ShowDialog() == true)
			{
				if (Tabs.OfType<LicenseModel>().FirstOrDefault() is LicenseModel license)
				{
					SelectedTab = license;
				}
				else
				{
					AddTab(new LicenseModel());
				}
			}
		}
		private void SelectPageCommand_Execute(PageTemplate page)
		{
			ProjectModel project = (ProjectModel)SelectedTab;

			switch (page)
			{
				case PageTemplate.Stub:
					project.Stub.IsSelected = true;
					break;
				case PageTemplate.Startup:
					project.Startup.IsSelected = true;
					break;
				case PageTemplate.VersionInfo:
					project.VersionInfo.IsSelected = true;
					break;
				case PageTemplate.Manifest:
					project.Manifest.IsSelected = true;
					break;
				default:
					throw new InvalidOperationException();
			}
		}
		private void NewRunPEItemCommand_Execute()
		{
			((ProjectModel)SelectedTab).NewRunPEItem();
		}
		private void NewInvokeItemCommand_Execute()
		{
			((ProjectModel)SelectedTab).NewInvokeItem();
		}
		private void NewDropItemCommand_Execute()
		{
			((ProjectModel)SelectedTab).NewDropItem();
		}
		private void NewMessageBoxItemCommand_Execute()
		{
			((ProjectModel)SelectedTab).NewMessageBoxItem();
		}
		private void MoveItemUpCommand_Execute()
		{
			ProjectModel project = (ProjectModel)SelectedTab;
			project.MoveItem((ProjectItemModel)project.SelectedPage, -1);
		}
		private bool MoveItemUpCommand_CanExecute()
		{
			return SelectedTab is ProjectModel project && project.SelectedPage is ProjectItemModel item && project.Items.IndexOf(item) > 0;
		}
		private void MoveItemDownCommand_Execute()
		{
			ProjectModel project = (ProjectModel)SelectedTab;
			project.MoveItem((ProjectItemModel)project.SelectedPage, 1);
		}
		private bool MoveItemDownCommand_CanExecute()
		{
			return SelectedTab is ProjectModel project && project.SelectedPage is ProjectItemModel item && project.Items.IndexOf(item) < project.Items.Count - 1;
		}
		private void FilePropertiesCommand_Execute()
		{
			string path = ((ProjectItemModel)((ProjectModel)SelectedTab).SelectedPage).SourceEmbeddedPath;

			if (File.Exists(path))
			{
				FileEx.ShowPropertiesDialog(path);
			}
			else
			{
				MessageBoxes.Error("File '" + Path.GetFileName(path) + "' not found.");
			}
		}
		private bool FilePropertiesCommand_CanExecute()
		{
			return SelectedTab is ProjectModel project && project.SelectedPage is ProjectItemModel item && item.Source == ProjectItemSource.Embedded && !item.SourceEmbeddedPath.IsNullOrEmpty();
		}
		private void OpenFileLocationCommand_Execute()
		{
			string path = Path.GetDirectoryName(((ProjectItemModel)((ProjectModel)SelectedTab).SelectedPage).SourceEmbeddedPath);

			if (Directory.Exists(path))
			{
				Process.Start(path);
			}
			else
			{
				MessageBoxes.Error("Directory '" + path + "' not found.");
			}
		}
		private bool OpenFileLocationCommand_CanExecute()
		{
			return SelectedTab is ProjectModel project && project.SelectedPage is ProjectItemModel item && item.Source == ProjectItemSource.Embedded && !item.SourceEmbeddedPath.IsNullOrEmpty();
		}
		private void RemoveItemCommand_Execute()
		{
			ProjectModel project = (ProjectModel)SelectedTab;
			project.RemoveItem((ProjectItemModel)project.SelectedPage);
		}
		private bool RemoveItemCommand_CanExecute()
		{
			return SelectedTab is ProjectModel project && project.SelectedPage is ProjectItemModel;
		}
		private void CopyStubIconCommand_Execute()
		{
			ProjectModel project = (ProjectModel)SelectedTab;
			ProjectItemModel[] items = project.Items
				.Where(item => item.Source == ProjectItemSource.Embedded)
				.Where(item => Path.GetExtension(item.SourceEmbeddedPath)?.Equals(".exe", StringComparison.OrdinalIgnoreCase) == true)
				.ToArray();

			if (items.Any())
			{
				SelectItemDialog dialog = new SelectItemDialog("Select an executable to copy the icon from.", "Select File", items);

				if (dialog.ShowDialog() == true)
				{
					if (File.Exists(dialog.SelectedItem.SourceEmbeddedPath))
					{
						if (IconExtractor.HasIcon(dialog.SelectedItem.SourceEmbeddedPath))
						{
							project.Stub.IconPath = dialog.SelectedItem.SourceEmbeddedPath;
						}
						else
						{
							MessageBoxes.Warning("File '" + Path.GetFileName(dialog.SelectedItem.SourceEmbeddedPath) + "' does not have an icon.");
						}
					}
					else
					{
						MessageBoxes.Error("File '" + Path.GetFileName(dialog.SelectedItem.SourceEmbeddedPath) + "' not found.");
					}
				}
			}
			else
			{
				MessageBoxes.Warning("This project does not contain executable files.\r\nAdd an executable to copy the icon from, or click 'Browse...' to select an executable file.");
			}
		}
		private void CopyVersionInfoFromItemCommand_Execute()
		{
			ProjectModel project = (ProjectModel)SelectedTab;
			ProjectItemModel[] items = project.Items
				.Where(item => item.Source == ProjectItemSource.Embedded)
				.Where(item => new[] { ".exe", ".dll" }.Any(e => Path.GetExtension(item.SourceEmbeddedPath)?.Equals(e, StringComparison.OrdinalIgnoreCase) == true))
				.ToArray();

			if (items.Any())
			{
				SelectItemDialog dialog = new SelectItemDialog("Select a file to copy the version information from.", "Select File", items);

				if (dialog.ShowDialog() == true)
				{
					project.CopyVersionInfo(dialog.SelectedItem.SourceEmbeddedPath);
				}
			}
			else
			{
				MessageBoxes.Warning("This project does not contain embedded files.\r\nAdd an item to copy the version information from.");
			}
		}
		private void CopyVersionInfoFromFileCommand_Execute()
		{
			if (FileDialogs.Open("exe") is string path)
			{
				((ProjectModel)SelectedTab).CopyVersionInfo(path);
			}
		}
		private void ManifestPreviewCommand_Execute()
		{
			ProjectModel project = (ProjectModel)SelectedTab;
			string fileName = project.Manifest.Template.GetDescription() + ".manifest";

			string path = Path.Combine(ApplicationBase.Path, "Stub");

			switch (project.Stub.Type)
			{
				case StubType.Pe32:
					path = Path.Combine(path, "pe32");
					break;
				case StubType.DotNet32:
				case StubType.DotNet64:
					path = Path.Combine(path, "dotnet");
					break;
				default:
					throw new InvalidEnumArgumentException();
			}

			path = Path.Combine(path, "Resources", fileName);

			if (File.Exists(path))
			{
				new TextDialog(fileName, File.ReadAllText(path)).ShowDialog();
			}
			else
			{
				MessageBoxes.Error("File '" + fileName + "' not found.");
			}
		}
		private void MessageBoxPreviewCommand_Execute(ProjectMessageBoxItemModel parameter)
		{
			System.Windows.Forms.MessageBox.Show
			(
				parameter.Text ?? "",
				parameter.Title ?? "",
				(System.Windows.Forms.MessageBoxButtons)parameter.Buttons,
				(System.Windows.Forms.MessageBoxIcon)parameter.Icon
			);
		}
		private void ShowErrorDetailsCommand_Execute(Error parameter)
		{
			new ErrorDetailsDialog(parameter).ShowDialog();
		}
		private void RtloSaveCommand_Execute(RtloModel parameter)
		{
			parameter.Save();
		}
		private bool RtloSaveCommand_CanExecute(RtloModel parameter)
		{
			return parameter?.IsValid == true;
		}

		private void Config_Recent_ProjectsChanged(object sender, EventArgs e)
		{
			RecentProjects = Config.Recent.Projects ?? new string[0];
		}
	}
}