using BytecodeApi;
using BytecodeApi.UI;
using BytecodeApi.UI.Data;
using BytecodeApi.UI.Dialogs;
using System.Diagnostics;
using System.IO;

namespace PEunion
{
	public abstract class ViewModelBase : ObservableObject
	{
		private DelegateCommand<string> _OpenFileCommand;
		private DelegateCommand<string> _OpenUrlCommand;
		public DelegateCommand<string> OpenFileCommand => _OpenFileCommand ?? (_OpenFileCommand = new DelegateCommand<string>(OpenFileCommand_Execute));
		public DelegateCommand<string> OpenUrlCommand => _OpenUrlCommand ?? (_OpenUrlCommand = new DelegateCommand<string>(OpenUrlCommand_Execute));

		private void OpenFileCommand_Execute(string parameter)
		{
			string path = Path.Combine(ApplicationBase.Path, parameter);

			if (File.Exists(path) || Directory.Exists(path))
			{
				Process.Start(path);
			}
			else
			{
				MessageBoxes.Error("File '" + Path.GetFileName(path) + "' not found.");
			}
		}
		private void OpenUrlCommand_Execute(string parameter)
		{
			Process.Start(parameter);
		}
	}
}