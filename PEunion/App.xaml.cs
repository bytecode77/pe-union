using BytecodeApi;
using BytecodeApi.UI;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PEunion
{
	public partial class App : Application
	{
		public static SingleInstance SingleInstance { get; private set; }

		public App()
		{
			SingleInstance = new SingleInstance("PEUNION_SINGLE_INSTANCE");
			if (SingleInstance.CheckInstanceRunning())
			{
				SingleInstance.SendActivationMessage();
				Shutdown();
			}
			else
			{
				ShutdownMode = ShutdownMode.OnMainWindowClose;
				new SplashScreen("Resources/Images/SplashScreen.png").Show(true, true);
			}

			SystemParametersHelper.Initialize();
		}
		private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			e.Handled = true;

			if (CSharp.Try(() => MainWindowViewModel.Singleton.IsInitialized))
			{
				if (new UnhandledExceptionDialog(e.Exception, true).ShowDialog() == true)
				{
					Shutdown();
				}
			}
			else
			{
				UnhandledExceptionDialog dialog = new UnhandledExceptionDialog(e.Exception, false);
				dialog.Show();
				dialog.Activate();
			}
		}

		public static ImageSource GetIcon(string name)
		{
			return new BitmapImage(new Uri("/PEunion;component/Resources/Icons/" + name + ".png", UriKind.Relative));
		}
	}
}