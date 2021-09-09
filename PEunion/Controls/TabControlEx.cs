using BytecodeApi.UI;
using BytecodeApi.UI.Extensions;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PEunion
{
	public class TabControlEx : TabControl
	{
		public static readonly DependencyProperty CloseCommandProperty = DependencyPropertyEx.Register(nameof(CloseCommand));
		public static readonly DependencyProperty IsTabControlFocusedProperty = DependencyProperty.RegisterAttached("IsTabControlFocused", typeof(bool), typeof(TabControlEx), new PropertyMetadata(false));
		public ICommand CloseCommand
		{
			get => this.GetValue<ICommand>(CloseCommandProperty);
			set => SetValue(CloseCommandProperty, value);
		}
		public static object GetIsTabControlFocused(DependencyObject dependencyObject)
		{
			return dependencyObject.GetValue(IsTabControlFocusedProperty);
		}
		public static void SetIsTabControlFocused(DependencyObject dependencyObject, object value)
		{
			dependencyObject.SetValue(IsTabControlFocusedProperty, value);
		}

		private void SetIsTabControlFocused(bool isTabControlFocused)
		{
			foreach (TabItem tabItem in Items.Cast<object>().Select(item => (TabItem)ItemContainerGenerator.ContainerFromItem(item)))
			{
				SetIsTabControlFocused(tabItem, isTabControlFocused);
			}
		}

		protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnGotKeyboardFocus(e);

			SetIsTabControlFocused(true);
		}
		protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnLostKeyboardFocus(e);

			SetIsTabControlFocused(false);
		}
		protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseUp(e);

			if (e.ChangedButton == MouseButton.Middle && ((DependencyObject)e.OriginalSource).FindParent<TabItem>(UITreeType.Visual) != null)
			{
				CloseCommand?.Execute(UIContext.Find<object>(e.OriginalSource));
			}
		}
	}
}