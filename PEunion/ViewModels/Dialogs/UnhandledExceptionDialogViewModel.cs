using BytecodeApi.Extensions;
using BytecodeApi.UI;
using System;
using System.Diagnostics;
using System.Windows;

namespace PEunion
{
	public sealed class UnhandledExceptionDialogViewModel : ViewModelBase
	{
		public UnhandledExceptionDialog View { get; set; }

		private DelegateCommand _ReportIssueCommand;
		private DelegateCommand _ContinueCommand;
		private DelegateCommand _ExitCommand;
		private DelegateCommand _WindowClosedCommand;
		public DelegateCommand ReportIssueCommand => _ReportIssueCommand ?? (_ReportIssueCommand = new DelegateCommand(ReportIssueCommand_Execute));
		public DelegateCommand ContinueCommand => _ContinueCommand ?? (_ContinueCommand = new DelegateCommand(ContinueCommand_Execute));
		public DelegateCommand ExitCommand => _ExitCommand ?? (_ExitCommand = new DelegateCommand(ExitCommand_Execute));
		public DelegateCommand WindowClosedCommand => _WindowClosedCommand ?? (_WindowClosedCommand = new DelegateCommand(WindowClosedCommand_Execute));

		public Exception Exception { get; private set; }
		public bool CanContinue { get; private set; }
		public string StackTrace { get; private set; }

		public UnhandledExceptionDialogViewModel(UnhandledExceptionDialog view, Exception exception, bool canContinue)
		{
			View = view;
			Exception = exception;
			CanContinue = canContinue;
			StackTrace = exception.GetFullStackTrace();
		}

		private void ReportIssueCommand_Execute()
		{
			Process.Start("https://github.com/bytecode77/pe-union/issues");
		}
		private void ContinueCommand_Execute()
		{
			View.Close();
		}
		private void ExitCommand_Execute()
		{
			if (CanContinue)
			{
				View.DialogResult = true;
			}
			else
			{
				Application.Current.Shutdown();
			}
		}
		private void WindowClosedCommand_Execute()
		{
			if (!CanContinue)
			{
				Application.Current.Shutdown();
			}
		}
	}
}