using BytecodeApi.UI;
using BytecodeApi.UI.Extensions;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PEunion
{
	public class TreeViewEx : TreeView
	{
		public static readonly DependencyProperty SelectedItemBindingProperty = DependencyPropertyEx.Register(nameof(SelectedItemBinding));
		public object SelectedItemBinding
		{
			get => GetValue(SelectedItemBindingProperty);
			set => SetValue(SelectedItemBindingProperty, value);
		}

		protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
		{
			base.OnSelectedItemChanged(e);

			SelectedItemBinding = SelectedItem;
		}
		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);

			if (((DependencyObject)e.OriginalSource).GetParents(UITreeType.Logical).OfType<TextDisplay>().FirstOrDefault() is TextDisplay textDisplay &&
				textDisplay.FindParent<TreeViewItem>(UITreeType.Visual) is TreeViewItem item)
			{
				item.Focus();
				e.Handled = true;
			}
		}
	}
}