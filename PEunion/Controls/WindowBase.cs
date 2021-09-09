using BytecodeApi.UI;
using BytecodeApi.UI.Controls;
using BytecodeApi.UI.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shell;

namespace PEunion
{
	public class WindowBase : ObservableWindow
	{
		public WindowBase()
		{
			UseLayoutRounding = true;
			MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 2; //TODO: Bug: Does not work with multiple screens, or if task bar is not at the bottom
			Template = this.FindResource<ControlTemplate>("WindowBaseTemplate");

			WindowChrome.SetWindowChrome(this, new WindowChrome
			{
				CaptionHeight = 30,
				ResizeBorderThickness = new Thickness(8)
			});
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			WindowChrome.SetResizeGripDirection(this.FindChild<ResizeGrip>(UITreeType.Visual, child => true), ResizeGripDirection.BottomRight);
		}
	}
}