using System.Collections.Generic;

namespace PEunion
{
	public sealed class ProjectPage : PageModel
	{
		public ProjectPage(IEnumerable<PageModel> subPages) : base(PageTemplate.Project, "Project", subPages)
		{
		}
	}
}