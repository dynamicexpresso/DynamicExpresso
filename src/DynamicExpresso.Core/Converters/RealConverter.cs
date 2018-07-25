using System;
using System.Globalization;

namespace DynamicExpresso.Converters
{
	public class RealConverter : IConverter
	{
		private const NumberStyles ParseLiteralUnsignedNumberStyle = NumberStyles.AllowLeadingSign;
		private const NumberStyles ParseLiteralNumberStyle = NumberStyles.AllowLeadingSign;
		private const NumberStyles ParseLiteralDecimalNumberStyle = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

		private static readonly CultureInfo ParseCulture = CultureInfo.InvariantCulture;

		public virtual object Convert(string text)
		{
			object value = null;
			var last = text[text.Length - 1];

			if (last == 'F' || last == 'f')
			{
				if (float.TryParse(text.Substring(0, text.Length - 1), ParseLiteralDecimalNumberStyle, ParseCulture, out float f))
					value = f;
			}
			else if (last == 'M' || last == 'm')
			{
				if (decimal.TryParse(text.Substring(0, text.Length - 1), ParseLiteralDecimalNumberStyle, ParseCulture, out decimal dc))
					value = dc;
			}
			else
			{
				if (double.TryParse(text, ParseLiteralDecimalNumberStyle, ParseCulture, out double d))
					value = d;
			}

			if (value == null)
				throw new Exception();

			return value;
		}
	}
}
