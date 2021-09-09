using System;

namespace PEunion.Compiler.Errors
{
	/// <summary>
	/// Defines an exception with an <see cref="Error" /> that terminates the current parsing or compilation operation.
	/// </summary>
	public sealed class ErrorException : Exception
	{
		/// <summary>
		/// Gets optional details about the error.
		/// </summary>
		public string Details { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ErrorException" /> class.
		/// </summary>
		/// <param name="message">The error message.</param>
		public ErrorException(string message) : base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="ErrorException" /> class.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <param name="details">Optional details about the error.</param>
		public ErrorException(string message, string details) : base(message)
		{
			Details = details;
		}
	}
}