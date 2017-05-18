using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

namespace DynamicExpresso.Parsing
{
	public enum ResolveExpressionType
	{
		Unknown,

		Identifier,
		PropertyOrField,
		Method,
		ExtensionMethod
	}
}
