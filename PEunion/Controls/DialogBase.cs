using BytecodeApi;
using BytecodeApi.UI.Controls;
using System.Windows;

namespace PEunion
{
	public class DialogBase : ObservableWindow
	{
		public DialogBase()
		{
			CSharp.Try(() => Owner = MainWindow.Singleton);
			UseLayoutRounding = true;
			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			ResizeMode = ResizeMode.NoResize;
			ShowInTaskbar = false;
		}
	}
}