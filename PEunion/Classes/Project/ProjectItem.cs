using BytecodeApi;

namespace PEunion
{
	public class ProjectItem : ObservableObject
	{
		private Project _Project;
		private bool _TreeViewItemIsSelected;
		public Project Project
		{
			get => _Project;
			set
			{
				Set(() => Project, ref _Project, value);
			}
		}
		public bool TreeViewItemIsSelected
		{
			get => _TreeViewItemIsSelected;
			set => Set(() => TreeViewItemIsSelected, ref _TreeViewItemIsSelected, value);
		}

		public ProjectItem(Project project)
		{
			Project = project;
		}
	}
}