using BytecodeApi.UI;
using BytecodeApi.UI.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace PEunion
{
	public sealed class TabDockingPanelItem : ContentControl
	{
		public static readonly DependencyProperty HeaderProperty = DependencyPropertyEx.Register(nameof(Header));
		public string Header
		{
			get => this.GetValue<string>(HeaderProperty);
			set => SetValue(HeaderProperty, value);
		}
	}
}