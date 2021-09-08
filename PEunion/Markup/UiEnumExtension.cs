using BytecodeApi;
using PEunion.Compiler.UI;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace PEunion
{
	public sealed class UiEnumExtension : MarkupExtension
	{
		public Type Type { get; set; }

		public UiEnumExtension(Type type)
		{
			Type = type;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return EnumEx
				.GetValues(Type)
				.OrderBy(value =>
				{
					FieldInfo field = Type.GetField(value.ToString());
					return field.IsDefined(typeof(UiSortOrderAttribute)) ? field.GetCustomAttribute<UiSortOrderAttribute>().SortOrder : 0;
				})
				.ToDictionary(value => value, value => Type.GetField(value.ToString()).GetCustomAttribute<UiNameAttribute>().Name);
		}
	}
}