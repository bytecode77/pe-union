using PEunion.Compiler.UI;
using System.ComponentModel;

namespace PEunion.Compiler.Project
{
	/// <summary>
	/// Defines the type of the stub.
	/// </summary>
	public enum StubType
	{
		/// <summary>
		/// The stub is a 32-bit native executable written in FASM.
		/// </summary>
		[Description("pe32")]
		[UiName("Native (32-bit)")]
		Pe32,
		/// <summary>
		/// The stub is a 32-bit MSIL executable written in C#.
		/// </summary>
		[Description("net32")]
		[UiName(".NET (32-bit)")]
		DotNet32,
		/// <summary>
		/// The stub is a 64-bit MSIL executable written in C#.
		/// </summary>
		[Description("net64")]
		[UiName(".NET (64-bit)")]
		DotNet64
	}
}