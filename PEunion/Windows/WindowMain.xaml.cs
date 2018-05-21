using BytecodeApi;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PEunion
{
	public partial class WindowMain : ObservableWindow
	{
		public static readonly RoutedUICommand NewCommand = new RoutedUICommand("New", "NewCommand", typeof(WindowMain));
		public static readonly RoutedUICommand OpenCommand = new RoutedUICommand("Open", "OpenCommand", typeof(WindowMain));
		public static readonly RoutedUICommand SaveCommand = new RoutedUICommand("Save", "SaveCommand", typeof(WindowMain));
		public static readonly RoutedUICommand SaveAsCommand = new RoutedUICommand("SaveAs", "SaveAsCommand", typeof(WindowMain));
		public static readonly RoutedUICommand BuildCommand = new RoutedUICommand("Build", "BuildCommand", typeof(WindowMain));
		public static readonly RoutedUICommand BuildCodeCommand = new RoutedUICommand("BuildCode", "BuildCodeCommand", typeof(WindowMain));
		public static readonly RoutedUICommand MoveSelectedProjectItemCommand = new RoutedUICommand("MoveSelectedProjectItem", "MoveSelectedProjectItemCommand", typeof(WindowMain));
		public static readonly RoutedUICommand DeleteSelectedProjectItemCommand = new RoutedUICommand("DeleteSelectedProjectItem", "DeleteSelectedProjectItemCommand", typeof(WindowMain));
		public static WindowMain Singleton { get; private set; }

		public string OverlayTitle
		{
			get => Get(() => OverlayTitle);
			set => Set(() => OverlayTitle, value);
		}
		public bool OverlayIsIndeterminate
		{
			get => Get(() => OverlayIsIndeterminate);
			set => Set(() => OverlayIsIndeterminate, value);
		}
		public double OverlayProgress
		{
			get => Get(() => OverlayProgress);
			set => Set(() => OverlayProgress, value);
		}
		public Project Project
		{
			get => Get(() => Project);
			set
			{
				if (Project != null) Project.ValidationErrorsChanged -= Project_ValidationErrorsChanged;
				Set(() => Project, value);
				value.ValidationErrorsChanged += Project_ValidationErrorsChanged;
			}
		}
		public ProjectItem SelectedProjectItem
		{
			get => Get(() => SelectedProjectItem);
			set => Set(() => SelectedProjectItem, value);
		}
		public ValidationError[] ValidationErrors
		{
			get => Get(() => ValidationErrors);
			set => Set(() => ValidationErrors, value);
		}
		public string[] RecentProjects
		{
			get
			{
				string path = Path.Combine(App.ApplicationDirectoryPath, "RecentProjects.txt");
				if (File.Exists(path)) return File.ReadAllLines(path).Where(line => line != "").Take(10).ToArray();
				else return new string[0];
			}
			set
			{
				string path = Path.Combine(App.ApplicationDirectoryPath, "RecentProjects.txt");
				if (value == null)
				{
					File.Delete(path);
				}
				else if (value.Length == 1 && value.First().StartsWith("-"))
				{
					File.WriteAllLines(path, RecentProjects.Except(value.First().Substring(1).CreateSingletonArray()).ToArray());
				}
				else
				{
					File.WriteAllLines(path, value.Union(RecentProjects).Take(10).ToArray());
				}

				RaisePropertyChanged(() => RecentProjectMenuItems);
			}
		}
		public string[] RecentFiles
		{
			get
			{
				string path = Path.Combine(App.ApplicationDirectoryPath, "RecentFiles.txt");
				if (File.Exists(path)) return File.ReadAllLines(path).Where(line => line != "").Take(10).ToArray();
				else return new string[0];
			}
			set
			{
				string path = Path.Combine(App.ApplicationDirectoryPath, "RecentFiles.txt");
				if (value == null)
				{
					File.Delete(path);
				}
				else if (value.Length == 1 && value.First().StartsWith("-"))
				{
					File.WriteAllLines(path, RecentFiles.Except(value.First().Substring(1).CreateSingletonArray()).ToArray());
				}
				else
				{
					File.WriteAllLines(path, value.Union(RecentFiles).Take(10).ToArray());
				}

				RaisePropertyChanged(() => RecentFileMenuItems);
			}
		}
		public Control[] RecentProjectMenuItems => RecentProjects
			.Select((file, i) => new MenuItem
			{
				Header = (i == 9 ? "1_0" : "_" + (i + 1)) + " " + file,
				DataContext = file
			} as Control)
			.Concat(new Separator())
			.Concat(new MenuItem
			{
				Header = "_Clear Recent Project List",
				Icon = new Image { Source = Utility.GetImageResource("IconClearRecents") },
				DataContext = ":Clear",
				IsEnabled = RecentProjects.Length > 0
			})
			.ToArray();
		public Control[] RecentFileMenuItems => RecentFiles
			.Select((file, i) => new MenuItem
			{
				Header = (i == 9 ? "1_0" : "_" + (i + 1)) + " " + file,
				DataContext = file
			} as Control)
			.Concat(new Separator())
			.Concat(new MenuItem
			{
				Header = "_Clear Recent File List",
				Icon = new Image { Source = Utility.GetImageResource("IconClearRecents") },
				DataContext = ":Clear",
				IsEnabled = RecentFiles.Length > 0
			})
			.ToArray();

		private ContextMenu mnuTreeItems => this.FindResource<ContextMenu>("mnuTreeItems");
		private ContextMenu mnuTreeFile => this.FindResource<ContextMenu>("mnuTreeFile");
		private ContextMenu mnuTreeUrl => this.FindResource<ContextMenu>("mnuTreeUrl");
		private ContextMenu mnuTreeMessageBox => this.FindResource<ContextMenu>("mnuTreeMessageBox");

		public WindowMain()
		{
			InitializeComponent();

			Singleton = this;
			MessageBoxes.Window = this;
			DataContext = this;
			treMain.Focus();
			NewCommand.Execute(null, this);

			string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();
			if (args.Length > 0)
			{
				if (File.Exists(args[0])) LoadProject(args[0]);
				else MessageBoxes.Warning("'" + args[0] + "' not found.");

				if (args.Length > 1)
				{
					MessageBoxes.Warning("Cannot open multiple project files.");
				}
			}
		}
		private void Window_SourceInitialized(object sender, EventArgs e)
		{
			treMainItems.IsSelected = true;
		}
		private void Window_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = Project.IsDirty && !ConfirmSaveChanges();
		}

		private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (ConfirmSaveChanges())
			{
				Project = new Project();
				SelectedProjectItem = null;
				UpdateValidationErrorList();
			}
		}
		private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (ConfirmSaveChanges())
			{
				string path = Dialogs.Open("peu".CreateSingletonArray(), "PEunion Project Files");
				if (path != null) LoadProject(path);
			}
		}
		private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			FileSave();
		}
		private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			FileSaveAs();
		}
		private async void BuildCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (PrepareBuild())
			{
				string path = Dialogs.Save(Project.ProjectName, "exe");
				if (path != null)
				{
					ctrlOverlay.Show();
					CompilerResults result = await Builder.BuildAsync(path, Project);
					ctrlOverlay.Hide();

					if (result.Errors.Count > 0)
					{
						ValidationErrors = result.Errors
							.Cast<CompilerError>()
							.Select(error => ValidationError.CreateCompileError(error.Line, error.ErrorNumber + ": " + error.ErrorText))
							.ToArray();

						MessageBoxes.Error(result.Errors.Count + " compilation errors.");
					}
				}
			}
		}
		private async void BuildCodeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (PrepareBuild())
			{
				string path = Dialogs.Save(Project.ProjectName, "cs");
				if (path != null)
				{
					ctrlOverlay.Show();
					string code = await Builder.BuildCodeAsync(Project);
					File.WriteAllText(path, code);
					ctrlOverlay.Hide();
				}
			}
		}
		private void MoveSelectedProjectItem_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ProjectItem item = SelectedProjectItem;
			int index = Project.Items.IndexOf(item);
			Project.Items.Remove(item);
			Project.Items.Insert(index + (int)e.Parameter, item);
			item.TreeViewItemIsSelected = true;
			Project.IsDirty = true;
		}
		private void MoveSelectedProjectItem_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			int newIndex = Project.Items.IndexOf(SelectedProjectItem) + (int)e.Parameter;
			e.CanExecute = newIndex >= 0 && newIndex < Project.Items.Count;
		}
		private void DeleteSelectedProjectItem_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			string name;
			if (SelectedProjectItem is ProjectFile file) name = file.Name.IsNullOrWhiteSpace() ? file.SourceFileName : "'" + file.Name + "'";
			else if (SelectedProjectItem is ProjectMessageBox) name = "this Message Box";
			else if (SelectedProjectItem is ProjectUrl url) name = url.Url.IsNullOrWhiteSpace() ? "this URL" : "'" + url.Url + "'";
			else throw new InvalidOperationException();

			if (MessageBoxes.Confirmation("Do you want to delete " + name + "?", true))
			{
				int index = Project.Items.IndexOf(SelectedProjectItem);
				Project.Items.Remove(SelectedProjectItem);
				Project.IsDirty = true;

				if (Project.Items.Count == 0) SelectedProjectItem = null;
				else Project.Items[Math.Min(index, Project.Items.Count - 1)].TreeViewItemIsSelected = true;
			}
		}
		private void mnuFileOpenProjectFolder_Click(object sender, RoutedEventArgs e)
		{
			Process.Start(Path.GetDirectoryName(Project.SaveLocation));
		}
		private void mnuFileRecentProject_Click(object sender, RoutedEventArgs e)
		{
			string path = (sender as FrameworkElement).DataContext as string;
			if (path == ":Clear")
			{
				if (MessageBoxes.Confirmation("Do you want to clear recent projects?", true)) RecentProjects = null;
			}
			else if (File.Exists(path))
			{
				if (ConfirmSaveChanges()) LoadProject(path);
			}
			else if (path != null)
			{
				if (MessageBoxes.Confirmation("'" + path + "' not found.\r\nDo you want to remove it from recent projects?", true, true))
				{
					RecentProjects = ("-" + path).CreateSingletonArray();
				}
			}
		}
		private void mnuFileRecentFile_Click(object sender, RoutedEventArgs e)
		{
			string path = (sender as FrameworkElement).DataContext as string;
			if (path == ":Clear")
			{
				if (MessageBoxes.Confirmation("Do you want to clear recent files?", true)) RecentFiles = null;
			}
			else if (File.Exists(path))
			{
				AddFiles(path.CreateSingletonArray());
			}
			else if (path != null)
			{
				if (MessageBoxes.Confirmation("'" + path + "' not found.\r\nDo you want to remove it from recent files?", true, true))
				{
					RecentFiles = ("-" + path).CreateSingletonArray();
				}
			}
		}
		private void mnuFileExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		private void mnuToolsOpenAppDataDirectory_Click(object sender, RoutedEventArgs e)
		{
			Process.Start(App.ApplicationDirectoryPath);
		}
		private void mnuToolsRegisterFileExtension_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBoxes.Confirmation("This will register the .peu file extension for the installation directory\r\n'" + ApplicationBase.StartupPath + "'.\r\nProceed?", false, true))
			{
				string iconPath = Path.Combine(App.ApplicationDirectoryPath, "ShellIcon.ico");
				File.WriteAllBytes(iconPath, Properties.Resources.FileShellIcon);

				TempDirectory.ExecuteFile
				(
					"PEunion_RegisterFileExtension.reg",
					Properties.Resources.FileRegisterExtension
						.Replace("{IconPath}", iconPath.Replace(@"\", @"\\"))
						.Replace("{ApplicationPath}", ApplicationBase.ExecutablePath.Replace(@"\", @"\\"))
						.ToAnsi()
				);
			}
		}
		private void mnuToolsRtlo_Click(object sender, RoutedEventArgs e)
		{
			AddTab("Right to Left Override", new TabRtlo(), "Rtlo");
		}
		private void mnuHelpGitHub_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("https://github.com/bytecode77/pe-union");
		}
		private void mnuHelpAbout_Click(object sender, RoutedEventArgs e)
		{
			new WindowAbout(this).ShowDialog();
		}
		private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//TODO: Workaround for App.xaml style lacking "Foreground" support for TextBlock
			foreach (TabItem tab in tabMain.Items)
			{
				tab.FindLogicalChildren<TextBlock>().First().Foreground = tabMain.SelectedItem == tab ? Brushes.Black : Brushes.White;
			}
		}
		private void tabMain_Close_Click(object sender, RoutedEventArgs e)
		{
			tabMain.Items.Remove((sender as FrameworkElement).FindParent<TabItem>(UITreeType.Logical));
		}
		private void tabMain_TabItem_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Middle)
			{
				if ((sender as TabItem) == tabMain.Items[0]) NewCommand.Execute(null, this);
				else tabMain.Items.Remove(sender);
			}
		}
		private void treMain_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			IEnumerable<Grid> grids = grdTabs.Children.OfType<Grid>();
			grids.ForEach(grid => grid.Hide());

			if (e.NewValue is TreeViewItem treeViewItem)
			{
				ShowGridTab(treeViewItem.Tag as string);
				SelectedProjectItem = null;
			}
			else if (e.NewValue is ProjectFile file)
			{
				SelectedProjectItem = file;
				ShowGridTab("ProjectFile");
			}
			else if (e.NewValue is ProjectUrl url)
			{
				SelectedProjectItem = url;
				ShowGridTab("ProjectUrl");
			}
			else if (e.NewValue is ProjectMessageBox messageBox)
			{
				SelectedProjectItem = messageBox;
				ShowGridTab("ProjectMessageBox");
			}

			void ShowGridTab(string tab) => grids.First(child => child.Tag as string == tab).Show();
		}
		private void treMain_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			if ((e.OriginalSource as FrameworkElement)?.DataContext is ProjectItem item)
			{
				if (item is ProjectFile) treMain.ContextMenu = mnuTreeFile;
				else if (item is ProjectUrl) treMain.ContextMenu = mnuTreeUrl;
				else if (item is ProjectMessageBox) treMain.ContextMenu = mnuTreeMessageBox;
				else treMain.ContextMenu = mnuTreeItems;

				item.TreeViewItemIsSelected = true;
				return;
			}
			else
			{
				TreeViewItem treeViewItem = (e.Source as FrameworkElement).FindParent<TreeViewItem>(UITreeType.Logical);
				if (treeViewItem != null)
				{
					treeViewItem.IsSelected = true;
					treMain.ContextMenu = mnuTreeItems;
					return;
				}
			}

			e.Handled = true;
		}
		private void treMain_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Delete && SelectedProjectItem != null) DeleteSelectedProjectItemCommand.Execute(null, this);
		}
		private void mnuTreeItemsAddFiles_Click(object sender, RoutedEventArgs e)
		{
			string[] files = Dialogs.OpenMultiple();
			if (files != null) AddFiles(files);
		}
		private void mnuTreeItemsAddUrl_Click(object sender, RoutedEventArgs e)
		{
			AddUrl();
		}
		private void mnuTreeItemsAddMessageBox_Click(object sender, RoutedEventArgs e)
		{
			AddMessageBox();
		}
		private void mnuTreeFileOpenSourceDirectory_Click(object sender, RoutedEventArgs e)
		{
			Process.Start((SelectedProjectItem as ProjectFile).SourceDirectory);
		}
		private void mnuTreeFileProperties_Click(object sender, RoutedEventArgs e)
		{
			FileEx.ShowPropertiesDialog((SelectedProjectItem as ProjectFile).FullName);
		}
		private void mnuTreeMessageBoxPreview_Click(object sender, RoutedEventArgs e)
		{
			MessageBoxPreview();
		}
		private void ctrlBrowseIcon_FilesSelect(object sender, string[] e)
		{
			Project.IconPath = e.First();
		}
		private void ctrlBrowseIcon_Reset(object sender, EventArgs e)
		{
			Project.IconPath = null;
		}
		private void btnSelectAssemblyInfo_Click(object sender, RoutedEventArgs e)
		{
			string path = Dialogs.Open();
			if (path != null)
			{
				FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(path);
				Project.AssemblyTitle = fileInfo.FileDescription?.Trim();
				Project.AssemblyProduct = fileInfo.ProductName?.Trim();
				Project.AssemblyCopyright = fileInfo.LegalCopyright?.Trim();
				Project.AssemblyVersion = fileInfo.ProductVersion?.Trim();
			}
		}
		private void ctrlAddFiles_FilesSelect(object sender, string[] e)
		{
			AddFiles(e);
		}
		private void btnAddUrl_Click(object sender, RoutedEventArgs e)
		{
			AddUrl();
		}
		private void btnAddMessageBox_Click(object sender, RoutedEventArgs e)
		{
			AddMessageBox();
		}
		private void btnPreviewMessageBox_Click(object sender, RoutedEventArgs e)
		{
			MessageBoxPreview();
		}
		private void btnShowValidationErrors_CheckedUnchecked(object sender, RoutedEventArgs e)
		{
			UpdateValidationErrorList();
		}
		private void Project_ValidationErrorsChanged(object sender, EventArgs e)
		{
			UpdateValidationErrorList();
		}

		private bool FileSave()
		{
			if (Project.SaveLocation == null)
			{
				return FileSaveAs();
			}
			else
			{
				Project.Save();
				RecentProjects = Project.SaveLocation.CreateSingletonArray();
				return true;
			}
		}
		private bool FileSaveAs()
		{
			string path = Dialogs.Save(Project.ProjectName, "peu");
			if (path != null)
			{
				Project.SaveLocation = path;
				return FileSave();
			}
			else
			{
				return false;
			}
		}
		private bool ConfirmSaveChanges()
		{
			if (Project?.IsDirty == true)
			{
				switch (MessageBoxes.ConfirmationWithCancel("Do you want to save changes to '" + Project.ProjectName + "'?", true))
				{
					case true: return FileSave();
					case false: return true;
					default: return false;
				}
			}
			else
			{
				return true;
			}
		}
		private void LoadProject(string path)
		{
			Project = Project.Load(path);
			RecentProjects = path.CreateSingletonArray();
			UpdateValidationErrorList();
		}
		private bool PrepareBuild()
		{
			Project.ValidateBuild();
			UpdateValidationErrorList();
			if (Project.ValidationErrorCount > 0)
			{
				Desktop.Beep(false);
				return false;
			}
			else
			{
				return true;
			}
		}
		private void UpdateValidationErrorList()
		{
			ValidationErrors = Project?.ValidationErrors
				.Where(error => btnShowValidationErrors.IsChecked == true || error.Type != ValidationErrorType.Error)
				.Where(error => btnShowValidationWarnings.IsChecked == true || error.Type != ValidationErrorType.Warning)
				.Where(error => btnShowValidationMessages.IsChecked == true || error.Type != ValidationErrorType.Message)
				.ToArray();
		}
		private void AddFiles(string[] files)
		{
			Project.Items.AddRange(files.Select(file => new ProjectFile(Project, file)));
			Project.IsDirty = true;
			RecentFiles = files;
		}
		private void AddUrl()
		{
			ProjectUrl url = new ProjectUrl(Project);
			Project.Items.Add(url);
			Project.IsDirty = true;
			url.TreeViewItemIsSelected = true;
		}
		private void AddMessageBox()
		{
			ProjectMessageBox messageBox = new ProjectMessageBox(Project);
			Project.Items.Add(messageBox);
			Project.IsDirty = true;
			messageBox.TreeViewItemIsSelected = true;
		}
		private void MessageBoxPreview()
		{
			ProjectMessageBox messageBox = SelectedProjectItem as ProjectMessageBox;
			System.Windows.Forms.MessageBox.Show(messageBox.Text ?? "", messageBox.Title ?? "", messageBox.Buttons, messageBox.Icon);
		}
		private void AddTab(string header, object content, string icon)
		{
			StackPanel headerControl = new StackPanel { Orientation = Orientation.Horizontal };
			Button closeButton = new Button { Style = Application.Current.FindResource<Style>("CloseButton") };
			closeButton.Click += tabMain_Close_Click;

			headerControl.Children.Add(new Image { Source = Utility.GetImageResource("Icon" + icon), Margin = new Thickness(0, 0, 5, 0) });
			headerControl.Children.Add(new TextDisplay { Text = header });
			headerControl.Children.Add(closeButton);

			tabMain.Items.Add(new TabItem
			{
				Header = headerControl,
				Content = content,
				IsSelected = true
			});
		}
	}
}