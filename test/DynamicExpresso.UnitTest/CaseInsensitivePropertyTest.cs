using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class CaseInsensitivePropertyTest
	{
		[Test]
		public void CaseInsensitive_Property_Default()
		{
			var target = new Interpreter();

			Assert.IsFalse(target.CaseInsensitive);
		}

		[Test]
		public void Setting_CaseInsensitive()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);

			Assert.IsTrue(target.CaseInsensitive);
		}

	}
}
