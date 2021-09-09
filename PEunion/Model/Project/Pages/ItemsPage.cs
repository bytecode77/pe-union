using System.Collections.Generic;

namespace PEunion
{
	public sealed class ItemsPage : PageModel
	{
		public ItemsPage(IEnumerable<PageModel> subPages) : base(PageTemplate.Items, "Items", subPages)
		{
		}
	}
}