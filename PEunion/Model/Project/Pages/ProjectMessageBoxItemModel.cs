using PEunion.Compiler.Project;

namespace PEunion
{
	public sealed class ProjectMessageBoxItemModel : ProjectItemModel
	{
		public string Title
		{
			get => Get(() => Title);
			set => Set(() => Title, value);
		}
		public string Text
		{
			get => Get(() => Text);
			set => Set(() => Text, value);
		}
		public MessageBoxIcon Icon
		{
			get => Get(() => Icon, MessageBoxIcon.None);
			set => Set(() => Icon, value);
		}
		public MessageBoxButtons Buttons
		{
			get => Get(() => Buttons, MessageBoxButtons.Ok);
			set => Set(() => Buttons, value);
		}
		public ActionEvent OnOk
		{
			get => Get(() => OnOk, ActionEvent.None);
			set => Set(() => OnOk, value);
		}
		public ActionEvent OnCancel
		{
			get => Get(() => OnCancel, ActionEvent.None);
			set => Set(() => OnCancel, value);
		}
		public ActionEvent OnYes
		{
			get => Get(() => OnYes, ActionEvent.None);
			set => Set(() => OnYes, value);
		}
		public ActionEvent OnNo
		{
			get => Get(() => OnNo, ActionEvent.None);
			set => Set(() => OnNo, value);
		}
		public ActionEvent OnAbort
		{
			get => Get(() => OnAbort, ActionEvent.None);
			set => Set(() => OnAbort, value);
		}
		public ActionEvent OnRetry
		{
			get => Get(() => OnRetry, ActionEvent.None);
			set => Set(() => OnRetry, value);
		}
		public ActionEvent OnIgnore
		{
			get => Get(() => OnIgnore, ActionEvent.None);
			set => Set(() => OnIgnore, value);
		}

		public ProjectMessageBoxItemModel() : base(PageTemplate.MessageBoxItem, "MessageBox")
		{
		}
	}
}