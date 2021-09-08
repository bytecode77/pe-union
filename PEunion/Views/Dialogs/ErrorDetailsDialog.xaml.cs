using PEunion.Compiler.Errors;

namespace PEunion
{
	public partial class ErrorDetailsDialog
	{
		public ErrorDetailsDialogViewModel ViewModel { get; set; }

		public ErrorDetailsDialog(Error error)
		{
			ViewModel = new ErrorDetailsDialogViewModel(this, error);
			InitializeComponent();
		}
	}
}