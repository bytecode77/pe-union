using System;

namespace PEunion
{
	public partial class UnhandledExceptionDialog
	{
		public UnhandledExceptionDialogViewModel ViewModel { get; set; }

		public UnhandledExceptionDialog(Exception exception, bool canContinue)
		{
			ViewModel = new UnhandledExceptionDialogViewModel(this, exception, canContinue);
			InitializeComponent();
		}
	}
}