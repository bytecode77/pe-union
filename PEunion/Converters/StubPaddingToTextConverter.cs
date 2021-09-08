using BytecodeApi.UI;
using System;

namespace PEunion
{
	public sealed class StubPaddingToTextConverter : ConverterBase<int, string>
	{
		public override string Convert(int value)
		{
			if (value < 0) return null;
			else if (value == 0) return "The compiled file is packed with no extra padding (high entropy).";
			else if (value < 25) return "The compiled file will be about " + value + " % larger (high entroy).";
			else if (value < 50) return "The compiled file will be about " + value + " % larger (medium to high entroy).";
			else if (value < 100) return "The compiled file will be about " + value + " % larger (medium entroy).";
			else return "The compiled file will be about " + Math.Round(1 + value / 100f, 1) + " times larger (low entroy).";
		}
	}
}