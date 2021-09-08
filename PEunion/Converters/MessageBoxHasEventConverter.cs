using BytecodeApi.UI;
using PEunion.Compiler.Project;

namespace PEunion
{
	public sealed class MessageBoxHasEventConverter : ConverterBase<MessageBoxButtons, MessageBoxEvent, bool>
	{
		public override bool Convert(MessageBoxButtons value, MessageBoxEvent parameter)
		{
			return MessageBoxAction.HasEvent(value, parameter);
		}
	}
}