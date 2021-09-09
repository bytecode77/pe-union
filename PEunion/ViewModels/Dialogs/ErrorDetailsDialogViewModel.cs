using BytecodeApi.Extensions;
using BytecodeApi.UI;
using PEunion.Compiler.Errors;

namespace PEunion
{
	public sealed class ErrorDetailsDialogViewModel : ViewModelBase
	{
		public ErrorDetailsDialog View { get; set; }

		private DelegateCommand _CloseCommand;
		public DelegateCommand CloseCommand => _CloseCommand ?? (_CloseCommand = new DelegateCommand(CloseCommand_Execute));

		public string Title { get; private set; }
		public Error Error { get; private set; }

		public ErrorDetailsDialogViewModel(ErrorDetailsDialog view, Error error)
		{
			View = view;
			Title = error.Severity.GetDescription();
			Error = error;
		}

		private void CloseCommand_Execute()
		{
			View.Close();
		}
	}
}