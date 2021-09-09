using BytecodeApi.FileIcons;
using BytecodeApi.UI;
using System.IO;
using System.Windows.Media;

namespace PEunion
{
	public sealed class PathToFileIconConverter : ConverterBase<string, ImageSource>
	{
		public override ImageSource Convert(string value)
		{
			if (Path.HasExtension(value))
			{
				return FileIcon.FromExtension(Path.GetExtension(value))?.Icon16ImageSource ?? SpecialFileIcons.Unknown.Icon16ImageSource;
			}
			else
			{
				return null;
			}
		}
	}
}