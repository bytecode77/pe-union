namespace PEunion.Compiler.Helper
{
	/// <summary>
	/// Provides extensions to <see cref="string" /> objects for indentation in generated source code files.
	/// </summary>
	public static class TabStringExtensions
	{
		/// <summary>
		/// Indents a <see cref="string" /> with a specified number of spaces before and after the <see cref="string" />.
		/// Whole multiples of 4 spaces are represented as tabs.
		/// </summary>
		/// <param name="str">The <see cref="string" /> to indent.</param>
		/// <param name="before">The number of spaces before <paramref name="str" />.</param>
		/// <param name="after">The number of spaces after <paramref name="str" />.</param>
		/// <returns>
		/// The indented <see cref="string" /> equivalent of <paramref name="str" />.
		/// </returns>
		public static string TabIndent(this string str, int before, int after)
		{
			if (after > 0)
			{
				for (int length = str.TabLength(); length < after / 4 * 4; length = (length + 4) / 4 * 4)
				{
					str += '\t';
				}

				str += new string(' ', after % 4);
			}

			if (before > 0)
			{
				str = new string('\t', before / 4) + new string(' ', before % 4) + str;
			}

			return str;
		}
		/// <summary>
		/// Computes the width of a <see cref="string" /> respecting mixed tabs and spaces.
		/// </summary>
		/// <param name="str">The <see cref="string" /> to compute the width from.</param>
		/// <returns>
		/// The width of <paramref name="str" />.
		/// </returns>
		public static int TabLength(this string str)
		{
			int length = 0;
			foreach (char c in str)
			{
				if (c == '\t') length = (length + 4) / 4 * 4;
				else length++;
			}

			return length;
		}
	}
}