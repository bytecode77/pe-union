using PEunion.Compiler.Helper;
using System;
using System.IO;
using System.Text;

namespace PEunion.Compiler.Compiler
{
	/// <summary>
	/// Provides a C# file writer.
	/// </summary>
	public sealed class CSharpStream : IDisposable
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
		/// Initializes a new instance of the <see cref="CSharpStream" /> class.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		public CSharpStream(Stream stream)
		{
			BaseStream = new StreamWriter(stream, Encoding.UTF8);
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="CSharpStream" /> class.
		/// </summary>
		/// <param name="path">The path to a file to write to.</param>
		public CSharpStream(string path) : this(File.Create(path))
		{
		}
		/// <summary>
		/// Closes the underlying stream and releases all resources used by the current instance of the <see cref="CSharpStream" /> class.
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
		/// Emits the specified code.
		/// </summary>
		/// <param name="code">The code to emit.</param>
		public void Emit(string code)
		{
			BaseStream.WriteLine(code.TabIndent(Indent, 0));
		}
		/// <summary>
		/// Emits a label:
		/// <para>name:</para>
		/// </summary>
		/// <param name="name">The name of the label.</param>
		public void EmitLabel(string name)
		{
			BaseStream.WriteLine(name + ":");
		}
		/// <summary>
		/// Writes a curly bracket and increments <see cref="Indent" /> by 4.
		/// </summary>
		public void BlockBegin()
		{
			BaseStream.WriteLine("{".TabIndent(Indent, 0));
			Indent += 4;
		}
		/// <summary>
		/// Decrements <see cref="Indent" /> by 4 and writes a curly bracket.
		/// </summary>
		public void BlockEnd()
		{
			Indent -= 4;
			BaseStream.WriteLine("}".TabIndent(Indent, 0));
		}
		/// <summary>
		/// Emits a comment:
		/// <para>// comment</para>
		/// </summary>
		/// <param name="comment">The comment text.</param>
		public void EmitComment(string comment)
		{
			BaseStream.WriteLine(("// " + comment?.Replace("\r", @"\r").Replace("\n", @"\n")).TabIndent(Indent, 0));
		}
	}
}