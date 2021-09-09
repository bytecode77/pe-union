using System;

namespace PEunion.Compiler.UI
{
	/// <summary>
	/// Specifies the display name for UI components.
	/// </summary>
	public sealed class UiNameAttribute : Attribute
	{
		/// <summary>
		/// Gets the display name for UI components.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="UiNameAttribute" /> class.
		/// </summary>
		/// <param name="name">The display name for UI components.</param>
		public UiNameAttribute(string name)
		{
			Name = name;
		}
	}
}