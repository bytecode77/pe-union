using System;

namespace PEunion
{
	public partial class SaveChangesDialog
	{
		public SaveChangesDialogViewModel ViewModel { get; set; }

		public SaveChangesDialog(string[] itemNames, Func<bool> saveCallback)
		{
			ViewModel = new SaveChangesDialogViewModel(this, itemNames, saveCallback);
			InitializeComponent();
		}
	}
}