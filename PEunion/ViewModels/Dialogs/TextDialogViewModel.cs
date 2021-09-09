using BytecodeApi.UI;

namespace PEunion
{
	public sealed class TextDialogViewModel : ViewModelBase
	{
		public TextDialog View { get; set; }

		private DelegateCommand _CloseCommand;
		public DelegateCommand CloseCommand => _CloseCommand ?? (_CloseCommand = new DelegateCommand(CloseCommand_Execute));

		public string Title { get; private set; }
		public string Text { get; private set; }

		public TextDialogViewModel(TextDialog view, string title, string text)
		{
			View = view;
			Title = title;
			Text = text;
		}

		private void CloseCommand_Execute()
		{
			View.Close();
		}
	}
}