using PEunion.Compiler.UI;
using System.ComponentModel;

namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines the icon of a MessageBox.
	/// </summary>
	public enum MessageBoxIcon
	{
		/// <summary>
		/// The MessageBox has no icon.
		/// </summary>
		[Description("none")]
		[UiName("None")]
		[UiSortOrder(1)]
		None = 0,
		/// <summary>
		/// The MessageBox has a lower case i letter in a blue circle.
		/// </summary>
		[Description("information")]
		[UiName("Information")]
		[UiSortOrder(2)]
		Information = 0x40,
		/// <summary>
		/// The MessageBox has a questionmark in a blue circle.
		/// </summary>
		[Description("confirmation")]
		[UiName("Confirmation")]
		[UiSortOrder(3)]
		Confirmation = 0x20,
		/// <summary>
		/// The MessageBox has an exclamation mark in a yellow triangle.
		/// </summary>
		[Description("warning")]
		[UiName("Warning")]
		[UiSortOrder(4)]
		Warning = 0x30,
		/// <summary>
		/// The MessageBox has an X letter in a red circle.
		/// </summary>
		[Description("error")]
		[UiName("Error")]
		[UiSortOrder(5)]
		Error = 0x10
	}
}