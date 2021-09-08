using BytecodeApi.Threading;
using BytecodeApi.UI;
using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace PEunion
{
	public sealed class BuildProgressDialogViewModel : ViewModelBase
	{
		public BuildProgressDialog View { get; set; }

		private DelegateCommand _WindowClosingCommand;
		private DelegateCommand _CancelCommand;
		public DelegateCommand WindowClosingCommand => _WindowClosingCommand ?? (_WindowClosingCommand = new DelegateCommand(WindowClosingCommand_Execute));
		public DelegateCommand CancelCommand => _CancelCommand ?? (_CancelCommand = new DelegateCommand(CancelCommand_Execute));

		private readonly DispatcherTimer Timer;
		private readonly Process Process;
		private bool Canceled;
		public string OutputFileName { get; private set; }
		public int? ExitCode { get; private set; }

		public BuildProgressDialogViewModel(BuildProgressDialog view, string outputFileName, Process process)
		{
			View = view;
			OutputFileName = outputFileName;
			Process = process;

			Process.Start();
			Timer = ThreadFactory.StartDispatcherTimer(Timer_Tick, TimeSpan.FromMilliseconds(10));
		}

		private void WindowClosingCommand_Execute()
		{
			if (ExitCode == null && !Canceled)
			{
				Timer.Stop();
				Canceled = true;

				if (!Process.HasExited) Process.Kill();
				Process.Dispose();
			}
		}
		private void CancelCommand_Execute()
		{
			View.Close();
		}
		private void Timer_Tick()
		{
			if (Process.HasExited)
			{
				Timer.Stop();
				ExitCode = Process.ExitCode;

				Process.Dispose();
				View.Close();
			}
		}
	}
}