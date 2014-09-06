using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections;

namespace DynamicExpresso.UnitTest
{
	[TestClass]
	public class CaseInsensitivePropertyTest
	{
		[TestMethod]
		public void CaseInsensitive_Property_Default()
		{
			var target = new Interpreter();

			Assert.IsFalse(target.CaseInsensitive);
		}

		[TestMethod]
		public void Setting_CaseInsensitive()
		{
			var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);

			Assert.IsTrue(target.CaseInsensitive);
		}

	}
}
