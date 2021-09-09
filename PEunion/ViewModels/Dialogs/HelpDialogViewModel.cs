using BytecodeApi.UI;

namespace PEunion
{
	public sealed class HelpDialogViewModel : ViewModelBase
	{
		public HelpDialog View { get; set; }

		private DelegateCommand _CloseCommand;
		public DelegateCommand CloseCommand => _CloseCommand ?? (_CloseCommand = new DelegateCommand(CloseCommand_Execute));

		public string Html { get; private set; }

		public HelpDialogViewModel(HelpDialog view, string html)
		{
			View = view;
			Html = html;
		}

		private void CloseCommand_Execute()
		{
			View.Close();
		}
	}
}