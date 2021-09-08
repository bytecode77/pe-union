using BytecodeApi;
using BytecodeApi.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace PEunion.Compiler.Errors
{
	/// <summary>
	/// Defines a collection of errors that can occur during compilation or project file parsing.
	/// </summary>
	public sealed class ErrorCollection : Collection<Error>
	{
		/// <summary>
		/// Gets a <see cref="bool" /> value indicating whether this collection contains errors with the <see cref="ErrorSeverity.Error" /> severity.
		/// </summary>
		public bool HasErrors => this.Any(error => error.Severity == ErrorSeverity.Error);

		/// <summary>
		/// Reads an <see cref="ErrorCollection" /> from a file that was written using the <see cref="ToFile(string)" /> method.
		/// </summary>
		/// <param name="path">The path to the file to read.</param>
		/// <returns>
		/// A new <see cref="ErrorCollection" /> read from the specified file.
		/// </returns>
		public static ErrorCollection FromFile(string path)
		{
			ErrorCollection errors = new ErrorCollection();
			string[] lines = File.ReadAllLines(path);

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];

				if (line.StartsWith("[") && line.Contains("]"))
				{
					ErrorSource source;
					ErrorSeverity severity;
					string message;
					string details;

					if (EnumEx.FindValueByDescription<ErrorSource>(line.SubstringFrom("[").SubstringUntil("]").Trim()) is ErrorSource src) source = src;
					else FormatError();

					line = line.SubstringFrom("]");

					if (line.Contains(":") && EnumEx.FindValueByDescription<ErrorSeverity>(line.SubstringUntil(":").Trim()) is ErrorSeverity sev) severity = sev;
					else FormatError();

					message = UnscapeString(line.SubstringFrom(":").Trim());

					if (i < lines.Length - 1 && lines[i + 1].StartsWith("Details:"))
					{
						details = UnscapeString(lines[i + 1].TrimStartString("Details:").Trim().ToNullIfEmptyOrWhiteSpace());
						i++;
					}
					else
					{
						details = null;
					}

					errors.Add(source, severity, message, details);
				}
				else
				{
					FormatError();
				}

				void FormatError()
				{
					throw new FormatException("Failed to parse error file at line " + (i + 1) + ".");
				}
			}

			return errors;

			string UnscapeString(string str) => str?.Replace(@"{\r}", "\r").Replace(@"{\n}", "\n");
		}

		/// <summary>
		/// Adds an error to this collection.
		/// </summary>
		/// <param name="source">The source of the error.</param>
		/// <param name="severity">The severity of the error.</param>
		/// <param name="message">The error message.</param>
		public void Add(ErrorSource source, ErrorSeverity severity, string message)
		{
			Add(new Error(source, severity, message));
		}
		/// <summary>
		/// Adds an error to this collection.
		/// </summary>
		/// <param name="source">The source of the error.</param>
		/// <param name="severity">The severity of the error.</param>
		/// <param name="message">The error message.</param>
		/// <param name="details">Optional details about the error.</param>
		public void Add(ErrorSource source, ErrorSeverity severity, string message, string details)
		{
			Add(new Error(source, severity, message, details));
		}

		/// <summary>
		/// Outputs a formatted version of this <see cref="ErrorCollection" /> to the console.
		/// </summary>
		public void WriteToConsole()
		{
			try
			{
				foreach (Error error in this)
				{
					switch (error.Severity)
					{
						case ErrorSeverity.Message:
							Console.ForegroundColor = ConsoleColor.Black;
							Console.BackgroundColor = ConsoleColor.White;
							break;
						case ErrorSeverity.Warning:
							Console.ForegroundColor = ConsoleColor.Black;
							Console.BackgroundColor = ConsoleColor.Yellow;
							break;
						case ErrorSeverity.Error:
							Console.ForegroundColor = ConsoleColor.White;
							Console.BackgroundColor = ConsoleColor.DarkRed;
							break;
						default:
							throw new InvalidEnumArgumentException();
					}

					Console.Write(error.Severity.GetDescription() + ":");
					Console.ResetColor();

					switch (error.Severity)
					{
						case ErrorSeverity.Message:
							Console.ForegroundColor = ConsoleColor.White;
							break;
						case ErrorSeverity.Warning:
							Console.ForegroundColor = ConsoleColor.Yellow;
							break;
						case ErrorSeverity.Error:
							Console.ForegroundColor = ConsoleColor.Red;
							break;
						default:
							throw new InvalidEnumArgumentException();
					}

					Console.WriteLine(" " + error.Message);
					if (!error.Details.IsNullOrEmpty()) Console.WriteLine(error.Details);
					Console.ResetColor();
				}
			}
			finally
			{
				Console.ResetColor();
			}
		}
		/// <summary>
		/// Writes this collection to a file.
		/// </summary>
		/// <param name="path">The path to the file to write.</param>
		public void ToFile(string path)
		{
			if (this.Any())
			{
				using (StreamWriter file = File.CreateText(path))
				{
					foreach (Error error in this)
					{
						file.Write("[" + error.Source.GetDescription() + "] ");
						file.Write(error.Severity.GetDescription() + ": ");
						file.WriteLine(EscapeString(error.Message));

						if (!error.Details.IsNullOrEmpty()) file.WriteLine("Details: " + EscapeString(error.Details));
					}
				}
			}
			else if (File.Exists(path))
			{
				File.Delete(path);
			}

			string EscapeString(string str) => str?.Replace("\r", @"{\r}").Replace("\n", @"{\n}");
		}
	}
}