using BytecodeApi.Extensions;
using PEunion.Compiler.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PEunion.Compiler.Compiler
{
	/// <summary>
	/// Provides an assembly (.asm) file writer.
	/// </summary>
	public sealed class AssemblyStream : IDisposable
	{
		/// <summary>
		/// Gets the underlying stream that interfaces with a backing store.
		/// </summary>
		public StreamWriter BaseStream { get; private set; }
		/// <summary>
		/// Gets or sets the current indent in spaces.
		/// </summary>
		public int Indent { get; set; }
		/// <summary>
		/// Gets or sets the current indent for binary data names in spaces.
		/// </summary>
		public int BinaryDataNameIndent { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AssemblyStream" /> class.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		public AssemblyStream(Stream stream)
		{
			BaseStream = new StreamWriter(stream, Encoding.Default);
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="AssemblyStream" /> class.
		/// </summary>
		/// <param name="path">The path to a file to write to.</param>
		public AssemblyStream(string path) : this(File.Create(path))
		{
		}
		/// <summary>
		/// Closes the underlying stream and releases all resources used by the current instance of the <see cref="AssemblyStream" /> class.
		/// </summary>
		public void Dispose()
		{
			BaseStream.Dispose();
		}

		/// <summary>
		/// Writes a line terminator to the stream.
		/// </summary>
		public void WriteLine()
		{
			BaseStream.WriteLine();
		}
		/// <summary>
		/// Writes a <see cref="string" /> followed by a line terminator to the stream.
		/// </summary>
		/// <param name="str">The <see cref="string" /> to write.</param>
		public void WriteLine(string str)
		{
			BaseStream.WriteLine(str);
		}
		/// <summary>
		/// Emits the specified opcode.
		/// </summary>
		/// <param name="opcode">The assembly opcode.</param>
		public void Emit(string opcode)
		{
			BaseStream.WriteLine(opcode.TabIndent(Indent, 0));
		}
		/// <summary>
		/// Emits the specified opcode and parameters.
		/// </summary>
		/// <param name="opcode">The assembly opcode.</param>
		/// <param name="parameters">The parameters of <paramref name="opcode" />.</param>
		public void Emit(string opcode, string parameters)
		{
			BaseStream.WriteLine(opcode.TabIndent(Indent, 8) + parameters);
		}
		/// <summary>
		/// Emits an anonymous label:
		/// <para>@@:</para>
		/// </summary>
		public void EmitLabel()
		{
			BaseStream.WriteLine("@@:");
		}
		/// <summary>
		/// Emits a label:
		/// <para>.name:</para>
		/// </summary>
		/// <param name="name">The name of the label.</param>
		public void EmitLabel(string name)
		{
			BaseStream.WriteLine("." + name + ":", false);
		}
		/// <summary>
		/// Emits a label:
		/// <para>.name:</para>
		/// <para>or name:</para>
		/// </summary>
		/// <param name="name">The name of the label.</param>
		/// <param name="global"><see langword="true" /> to define a global label; <see langword="false" /> to define a local label with the dot prefix.</param>
		public void EmitLabel(string name, bool global)
		{
			BaseStream.WriteLine((global ? null : ".") + name + ":");
		}
		/// <summary>
		/// Emits a constant:
		/// <para>name = value</para>
		/// </summary>
		/// <param name="name">The name of the constant.</param>
		/// <param name="value">The value of the constant.</param>
		public void EmitConstant(string name, string value)
		{
			BaseStream.WriteLine((name + " = " + value).TabIndent(Indent, 0));
		}
		/// <summary>
		/// Emits an include statement:
		/// <para>include 'fileName'</para>
		/// </summary>
		/// <param name="fileName">The name or path of the file to include.</param>
		public void EmitInclude(string fileName)
		{
			BaseStream.WriteLine(("include '" + fileName + "'").TabIndent(Indent, 0));
		}
		/// <summary>
		/// Emits a definition, e.g. a RSRC directory definition:
		/// <para>
		/// title \<br />
		/// line1 \<br />
		/// line2
		/// </para>
		/// </summary>
		/// <param name="title">The title of the definition.</param>
		/// <param name="lines">A collection of lines.</param>
		public void EmitDefinition(string title, IEnumerable<string> lines)
		{
			EmitDefinition(title, lines.ToArray());
		}
		/// <summary>
		/// Emits a definition, e.g. a RSRC directory definition:
		/// <para>
		/// title \<br />
		/// line1 \<br />
		/// line2
		/// </para>
		/// </summary>
		/// <param name="title">The title of the definition.</param>
		/// <param name="lines">A collection of lines.</param>
		public void EmitDefinition(string title, params string[] lines)
		{
			BaseStream.WriteLine(title.TabIndent(Indent, 0) + @" \");

			for (int i = 0; i < lines.Length; i++)
			{
				BaseStream.WriteLine(lines[i].TabIndent(Indent + 4, 0) + (i < lines.Length - 1 ? @", \" : null));
			}
		}
		/// <summary>
		/// Emits a comment:
		/// <para>; comment</para>
		/// </summary>
		/// <param name="comment">The comment text.</param>
		public void EmitComment(string comment)
		{
			BaseStream.WriteLine(("; " + comment?.Replace("\r", @"\r").Replace("\n", @"\n")).TabIndent(Indent, 0));
		}
		/// <summary>
		/// Emits binary data:
		/// <para>name db 0x...</para>
		/// </summary>
		/// <param name="name">The name of the data.</param>
		/// <param name="data">A <see cref="byte" />[] with binary data.</param>
		/// <returns>
		/// The number of emitted bytes.
		/// </returns>
		public int EmitBinaryData(string name, byte[] data)
		{
			using (MemoryStream memoryStream = new MemoryStream(data))
			{
				return EmitBinaryData(name, memoryStream);
			}
		}
		/// <summary>
		/// Emits binary data:
		/// <para>name db 0x...</para>
		/// </summary>
		/// <param name="name">The name of the data.</param>
		/// <param name="stream">The stream to read the data from.</param>
		/// <returns>
		/// The number of emitted bytes.
		/// </returns>
		public int EmitBinaryData(string name, Stream stream)
		{
			byte[] buffer = new byte[16];
			int bytesRead;
			int totalBytesRead = 0;

			string title = (name + "\t").TabIndent(Indent, BinaryDataNameIndent);
			int indent = title.TabLength();
			BaseStream.Write(title + "db ");

			do
			{
				if ((bytesRead = stream.Read(buffer)) > 0)
				{
					if (totalBytesRead > 0)
					{
						BaseStream.Write("db ".TabIndent(indent, 0));
					}

					for (int i = 0; i < bytesRead; i++)
					{
						BaseStream.Write("0x" + buffer[i].ToString("x2"));
						if (i < bytesRead - 1) BaseStream.Write(", ");
					}

					BaseStream.WriteLine();
					totalBytesRead += bytesRead;
				}
			}
			while (bytesRead > 0);

			return totalBytesRead;
		}
		/// <summary>
		/// Emits a string as unicode binary data with a null terminator:
		/// <para>name db 0x..., 0x00, 0x00</para>
		/// </summary>
		/// <param name="name">The name of the string.</param>
		/// <param name="str">The string to emit as a unicode binary.</param>
		/// <returns>
		/// The number of emitted bytes.
		/// </returns>
		public int EmitStringData(string name, string str)
		{
			return EmitBinaryData(name, ((str ?? "") + '\0').ToUnicodeBytes());
		}
		/// <summary>
		/// Emits a file as a binary data include:
		/// <para>file 'path'</para>
		/// </summary>
		/// <param name="name">The name of the data.</param>
		/// <param name="path">The path of the file to be included.</param>
		public void EmitFileData(string name, string path)
		{
			BaseStream.Write(name.TabIndent(Indent, 0) + " file '" + path + "'");
		}
	}
}