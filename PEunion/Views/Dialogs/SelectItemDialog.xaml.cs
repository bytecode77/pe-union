using System.Collections.Generic;
using System.Windows.Input;

namespace PEunion
{
	public partial class SelectItemDialog
	{
		public SelectItemDialogViewModel ViewModel { get; set; }

		public ProjectItemModel SelectedItem => ViewModel.SelectedItem is ProjectItemModel item ? item : null;

		public SelectItemDialog(string text, string buttonText, IEnumerable<ProjectItemModel> items)
		{
			ViewModel = new SelectItemDialogViewModel(this, text, buttonText, items);
			InitializeComponent();
		}

		private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (DialogResult == null && SelectedItem is ProjectItemModel)
			{
				ViewModel.SelectItemCommand.Execute();
			}
		}
	}
}