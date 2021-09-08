using PEunion.Compiler.UI;
using System.ComponentModel;

namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines the buttons of a MessageBox.
	/// </summary>
	public enum MessageBoxButtons
	{
		/// <summary>
		/// The message box has an OK button.
		/// </summary>
		[Description("ok")]
		[UiName("Ok")]
		[UiSortOrder(1)]
		Ok = 0,
		/// <summary>
		/// The message box has an OK and Cancel button.
		/// </summary>
		[Description("okcancel")]
		[UiName("Ok Cancel")]
		[UiSortOrder(2)]
		OkCancel = 1,
		/// <summary>
		/// The message box has a Yes and No button.
		/// </summary>
		[Description("yesno")]
		[UiName("Yes No")]
		[UiSortOrder(3)]
		YesNo = 4,
		/// <summary>
		/// The message box has a Yes, No and Cancel button.
		/// </summary>
		[Description("yesnocancel")]
		[UiName("Yes No Cancel")]
		[UiSortOrder(4)]
		YesNoCancel = 3,
		/// <summary>
		/// The message box has an Abort, Retry and Ignore button.
		/// </summary>
		[Description("abortretryignore")]
		[UiName("Abort Retry Ignore")]
		[UiSortOrder(5)]
		AbortRetryIgnore = 2,
		/// <summary>
		/// The message box has a Retry and Cancel button.
		/// </summary>
		[Description("retrycancel")]
		[UiName("Retry Cancel")]
		[UiSortOrder(6)]
		RetryCancel = 5
	}
}