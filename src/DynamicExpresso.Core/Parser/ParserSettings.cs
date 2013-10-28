using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DynamicExpresso
{
	internal class ParserSettings
	{
		static Type[] predefinedTypes = {
            typeof(Object),
            typeof(Boolean),
            typeof(Char),
            typeof(String),
            typeof(SByte),
            typeof(Byte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(Math),
            typeof(Convert)
        };

		Dictionary<string, Expression> keywords;

		Dictionary<string, object> externals;

		Dictionary<string, Type> knownTypes;

		List<MethodInfo> _extensionMethods;

		public IDictionary<string, Type> KnownTypes
		{
			get { return knownTypes; }
		}
		public IDictionary<string, object> Externals
		{
			get { return externals; }
		}
		public IDictionary<string, Expression> Keywords
		{
			get { return keywords; }
		}
		public IList<MethodInfo> ExtensionMethods
		{
			get { return _extensionMethods; }
		}

		public ParserSettings()
		{
			keywords = new Dictionary<string, Expression>();
			externals = new Dictionary<string, object>();
			knownTypes = new Dictionary<string, Type>();
			_extensionMethods = new List<MethodInfo>();

			FillKnownTypes();
			FillKeywords();
		}

		void FillKeywords()
		{
			keywords.Add("true", Expression.Constant(true));
			keywords.Add("false", Expression.Constant(false));
			keywords.Add("null", ParserConstants.nullLiteral);
		}

		void FillKnownTypes()
		{
			foreach (Type type in predefinedTypes)
			{
				knownTypes.Add(type.Name, type);
			}

			knownTypes.Add("object", typeof(object));
			knownTypes.Add("string", typeof(string));
			knownTypes.Add("char", typeof(char));
			knownTypes.Add("bool", typeof(bool));
			knownTypes.Add("byte", typeof(byte));
			knownTypes.Add("int", typeof(int));
			knownTypes.Add("long", typeof(long));
			knownTypes.Add("double", typeof(double));
			knownTypes.Add("decimal", typeof(decimal));
		}
	}
}
