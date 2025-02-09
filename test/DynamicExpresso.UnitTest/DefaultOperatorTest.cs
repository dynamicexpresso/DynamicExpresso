using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class DefaultOperatorTest
	{
		[Test]
		public void Default_value_type()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("default(bool)"), Is.EqualTo(default(bool)));
			Assert.That(target.Eval("default(char)"), Is.EqualTo(default(char)));
			Assert.That(target.Eval("default(sbyte)"), Is.EqualTo(default(sbyte)));
			Assert.That(target.Eval("default(byte)"), Is.EqualTo(default(byte)));
			Assert.That(target.Eval("default(short)"), Is.EqualTo(default(short)));
			Assert.That(target.Eval("default(ushort)"), Is.EqualTo(default(ushort)));
			Assert.That(target.Eval("default(int)"), Is.EqualTo(default(int)));
			Assert.That(target.Eval("default(uint)"), Is.EqualTo(default(uint)));
			Assert.That(target.Eval("default(long)"), Is.EqualTo(default(long)));
			Assert.That(target.Eval("default(ulong)"), Is.EqualTo(default(ulong)));
			Assert.That(target.Eval("default(float)"), Is.EqualTo(default(float)));
			Assert.That(target.Eval("default(double)"), Is.EqualTo(default(double)));
			Assert.That(target.Eval("default(decimal)"), Is.EqualTo(default(decimal)));
			Assert.That(target.Eval("default(DateTime)"), Is.EqualTo(default(System.DateTime)));
			Assert.That(target.Eval("default(TimeSpan)"), Is.EqualTo(default(System.TimeSpan)));
			Assert.That(target.Eval("default(Guid)"), Is.EqualTo(default(System.Guid)));

			Assert.That(target.Eval("default(bool)").GetType(), Is.EqualTo(typeof(bool)));
			Assert.That(target.Eval("default(char)").GetType(), Is.EqualTo(typeof(char)));
			Assert.That(target.Eval("default(sbyte)").GetType(), Is.EqualTo(typeof(sbyte)));
			Assert.That(target.Eval("default(byte)").GetType(), Is.EqualTo(typeof(byte)));
			Assert.That(target.Eval("default(short)").GetType(), Is.EqualTo(typeof(short)));
			Assert.That(target.Eval("default(ushort)").GetType(), Is.EqualTo(typeof(ushort)));
			Assert.That(target.Eval("default(int)").GetType(), Is.EqualTo(typeof(int)));
			Assert.That(target.Eval("default(uint)").GetType(), Is.EqualTo(typeof(uint)));
			Assert.That(target.Eval("default(long)").GetType(), Is.EqualTo(typeof(long)));
			Assert.That(target.Eval("default(ulong)").GetType(), Is.EqualTo(typeof(ulong)));
			Assert.That(target.Eval("default(float)").GetType(), Is.EqualTo(typeof(float)));
			Assert.That(target.Eval("default(double)").GetType(), Is.EqualTo(typeof(double)));
			Assert.That(target.Eval("default(decimal)").GetType(), Is.EqualTo(typeof(decimal)));
			Assert.That(target.Eval("default(DateTime)").GetType(), Is.EqualTo(typeof(System.DateTime)));
			Assert.That(target.Eval("default(TimeSpan)").GetType(), Is.EqualTo(typeof(System.TimeSpan)));
			Assert.That(target.Eval("default(Guid)").GetType(), Is.EqualTo(typeof(System.Guid)));
		}

		[Test]
		public void Default_reference_type()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("default(object)"), Is.EqualTo(default(object)));
			Assert.That(target.Eval("default(string)"), Is.EqualTo(default(string)));
		}

		[Test]
		public void Default_nullable_type()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("default(int?)"), Is.EqualTo(default(int?)));
			Assert.That(target.Eval("default(double?)"), Is.EqualTo(default(double?)));
			Assert.That(target.Eval("default(DateTime?)"), Is.EqualTo(default(System.DateTime?)));
		}
	}
}
