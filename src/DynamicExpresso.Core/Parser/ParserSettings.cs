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
		readonly Dictionary<string, Expression> _identifiers;
		readonly Dictionary<string, Type> _knownTypes;
		readonly List<MethodInfo> _extensionMethods;

		public ParserSettings(bool caseInsensitive)
		{
			CaseInsensitive = caseInsensitive;

			SettingsKeyComparer = CaseInsensitive ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture;

			_identifiers = new Dictionary<string, Expression>(SettingsKeyComparer);

			_knownTypes = new Dictionary<string, Type>(SettingsKeyComparer);

			_extensionMethods = new List<MethodInfo>();
		}

		public IDictionary<string, Type> KnownTypes
		{
			get { return _knownTypes; }
		}

		public IDictionary<string, Expression> Identifiers
		{
			get { return _identifiers; }
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

	}
}
