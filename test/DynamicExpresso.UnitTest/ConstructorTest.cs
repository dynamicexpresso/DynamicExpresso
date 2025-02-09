using System;
using System.Collections;
using System.Linq;
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

			Assert.That(target.Eval("new DateTime(2015, 1, 24)"), Is.EqualTo(new DateTime(2015, 1, 24)));
			Assert.That(target.Eval("new string('a', 10)"), Is.EqualTo(new string('a', 10)));
		}

		[Test]
		public void New_Of_Custom_Type()
		{
			var target = new Interpreter();

			target.Reference(typeof(Uri));

			Assert.That(target.Eval("new Uri(\"http://www.google.com\")"), Is.EqualTo(new Uri("http://www.google.com")));
		}

		[Test]
		public void New_And_Member_Access()
		{
			var target = new Interpreter();

			Assert.That(target.Eval("new DateTime(2015,   1, 24).Month"), Is.EqualTo(new DateTime(2015, 1, 24).Month));
			Assert.That(target.Eval("new DateTime( 2015, 1, 24).Month + 34"), Is.EqualTo(new DateTime(2015, 1, 24).Month + 34));
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

			Assert.That(target.Eval("new MyClass() {}"), Is.EqualTo(new MyClass() { }));
			Assert.That(target.Eval("new MyClass(\"test\") {}"), Is.EqualTo(new MyClass("test") { }));
			Assert.That(target.Eval("new MyClass{}"), Is.EqualTo(new MyClass { }));
		}

		[Test]
		public void Object_initializer()
		{
			var target = new Interpreter();
			target.Reference(typeof(MyClass));

			// each member initializer can end with a comma, even if there's nothing afterwards
			Assert.That(target.Eval("new MyClass { StrProp = \"test\", }"), Is.EqualTo(new MyClass { StrProp = "test", }));
			Assert.That(target.Eval("new MyClass { StrProp = \"test\" }"), Is.EqualTo(new MyClass { StrProp = "test" }));

			Assert.That(target.Eval("new MyClass(\"test\") { IntField = 5, }"), Is.EqualTo(new MyClass("test") { IntField = 5, }));
			Assert.That(target.Eval("new MyClass(\"test\") { IntField = 5 }"), Is.EqualTo(new MyClass("test") { IntField = 5 }));

			Assert.That(target.Eval("new MyClass() { StrProp = \"test\", IntField = 5, }"), Is.EqualTo(new MyClass() { StrProp = "test", IntField = 5, }));
			Assert.That(target.Eval("new MyClass() { StrProp = \"test\", IntField = 5 }"), Is.EqualTo(new MyClass() { StrProp = "test", IntField = 5 }));
		}

		[Test]
		public void Constructor_invocation_generics()
		{
			var target = new Interpreter();
			target.Reference(typeof(Tuple<>));
			target.Reference(typeof(Tuple<,>));
			target.Reference(typeof(Tuple<,,>));
			Assert.That(target.Eval("new Tuple<int>(1).Item1"), Is.EqualTo(1));
			Assert.That(target.Eval("new Tuple<int, string>(5, \"My str item\").Item2"), Is.EqualTo("My str item"));
			Assert.That(target.Eval("new Tuple<int, int, int>(1, 2, 3).Item3"), Is.EqualTo(3));
		}

		[Test]
		public void Constructor_invocation_named_generics_with_arity()
		{
			var target = new Interpreter();
			target.Reference(typeof(Tuple<>), "Toto`1");
			target.Reference(typeof(Tuple<,>), "Toto`2");
			target.Reference(typeof(Tuple<,,>), "Toto`3");
			Assert.That(target.Eval("new Toto<int>(1).Item1"), Is.EqualTo(1));
			Assert.That(target.Eval("new Toto<int, string>(5, \"My str item\").Item2"), Is.EqualTo("My str item"));
			Assert.That(target.Eval("new Toto<int, int, int>(1, 2, 3).Item3"), Is.EqualTo(3));
		}

		[Test]
		public void Constructor_invocation_named_generics()
		{
			var target = new Interpreter();
			target.Reference(typeof(Tuple<,>), "Tuple");
			Assert.That(target.Eval("new Tuple<int, string>(5, \"My str item\").Item2"), Is.EqualTo("My str item"));

			target.Reference(typeof(Tuple<>), "Tuple1");
			target.Reference(typeof(Tuple<,,>), "Tuple3");
			Assert.That(target.Eval("new Tuple1<int>(1).Item1"), Is.EqualTo(1));
			Assert.That(target.Eval("new Tuple3<int, int, int>(1, 2, 3).Item3"), Is.EqualTo(3));
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
			Assert.Throws<ParseException>(() => target.Parse("new MyClass() {5}")); // collection initializer not supported
		}

		[Test]
		public void Ctor_params_array()
		{
			var target = new Interpreter();
			target.Reference(typeof(MyClass));
			Assert.That(target.Eval("new MyClass(6, 5, 4, 3).MyArr"), Is.EqualTo(new MyClass(6, 5, 4, 3).MyArr));
		}

		[Test]
		public void Array_constructor()
		{
			var target = new Interpreter();
			var arr = target.Eval<int[]>("new int[] { 1, 2 }");
			Assert.That(arr.Length, Is.EqualTo(2));
			Assert.That(arr[0], Is.EqualTo(1));
			Assert.That(arr[1], Is.EqualTo(2));
		}

		[Test]
		public void Array_constructor_type_mismatch()
		{
			// Exception: an expression of type 'System.Char' cannot be used to initialize an array of type 'System.Int32'
			var target = new Interpreter();
			Assert.Throws<ParseException>(() => target.Eval<int[]>("new int[] { 1, 'a' }"));
		}

		[Test]
		public void Jagged_array_constructor()
		{
			var target = new Interpreter();
			var arr = target.Eval<int[][]>("new int[][] { new int[] { 1, 2, }, new int[] { 3, 4, }, }");
			Assert.That(arr.Length, Is.EqualTo(2));
			Assert.That(arr[0][0], Is.EqualTo(1));
			Assert.That(arr[0][1], Is.EqualTo(2));
			Assert.That(arr[1][0], Is.EqualTo(3));
			Assert.That(arr[1][1], Is.EqualTo(4));
		}

		[Test]
		public void Array_multi_dimension_constructor()
		{
			// creating a multidimensional array is not supported
			var target = new Interpreter();
			Assert.Throws<ParseException>(() => target.Parse("new int[,] { { 1 }, { 2 } }"));
		}

		[Test]
		public void Ctor_NewDictionaryWithItems()
		{
			var target = new Interpreter();
			target.Reference(typeof(System.Collections.Generic.Dictionary<,>));
			var l = target.Eval<System.Collections.Generic.Dictionary<int, string>>("new Dictionary<int, string>(){{1, \"1\"}, {2, \"2\"}, {3, \"3\"}, {4, \"4\"}, {5, \"5\"}}");
			Assert.That(l.Count, Is.EqualTo(5));
			for (int i = 0; i < l.Count; ++i)
			{
				Assert.That(l[i + 1], Is.EqualTo(i + 1 + ""));
			}
		}

		[Test]
		public void Ctor_NewMyClassWithItems()
		{
			var target = new Interpreter();
			target.Reference(typeof(MyClassAdder));

			Assert.That(target.Eval<MyClassAdder>("new MyClassAdder(){{ 1, 2, 3, 4, 5},{\"6\" },7	}.Add(true)"), Is.EqualTo(new MyClassAdder() { { 1, 2, 3, 4, 5 }, { "6" }, 7 }));
		}

		[Test]
		public void Ctor_NewMyClass_ExpectedValues()
		{
			var target = new Interpreter();
			target.Reference(typeof(MyClassAdder));
			target.Reference(typeof(MyClass));
			var strProp = new Parameter("StrProp", typeof(string).MakeByRefType(), "0");
			var intProp = new Parameter("IntField", typeof(int).MakeByRefType(), int.MaxValue);
			var args = new object[]
			{
				strProp.Value,
				intProp.Value
			};
			Assert.That(
				target.Parse("new MyClassAdder(){{ 1, 2, 3, 4, 5},{StrProp = \"6\" },7}", strProp, intProp).Invoke(args),
				Is.EqualTo(new MyClassAdder() { { 1, 2, 3, 4, 5 }, "6", 7 }));
			Assert.That(
				target.Eval<MyClassAdder>("new MyClassAdder(){{ 1, 2, 3, 4, 5},string.Empty, 7}"),
				Is.EqualTo(new MyClassAdder() { { 1, 2, 3, 4, 5 }, string.Empty, 7 }));

			var IntField = int.MaxValue;
			Assert.That(
				target.Parse("new MyClassAdder(){ { IntField = 5 }, { 1, 2, 3, 4, 5},{StrProp = \"6\" }, IntField}", strProp, intProp).Invoke(args),
				Is.EqualTo(new MyClassAdder() { { IntField = 5 }, { 1, 2, 3, 4, IntField }, "6" }));
		}

		[Test]
		public void Ctor_NewMyClass_CanStillUseMemberSyntax()
		{
			var target = new Interpreter();
			target.Reference(typeof(MyClassAdder));
			target.Reference(typeof(MyClass));
			Assert.That(
				target.Eval<MyClassAdder>("new MyClassAdder() {StrProp = string.Empty, MyArr = new int[] {1, 2, 3, 4, 5}, IntField = int.MinValue }"),
				Is.EqualTo(new MyClassAdder() { StrProp = string.Empty, MyArr = new[] { 1, 2, 3, 4, 5 }, IntField = int.MinValue }));
		}

		[Test]
		public void Ctor_InvalidInitializerMemberDeclarator()
		{
			var target = new Interpreter();
			target.Reference(typeof(MyClassAdder));
			target.Reference(typeof(MyClass));
			Assert.Throws<ParseException>(() => target.Eval<MyClassAdder>("new MyClassAdder(){{ 1, 2, 3, 4, 5},{StrProp = \"6\" },7	}"));
			//Start with collection, then do member init
			Assert.Throws<ParseException>(() => target.Eval<MyClassAdder>("new MyClassAdder(){{ 1, 2, 3, 4, 5},StrProp = \"6\" ,7	}"));
			//Member init first, then attempt a collection init.
			Assert.Throws<ParseException>(() => target.Eval<MyClassAdder>("new MyClassAdder(){StrProp = \"6\" ,{ 1, 2, 3, 4, 5},7	}"));
		}

		[Test]
		public void Ctor_CannotUseCollectionInitDoesNotImplementIEnumerable()
		{
			var target = new Interpreter();
			target.Reference(typeof(MyClassAdder));
			target.Reference(typeof(MyClass));
			Assert.Throws<ParseException>(() => target.Eval<MyClass>("new MyClass(){ 1, 2, 3, 4, 5}"));
		}

		[Test]
		public void Ctor_NewListWithItems()
		{
			Ctor_NewListGeneric<string>("\"1\"", "\"2\"", "\"3\"", "{string.Empty}", "string.Empty", "int.MaxValue.ToString()", "{int.MinValue.ToString()}");
			Ctor_NewListGeneric<int>("1", "2", "3", "int.MinValue", "int.MaxValue", "{int.MinValue}", "{int.MaxValue}");
			Ctor_NewListGeneric<object>("string.Empty", "int.MinValue");
		}

		[Test]
		public void Ctor_NewListCantFindAddMethod()
		{
			var target = new Interpreter();
			target.Reference(typeof(System.Collections.Generic.List<>));
			try
			{
				target.Eval<System.Collections.Generic.List<int>>("new List<int>(){string.Empty}");
			}
			catch (ParseException ex)
			{
				if (ex.Message.Contains("The best overloaded Add "))
				{
					Assert.That(ex.Message, Does.Contain("Add"));
				}
				else
				{
					throw;
				}
			}
			Assert.Throws<ParseException>(() => target.Eval<System.Collections.Generic.List<string>>("new List<string>(){int.MaxValue}"));
		}

		public void Ctor_NewListGeneric<TObject>(params string[] items)
		{
			var target = new Interpreter();
			target.Reference(typeof(System.Collections.Generic.List<>));
			target.Reference(typeof(TObject));
			//Create a random list of values to test.
			var actual = new System.Collections.Generic.List<TObject>();
			foreach (var v in items)
			{
				actual.Add(target.Eval<TObject>(v.Trim('}', '{')));
			}
			for (var min = 0; min < actual.Count; ++min)
			{
				for (var count = Math.Min(min, 1); count <= actual.Count - min; ++count)
				{
					var evalText = $"new List<{typeof(TObject).Name}>(){{{string.Join(",", items.Skip(min).Take(count))}}}";
					System.Collections.Generic.List<TObject> eval = null;
					Assert.DoesNotThrow(() => eval = target.Eval<System.Collections.Generic.List<TObject>>(evalText), evalText);
					Assert.That(eval.Count, Is.EqualTo(count));
					for (var i = 0; i < count; ++i)
					{
						Assert.That(eval[i], Is.EqualTo(actual[i + min]));
					}
				}
			}
		}

		[Test]
		public void Ctor_NewListWithString()
		{
			var target = new Interpreter();
			target.Reference(typeof(System.Collections.Generic.List<>));
			var list = target.Eval<System.Collections.Generic.List<string>>("new List<string>(){string.Empty}");
			Assert.That(list.Count, Is.EqualTo(1));
			for (int i = 0; i < list.Count; ++i)
			{
				Assert.That(list[i], Is.SameAs(string.Empty));
			}
			list = target.Eval<System.Collections.Generic.List<string>>("new List<string>(){StrProp = string.Empty}", new Parameter("StrProp", "0"));
			Assert.That(list[0], Is.SameAs(string.Empty));
			list = target.Eval<System.Collections.Generic.List<string>>("new List<string>(){{StrProp = string.Empty}}", new Parameter("StrProp", "0"));
			Assert.That(list[0], Is.SameAs(string.Empty));
			list = target.Eval<System.Collections.Generic.List<string>>("new List<string>(){StrValue()}", new Parameter("StrValue", new Func<string>(() => "Func")));
			Assert.That(list[0], Is.EqualTo("Func"));
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

		private class MyClassAdder : MyClass, System.Collections.IEnumerable
		{

			public MyClassAdder Add(string s)
			{
				StrProp = s;
				return this;
			}

			public MyClassAdder Add(int intValue)
			{
				IntField = intValue;
				return this;
			}

			public MyClassAdder Add(params int[] intValues)
			{
				MyArr = intValues;
				return this;
			}

			public MyClassAdder Add(bool returnMe)
			{
				if (returnMe)
				{
					return this;
				}
				return null;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				yield break;
			}

		}
	}
}
