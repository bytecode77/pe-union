using BytecodeApi.UI.Data;

namespace PEunion
{
	public class ProjectItem : ObservableObject
	{
		public Project Project
		{
			get => Get(() => Project);
			set => Set(() => Project, value);
		}
		public bool TreeViewItemIsSelected
		{
			get => Get(() => TreeViewItemIsSelected);
			set => Set(() => TreeViewItemIsSelected, value);
		}

		public ProjectItem(Project project)
		{
			Project = project;
		}
	}
}