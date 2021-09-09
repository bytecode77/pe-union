using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace PEunion
{
	public static class SystemParametersHelper
	{
		public static void Initialize()
		{
			UpdateProperties();
			SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
		}

		private static void UpdateProperties()
		{
			try
			{
				if (SystemParameters.MenuDropAlignment)
				{
					typeof(SystemParameters)
						.GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static)
						.SetValue(null, false);
				}
			}
			catch
			{
			}
		}

		private static void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			UpdateProperties();
		}
	}
}