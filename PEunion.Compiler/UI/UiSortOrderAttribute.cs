using System;

namespace PEunion.Compiler.UI
{
	/// <summary>
	/// Specifies the sort order for item lists, such as combo boxes in UI components.
	/// </summary>
	public sealed class UiSortOrderAttribute : Attribute
	{
		/// <summary>
		/// Gets the sort order for item lists, such as combo boxes in UI components.
		/// </summary>
		public int SortOrder { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="UiSortOrderAttribute" /> class.
		/// </summary>
		/// <param name="sortOrder">The sort order for item lists, such as combo boxes in UI components.</param>
		public UiSortOrderAttribute(int sortOrder)
		{
			SortOrder = sortOrder;
		}
	}
}