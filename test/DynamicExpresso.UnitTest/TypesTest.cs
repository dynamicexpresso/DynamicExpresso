﻿using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using DynamicExpresso.Exceptions;

namespace DynamicExpresso.UnitTest
{
	[TestFixture]
	public class TypesTest
	{
		[Test]
		public void Default_Types()
		{
			var target = new Interpreter();

			var predefinedTypes = new Dictionary<string, Type>{
                    {"Object", typeof(object)},
                    {"object", typeof(object)},
                    {"Boolean", typeof(bool)},
                    {"bool", typeof(bool)},
                    {"Char", typeof(char)},
                    {"char", typeof(char)},
                    {"String", typeof(string)},
                    {"string", typeof(string)},
                    {"SByte", typeof(sbyte)},
                    {"Byte", typeof(byte)},
                    {"byte", typeof(byte)},
                    {"Int16", typeof(short)},
                    {"UInt16", typeof(ushort)},
                    {"Int32", typeof(int)},
                    {"int", typeof(int)},
                    {"UInt32", typeof(uint)},
                    {"Int64", typeof(long)},
                    {"long", typeof(long)},
                    {"UInt64", typeof(ulong)},
                    {"Single", typeof(float)},
                    {"Double", typeof(double)},
                    {"double", typeof(double)},
                    {"Decimal", typeof(decimal)},
                    {"decimal", typeof(decimal)},
                    {"DateTime", typeof(DateTime)},
                    {"TimeSpan", typeof(TimeSpan)},
                    {"Guid", typeof(Guid)},
                    {"Math", typeof(Math)},
                    {"Convert", typeof(Convert)},
                };

			foreach (var t in predefinedTypes)
				Assert.AreEqual(t.Value, target.Eval(string.Format("typeof({0})", t.Key)));
		}

		[Test]
		public void Load_interpreter_without_any_configuration_doesn_t_recognize_types()
		{
			var target = new Interpreter(InterpreterOptions.None);

			Assert.Throws<UnknownIdentifierException>(() => target.Eval("typeof(string)"));
		}

		[Test]
		public void Custom_Types()
		{
			var target = new Interpreter()
											.Reference(typeof(Uri));

			Assert.AreEqual(typeof(Uri), target.Eval("typeof(Uri)"));
			Assert.AreEqual(new Uri("http://test"), target.Eval("new Uri(\"http://test\")"));
		}

		[Test]
		public void Reference_the_same_type_multiple_times_doesn_t_have_effect()
		{
			var target = new Interpreter()
											.Reference(typeof(string))
											.Reference(typeof(string))
											.Reference(typeof(Uri))
											.Reference(typeof(Uri))
											.Reference(typeof(Uri));

			Assert.AreEqual(typeof(Uri), target.Eval("typeof(Uri)"));
			Assert.AreEqual(new Uri("http://test"), target.Eval("new Uri(\"http://test\")"));
		}

		[Test]
		public void References_can_be_case_insensitive()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive)
											.Reference(typeof(string))
											.Reference(typeof(Uri));

			Assert.AreEqual(typeof(Uri), target.Eval("typeof(Uri)"));
			Assert.AreEqual(typeof(Uri), target.Eval("typeof(uri)"));
			Assert.AreEqual(typeof(Uri), target.Eval("typeof(URI)"));
			Assert.AreEqual(string.Empty, target.Eval("STRING.Empty"));
		}

		[Test]
		public void Custom_Type_Constructor()
		{
			var target = new Interpreter()
											.Reference(typeof(MyDataContract));

			Assert.AreEqual(new MyDataContract("davide").Name, target.Eval("new MyDataContract(\"davide\").Name"));
			Assert.AreEqual(new MyDataContract(44, 88).Name, target.Eval("new MyDataContract(44 , 88).Name"));
		}

		[Test]
		public void Custom_Type_Alias()
		{
			var target = new Interpreter()
											.Reference(typeof(MyDataContract), "DC");

			Assert.AreEqual(typeof(MyDataContract), target.Parse("new DC(\"davide\")").ReturnType);
		}

		[Test]
		public void Custom_Enum()
		{
			var target = new Interpreter()
											.Reference(typeof(MyCustomEnum));

			Assert.AreEqual(MyCustomEnum.Value1, target.Eval("MyCustomEnum.Value1"));
			Assert.AreEqual(MyCustomEnum.Value2, target.Eval("MyCustomEnum.Value2"));
		}

		[Test]
		public void Enum_are_case_sensitive_by_default()
		{
			var target = new Interpreter()
											.Reference(typeof(EnumCaseSensitive));

			Assert.AreEqual(EnumCaseSensitive.Test, target.Eval("EnumCaseSensitive.Test"));
			Assert.AreEqual(EnumCaseSensitive.TEST, target.Eval("EnumCaseSensitive.TEST"));
		}

		[Test]
		public void Getting_the_list_of_used_types()
		{
			var target = new Interpreter();

			var lambda = target.Parse("Math.Max(a, typeof(string).Name.Length)", new Parameter("a", 1));

			Assert.AreEqual(2, lambda.Types.Count());
			Assert.AreEqual("Math", lambda.Types.ElementAt(0).Name);
			Assert.AreEqual("string", lambda.Types.ElementAt(1).Name);
		}

		public enum EnumCaseSensitive
		{
			Test = 1,
			// ReSharper disable once InconsistentNaming
			TEST = 2
		}

		public enum MyCustomEnum
		{
			Value1,
			Value2
		}

		private class MyDataContract
		{
			public MyDataContract(string name)
			{
				Name = name;
			}

			public MyDataContract(int x, int y)
			{
				Name = string.Format("{0} - {1}", x, y);
			}

			public string Name { get; set; }
		}

		public class MyTestClass<T>
		{
			private T _val;
			public MyTestClass(T val)
			{
				_val = val;
			}

			public override string ToString()
			{
				if (_val == null)
					return "null";

				return _val.ToString();
			}
		}
	}
}
