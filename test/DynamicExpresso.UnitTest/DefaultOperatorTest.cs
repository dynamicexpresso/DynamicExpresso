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

			Assert.AreEqual(default(bool), target.Eval("default(bool)"));
			Assert.AreEqual(default(char), target.Eval("default(char)"));
			Assert.AreEqual(default(sbyte), target.Eval("default(sbyte)"));
			Assert.AreEqual(default(byte), target.Eval("default(byte)"));
			Assert.AreEqual(default(short), target.Eval("default(short)"));
			Assert.AreEqual(default(ushort), target.Eval("default(ushort)"));
			Assert.AreEqual(default(int), target.Eval("default(int)"));
			Assert.AreEqual(default(uint), target.Eval("default(uint)"));
			Assert.AreEqual(default(long), target.Eval("default(long)"));
			Assert.AreEqual(default(ulong), target.Eval("default(ulong)"));
			Assert.AreEqual(default(float), target.Eval("default(float)"));
			Assert.AreEqual(default(double), target.Eval("default(double)"));
			Assert.AreEqual(default(decimal), target.Eval("default(decimal)"));
			Assert.AreEqual(default(System.DateTime), target.Eval("default(DateTime)"));
			Assert.AreEqual(default(System.TimeSpan), target.Eval("default(TimeSpan)"));
			Assert.AreEqual(default(System.Guid), target.Eval("default(Guid)"));

			Assert.AreEqual(typeof(bool), target.Eval("default(bool)").GetType());
			Assert.AreEqual(typeof(char), target.Eval("default(char)").GetType());
			Assert.AreEqual(typeof(sbyte), target.Eval("default(sbyte)").GetType());
			Assert.AreEqual(typeof(byte), target.Eval("default(byte)").GetType());
			Assert.AreEqual(typeof(short), target.Eval("default(short)").GetType());
			Assert.AreEqual(typeof(ushort), target.Eval("default(ushort)").GetType());
			Assert.AreEqual(typeof(int), target.Eval("default(int)").GetType());
			Assert.AreEqual(typeof(uint), target.Eval("default(uint)").GetType());
			Assert.AreEqual(typeof(long), target.Eval("default(long)").GetType());
			Assert.AreEqual(typeof(ulong), target.Eval("default(ulong)").GetType());
			Assert.AreEqual(typeof(float), target.Eval("default(float)").GetType());
			Assert.AreEqual(typeof(double), target.Eval("default(double)").GetType());
			Assert.AreEqual(typeof(decimal), target.Eval("default(decimal)").GetType());
			Assert.AreEqual(typeof(System.DateTime), target.Eval("default(DateTime)").GetType());
			Assert.AreEqual(typeof(System.TimeSpan), target.Eval("default(TimeSpan)").GetType());
			Assert.AreEqual(typeof(System.Guid), target.Eval("default(Guid)").GetType());
		}

		[Test]
		public void Default_reference_type()
		{
			var target = new Interpreter();

			Assert.AreEqual(default(object), target.Eval("default(object)"));
			Assert.AreEqual(default(string), target.Eval("default(string)"));
		}

		[Test]
		public void Default_nullable_type()
		{
			var target = new Interpreter();

			Assert.AreEqual(default(int?), target.Eval("default(int?)"));
			Assert.AreEqual(default(double?), target.Eval("default(double?)"));
			Assert.AreEqual(default(System.DateTime?), target.Eval("default(DateTime?)"));
		}
	}
}
