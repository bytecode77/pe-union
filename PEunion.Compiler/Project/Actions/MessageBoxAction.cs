using BytecodeApi;
using System.ComponentModel;

namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines an action that displays a MessageBox.
	/// </summary>
	public sealed class MessageBoxAction : ProjectAction
	{
		/// <summary>
		/// Gets or sets the title of the MessageBox.
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// Gets or sets the text of the MessageBox.
		/// </summary>
		public string Text { get; set; }
		/// <summary>
		/// Gets or sets the MessageBox icon.
		/// </summary>
		public MessageBoxIcon Icon { get; set; }
		/// <summary>
		/// Gets or sets the MessageBox buttons.
		/// </summary>
		public MessageBoxButtons Buttons { get; set; }
		/// <summary>
		/// Gets or sets the <see cref="ActionEvent" /> that is raised, when the "Ok" button is clicked.
		/// </summary>
		public ActionEvent OnOk { get; set; }
		/// <summary>
		/// Gets or sets the <see cref="ActionEvent" /> that is raised, when the "Cancel" button is clicked.
		/// </summary>
		public ActionEvent OnCancel { get; set; }
		/// <summary>
		/// Gets or sets the <see cref="ActionEvent" /> that is raised, when the "Yes" button is clicked.
		/// </summary>
		public ActionEvent OnYes { get; set; }
		/// <summary>
		/// Gets or sets the <see cref="ActionEvent" /> that is raised, when the "No" button is clicked.
		/// </summary>
		public ActionEvent OnNo { get; set; }
		/// <summary>
		/// Gets or sets the <see cref="ActionEvent" /> that is raised, when the "Abort" button is clicked.
		/// </summary>
		public ActionEvent OnAbort { get; set; }
		/// <summary>
		/// Gets or sets the <see cref="ActionEvent" /> that is raised, when the "Retry" button is clicked.
		/// </summary>
		public ActionEvent OnRetry { get; set; }
		/// <summary>
		/// Gets or sets the <see cref="ActionEvent" /> that is raised, when the "Ignore" button is clicked.
		/// </summary>
		public ActionEvent OnIgnore { get; set; }
		/// <summary>
		/// Gets a <see cref="bool" /> value indicating whether this <see cref="MessageBoxAction" /> defines any events.
		/// </summary>
		public bool HasEvents =>
			OnOk != ActionEvent.None ||
			OnCancel != ActionEvent.None ||
			OnYes != ActionEvent.None ||
			OnNo != ActionEvent.None ||
			OnAbort != ActionEvent.None ||
			OnRetry != ActionEvent.None ||
			OnIgnore != ActionEvent.None;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageBoxAction" /> class.
		/// </summary>
		public MessageBoxAction()
		{
			Icon = MessageBoxIcon.None;
			Buttons = MessageBoxButtons.Ok;
			OnOk = ActionEvent.None;
			OnCancel = ActionEvent.None;
			OnYes = ActionEvent.None;
			OnNo = ActionEvent.None;
			OnAbort = ActionEvent.None;
			OnRetry = ActionEvent.None;
			OnIgnore = ActionEvent.None;
		}

		/// <summary>
		/// Determines whether the specified set of <see cref="MessageBoxButtons" /> can raise the specified <see cref="MessageBoxEvent" />.
		/// </summary>
		/// <param name="buttons">A <see cref="MessageBoxButtons" /> value.</param>
		/// <param name="e">A <see cref="MessageBoxEvent" /> event.</param>
		/// <returns>
		/// <see langword="true" />, if the specified set of <see cref="MessageBoxButtons" /> can raise the specified <see cref="MessageBoxEvent" />;
		/// otherwise, <see langword="false" />.
		/// </returns>
		public static bool HasEvent(MessageBoxButtons buttons, MessageBoxEvent e)
		{
			switch (e)
			{
				case MessageBoxEvent.Ok: return CSharp.EqualsAny(buttons, MessageBoxButtons.Ok, MessageBoxButtons.OkCancel);
				case MessageBoxEvent.Cancel: return CSharp.EqualsAny(buttons, MessageBoxButtons.OkCancel, MessageBoxButtons.YesNoCancel, MessageBoxButtons.RetryCancel);
				case MessageBoxEvent.Yes: return CSharp.EqualsAny(buttons, MessageBoxButtons.YesNo, MessageBoxButtons.YesNoCancel);
				case MessageBoxEvent.No: return CSharp.EqualsAny(buttons, MessageBoxButtons.YesNo, MessageBoxButtons.YesNoCancel);
				case MessageBoxEvent.Abort: return CSharp.EqualsAny(buttons, MessageBoxButtons.AbortRetryIgnore);
				case MessageBoxEvent.Retry: return CSharp.EqualsAny(buttons, MessageBoxButtons.AbortRetryIgnore, MessageBoxButtons.RetryCancel);
				case MessageBoxEvent.Ignore: return CSharp.EqualsAny(buttons, MessageBoxButtons.AbortRetryIgnore);
				default: throw new InvalidEnumArgumentException();
			}
		}
	}
}