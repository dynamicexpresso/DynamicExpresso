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

		public bool CaseInsensitive
		{
			get;
			private set;
		}

		public IEqualityComparer<string> SettingsKeyComparer
		{
			get;
			private set;
		}

		public ParserSettings(bool caseInsensitive)
		{
			CaseInsensitive = caseInsensitive;

			SettingsKeyComparer = CaseInsensitive ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture;

			keywords = new Dictionary<string, Expression>(SettingsKeyComparer);
			externals = new Dictionary<string, object>();
			knownTypes = new Dictionary<string, Type>(SettingsKeyComparer);

			_extensionMethods = new List<MethodInfo>();
		}
	}
}
