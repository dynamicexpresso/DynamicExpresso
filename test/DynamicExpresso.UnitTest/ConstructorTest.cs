using System;
using DynamicExpresso.Exceptions;
using NUnit.Framework;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class ConstructorTest
	{
		[Test]
		public void New_Of_Base_Type()
		{
			var target = new Interpreter();

			Assert.AreEqual(new DateTime(2015, 1, 24), target.Eval("new DateTime(2015, 1, 24)"));
			Assert.AreEqual(new string('a', 10), target.Eval("new string('a', 10)"));
		}

		[Test]
		public void New_Of_Custom_Type()
		{
			var target = new Interpreter();

			target.Reference(typeof(Uri));

			Assert.AreEqual(new Uri("http://www.google.com"), target.Eval("new Uri(\"http://www.google.com\")"));
		}

		[Test]
		public void New_And_Member_Access()
		{
			var target = new Interpreter();

			Assert.AreEqual(new DateTime(2015, 1, 24).Month, target.Eval("new DateTime(2015,   1, 24).Month"));
			Assert.AreEqual(new DateTime(2015, 1, 24).Month + 34, target.Eval("new DateTime( 2015, 1, 24).Month + 34"));
		}

		[Test]
		public void Constructor_invocation_without_new_is_not_supported()
		{
			var target = new Interpreter();

			Assert.Throws<ParseException>(() => target.Parse("DateTime(2010, 5, 23)"));
		}

		[Test]
		public void Unknown_New_Type_Is_Not_Supported()
		{
			var target = new Interpreter();

			Assert.Throws<UnknownIdentifierException>(() => target.Parse("new unkkeyword()"));
		}

		[Test]
		public void Empty_object_initializer()
		{
			var target = new Interpreter();
			target.Reference(typeof(MyClass));

			Assert.AreEqual(new MyClass() { }, target.Eval("new MyClass() {}"));
			Assert.AreEqual(new MyClass("test") { }, target.Eval("new MyClass(\"test\") {}"));
			Assert.AreEqual(new MyClass { }, target.Eval("new MyClass{}"));
		}

		[Test]
		public void Object_initializer()
		{
			var target = new Interpreter();
			target.Reference(typeof(MyClass));

			// each member initializer can end with a comma, even if there's nothing afterwards
			Assert.AreEqual(new MyClass { StrProp = "test", }, target.Eval("new MyClass { StrProp = \"test\", }"));
			Assert.AreEqual(new MyClass { StrProp = "test" }, target.Eval("new MyClass { StrProp = \"test\" }"));

			Assert.AreEqual(new MyClass("test") { IntField = 5, }, target.Eval("new MyClass(\"test\") { IntField = 5, }"));
			Assert.AreEqual(new MyClass("test") { IntField = 5 }, target.Eval("new MyClass(\"test\") { IntField = 5 }"));

			Assert.AreEqual(new MyClass() { StrProp = "test", IntField = 5, }, target.Eval("new MyClass() { StrProp = \"test\", IntField = 5, }"));
			Assert.AreEqual(new MyClass() { StrProp = "test", IntField = 5 }, target.Eval("new MyClass() { StrProp = \"test\", IntField = 5 }"));
		}

		[Test]
		public void Constructor_invocation_generics()
		{
			var target = new Interpreter();
			target.Reference(typeof(Tuple<>));
			target.Reference(typeof(Tuple<,>));
			target.Reference(typeof(Tuple<,,>));
			Assert.AreEqual(1, target.Eval("new Tuple<int>(1).Item1"));
			Assert.AreEqual("My str item", target.Eval("new Tuple<int, string>(5, \"My str item\").Item2"));
			Assert.AreEqual(3, target.Eval("new Tuple<int, int, int>(1, 2, 3).Item3"));
		}

		[Test]
		public void Constructor_invocation_named_generics_with_arity()
		{
			var target = new Interpreter();
			target.Reference(typeof(Tuple<>), "Toto`1");
			target.Reference(typeof(Tuple<,>), "Toto`2");
			target.Reference(typeof(Tuple<,,>), "Toto`3");
			Assert.AreEqual(1, target.Eval("new Toto<int>(1).Item1"));
			Assert.AreEqual("My str item", target.Eval("new Toto<int, string>(5, \"My str item\").Item2"));
			Assert.AreEqual(3, target.Eval("new Toto<int, int, int>(1, 2, 3).Item3"));
		}

		[Test]
		public void Constructor_invocation_named_generics()
		{
			var target = new Interpreter();
			target.Reference(typeof(Tuple<,>), "Tuple");
			Assert.AreEqual("My str item", target.Eval("new Tuple<int, string>(5, \"My str item\").Item2"));

			target.Reference(typeof(Tuple<>), "Tuple1");
			target.Reference(typeof(Tuple<,,>), "Tuple3");
			Assert.AreEqual(1, target.Eval("new Tuple1<int>(1).Item1"));
			Assert.AreEqual(3, target.Eval("new Tuple3<int, int, int>(1, 2, 3).Item3"));
		}

		[Test]
		public void Object_initializer_syntax_error()
		{
			var target = new Interpreter();
			target.Reference(typeof(MyClass));

			Assert.Throws<ParseException>(() => target.Parse("new MyClass() { StrProp }"));
			Assert.Throws<ParseException>(() => target.Parse("new MyClass() { StrProp = }"));
			Assert.Throws<ArgumentException>(() => target.Parse("new MyClass() { StrProp = 5 }")); // type mismatch
			Assert.Throws<ArgumentException>(() => target.Parse("new MyClass() { ReadOnlyProp = 5 }")); // read only prop
			Assert.Throws<ParseException>(() => target.Parse("new MyClass() { UnkProp = 5 }")); // unknown property
			Assert.Throws<ParseException>(() => target.Parse("new MyClass() { StrProp ")); // no close bracket
			Assert.Throws<ParseException>(() => target.Parse("new MyClass() StrProp }")); // no open bracket
			Assert.Throws<ParseException>(() => target.Parse("new MyClass() {{IntField = 5}}")); // multiple bracket
			Assert.Throws<ParseException>(() => target.Parse("new MyClass() {5}")); // no field name
		}

		[Test]
		public void Ctor_params_array()
		{
			var target = new Interpreter();
			target.Reference(typeof(MyClass));
			Assert.AreEqual(new MyClass(6, 5, 4, 3).MyArr, target.Eval("new MyClass(6, 5, 4, 3).MyArr"));
		}

		private class MyClass
		{
			public int IntField;
			public string StrProp { get; set; }
			public int ReadOnlyProp { get; }
			public int[] MyArr { get; set; }

			public MyClass()
			{
			}

			public MyClass(string strValue)
			{
				StrProp = strValue;
			}

			public MyClass(params int[] intValues)
			{
				MyArr = intValues;
			}

			public override bool Equals(object obj)
			{
				return Equals(obj as MyClass);
			}

			public bool Equals(MyClass p)
			{
				if (p is null) return false;
				if (ReferenceEquals(this, p)) return true;
				return IntField == p.IntField && StrProp == p.StrProp && ReadOnlyProp == p.ReadOnlyProp;
			}

			// remove compilation warning
			public override int GetHashCode()
			{
				return 0;
			}
		}
	}
}
