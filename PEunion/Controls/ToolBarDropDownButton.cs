using BytecodeApi.UI;
using BytecodeApi.UI.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace PEunion
{
	public class ToolBarDropDownButton : ToggleButton
	{
		public static readonly DependencyProperty DropDownMenuProperty = DependencyPropertyEx.Register(nameof(DropDownMenu), new PropertyMetadata(DropDownMenu_Change));
		public ContextMenu DropDownMenu
		{
			get => this.GetValue<ContextMenu>(DropDownMenuProperty);
			set => SetValue(DropDownMenuProperty, value);
		}

		private static void DropDownMenu_Change(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolBarDropDownButton control = (ToolBarDropDownButton)d;

			if (e.OldValue is ContextMenu oldDropDownMenu)
			{
				oldDropDownMenu.Closed -= control.DropDownMenu_Closed;
			}

			if (e.NewValue is ContextMenu newDropDownMenu)
			{
				newDropDownMenu.Placement = PlacementMode.Bottom;
				newDropDownMenu.PlacementTarget = control;
				newDropDownMenu.Closed += control.DropDownMenu_Closed;
			}
		}
		private void DropDownMenu_Closed(object sender, RoutedEventArgs e)
		{
			IsChecked = false;
		}
		protected override void OnChecked(RoutedEventArgs e)
		{
			base.OnChecked(e);

			//TODO: Bug: When clicking the ToggleButton when the ContextMenu is open, the ContextMenu closes and immediately re-opens
			if (DropDownMenu != null) DropDownMenu.IsOpen = true;
		}
	}
}