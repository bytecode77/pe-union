using BytecodeApi.UI;

namespace PEunion
{
	public sealed class AboutDialogViewModel : ViewModelBase
	{
		public AboutDialog View { get; set; }

		private DelegateCommand _LicenseCommand;
		private DelegateCommand _CloseCommand;
		public DelegateCommand LicenseCommand => _LicenseCommand ?? (_LicenseCommand = new DelegateCommand(LicenseCommand_Execute));
		public DelegateCommand CloseCommand => _CloseCommand ?? (_CloseCommand = new DelegateCommand(CloseCommand_Execute));

		public AboutDialogViewModel(AboutDialog view)
		{
			View = view;
		}

		private void LicenseCommand_Execute()
		{
			View.DialogResult = true;
		}
		private void CloseCommand_Execute()
		{
			View.Close();
		}
	}
}