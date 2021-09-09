using System.Diagnostics;

namespace PEunion
{
	public partial class BuildProgressDialog
	{
		public BuildProgressDialogViewModel ViewModel { get; set; }

		public int? ExitCode => ViewModel.ExitCode;

		public BuildProgressDialog(string outputFileName, Process process)
		{
			ViewModel = new BuildProgressDialogViewModel(this, outputFileName, process);
			InitializeComponent();
		}
	}
}