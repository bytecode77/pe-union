using BytecodeApi;
using System.Windows.Forms;

namespace PEunion
{
	public class ProjectMessageBox : ProjectItem
	{
		private string _Title;
		private string _Text;
		private MessageBoxButtons _Buttons;
		private MessageBoxIcon _Icon;

		public string Title
		{
			get => _Title;
			set
			{
				Set(() => Title, ref _Title, value);
				Project.IsDirty = true;
				RaisePropertyChanged(() => TreeViewItemText);
			}
		}
		public string Text
		{
			get => _Text;
			set
			{
				Set(() => Text, ref _Text, value);
				Project.IsDirty = true;
			}
		}
		public MessageBoxButtons Buttons
		{
			get => _Buttons;
			set
			{
				Set(() => Buttons, ref _Buttons, value);
				Project.IsDirty = true;
			}
		}
		public MessageBoxIcon Icon
		{
			get => _Icon;
			set
			{
				Set(() => Icon, ref _Icon, value);
				Project.IsDirty = true;
				RaisePropertyChanged(() => IsIconNone);
				RaisePropertyChanged(() => IsIconInformation);
				RaisePropertyChanged(() => IsIconWarning);
				RaisePropertyChanged(() => IsIconError);
				RaisePropertyChanged(() => IsIconConfirmation);
			}
		}

		public string TreeViewItemText => "Message Box" + (Title.IsNullOrWhiteSpace() ? null : ": " + Wording.TrimText(Title.Trim(), 20));
		public bool IsIconNone
		{
			get => Icon == MessageBoxIcon.None;
			set => Icon = MessageBoxIcon.None;
		}
		public bool IsIconInformation
		{
			get => Icon == MessageBoxIcon.Information;
			set => Icon = MessageBoxIcon.Information;
		}
		public bool IsIconWarning
		{
			get => Icon == MessageBoxIcon.Warning;
			set => Icon = MessageBoxIcon.Warning;
		}
		public bool IsIconError
		{
			get => Icon == MessageBoxIcon.Error;
			set => Icon = MessageBoxIcon.Error;
		}
		public bool IsIconConfirmation
		{
			get => Icon == MessageBoxIcon.Question;
			set => Icon = MessageBoxIcon.Question;
		}

		public ProjectMessageBox(Project project) : base(project)
		{
			Buttons = MessageBoxButtons.OK;
			Icon = MessageBoxIcon.Information;
		}
	}
}