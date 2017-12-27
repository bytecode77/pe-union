using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace PEunion
{
	public partial class App : Application
	{
		public static readonly string Version = Assembly.GetEntryAssembly().GetName().Version.ToString(3);
		public static string ApplicationDirectoryPath
		{
			get
			{
				string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PEunion");
				Directory.CreateDirectory(path);
				return path;
			}
		}

		static App()
		{
			AppDomain.CurrentDomain.AssemblyResolve += AppDomain_ResolveAssembly;
		}
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(0));
			ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(int.MaxValue));
		}
		private static Assembly AppDomain_ResolveAssembly(object sender, ResolveEventArgs e)
		{
			return new AssemblyName(e.Name).Name == "BytecodeApi" ? Assembly.Load(PEunion.Properties.Resources.BytecodeApi) : null;
		}
	}
}