using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DynamicExpresso.Parsing
{
	internal class ParserSettings
	{
		readonly Dictionary<string, Identifier> _identifiers;
		readonly Dictionary<string, ReferenceType> _knownTypes;
		readonly List<MethodInfo> _extensionMethods;

		public ParserSettings(bool caseInsensitive)
		{
			CaseInsensitive = caseInsensitive;

			SettingsKeyComparer = CaseInsensitive ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture;

			_identifiers = new Dictionary<string, Identifier>(SettingsKeyComparer);

			_knownTypes = new Dictionary<string, ReferenceType>(SettingsKeyComparer);

			_extensionMethods = new List<MethodInfo>();
		}

		public IDictionary<string, ReferenceType> KnownTypes
		{
			get { return _knownTypes; }
		}

		public IDictionary<string, Identifier> Identifiers
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
