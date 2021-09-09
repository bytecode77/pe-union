using BytecodeApi.UI;
using System.Collections.Generic;

namespace PEunion
{
	public sealed class SelectItemDialogViewModel : ViewModelBase
	{
		public SelectItemDialog View { get; set; }

		private DelegateCommand _SelectItemCommand;
		private DelegateCommand _CancelCommand;
		public DelegateCommand SelectItemCommand => _SelectItemCommand ?? (_SelectItemCommand = new DelegateCommand(SelectItemCommand_Execute, SelectItemCommand_CanExecute));
		public DelegateCommand CancelCommand => _CancelCommand ?? (_CancelCommand = new DelegateCommand(CancelCommand_Execute));

		public string Text { get; private set; }
		public string ButtonText { get; private set; }
		public PageModel[] Items { get; private set; }
		public PageModel SelectedItem { get; set; }

		public SelectItemDialogViewModel(SelectItemDialog view, string text, string buttonText, IEnumerable<ProjectItemModel> items)
		{
			View = view;
			Text = text;
			ButtonText = buttonText;
			Items = new[] { new ItemsPage(items) { IsExpanded = true } };
		}

		private void SelectItemCommand_Execute()
		{
			View.DialogResult = true;
		}
		private bool SelectItemCommand_CanExecute()
		{
			return SelectedItem is ProjectItemModel;
		}
		private void CancelCommand_Execute()
		{
			View.Close();
		}
	}
}