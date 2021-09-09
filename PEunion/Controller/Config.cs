using BytecodeApi.Extensions;
using BytecodeApi.FileFormats.Ini;
using System;
using System.Linq;
using System.Windows;

namespace PEunion
{
	public static class Config
	{
		public static class ViewState
		{
			private static readonly ConfigFile ConfigFile;

			private static int? _WindowX;
			private static int? _WindowY;
			private static int? _WindowWidth;
			private static int? _WindowHeight;
			private static bool _WindowMaximized;
			private static int? _WindowSplitter1;
			private static int? _WindowSplitter2;
			private static int? _TextDialogWidth;
			private static int? _TextDialogHeight;
			private static int? _HelpDialogWidth;
			private static int? _HelpDialogHeight;
			public static int? WindowX
			{
				get => _WindowX;
				set
				{
					_WindowX = value;
					Save(true);
				}
			}
			public static int? WindowY
			{
				get => _WindowY;
				set
				{
					_WindowY = value;
					Save(true);
				}
			}
			public static int? WindowWidth
			{
				get => _WindowWidth;
				set
				{
					_WindowWidth = value;
					Save(true);
				}
			}
			public static int? WindowHeight
			{
				get => _WindowHeight;
				set
				{
					_WindowHeight = value;
					Save(true);
				}
			}
			public static bool WindowMaximized
			{
				get => _WindowMaximized;
				set
				{
					_WindowMaximized = value;
					Save(true);
				}
			}
			public static int? WindowSplitter1
			{
				get => _WindowSplitter1;
				set
				{
					_WindowSplitter1 = value;
					Save(true);
				}
			}
			public static int? WindowSplitter2
			{
				get => _WindowSplitter2;
				set
				{
					_WindowSplitter2 = value;
					Save(true);
				}
			}
			public static int? TextDialogWidth
			{
				get => _TextDialogWidth;
				set
				{
					_TextDialogWidth = value;
					Save(true);
				}
			}
			public static int? TextDialogHeight
			{
				get => _TextDialogHeight;
				set
				{
					_TextDialogHeight = value;
					Save(true);
				}
			}
			public static int? HelpDialogWidth
			{
				get => _HelpDialogWidth;
				set
				{
					_HelpDialogWidth = value;
					Save(true);
				}
			}
			public static int? HelpDialogHeight
			{
				get => _HelpDialogHeight;
				set
				{
					_HelpDialogHeight = value;
					Save(true);
				}
			}

			static ViewState()
			{
				ConfigFile = new ConfigFile("viewstate.ini");

				if (ConfigFile.Load() is IniFile ini)
				{
					if (ini.HasSection("window"))
					{
						IniSection section = ini.Sections["window"];
						if (section.HasProperty("x")) WindowX = section.Properties["x"].Int32Value;
						if (section.HasProperty("y")) WindowY = section.Properties["y"].Int32Value;
						if (section.HasProperty("width")) WindowWidth = section.Properties["width"].Int32Value;
						if (section.HasProperty("height")) WindowHeight = section.Properties["height"].Int32Value;
						if (section.HasProperty("maximized")) WindowMaximized = section.Properties["maximized"].Value == "true";
						if (section.HasProperty("splitter1")) WindowSplitter1 = section.Properties["splitter1"].Int32Value;
						if (section.HasProperty("splitter2")) WindowSplitter2 = section.Properties["splitter2"].Int32Value;
					}

					if (ini.HasSection("text_dialog"))
					{
						IniSection section = ini.Sections["text_dialog"];
						if (section.HasProperty("width")) TextDialogWidth = section.Properties["width"].Int32Value;
						if (section.HasProperty("height")) TextDialogHeight = section.Properties["height"].Int32Value;
					}

					if (ini.HasSection("help_dialog"))
					{
						IniSection section = ini.Sections["help_dialog"];
						if (section.HasProperty("width")) HelpDialogWidth = section.Properties["width"].Int32Value;
						if (section.HasProperty("height")) HelpDialogHeight = section.Properties["height"].Int32Value;
					}
				}

				Application.Current.Exit += delegate { Save(false); };
			}

			private static void Save(bool deferred)
			{
				ConfigFile.Save(deferred, () =>
				{
					IniFile ini = new IniFile();

					IniSection windowSection = new IniSection("window");
					if (WindowX != null) windowSection.Properties.Add(new IniProperty("x", WindowX.ToString()));
					if (WindowY != null) windowSection.Properties.Add(new IniProperty("y", WindowY.ToString()));
					if (WindowWidth != null) windowSection.Properties.Add(new IniProperty("width", WindowWidth.ToString()));
					if (WindowHeight != null) windowSection.Properties.Add(new IniProperty("height", WindowHeight.ToString()));
					windowSection.Properties.Add(new IniProperty("maximized", WindowMaximized ? "true" : "false"));
					if (WindowSplitter1 != null) windowSection.Properties.Add(new IniProperty("splitter1", WindowSplitter1.ToString()));
					if (WindowSplitter2 != null) windowSection.Properties.Add(new IniProperty("splitter2", WindowSplitter2.ToString()));
					ini.Sections.Add(windowSection);

					IniSection textDialogSection = new IniSection("text_dialog");
					if (TextDialogWidth != null) textDialogSection.Properties.Add(new IniProperty("width", TextDialogWidth.ToString()));
					if (TextDialogHeight != null) textDialogSection.Properties.Add(new IniProperty("height", TextDialogHeight.ToString()));
					ini.Sections.Add(textDialogSection);

					IniSection helpDialogSection = new IniSection("help_dialog");
					if (HelpDialogWidth != null) helpDialogSection.Properties.Add(new IniProperty("width", HelpDialogWidth.ToString()));
					if (HelpDialogHeight != null) helpDialogSection.Properties.Add(new IniProperty("height", HelpDialogHeight.ToString()));
					ini.Sections.Add(helpDialogSection);

					return ini;
				});
			}
		}
		public static class Recent
		{
			private static readonly ConfigFile ConfigFile;

			private static string[] _Projects;
			public static string[] Projects
			{
				get => _Projects;
				set
				{
					_Projects = value;
					Save(true);
					ProjectsChanged?.Invoke(null, EventArgs.Empty);
				}
			}
			public static event EventHandler ProjectsChanged;

			static Recent()
			{
				ConfigFile = new ConfigFile("recent.ini");

				if (ConfigFile.Load() is IniFile ini)
				{
					if (ini.HasSection("projects"))
					{
						IniSection section = ini.Sections["projects"];
						Projects = section.Properties.Select(property => property.Value).ToArray();
					}
				}

				Application.Current.Exit += delegate { Save(false); };
			}

			public static void AddProject(string path)
			{
				Projects = new[] { path }
					.Concat(Projects?.Where(recent => !recent.Equals(path, StringComparison.OrdinalIgnoreCase)) ?? new string[0])
					.Take(10)
					.ToArray();
			}
			private static void Save(bool deferred)
			{
				ConfigFile.Save(deferred, () =>
				{
					IniFile ini = new IniFile();

					IniSection projectsSection = new IniSection("projects");
					if (Projects != null) projectsSection.Properties.AddRange(Projects.Select((project, i) => new IniProperty((i + 1).ToString(), project)));
					ini.Sections.Add(projectsSection);

					return ini;
				});
			}
		}
	}
}