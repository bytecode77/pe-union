using BytecodeApi.UI.Controls;
using System.Diagnostics;
using System.Windows;

namespace PEunion
{
	public partial class WindowAbout : ObservableWindow
	{
		public string License => "\r\n\r\n" + Properties.Resources.FileLicense + "\r\n\r\n";

		public WindowAbout(Window owner)
		{
			InitializeComponent();
			DataContext = this;
			Owner = owner;
		}

		private void lnkWebsite_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("https://bytecode77.com");
		}
		private void lnkGitHubUser_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("https://github.com/bytecode77");
		}
		private void lnkGitHub_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("https://github.com/bytecode77/pe-union");
		}
	}
}