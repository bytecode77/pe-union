using BytecodeApi.UI;
using System;

namespace PEunion
{
	public sealed class SaveChangesDialogViewModel : ViewModelBase
	{
		public SaveChangesDialog View { get; set; }

		private DelegateCommand _SaveCommand;
		private DelegateCommand _DontSaveCommand;
		private DelegateCommand _CancelCommand;
		public DelegateCommand SaveCommand => _SaveCommand ?? (_SaveCommand = new DelegateCommand(SaveCommand_Execute));
		public DelegateCommand DontSaveCommand => _DontSaveCommand ?? (_DontSaveCommand = new DelegateCommand(DontSaveCommand_Execute));
		public DelegateCommand CancelCommand => _CancelCommand ?? (_CancelCommand = new DelegateCommand(CancelCommand_Execute));

		private readonly Func<bool> SaveCallback;
		public string[] ItemNames { get; private set; }

		public SaveChangesDialogViewModel(SaveChangesDialog view, string[] itemNames, Func<bool> saveCallback)
		{
			View = view;
			ItemNames = itemNames;
			SaveCallback = saveCallback;
		}

		private void SaveCommand_Execute()
		{
			View.DialogResult = SaveCallback();
		}
		private void DontSaveCommand_Execute()
		{
			View.DialogResult = true;
		}
		private void CancelCommand_Execute()
		{
			View.Close();
		}
	}
}