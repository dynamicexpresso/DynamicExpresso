using System;
using System.Globalization;

namespace DynamicExpresso.Converters
{
	public class IntegerConverter : IConverter
	{
		private const NumberStyles ParseLiteralUnsignedNumberStyle = NumberStyles.AllowLeadingSign;
		private const NumberStyles ParseLiteralNumberStyle = NumberStyles.AllowLeadingSign;

		private static readonly CultureInfo ParseCulture = CultureInfo.InvariantCulture;

		public virtual object Convert(string text)
		{
			if (text[0] != '-')
			{
				if (!ulong.TryParse(text, ParseLiteralUnsignedNumberStyle, ParseCulture, out ulong value))
					throw new Exception();

				if (value <= int.MaxValue)
					return (int)value;
				if (value <= uint.MaxValue)
					return (uint)value;
				if (value <= long.MaxValue)
					return (long)value;

				return value;
			}
			else
			{
				if (!long.TryParse(text, ParseLiteralNumberStyle, ParseCulture, out long value))
					throw new Exception();

				if (value >= int.MinValue && value <= int.MaxValue)
					return (int)value;

				return value;
			}
		}
	}
}
