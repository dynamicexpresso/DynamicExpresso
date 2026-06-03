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

			Assert.That(target.CaseInsensitive, Is.False);
		}

		[Test]
		public void Setting_CaseInsensitive()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);

			Assert.That(target.CaseInsensitive, Is.True);
		}

	}
}
