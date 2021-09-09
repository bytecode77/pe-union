namespace PEunion
{
	public partial class AboutDialog
	{
		public AboutDialogViewModel ViewModel { get; set; }

		public AboutDialog()
		{
			ViewModel = new AboutDialogViewModel(this);
			InitializeComponent();
		}
	}
}