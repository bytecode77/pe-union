using PEunion.Compiler.Project;

namespace PEunion
{
	public sealed class ProjectMessageBoxItemModel : ProjectItemModel
	{
		private string _Title;
		private string _Text;
		private MessageBoxIcon _Icon = MessageBoxIcon.None;
		private MessageBoxButtons _Buttons = MessageBoxButtons.Ok;
		private ActionEvent _OnOk = ActionEvent.None;
		private ActionEvent _OnCancel = ActionEvent.None;
		private ActionEvent _OnYes = ActionEvent.None;
		private ActionEvent _OnNo = ActionEvent.None;
		private ActionEvent _OnAbort = ActionEvent.None;
		private ActionEvent _OnRetry = ActionEvent.None;
		private ActionEvent _OnIgnore = ActionEvent.None;
		public string Title
		{
			get => _Title;
			set => Set(ref _Title, value);
		}
		public string Text
		{
			get => _Text;
			set => Set(ref _Text, value);
		}
		public MessageBoxIcon Icon
		{
			get => _Icon;
			set => Set(ref _Icon, value);
		}
		public MessageBoxButtons Buttons
		{
			get => _Buttons;
			set => Set(ref _Buttons, value);
		}
		public ActionEvent OnOk
		{
			get => _OnOk;
			set => Set(ref _OnOk, value);
		}
		public ActionEvent OnCancel
		{
			get => _OnCancel;
			set => Set(ref _OnCancel, value);
		}
		public ActionEvent OnYes
		{
			get => _OnYes;
			set => Set(ref _OnYes, value);
		}
		public ActionEvent OnNo
		{
			get => _OnNo;
			set => Set(ref _OnNo, value);
		}
		public ActionEvent OnAbort
		{
			get => _OnAbort;
			set => Set(ref _OnAbort, value);
		}
		public ActionEvent OnRetry
		{
			get => _OnRetry;
			set => Set(ref _OnRetry, value);
		}
		public ActionEvent OnIgnore
		{
			get => _OnIgnore;
			set => Set(ref _OnIgnore, value);
		}

		public ProjectMessageBoxItemModel() : base(PageTemplate.MessageBoxItem, "MessageBox")
		{
		}
	}
}