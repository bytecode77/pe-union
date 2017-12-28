using BytecodeApi;
using System.Linq;

namespace PEunion
{
	public partial class TabRtlo : ObservableUserControl
	{
		private string _SourceFile;
		public string SourceFile
		{
			get => _SourceFile;
			set => Set(() => SourceFile, ref _SourceFile, value);
		}

		public TabRtlo()
		{
			InitializeComponent();
			DataContext = this;
		}

		private void ctrlBrowseSourceFile_FilesSelect(object sender, string[] e)
		{
			SourceFile = e.First();
		}
	}
}