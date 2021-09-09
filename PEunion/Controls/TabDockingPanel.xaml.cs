using BytecodeApi.UI;
using BytecodeApi.UI.Extensions;
using System.Windows;

namespace PEunion
{
	public partial class TabDockingPanel
	{
		public static readonly DependencyProperty SelectedIndexProperty = DependencyPropertyEx.Register(nameof(SelectedIndex));
		public int SelectedIndex
		{
			get => this.GetValue<int>(SelectedIndexProperty);
			set => SetValue(SelectedIndexProperty, value);
		}

		public TabDockingPanel()
		{
			InitializeComponent();
		}
	}
}