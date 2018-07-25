using System;
using DynamicExpresso.Exceptions;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class ConvertersTest
	{
		class IntegerConveter : Converters.IConverter
		{
			public object Convert(string text)
			{
				if (!decimal.TryParse(text, out decimal result))
					throw new Exception();

				return result;
			}
		}

		[Test]
		public void Multiplicate_Nullable_Decimal_By_Integer_Number()
		{
			var target = new Interpreter();

			var expression = target.Parse("x * y * 100", 
				new IntegerConveter(), null,
				new Parameter("x", typeof(decimal)), new Parameter("y", typeof(decimal?)));

			Assert.AreEqual(7 * 3 * 100, expression.Invoke(7M, 3M));
		}
	}
}
