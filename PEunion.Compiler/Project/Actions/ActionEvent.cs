using PEunion.Compiler.UI;
using System.ComponentModel;

namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines what action is taken on an event that is raised by specific types of actions, such as the <see cref="MessageBoxAction" />.
	/// </summary>
	public enum ActionEvent
	{
		/// <summary>
		/// No action is taken.
		/// </summary>
		[Description("none")]
		[UiName("Do nothing")]
		None,
		/// <summary>
		/// The next action is skipped.
		/// </summary>
		[Description("skip_next_action")]
		[UiName("Skip next action")]
		SkipNextAction,
		/// <summary>
		/// The stub exits.
		/// </summary>
		[Description("exit")]
		[UiName("Exit")]
		Exit
	}
}